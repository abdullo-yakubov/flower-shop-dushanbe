import { useState, useMemo, useEffect } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import type { TFunction } from 'i18next'
import { motion, AnimatePresence } from 'framer-motion'
import { useCartStore } from '@/features/cart/store'
import { TIME_SLOTS } from '@/shared/config/deliveryConfig'
import { useOrdersStore } from '@/features/orders/store'
import type { OrderAddon } from '@/features/orders/store'
import { useLoyaltyStore, calcEarn, MIN_REDEEM } from '@/features/loyalty/store'
import { usePushNotifications } from '@/features/push/usePushNotifications'
import { usePushStore } from '@/features/push/store'
import { usePageTitle } from '@/shared/hooks/usePageTitle'
import { Button } from '@/shared/ui/Button'
import { Input } from '@/shared/ui/Input'
import { formatPrice } from '@/shared/lib/format'
import { useCheckoutResources, type CheckoutAddon } from './useCheckoutResources'
import { API_ENABLED, extractError } from '@/shared/lib/api/client'
import { DELIVERY_PRICE_DEFAULT, FREE_DELIVERY_THRESHOLD } from '@/shared/config/appConfig'
import { apiCreateOrder } from '@/shared/lib/api/orders'
import { toast } from '@/shared/ui/toast.store'

// ─── Config ──────────────────────────────────────────────────────────────────

const DELIVERY_FEE = DELIVERY_PRICE_DEFAULT

const PAYMENTS = [
  { id: 'dc', tKey: 'checkout.pay_dc', icon: '💳' },
  { id: 'alif', tKey: 'checkout.pay_alif', icon: '📱' },
  { id: 'cash', tKey: 'checkout.pay_cash', icon: '💵' },
] as const

// Use TIME_SLOTS from deliveryConfig — single source of truth for time slot strings
const TIMES = TIME_SLOTS

// ─── Helpers ─────────────────────────────────────────────────────────────────

function getDeliveryDates(t: TFunction) {
  const today = new Date()
  return Array.from({ length: 7 }, (_, i) => {
    const d = new Date(today)
    d.setDate(today.getDate() + i)
    return {
      label:
        i === 0
          ? t('checkout.delivery_today')
          : i === 1
            ? t('checkout.delivery_tomorrow')
            : `${t(`checkout.day_${d.getDay()}`)} ${d.getDate()}`,
      // Local calendar date — toISOString() is UTC and shifts "today" back a day before 05:00 in Dushanbe
      value: `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`,
    }
  })
}

// ─── Schema ──────────────────────────────────────────────────────────────────

function makeSchema(t: TFunction) {
  return z.object({
    isGift: z.boolean(),
    recipientName: z.string().optional(),
    recipientPhone: z.string().optional(),
    anonymous: z.boolean().optional(),
    address: z.string().min(5, t('checkout.err_address')),
    date: z.string().min(1, t('checkout.err_date')),
    time: z.string().min(1, t('checkout.err_time')),
    payment: z.enum(['dc', 'alif', 'cash']),
    name: z.string().min(2, t('checkout.err_name')),
    phone: z
      .string()
      .min(9, t('checkout.err_phone'))
      .regex(/^\+992/, t('checkout.err_phone_format')),
  })
}
type FormData = z.infer<ReturnType<typeof makeSchema>>

// ─── Addon item ───────────────────────────────────────────────────────────────

