import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { motion } from 'framer-motion'
import { Star } from 'lucide-react'
import { cn } from '@/shared/lib/cn'

interface StarRatingProps {
  value: number
  max?: number
  onChange?: (v: number) => void
  size?: 'sm' | 'md'
}

export function StarRating({ value, max = 5, onChange, size = 'sm' }: StarRatingProps) {
  const { t } = useTranslation()
  const [hover, setHover] = useState(0)
  const sz = size === 'sm' ? 'size-3.5' : 'size-6'

  return (
    <span className="inline-flex gap-0.5">
      {Array.from({ length: max }, (_, i) => i + 1).map((star) => {
        const active = (hover || value) >= star
        return (
          <motion.button
            key={star}
            type="button"
            disabled={!onChange}
            whileHover={onChange ? { scale: 1.22 } : undefined}
            whileTap={onChange ? { scale: 0.88 } : undefined}
            transition={{ type: 'spring', stiffness: 420, damping: 14 }}
            className={cn(!onChange && 'cursor-default', onChange && 'cursor-pointer')}
            onMouseEnter={() => onChange && setHover(star)}
            onMouseLeave={() => onChange && setHover(0)}
            onClick={() => onChange?.(star)}
            aria-label={`${star} ${t('common.star')}`}
          >
            <motion.span
              animate={{ color: active ? 'var(--gold)' : 'var(--border)' }}
              transition={{ duration: 0.12 }}
              style={{ display: 'inline-flex' }}
            >
              <Star className={sz} fill="currentColor" strokeWidth={0} aria-hidden />
            </motion.span>
          </motion.button>
        )
      })}
    </span>
  )
}
