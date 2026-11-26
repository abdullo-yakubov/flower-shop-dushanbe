import type { Product } from '@/entities/product/types'
import type { BotAnswers } from './store'
import { MOCK_PRODUCTS } from '@/entities/product/mock'

// ─── Chatbot script ───────────────────────────────────────────────────────────
// All question keys map to i18n `bot.*` keys
// ─────────────────────────────────────────────────────────────────────────────

export interface BotOption {
  id: string
  labelKey: string // i18n key, e.g. 'bot.opt_mom'
  emoji: string
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
      { id: 'mom', labelKey: 'bot.opt_mom', emoji: '💐' },
      { id: 'love', labelKey: 'bot.opt_love', emoji: '❤️' },
      { id: 'colleague', labelKey: 'bot.opt_colleague', emoji: '🤝' },
      { id: 'friend', labelKey: 'bot.opt_friend', emoji: '🙌' },
      { id: 'wedding', labelKey: 'bot.opt_wedding', emoji: '💍' },
      { id: 'myself', labelKey: 'bot.opt_myself', emoji: '✨' },
    ],
  },
  {
    id: 'occasion',
    messageKey: 'bot.q_occasion',
    field: 'occasion',
    options: [
      { id: 'birthday', labelKey: 'bot.opt_birthday', emoji: '🎂' },
      { id: 'just', labelKey: 'bot.opt_just', emoji: '🌸' },
      { id: 'romance', labelKey: 'bot.opt_romance', emoji: '💫' },
      { id: 'thanks', labelKey: 'bot.opt_thanks', emoji: '🙏' },
      { id: 'sorry', labelKey: 'bot.opt_sorry', emoji: '🕊️' },
      { id: 'funeral', labelKey: 'bot.opt_funeral', emoji: '🕯️' },
    ],
  },
  {
    id: 'budget',
    messageKey: 'bot.q_budget',
    field: 'budget',
    options: [
      { id: 'low', labelKey: 'bot.opt_budget_low', emoji: '💰' },
      { id: 'mid', labelKey: 'bot.opt_budget_mid', emoji: '💳' },
      { id: 'high', labelKey: 'bot.opt_budget_high', emoji: '💎' },
      { id: 'any', labelKey: 'bot.opt_budget_any', emoji: '🎁' },
    ],
  },
  {
    id: 'urgency',
    messageKey: 'bot.q_urgency',
    field: 'urgency',
    options: [
      { id: 'today', labelKey: 'bot.opt_today', emoji: '⚡' },
      { id: 'later', labelKey: 'bot.opt_later', emoji: '📅' },
    ],
  },
]

// ─── Filter logic ─────────────────────────────────────────────────────────────

const OCCASION_MAP: Record<string, string[]> = {
  birthday: ['birthday'],
  romance: ['wedding'],
  thanks: ['birthday', 'mom'],
  sorry: ['sorry'],
  funeral: ['funeral'],
  just: ['other', 'birthday'],
}

const FOR_WHOM_MAP: Record<string, string[]> = {
  mom: ['mom', 'birthday'],
  love: ['wedding', 'birthday'],
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

export function filterProducts(answers: BotAnswers): Product[] {
  const { forWhom, occasion, budget, urgency } = answers

  const occasionTags = occasion ? (OCCASION_MAP[occasion] ?? []) : []
  const forWhomTags = forWhom ? (FOR_WHOM_MAP[forWhom] ?? []) : []
  const combinedTags = [...new Set([...occasionTags, ...forWhomTags])]

  const [minP, maxP] = budget ? BUDGET_RANGE[budget] : [0, 99999]

  return MOCK_PRODUCTS.filter((p) => {
    // Price filter — use M-size price or product.price
    const price = p.sizes?.find((s) => s.id === 'm')?.price ?? p.price
    if (price < minP || price > maxP) return false

    // Urgency filter
    if (urgency === 'today' && !p.deliverToday) return false

    // Funeral filter: only show funeral products when funeral selected
    if (occasion === 'funeral') return p.occasions.includes('funeral')
    if (p.occasions.includes('funeral')) return false

    // Tag matching — at least one occasion tag matches
    if (combinedTags.length > 0) {
      const matched = combinedTags.some((tag) => p.occasions.includes(tag as never))
      if (!matched) return false
    }

    return true
  })
    .sort((a, b) => (b.rating ?? 0) - (a.rating ?? 0))
    .slice(0, 3)
}