function AddonItem({
  addon,
  qty,
  onAdd,
  onRemove,
  inscription,
  onInscription,
  t,
  useTranslationKeys,
}: {
  addon: CheckoutAddon
  qty: number
  onAdd: () => void
  onRemove: () => void
  inscription: string
  onInscription: (v: string) => void
  t: TFunction
  useTranslationKeys: boolean
}) {
  const displayName = useTranslationKeys ? t(addon.name) : addon.name
  return (
    <div className="flex gap-4 p-4 bg-surface rounded-xl border border-border">
      <div className="text-4xl w-12 h-12 flex items-center justify-center shrink-0">
        {addon.emoji}
      </div>
      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between gap-2">
          <div>
            <p className="text-sm font-medium text-ink">{displayName}</p>
            <p className="text-sm text-green-deep font-semibold">
              {addon.price} {t('common.som')}
            </p>
          </div>
          {qty === 0 ? (
            <button
              type="button"
              onClick={onAdd}
              className="shrink-0 px-4 py-1.5 rounded-full bg-green-deep text-white text-xs font-semibold hover:bg-green-light transition-colors"
            >
              {t('checkout.add_button')}
            </button>
          ) : (
            <div className="flex items-center gap-2 shrink-0">
              <button
                type="button"
                onClick={onRemove}
                className="w-7 h-7 rounded-full border border-border text-sm hover:bg-cream transition-colors text-ink"
              >
                −
              </button>
              <span className="text-sm font-medium tabular-nums text-ink w-4 text-center">
                {qty}
              </span>
              <button
                type="button"
                onClick={onAdd}
                className="w-7 h-7 rounded-full border border-border text-sm hover:bg-cream transition-colors text-ink"
              >
                +
              </button>
            </div>
          )}
        </div>
        {addon.hasInscription && qty > 0 && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            className="mt-2 overflow-hidden"
          >
            <input
              type="text"
              value={inscription}
              onChange={(e) => onInscription(e.target.value.slice(0, 40))}
              placeholder={t('checkout.inscription_ph')}
              className="w-full h-9 px-3 rounded-lg border border-border text-sm outline-none focus:border-green-deep bg-cream text-ink placeholder:text-muted"
            />
            <p className="text-[10px] text-muted mt-0.5 text-right">{inscription.length}/40</p>
          </motion.div>
        )}
      </div>
    </div>
  )
}

// ─── Checkout page ────────────────────────────────────────────────────────────

