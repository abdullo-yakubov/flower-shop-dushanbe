import { X } from 'lucide-react'
import { LogoMark } from '@/shared/ui/Logo'
import { useEffect, useRef } from 'react'
import { AnimatePresence, motion } from 'framer-motion'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useFlowerBotStore } from './store'
import { BOT_QUESTIONS, filterProducts } from './botScript'
import { useCartStore } from '@/features/cart/store'
import { formatPrice } from '@/shared/lib/format'

const TG_LINK = 'https://t.me/yakubov_111'

// Floating trigger button

function BotTrigger({ onClick }: { onClick: () => void }) {
  const { t } = useTranslation()
  return (
    <motion.button
      id="flower-bot-trigger"
      onClick={onClick}
      aria-label={t('bot.open')}
      whileHover={{ scale: 1.08 }}
      whileTap={{ scale: 0.94 }}
      className="fixed bottom-20 right-4 md:bottom-6 md:right-6 z-[90] flex items-center gap-2.5
                 bg-green-deep text-white rounded-full shadow-xl px-4 py-3 md:px-5 md:py-3.5
                 font-semibold text-sm md:text-base
                 transition-shadow hover:shadow-2xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-green-deep"
    >
      {/* pulse ring */}
      <span className="relative flex h-3 w-3">
        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-white opacity-50" />
        <span className="relative inline-flex rounded-full h-3 w-3 bg-white" />
      </span>
      <span>{t('bot.cta')}</span>
    </motion.button>
  )
}

// Bot message bubble

function BotBubble({ text }: { text: string }) {
  return (
    <motion.div
      initial={{ opacity: 0, x: -12 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ duration: 0.28, ease: [0.22, 1, 0.36, 1] }}
      className="flex items-start gap-2 max-w-[85%]"
    >
      <LogoMark className="size-6 mt-0.5" />
      <div className="bg-green-deep/10 dark:bg-green-deep/20 text-ink rounded-2xl rounded-tl-sm px-4 py-2.5 text-sm leading-relaxed border border-green-deep/15">
        {text}
      </div>
    </motion.div>
  )
}

// Option chips

interface OptionChipsProps {
  options: { id: string; labelKey: string }[]
  onSelect: (id: string) => void
}

function OptionChips({ options, onSelect }: OptionChipsProps) {
  const { t } = useTranslation()
  return (
    <motion.div
      initial="hidden"
      animate="visible"
      variants={{ visible: { transition: { staggerChildren: 0.05 } } }}
      className="flex flex-wrap gap-2 justify-end"
    >
      {options.map((opt) => (
        <motion.button
          key={opt.id}
          variants={{ hidden: { opacity: 0, y: 8 }, visible: { opacity: 1, y: 0 } }}
          whileHover={{ scale: 1.04 }}
          whileTap={{ scale: 0.96 }}
          onClick={() => onSelect(opt.id)}
          className="flex items-center gap-1.5 px-3 py-2 rounded-full border border-border
                     bg-cream text-ink text-sm font-medium
                     hover:border-green-deep hover:bg-green-deep/10 hover:text-green-deep
                     transition-all cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-green-deep"
        >
          <span>{t(opt.labelKey)}</span>
        </motion.button>
      ))}
    </motion.div>
  )
}

// Mini product card

function MiniProductCard({ product }: { product: import('@/entities/product/types').Product }) {
  const { t } = useTranslation()
  const addItem = useCartStore((s) => s.addItem)

  const mSize = product.sizes?.find((s) => s.id === 'm') ?? product.sizes?.[0]

  const handleAdd = () => {
    if (mSize) addItem(product, mSize, { snapshotPrice: mSize.price })
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      className="flex gap-3 p-3 rounded-2xl border border-border bg-cream/80 hover:border-green-deep/40 transition-colors"
    >
      <img
        src={product.images[0]}
        alt={product.name}
        className="w-16 h-20 object-cover rounded-xl shrink-0"
        onError={(e) => {
          ;(e.currentTarget as HTMLImageElement).src = '/placeholder-flower.svg'
        }}
      />
      <div className="flex-1 min-w-0 flex flex-col justify-between">
        <div>
          <p className="text-sm font-semibold text-ink leading-tight line-clamp-2">
            {product.name}
          </p>
          <p className="text-xs text-muted mt-0.5 line-clamp-1">{product.composition}</p>
        </div>
        <div className="flex items-center justify-between gap-2 mt-2">
          <span className="text-sm font-bold text-green-deep">
            {formatPrice(mSize?.price ?? product.price)} {t('common.som')}
          </span>
          <button
            onClick={handleAdd}
            className="text-xs font-semibold px-2.5 py-1 rounded-full bg-green-deep text-white hover:opacity-90 transition-opacity"
          >
            {t('common.add_to_cart')}
          </button>
        </div>
      </div>
    </motion.div>
  )
}

