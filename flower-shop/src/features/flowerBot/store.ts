import { create } from 'zustand'

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
  open: () => void
  close: () => void
  nextStep: (answer: string, field: keyof BotAnswers) => void
  reset: () => void
}

const initialState = {
  isOpen: false,
  step: 'idle' as BotStep,
  answers: {},
}

export const useFlowerBotStore = create<FlowerBotStore>((set) => ({
  ...initialState,

  open: () => set({ isOpen: true, step: 'for_whom', answers: {} }),

  close: () => set({ isOpen: false }),

  nextStep: (answer, field) =>
    set((state) => {
      const answers = { ...state.answers, [field]: answer }
      const steps: BotStep[] = ['for_whom', 'occasion', 'budget', 'urgency', 'results']
      const currentIdx = steps.indexOf(state.step as BotStep)
      const nextStep = steps[currentIdx + 1] ?? 'results'
      return { answers, step: nextStep }
    }),

  reset: () => set({ ...initialState }),
}))