export function CheckoutPage() {
  const { t, i18n } = useTranslation()
  usePageTitle(t('checkout.title'))
  const { items, total, promoDiscount, clearCart } = useCartStore()
  const { addOrder, confirmOrder, discardOrder } = useOrdersStore()
  const {
    points: loyaltyBalance,
    earnPoints,
    redeemPoints,
    maxRedeemable,
    syncFromServer: syncLoyalty,
  } = useLoyaltyStore()
  const navigate = useNavigate()
  const {
    addons: ADDONS,
    postcards: POSTCARD_DESIGNS,
    templates: MESSAGE_TEMPLATES,
    useTranslationKeys,
  } = useCheckoutResources()

  const [loading, setLoading] = useState(false)
  const [success, setSuccess] = useState(false)
  const [orderNum, setOrderNum] = useState('')
  const [earnedPts, setEarnedPts] = useState(0)

  // Points redemption
  const [usePoints, setUsePoints] = useState(false)
  const [redeemAmt, setRedeemAmt] = useState(0)

  // Postcard
  const [selectedDesign, setSelectedDesign] = useState<string | null>(null)
  const [cardText, setCardText] = useState('')
  const [activeCategory, setActiveCategory] = useState<
    'birthday' | 'romantic' | 'universal' | 'funeral'
  >('birthday')

  // Addons
  const [addonQtys, setAddonQtys] = useState<Record<string, number>>({})
  const [inscriptions, setInscriptions] = useState<Record<string, string>>({})

  const DATES = useMemo(() => getDeliveryDates(t), [t, i18n.language]) // eslint-disable-line react-hooks/exhaustive-deps
  const schema = useMemo(() => makeSchema(t), [t])

  const {
    register,
    handleSubmit,
    control,
    setValue,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { isGift: false, payment: 'dc', phone: '+992' },
  })

  const isGift = useWatch({ control, name: 'isGift', defaultValue: false })
  const selectedDate = useWatch({ control, name: 'date' })
  const selectedTime = useWatch({ control, name: 'time' })
  const selectedPayment = useWatch({ control, name: 'payment', defaultValue: 'dc' })

  // Use snapshotPrice — price locked at the time of adding to cart
  const subtotal = items.reduce((s, i) => s + i.snapshotPrice * i.quantity, 0)
  const delivery = subtotal >= FREE_DELIVERY_THRESHOLD ? 0 : DELIVERY_FEE
  const discount = promoDiscount ? Math.round((subtotal * promoDiscount) / 100) : 0
  const addonsTotal = ADDONS.reduce((s, a) => s + (addonQtys[a.id] || 0) * a.price, 0)
  const pointsDiscount = usePoints ? redeemAmt : 0
  const grandTotal = total() + delivery + addonsTotal - pointsDiscount

  const willEarn = calcEarn(grandTotal)
  const maxPts = maxRedeemable(total() + delivery + addonsTotal)

  const filteredTemplates = MESSAGE_TEMPLATES.filter(
    (tpl) => tpl.category === activeCategory || tpl.category === 'universal'
  )

  const toggleUsePoints = () => {
    if (!usePoints) {
      const def = Math.min(loyaltyBalance, maxPts)
      setRedeemAmt(def >= MIN_REDEEM ? def : 0)
    } else {
      setRedeemAmt(0)
    }
    setUsePoints((p) => !p)
  }

  const onSubmit = async (data: FormData) => {
    if (loading) return // guard against double submit
    setLoading(true)
    let tempId: string | null = null
    try {
      const orderAddons: OrderAddon[] = ADDONS.filter((a) => (addonQtys[a.id] || 0) > 0).map(
        (a) => ({
          id: a.id,
          name: useTranslationKeys ? t(a.name) : a.name,
          price: a.price,
          quantity: addonQtys[a.id],
          inscription: inscriptions[a.id] || undefined,
        })
      )

      // Create a local order optimistically for instant UI feedback
      tempId = addOrder({
        items: items.map((i) => ({
          productId: i.product.id,
          productName: i.product.name,
          productImg: i.product.images[0],
          size: i.stemCount != null ? `${i.stemCount} ${t('checkout.pcs')}` : i.size.label,
          quantity: i.quantity,
          price: i.snapshotPrice,
        })),
        addons: orderAddons,
        cardText: cardText || undefined,
        subtotal: subtotal + addonsTotal,
        deliveryFee: delivery,
        promoDiscount: discount,
        pointsDiscount: usePoints ? redeemAmt : 0,
        total: grandTotal,
        address: data.address,
        deliveryDate: DATES.find((d) => d.value === data.date)?.label ?? data.date,
        deliveryTime: data.time,
        payment: data.payment,
      })

      if (API_ENABLED) {
        // Submit to backend
        const result = await apiCreateOrder({
          items: items.map((i) => ({
            productId: i.product.id,
            sizeId: i.size.id,
            quantity: i.quantity,
            packaging: i.packaging,
            stemCount: i.stemCount ?? undefined,
          })),
          addons: orderAddons.map((a) => ({
            addonId: a.id,
            quantity: a.quantity,
            inscription: a.inscription,
          })),
          postcardId: selectedDesign || undefined,
          cardText: cardText || undefined,
          address: data.address,
          deliveryDate: data.date,
          deliveryTime: data.time,
          paymentMethod: data.payment,
          customerName: data.name,
          customerPhone: data.phone,
          isGift: data.isGift,
          recipientName: data.recipientName,
          recipientPhone: data.recipientPhone,
          isAnonymous: data.anonymous,
          promoCode: useCartStore.getState().promoCode || undefined,
          redeemPoints: usePoints && redeemAmt >= MIN_REDEEM ? redeemAmt : undefined,
        })

        // Confirm local order with server-issued number
        confirmOrder(tempId, result.orderId, result.orderNumber)
        setOrderNum(result.orderNumber)
        setEarnedPts(result.earnedPoints)

        // Redeem points locally to keep store in sync
        if (usePoints && redeemAmt >= MIN_REDEEM) redeemPoints(redeemAmt)

        // Refresh loyalty balance from server
        void syncLoyalty()
      } else {
        // Mock path: simulate delay + local points
        await new Promise((r) => setTimeout(r, 1500))
        if (usePoints && redeemAmt >= MIN_REDEEM) redeemPoints(redeemAmt)
        const earned = earnPoints(grandTotal)
        setEarnedPts(earned)
        setOrderNum(tempId)
      }

      setSuccess(true)
      clearCart()
    } catch (err) {
      // The server rejected the order — remove the optimistic copy so history stays truthful
      if (tempId) discardOrder(tempId)
      const apiError = extractError(err)
      toast.error(
        apiError.code === 'NETWORK_ERROR' || apiError.code === 'UNKNOWN'
          ? t('checkout.order_error')
          : apiError.message
      )
    } finally {
      setLoading(false)
    }
  }

  if (items.length === 0 && !success && !loading) {
    navigate('/')
    return null
  }

  if (success) return <SuccessScreen orderNum={orderNum} earnedPts={earnedPts} />

  return (
    <div className="max-w-5xl mx-auto px-4 md:px-6 py-8">
      <h1 className="font-serif text-3xl font-bold text-ink mb-8">{t('checkout.title')}</h1>

      <form
        onSubmit={handleSubmit(onSubmit)}
        className="grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-8"
      >
        <div className="space-y-6">
          {/* ── 1. Получатель ── */}
          <Section num={1} title={t('checkout.recipient')}>
            <div className="flex gap-3">
              {[t('checkout.for_me'), t('checkout.as_gift')].map((label, i) => (
                <button
                  key={label}
                  type="button"
                  onClick={() => setValue('isGift', !!i)}
                  className={`flex-1 py-3 rounded-xl border-2 text-sm font-medium transition-all ${
                    (i === 0) === !isGift
                      ? 'border-green-deep bg-green-deep/5 text-green-deep'
                      : 'border-border text-muted hover:border-green-deep/40 bg-surface'
                  }`}
                >
                  {label}
                </button>
              ))}
            </div>
            <Input label={t('checkout.name')} error={errors.name?.message} {...register('name')} />
            <Input
              label={t('checkout.phone')}
              type="tel"
              error={errors.phone?.message}
              {...register('phone')}
            />
            <AnimatePresence>
              {isGift && (
                <motion.div
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: 'auto' }}
                  exit={{ opacity: 0, height: 0 }}
                  className="overflow-hidden space-y-3"
                >
                  <Input label={t('checkout.recipient_name')} {...register('recipientName')} />
                  <Input
                    label={t('checkout.recipient_phone')}
                    type="tel"
                    {...register('recipientPhone')}
                  />
                  <label className="flex items-center gap-3 cursor-pointer">
                    <input
                      type="checkbox"
                      {...register('anonymous')}
                      className="w-4 h-4 accent-green-deep"
                    />
                    <span className="text-sm text-ink">{t('checkout.anonymous')}</span>
                  </label>
                </motion.div>
              )}
            </AnimatePresence>
          </Section>

          {/* ── 2. Открытка ── */}
          <Section num={2} title={t('checkout.postcard')}>
            <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
              {(
                [
                  { id: 'birthday', label: t('checkout.cat_birthday') },
                  { id: 'romantic', label: t('checkout.cat_romantic') },
                  { id: 'universal', label: t('checkout.cat_universal') },
                  { id: 'funeral', label: t('checkout.cat_funeral') },
                ] as const
              ).map((cat) => (
                <button
                  key={cat.id}
                  type="button"
                  onClick={() => setActiveCategory(cat.id)}
                  className={`shrink-0 px-3 py-1.5 rounded-full text-xs font-medium border transition-all ${
                    activeCategory === cat.id
                      ? 'border-green-deep bg-green-deep text-white'
                      : 'border-border text-muted hover:border-green-deep/40 bg-surface'
                  }`}
                >
                  {cat.label}
                </button>
              ))}
            </div>

            <div className="grid grid-cols-4 md:grid-cols-7 gap-2">
              {POSTCARD_DESIGNS.map((d) => (
                <button
                  key={d.id}
                  type="button"
                  onClick={() => setSelectedDesign(d.id === selectedDesign ? null : d.id)}
                  className={`relative aspect-square rounded-xl border-2 flex flex-col items-center justify-center gap-1 transition-all text-center p-1 ${
                    selectedDesign === d.id
                      ? 'border-green-deep bg-green-deep/8'
                      : 'border-border hover:border-green-deep/40 bg-surface'
                  }`}
                >
                  <span className="text-2xl">{d.emoji}</span>
                  {d.label && (
                    <span className="text-[9px] text-muted leading-tight">
                      {useTranslationKeys ? t(d.label) : d.label}
                    </span>
                  )}
                  {d.isPopular && (
                    <span className="absolute -top-1 -right-1 bg-rose-dust text-ink text-[8px] font-bold px-1 rounded-md leading-4">
                      ★
                    </span>
                  )}
                </button>
              ))}
            </div>

            <div>
              <p className="text-xs font-semibold text-muted uppercase tracking-wide mb-2">
                {t('checkout.templates')}
              </p>
              <div className="space-y-2">
                {filteredTemplates.map((tpl) => {
                  const displayText = useTranslationKeys ? t(tpl.text) : tpl.text
                  return (
                    <button
                      key={tpl.id}
                      type="button"
                      onClick={() => setCardText(displayText)}
                      className="w-full text-left px-3 py-2.5 rounded-xl border border-border text-xs text-muted hover:border-green-deep hover:text-ink transition-all bg-surface leading-relaxed"
                    >
                      {tpl.isPopular && <span className="text-amber-400 mr-1">★</span>}
                      {displayText}
                    </button>
                  )
                })}
              </div>
            </div>

            <div className="relative">
              <textarea
                value={cardText}
                onChange={(e) => setCardText(e.target.value.slice(0, 200))}
                placeholder={t('checkout.postcard_text_ph')}
                rows={3}
                className="w-full px-4 py-3 rounded-xl border border-border text-sm outline-none resize-none focus:border-green-deep focus:ring-2 focus:ring-green-deep/15 bg-surface text-ink placeholder:text-muted"
              />
              <span
                className={`absolute bottom-3 right-3 text-xs ${cardText.length >= 180 ? 'text-error' : 'text-muted'}`}
              >
                {cardText.length}/200
              </span>
            </div>

            <AnimatePresence>
              {(cardText || selectedDesign) && (
                <motion.div
                  initial={{ opacity: 0, y: 4 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0 }}
                  className="border border-border rounded-xl p-5 bg-cream"
                >
                  {selectedDesign && (
                    <p className="text-3xl mb-2">
                      {POSTCARD_DESIGNS.find((d) => d.id === selectedDesign)?.emoji}
                    </p>
                  )}
                  {cardText && (
                    <p className="font-serif text-base italic text-ink leading-relaxed">
                      {cardText}
                    </p>
                  )}
                </motion.div>
              )}
            </AnimatePresence>
          </Section>

          {/* ── 3. Доп. товары ── */}
          <Section num={3} title={t('checkout.addons')}>
            <p className="text-sm text-muted -mt-1">{t('checkout.addons_hint')}</p>
            <div className="space-y-3">
              {ADDONS.map((addon) => (
                <AddonItem
                  key={addon.id}
                  addon={addon}
                  qty={addonQtys[addon.id] || 0}
                  onAdd={() => setAddonQtys((p) => ({ ...p, [addon.id]: (p[addon.id] || 0) + 1 }))}
                  onRemove={() =>
                    setAddonQtys((p) => ({ ...p, [addon.id]: Math.max(0, (p[addon.id] || 0) - 1) }))
                  }
                  inscription={inscriptions[addon.id] || ''}
                  onInscription={(v) => setInscriptions((p) => ({ ...p, [addon.id]: v }))}
                  t={t}
                  useTranslationKeys={useTranslationKeys}
                />
              ))}
            </div>
          </Section>

          {/* ── 4. Доставка ── */}
          <Section num={4} title={t('checkout.delivery')}>
            <Input
              label={t('checkout.address')}
              placeholder={t('checkout.address_ph')}
              error={errors.address?.message}
              {...register('address')}
            />
            <div>
              <p className="text-sm font-semibold text-ink mb-2">{t('checkout.date')}</p>
              <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
                {DATES.map((d) => (
                  <button
                    key={d.value}
                    type="button"
                    onClick={() => setValue('date', d.value)}
                    className={`shrink-0 px-4 py-2.5 rounded-xl text-sm font-medium border-2 transition-all duration-150 ${
                      selectedDate === d.value
                        ? 'border-green-deep bg-green-deep text-white'
                        : 'border-border hover:border-green-deep/40 bg-surface text-ink'
                    }`}
                  >
                    {d.label}
                  </button>
                ))}
              </div>
              {errors.date && <p className="text-xs text-error mt-1">{errors.date.message}</p>}
            </div>
            <div>
              <p className="text-sm font-semibold text-ink mb-2">{t('checkout.time')}</p>
              <div className="flex flex-wrap gap-2">
                {TIMES.map((slot) => (
                  <button
                    key={slot}
                    type="button"
                    onClick={() => setValue('time', slot)}
                    className={`px-4 py-2 rounded-full text-sm border-2 transition-all duration-150 ${
                      selectedTime === slot
                        ? 'border-green-deep bg-green-deep text-white'
                        : 'border-border hover:border-green-deep/40 bg-surface text-ink'
                    }`}
                  >
                    {slot}
                  </button>
                ))}
              </div>
              {errors.time && <p className="text-xs text-error mt-1">{errors.time.message}</p>}
            </div>
          </Section>

          {/* ── 5. Оплата ── */}
          <Section num={5} title={t('checkout.payment')}>
            {PAYMENTS.map((pay) => (
              <label
                key={pay.id}
                className={`flex items-center gap-4 p-4 rounded-xl border-2 cursor-pointer transition-all duration-150 ${
                  selectedPayment === pay.id
                    ? 'border-green-deep bg-green-deep/5'
                    : 'border-border hover:border-green-deep/40 bg-surface'
                }`}
              >
                <input type="radio" {...register('payment')} value={pay.id} className="sr-only" />
                <span className="text-2xl">{pay.icon}</span>
                <span className="text-sm font-medium text-ink">{t(pay.tKey)}</span>
                {selectedPayment === pay.id && (
                  <motion.span
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    className="ml-auto text-green-deep font-semibold text-lg"
                  >
                    ✓
                  </motion.span>
                )}
              </label>
            ))}
          </Section>
        </div>

        {/* ── Order summary sidebar ── */}
        <div className="lg:sticky lg:top-20 self-start space-y-4">
          <div className="bg-surface rounded-2xl p-6 shadow-[0_4px_24px_rgba(0,0,0,0.06)] space-y-3">
            <h2 className="font-serif text-xl font-semibold text-ink mb-1">
              {t('checkout.your_order')}
            </h2>

            {items.map((item) => {
              const price =
                item.stemCount != null && item.product.stemPrice
                  ? item.product.stemPrice * item.stemCount
                  : item.size.price
              return (
                <div key={item.id} className="flex items-center gap-3">
                  <img
                    src={item.product.images[0]}
                    alt=""
                    className="w-12 h-12 rounded-lg object-cover shrink-0"
                  />
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-medium text-ink line-clamp-1">{item.product.name}</p>
                    <p className="text-xs text-muted">
                      {item.stemCount != null
                        ? `${item.stemCount} ${t('checkout.pcs')}`
                        : item.size.label}{' '}
                      × {item.quantity}
                    </p>
                  </div>
                  <span className="text-xs font-semibold tabular-nums text-green-deep">
                    {price * item.quantity} {t('common.som')}
                  </span>
                </div>
              )
            })}

            {ADDONS.filter((a) => (addonQtys[a.id] || 0) > 0).map((a) => (
              <div key={a.id} className="flex items-center gap-3">
                <div className="w-12 h-12 rounded-lg bg-cream flex items-center justify-center text-2xl shrink-0">
                  {a.emoji}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-xs font-medium text-ink line-clamp-1">
                    {useTranslationKeys ? t(a.name) : a.name}
                  </p>
                  <p className="text-xs text-muted">× {addonQtys[a.id]}</p>
                </div>
                <span className="text-xs font-semibold tabular-nums text-green-deep">
                  {a.price * addonQtys[a.id]} {t('common.som')}
                </span>
              </div>
            ))}

            <div className="border-t border-border pt-3 space-y-1.5">
              <div className="flex justify-between text-sm text-muted">
                <span>{t('checkout.items')}</span>
                <span className="tabular-nums">
                  {subtotal} {t('common.som')}
                </span>
              </div>
              {addonsTotal > 0 && (
                <div className="flex justify-between text-sm text-muted">
                  <span>{t('checkout.addons_label')}</span>
                  <span className="tabular-nums">
                    {addonsTotal} {t('common.som')}
                  </span>
                </div>
              )}
              {discount > 0 && (
                <div className="flex justify-between text-sm text-green-deep">
                  <span>{t('checkout.promo_label')}</span>
                  <span className="tabular-nums">
                    −{discount} {t('common.som')}
                  </span>
                </div>
              )}
              {pointsDiscount > 0 && (
                <div className="flex justify-between text-sm text-amber-600 dark:text-amber-400">
                  <span>{t('checkout.points_label')}</span>
                  <span className="tabular-nums">
                    −{pointsDiscount} {t('common.som')}
                  </span>
                </div>
              )}
              <div className="flex justify-between text-sm text-muted">
                <span>{t('common.delivery')}</span>
                <span className="tabular-nums">
                  {delivery === 0 ? (
                    <span className="text-green-deep font-medium">{t('common.free')}</span>
                  ) : (
                    `${delivery} ${t('common.som')}`
                  )}
                </span>
              </div>
              {delivery > 0 && (
                <p className="text-[10px] text-muted">
                  {t('checkout.free_delivery_from', { sum: FREE_DELIVERY_THRESHOLD })}
                </p>
              )}
              <div className="flex justify-between font-semibold text-base pt-1 border-t border-border text-ink">
                <span>{t('common.total')}</span>
                <span className="tabular-nums text-green-deep">{formatPrice(grandTotal)}</span>
              </div>
            </div>

            {/* Points earn preview */}
            <div className="bg-amber-50 dark:bg-amber-900/20 rounded-xl px-3 py-2.5 flex items-center gap-2 border border-amber-200 dark:border-amber-800">
              <span className="text-base">⭐</span>
              <p className="text-xs text-amber-700 dark:text-amber-300 font-medium">
                {t('checkout.earn_points')}{' '}
                <span className="font-bold">
                  {t('checkout.earn_points_value', { count: willEarn })}
                </span>
              </p>
            </div>

            <Button type="submit" fullWidth size="lg" loading={loading}>
              {loading ? '' : t('checkout.pay_button', { total: formatPrice(grandTotal) })}
            </Button>
            <p className="text-[10px] text-center text-muted">{t('checkout.pay_agreement')}</p>
          </div>

          {/* Redeem points block */}
          {loyaltyBalance >= MIN_REDEEM && (
            <motion.div
              initial={{ opacity: 0, y: 8 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-surface rounded-2xl p-4 shadow-[0_4px_24px_rgba(0,0,0,0.06)] border border-amber-200 dark:border-amber-800"
            >
              <label className="flex items-start gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={usePoints}
                  onChange={toggleUsePoints}
                  className="w-4 h-4 mt-0.5 accent-green-deep shrink-0"
                />
                <div className="flex-1">
                  <p className="text-sm font-semibold text-ink">
                    {t('checkout.redeem_points')}{' '}
                    <span className="font-normal text-muted">
                      {t('checkout.points_available', { count: loyaltyBalance })}
                    </span>
                  </p>
                  {usePoints && (
                    <motion.div
                      initial={{ opacity: 0, height: 0 }}
                      animate={{ opacity: 1, height: 'auto' }}
                      className="mt-2 space-y-2 overflow-hidden"
                    >
                      <input
                        type="range"
                        min={MIN_REDEEM}
                        max={maxPts}
                        step={10}
                        value={redeemAmt}
                        onChange={(e) => setRedeemAmt(Number(e.target.value))}
                        className="w-full accent-green-deep"
                      />
                      <div className="flex justify-between text-xs text-muted">
                        <span>
                          {MIN_REDEEM} {t('common.points_short')}
                        </span>
                        <span className="font-semibold text-amber-600">
                          {t('checkout.points_eq', { count: redeemAmt })}
                        </span>
                        <span>
                          {maxPts} {t('common.points_short')}
                        </span>
                      </div>
                    </motion.div>
                  )}
                </div>
              </label>
            </motion.div>
          )}
        </div>
      </form>
    </div>
  )
}

