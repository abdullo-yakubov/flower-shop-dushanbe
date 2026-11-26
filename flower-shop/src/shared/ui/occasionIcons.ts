import {
  CakeSlice,
  Feather,
  Flower2,
  PartyPopper,
  HandHeart,
  Heart,
  HeartHandshake,
  Sparkles,
  type LucideIcon,
} from 'lucide-react'

export const OCCASION_ICONS: Record<string, LucideIcon> = {
  birthday: CakeSlice,
  wedding: PartyPopper,
  nikah: HeartHandshake,
  romantic: Heart,
  mom: Flower2,
  sorry: HandHeart,
  funeral: Feather,
  other: Sparkles,
}
