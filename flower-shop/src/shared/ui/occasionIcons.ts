import {
  Cake,
  Feather,
  Flower2,
  Gem,
  HandHeart,
  Heart,
  HeartHandshake,
  Sparkles,
  type LucideIcon,
} from 'lucide-react'

export const OCCASION_ICONS: Record<string, LucideIcon> = {
  birthday: Cake,
  wedding: Gem,
  nikah: HeartHandshake,
  romantic: Heart,
  mom: Flower2,
  sorry: HandHeart,
  funeral: Feather,
  other: Sparkles,
}