// Results screen

function ResultsScreen({ onReset }: { onReset: () => void }) {
  const { t } = useTranslation()
  const { results } = useFlowerBotStore()

  const hasResults = results.length > 0

  return (
    <div className="flex flex-col gap-3">
      <BotBubble
        text={
          hasResults ? t('bot.results_found', { count: results.length }) : t('bot.results_none')
        }
      />

      {hasResults ? (
        <motion.div
          initial="hidden"
          animate="visible"
          variants={{ visible: { transition: { staggerChildren: 0.08 } } }}
          className="flex flex-col gap-2"
        >
          {results.map((p) => (
            <MiniProductCard key={p.id} product={p} />
          ))}
        </motion.div>
      ) : null}

      {/* CTA buttons */}
      <div className="flex flex-col gap-2 mt-1">
        <Link
          to="/catalog"
          className="w-full text-center py-2.5 rounded-full bg-green-deep text-white text-sm font-semibold hover:opacity-90 transition-opacity"
        >
          {t('bot.see_all')}
        </Link>

        <a
          href={TG_LINK}
          target="_blank"
          rel="noopener noreferrer"
          className="w-full text-center py-2.5 rounded-full border border-border text-ink text-sm font-medium
                     hover:border-green-deep hover:text-green-deep transition-colors flex items-center justify-center gap-2"
        >
          <svg className="w-4 h-4" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm4.64 6.8l-1.68 7.92c-.12.56-.44.7-.88.44l-2.44-1.8-1.18 1.14c-.13.13-.24.24-.5.24l.18-2.52 4.6-4.16c.2-.18-.04-.28-.31-.1L7.46 14.9l-2.4-.75c-.52-.16-.53-.52.11-.77l9.4-3.62c.43-.16.82.1.67.77l-.6-.03z" />
          </svg>
          {t('bot.contact_florist')}
        </a>
      </div>

      <button
        onClick={onReset}
        className="text-xs text-muted hover:text-ink transition-colors text-center mt-1"
      >
        {t('bot.restart')}
      </button>
    </div>
  )
}

// Main chat window