// ─── Section wrapper ──────────────────────────────────────────────────────────

function Section({
  num,
  title,
  children,
}: {
  num: number
  title: string
  children: React.ReactNode
}) {
  return (
    <section className="bg-surface rounded-2xl p-6 shadow-[0_4px_24px_rgba(0,0,0,0.06)] space-y-4">
      <h2 className="font-serif text-xl font-semibold text-ink">
        <span className="inline-flex items-center justify-center w-7 h-7 rounded-full bg-green-deep text-white text-sm font-bold mr-2">
          {num}
        </span>
        {title}
      </h2>
      {children}
    </section>
  )
}

// ─── Success screen ───────────────────────────────────────────────────────────

// Petals that burst around the checkmark circle
function SuccessPetals() {
  const petals = [
    { tx: '0px,-52px', delay: 0 },
    { tx: '40px,-36px', delay: 40 },
    { tx: '52px,0px', delay: 80 },
    { tx: '40px,36px', delay: 60 },
    { tx: '0px,52px', delay: 20 },
    { tx: '-40px,36px', delay: 100 },
    { tx: '-52px,0px', delay: 50 },
    { tx: '-40px,-36px', delay: 90 },
  ]
  return (
    <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
      {petals.map((p, i) => (
        <span
          key={i}
          className="petal-burst absolute text-sm"
          style={
            {
              '--tx': `translate(${p.tx}) scale(0.2)`,
              animationDelay: `${p.delay}ms`,
            } as React.CSSProperties
          }
        >
          🌸
        </span>
      ))}
    </div>
  )
}

