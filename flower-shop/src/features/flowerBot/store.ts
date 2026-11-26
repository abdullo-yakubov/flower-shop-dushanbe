import { create } from 'zustand'
import type { Product } from '@/entities/product/types'

export type BotStep = 'idle' | 'for_whom' | 'occasion' | 'budget' | 'urgency' | 'results'

export interface BotAnswers {
  forWhom?: string
  occasion?: string
  budget?: string
  urgency?: string
}

interface FlowerBotStore {
  isOpen: boolean
  step: BotStep
  answers: BotAnswers
  results: Product[]
  open: () => void
  close: () => void
  nextStep: (answer: string, field: keyof BotAnswers) => void
  setResults: (products: Product[]) => void
  reset: () => void
}

const initialState = {
  isOpen: false,
  step: 'idle' as BotStep,
  answers: {},
  results: [],
}

export const useFlowerBotStore = create<FlowerBotStore>((set) => ({
  ...initialState,

  open: () => set({ isOpen: true, step: 'for_whom', answers: {}, results: [] }),

  close: () => set({ isOpen: false }),

  nextStep: (answer, field) =>
    set((state) => {
      const answers = { ...state.answers, [field]: answer }
      const steps: BotStep[] = ['for_whom', 'occasion', 'budget', 'urgency', 'results']
      const currentIdx = steps.indexOf(state.step as BotStep)
      const nextStep = steps[currentIdx + 1] ?? 'results'
      return { answers, step: nextStep }
    }),

  setResults: (results) => set({ results }),

  reset: () => set({ ...initialState }),
}))