function BotWindow({ onClose }: { onClose: () => void }) {
  const { t } = useTranslation()
  const { step, answers, nextStep, setResults, open } = useFlowerBotStore()
  const scrollRef = useRef<HTMLDivElement>(null)

  const currentQuestion = BOT_QUESTIONS.find((q) => q.id === step)

  // Build chat history dynamically from state
  const history: { role: 'bot' | 'user'; text: string }[] = [
    { role: 'bot', text: t('bot.greeting') },
  ]

  for (const q of BOT_QUESTIONS) {
    history.push({ role: 'bot', text: t(q.messageKey) })
    const ans = answers[q.field]
    if (ans) {
      const opt = q.options.find((o) => o.id === ans)
      if (opt) {
        history.push({
          role: 'user',
          text: t(opt.labelKey),
        })
      }
    } else {
      break
    }
  }

  // When results step reached, calculate products
  useEffect(() => {
    if (step === 'results') {
      const found = filterProducts(answers)
      setResults(found)
    }
  }, [step, answers, setResults])

  // Auto-scroll to bottom when answers or step changes
  useEffect(() => {
    scrollRef.current?.scrollTo({ top: 9999, behavior: 'smooth' })
  }, [answers, step])

  const handleAnswer = (_questionId: string, optionId: string, field: keyof typeof answers) => {
    nextStep(optionId, field)
  }

  const handleReset = () => {
    open()
  }

  return (
    <motion.div
      id="flower-bot-window"
      initial={{ opacity: 0, y: 24, scale: 0.96 }}
      animate={{ opacity: 1, y: 0, scale: 1 }}
      exit={{ opacity: 0, y: 24, scale: 0.96 }}
      transition={{ duration: 0.3, ease: [0.22, 1, 0.36, 1] }}
      className="fixed bottom-[88px] right-4 md:bottom-20 md:right-6 z-[90]
                 w-[calc(100vw-32px)] max-w-[380px]
                 bg-cream/95 backdrop-blur-xl rounded-[24px] shadow-2xl
                 border border-border flex flex-col overflow-hidden"
      style={{ maxHeight: 'calc(100svh - 140px)' }}
    >
      {/* Header */}
      <div className="flex items-center justify-between px-5 py-4 border-b border-border shrink-0">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-full bg-green-deep/15 flex items-center justify-center">
            <LogoMark className="size-6" />
          </div>
          <div>
            <p className="text-sm font-semibold text-ink">{t('bot.name')}</p>
            <span className="flex items-center gap-1.5 text-xs text-muted">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 inline-block" />
              {t('bot.online')}
            </span>
          </div>
        </div>
        <button
          onClick={onClose}
          aria-label={t('common.close')}
          className="w-8 h-8 rounded-full flex items-center justify-center hover:bg-black/5 dark:hover:bg-white/10 transition-colors text-muted hover:text-ink"
        >
          <X className="size-4" aria-hidden />
        </button>
      </div>

      {/* Messages */}
      <div
        ref={scrollRef}
        className="flex-1 overflow-y-auto px-4 py-4 flex flex-col gap-3"
        style={{ scrollbarWidth: 'thin' }}
      >
        {/* History */}
        {history.map((msg, i) => (
          <div key={i} className={msg.role === 'bot' ? '' : 'flex justify-end'}>
            {msg.role === 'bot' ? (
              <BotBubble text={msg.text} />
            ) : (
              <motion.div
                initial={{ opacity: 0, x: 12 }}
                animate={{ opacity: 1, x: 0 }}
                className="bg-green-deep text-white rounded-2xl rounded-tr-sm px-4 py-2.5 text-sm max-w-[75%]"
              >
                {msg.text}
              </motion.div>
            )}
          </div>
        ))}

        {/* Current step options or results */}
        <AnimatePresence mode="wait">
          {step !== 'results' && currentQuestion ? (
            <OptionChips
              key={step}
              options={currentQuestion.options}
              onSelect={(id) => handleAnswer(currentQuestion.id, id, currentQuestion.field)}
            />
          ) : step === 'results' ? (
            <ResultsScreen key="results" onReset={handleReset} />
          ) : null}
        </AnimatePresence>
      </div>

      {/* Telegram footer */}
      <div className="shrink-0 border-t border-border px-4 py-3 bg-cream/80">
        <a
          href={TG_LINK}
          target="_blank"
          rel="noopener noreferrer"
          className="flex items-center justify-center gap-2 text-xs text-muted hover:text-green-deep transition-colors"
        >
          <svg className="w-3.5 h-3.5" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm4.64 6.8l-1.68 7.92c-.12.56-.44.7-.88.44l-2.44-1.8-1.18 1.14c-.13.13-.24.24-.5.24l.18-2.52 4.6-4.16c.2-.18-.04-.28-.31-.1L7.46 14.9l-2.4-.75c-.52-.16-.53-.52.11-.77l9.4-3.62c.43-.16.82.1.67.77l-.6-.03z" />
          </svg>
          {t('bot.footer_tg')}
        </a>
      </div>
    </motion.div>
  )
}

// Root component

export function FlowerBot() {
  const { isOpen, open, close } = useFlowerBotStore()

  return (
    <>
      <AnimatePresence>{isOpen && <BotWindow key="bot-window" onClose={close} />}</AnimatePresence>

      <AnimatePresence>
        {!isOpen && <BotTrigger key="bot-trigger" onClick={open} />}
      </AnimatePresence>
    </>
  )
}
