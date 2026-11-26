import type { Product } from '@/entities/product/types'
import type { BotAnswers } from './store'
import { unitPrice } from '@/features/cart/store'

// Chatbot script
// All question keys map to i18n `bot.*` keys

export interface BotOption {
  id: string
  labelKey: string // i18n key, e.g. 'bot.opt_mom'
}

export interface BotQuestion {
  id: string
  messageKey: string // i18n key
  field: keyof BotAnswers
  options: BotOption[]
}

export const BOT_QUESTIONS: BotQuestion[] = [
  {
    id: 'for_whom',
    messageKey: 'bot.q_for_whom',
    field: 'forWhom',
    options: [
      { id: 'mom', labelKey: 'bot.opt_mom' },
      { id: 'love', labelKey: 'bot.opt_love' },
      { id: 'colleague', labelKey: 'bot.opt_colleague' },
      { id: 'friend', labelKey: 'bot.opt_friend' },
      { id: 'wedding', labelKey: 'bot.opt_wedding' },
      { id: 'myself', labelKey: 'bot.opt_myself' },
    ],
  },
  {
    id: 'occasion',
    messageKey: 'bot.q_occasion',
    field: 'occasion',
    options: [
      { id: 'birthday', labelKey: 'bot.opt_birthday' },
      { id: 'just', labelKey: 'bot.opt_just' },
      { id: 'romance', labelKey: 'bot.opt_romance' },
      { id: 'thanks', labelKey: 'bot.opt_thanks' },
      { id: 'sorry', labelKey: 'bot.opt_sorry' },
      { id: 'funeral', labelKey: 'bot.opt_funeral' },
    ],
  },
  {
    id: 'budget',
    messageKey: 'bot.q_budget',
    field: 'budget',
    options: [
      { id: 'low', labelKey: 'bot.opt_budget_low' },
      { id: 'mid', labelKey: 'bot.opt_budget_mid' },
      { id: 'high', labelKey: 'bot.opt_budget_high' },
      { id: 'any', labelKey: 'bot.opt_budget_any' },
    ],
  },
  {
    id: 'urgency',
    messageKey: 'bot.q_urgency',
    field: 'urgency',
    options: [
      { id: 'today', labelKey: 'bot.opt_today' },
      { id: 'later', labelKey: 'bot.opt_later' },
    ],
  },
]

// Filter logic

const OCCASION_MAP: Record<string, string[]> = {
  birthday: ['birthday'],
  romance: ['romantic'],
  thanks: ['birthday', 'mom'],
  sorry: ['sorry'],
  funeral: ['funeral'],
  just: ['other', 'birthday'],
}

const FOR_WHOM_MAP: Record<string, string[]> = {
  mom: ['mom', 'birthday'],
  love: ['romantic', 'wedding'],
  colleague: ['birthday', 'other'],
  friend: ['birthday', 'other'],
  wedding: ['wedding'],
  myself: ['other', 'birthday'],
}

const BUDGET_RANGE: Record<string, [number, number]> = {
  low: [0, 400],
  mid: [400, 800],
  high: [800, 1500],
  any: [0, 99999],
}

export function filterProducts(answers: BotAnswers, products: Product[]): Product[] {
  const { forWhom, occasion, budget, urgency } = answers

  const occasionTags = occasion ? (OCCASION_MAP[occasion] ?? []) : []
  const forWhomTags = forWhom ? (FOR_WHOM_MAP[forWhom] ?? []) : []
  const [minP, maxP] = budget ? BUDGET_RANGE[budget] : [0, 99999]

  return products.flatMap((product) => {
    const option = getRecommendationOption(product)
    if (!option || !product.inStock || (urgency === 'today' && !product.deliverToday)) return []
    if (product.occasions.includes('funeral') !== (occasion === 'funeral')) return []
    if (option.price < minP || option.price > maxP) return []
    const matches = (tags: string[]) => product.occasions.some((tag) => tags.includes(tag))
    const score = Number(matches(occasionTags)) * 2 + Number(matches(forWhomTags))
    return [{ product, score, price: option.price }]
  })
    .sort((a, b) => b.score - a.score || b.product.rating - a.product.rating || a.price - b.price)
    .slice(0, 3)
    .map(({ product }) => product)
}

export function getRecommendationOption(product: Product) {
  const size = product.sizes.find(({ label }) => label === 'M') ?? product.sizes[0]
  if (!size) return undefined
  const packaging = product.packagingOptions.includes('no_basket') ? 'no_basket' : product.packagingOptions[0] ?? 'no_basket'
  return { size, packaging, price: unitPrice(product, size, packaging) }
}
