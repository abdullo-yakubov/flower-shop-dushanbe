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
  const stars = Array.from({ length: max }, (_, i) => i + 1)

  // Только отображение: одна картинка для скринридера, без фальшивых кнопок в порядке фокуса
  if (!onChange) {
    const rounded = Math.round(value * 10) / 10
    return (
      <span className="inline-flex gap-0.5" role="img" aria-label={t('common.rating_aria', { value: rounded, max })}>
        {stars.map((star) => (
          <Star
            key={star}
            className={sz}
            fill="currentColor"
            strokeWidth={0}
            aria-hidden
            style={{ color: Math.round(value) >= star ? 'var(--gold)' : 'var(--border)' }}
          />
        ))}
      </span>
    )
  }

  return (
    <span className="inline-flex" role="radiogroup" onMouseLeave={() => setHover(0)}>
      {stars.map((star) => {
        const active = (hover || value) >= star
        return (
          <motion.button
            key={star}
            type="button"
            role="radio"
            aria-checked={value === star}
            whileHover={{ scale: 1.22 }}
            whileTap={{ scale: 0.88 }}
            transition={{ type: 'spring', stiffness: 420, damping: 14 }}
            className={cn('cursor-pointer p-1.5 rounded-full focus-visible:outline-2 focus-visible:outline-green-deep')}
            onMouseEnter={() => setHover(star)}
            onFocus={() => setHover(star)}
            onBlur={() => setHover(0)}
            onClick={() => onChange(star)}
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