function SuccessScreen({ orderNum, earnedPts }: { orderNum: string; earnedPts: number }) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { isSupported, isSubscribed, subscribe } = usePushNotifications()
  const { permissionAsked } = usePushStore()
  const [showPetals, setShowPetals] = useState(false)

  // Fire petals after the check finishes drawing (300ms draw + 400ms delay)
  useEffect(() => {
    const timer = setTimeout(() => setShowPetals(true), 750)
    return () => clearTimeout(timer)
  }, [])

  return (
    <div className="min-h-[60svh] flex flex-col items-center justify-center text-center px-4 py-16 gap-6 max-w-sm mx-auto">
      {/* Animated checkmark circle */}
      <div className="relative">
        <motion.div
          initial={{ scale: 0, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ type: 'spring', stiffness: 220, damping: 16 }}
          className="w-24 h-24 rounded-full bg-green-deep flex items-center justify-center"
        >
          <svg
            className="draw-check-lg"
            width="44"
            height="44"
            viewBox="0 0 44 44"
            fill="none"
            stroke="white"
            strokeWidth="4"
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeDasharray="120"
            strokeDashoffset="120"
          >
            <polyline points="6,22 16,34 38,10" />
          </svg>
        </motion.div>
        {showPetals && <SuccessPetals />}
      </div>

      <motion.div
        initial={{ opacity: 0, y: 14 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.35 }}
        className="space-y-2"
      >
        <h2 className="font-serif text-3xl font-bold text-ink">{t('checkout.order_success')}</h2>
        <motion.p
          initial={{ opacity: 0, y: 8 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
          className="text-muted text-sm"
        >
          {t('checkout.order_number')} <span className="font-semibold text-ink">{orderNum}</span>
        </motion.p>
        <p className="text-sm text-muted">{t('checkout.order_success_hint')}</p>
      </motion.div>

      {/* Earned points banner */}
      {earnedPts > 0 && (
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ delay: 0.5 }}
          className="w-full bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-2xl px-4 py-3 flex items-center gap-3"
        >
          <span className="text-2xl">⭐</span>
          <div className="text-left">
            <p className="text-sm font-semibold text-amber-700 dark:text-amber-300">
              {t('checkout.earned_points', { count: earnedPts })}
            </p>
            <p className="text-xs text-amber-600 dark:text-amber-400">
              {t('checkout.earned_points_hint')}
            </p>
          </div>
        </motion.div>
      )}

      {/* Push notification prompt */}
      {isSupported && !isSubscribed && !permissionAsked && (
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ delay: 0.7 }}
          className="w-full bg-surface border border-border rounded-2xl p-4 space-y-3"
        >
          <div className="flex items-start gap-3 text-left">
            <span className="text-2xl">🔔</span>
            <div>
              <p className="text-sm font-semibold text-ink">{t('checkout.push_title')}</p>
              <p className="text-xs text-muted mt-0.5">{t('checkout.push_hint')}</p>
            </div>
          </div>
          <div className="flex gap-2">
            <Button size="sm" fullWidth onClick={subscribe}>
              {t('checkout.push_enable')}
            </Button>
            <Button size="sm" variant="ghost" fullWidth onClick={() => {}}>
              {t('checkout.push_decline')}
            </Button>
          </div>
        </motion.div>
      )}

      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.9 }}
        className="flex gap-3 w-full"
      >
        <Button variant="outline" fullWidth onClick={() => navigate('/account')}>
          {t('checkout.to_orders')}
        </Button>
        <Button fullWidth onClick={() => navigate('/')}>
          {t('checkout.to_catalog')}
        </Button>
      </motion.div>
    </div>
  )
}
