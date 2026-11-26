import {
  Cake,
  CakeSlice,
  Candy,
  Cherry,
  Feather,
  Flower,
  Flower2,
  Gift,
  Heart,
  Leaf,
  PawPrint,
  Sparkles,
  type LucideIcon,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { API_ENABLED } from '@/shared/lib/api/client'
import { apiGetAddons, apiGetPostcards, apiGetMessageTemplates } from '@/shared/lib/api/misc'

export interface CheckoutAddon {
  id: string
  name: string
  icon: LucideIcon
  price: number
  hasInscription: boolean
}

export interface CheckoutPostcard {
  id: string
  occasion: string
  icon: LucideIcon
  label: string
  isPopular: boolean
}

export interface CheckoutTemplate {
  id: string
  category: string
  text: string
  isPopular: boolean
}

const ADDON_ICONS: Record<string, LucideIcon> = {
  chocolate: Candy,
  teddy: PawPrint,
  choco_strawberry: Cherry,
  bento_cake: CakeSlice,
}

const POSTCARD_ICONS: Record<string, LucideIcon> = {
  birthday: Cake,
  romantic: Heart,
  universal: Sparkles,
  funeral: Feather,
}

// Used when the API is not configured

const STATIC_ADDONS: CheckoutAddon[] = [
  { id: 'choco', name: 'checkout.addon_choco', icon: Candy, price: 90, hasInscription: false },
  { id: 'teddy', name: 'checkout.addon_teddy', icon: PawPrint, price: 150, hasInscription: false },
  {
    id: 'choco_str',
    name: 'checkout.addon_choco_str',
    icon: Cherry,
    price: 220,
    hasInscription: false,
  },
  { id: 'bento', name: 'checkout.addon_bento', icon: CakeSlice, price: 250, hasInscription: true },
]

const STATIC_POSTCARDS: CheckoutPostcard[] = [
  {
    id: 'bd1',
    occasion: 'birthday',
    icon: Cake,
    label: 'checkout.design_classic',
    isPopular: true,
  },
  {
    id: 'bd2',
    occasion: 'birthday',
    icon: Flower2,
    label: 'checkout.design_floral',
    isPopular: false,
  },
  { id: 'rom1', occasion: 'romantic', icon: Heart, label: 'checkout.design_love', isPopular: true },
  {
    id: 'rom2',
    occasion: 'romantic',
    icon: Flower,
    label: 'checkout.design_roses',
    isPopular: false,
  },
  {
    id: 'uni1',
    occasion: 'universal',
    icon: Sparkles,
    label: 'checkout.design_universal',
    isPopular: true,
  },
  {
    id: 'uni2',
    occasion: 'universal',
    icon: Leaf,
    label: 'checkout.design_minimal',
    isPopular: false,
  },
  {
    id: 'fun1',
    occasion: 'funeral',
    icon: Feather,
    label: 'checkout.design_funeral',
    isPopular: false,
  },
]

const STATIC_TEMPLATES: CheckoutTemplate[] = [
  { id: 't1', category: 'birthday', text: 'checkout.tpl_t1', isPopular: true },
  { id: 't2', category: 'birthday', text: 'checkout.tpl_t2', isPopular: false },
  { id: 't3', category: 'romantic', text: 'checkout.tpl_t3', isPopular: true },
  { id: 't4', category: 'romantic', text: 'checkout.tpl_t4', isPopular: false },
  { id: 't5', category: 'universal', text: 'checkout.tpl_t5', isPopular: true },
]

export interface CheckoutResources {
  addons: CheckoutAddon[]
  postcards: CheckoutPostcard[]
  templates: CheckoutTemplate[]
  /** True when names/text are i18n keys (static mode) rather than resolved strings */
  useTranslationKeys: boolean
}

export function useCheckoutResources(): CheckoutResources {
  const [addons, setAddons] = useState<CheckoutAddon[]>(STATIC_ADDONS)
  const [postcards, setPostcards] = useState<CheckoutPostcard[]>(STATIC_POSTCARDS)
  const [templates, setTemplates] = useState<CheckoutTemplate[]>(STATIC_TEMPLATES)
  const [fromApi, setFromApi] = useState(false)

  useEffect(() => {
    if (!API_ENABLED) return
    let cancelled = false

    Promise.all([apiGetAddons(), apiGetPostcards(), apiGetMessageTemplates()])
      .then(([apiAddons, apiPostcards, apiTemplates]) => {
        if (cancelled) return
        setAddons(
          apiAddons.map((a) => ({
            id: a.id,
            name: a.name,
            icon: ADDON_ICONS[a.type] ?? Gift,
            price: a.price,
            hasInscription: a.hasInscription ?? false,
          }))
        )
        setPostcards(
          apiPostcards.map((p) => ({
            id: p.id,
            occasion: p.occasion,
            icon: POSTCARD_ICONS[p.occasion] ?? Flower2,
            label: '',
            isPopular: p.isPopular ?? false,
          }))
        )
        setTemplates(
          apiTemplates.map((tp) => ({
            id: tp.id,
            category: tp.category,
            text: tp.text,
            isPopular: tp.isPopular ?? false,
          }))
        )
        setFromApi(true)
      })
      .catch(() => {
        // silently keep static fallback
      })

    return () => {
      cancelled = true
    }
  }, [])

  return { addons, postcards, templates, useTranslationKeys: !fromApi }
}
