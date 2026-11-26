import { useTranslation } from 'react-i18next'
import { cn } from '@/shared/lib/cn'

interface LogoMarkProps {
  className?: string
  /** `inverse` — for dark/green backgrounds (footer) */
  tone?: 'default' | 'inverse'
}

/** Flower emblem used throughout the storefront. */
export function LogoMark({ className, tone = 'default' }: LogoMarkProps) {
  return (
    <svg
      viewBox="0 0 32 32"
      aria-hidden="true"
      focusable="false"
      className={cn('shrink-0', className)}
    >
      <rect x="1" y="1" width="30" height="30" rx="11" fill={tone === 'inverse' ? '#F7EDE4' : '#294739'} />
      <path d="M16 15.5c-5.7 0-8.5-3-8.5-8 4.6.4 7.4 3.1 8.5 8Zm0 0c5.7 0 8.5-3 8.5-8-4.6.4-7.4 3.1-8.5 8Zm0-2.2c-2.8-2.8-2.8-5.6 0-8.6 2.8 3 2.8 5.8 0 8.6Z" fill={tone === 'inverse' ? '#9C4F62' : '#F3C0BA'} />
      <path d="M16 16v10m0-3c-3.4 0-5.7-1.5-6.7-4m6.7 2.1c3.1 0 5.1-1.3 6.3-3.4" fill="none" stroke={tone === 'inverse' ? '#294739' : '#E5EDCB'} strokeWidth="1.4" strokeLinecap="round" />
    </svg>
  )
}

interface LogoProps {
  className?: string
  tone?: 'default' | 'inverse'
  /** Hide the wordmark on very narrow screens */
  compact?: boolean
}

export function Logo({ className, tone = 'default', compact = false }: LogoProps) {
  const { t } = useTranslation()
  return (
    <span className={cn('inline-flex items-center gap-2', className)}>
      <LogoMark tone={tone} className="h-7 w-7 md:h-8 md:w-8" />
      <span
        className={cn(
          'font-serif text-xl font-semibold tracking-tight leading-none',
          tone === 'inverse' ? 'text-white' : 'text-green-deep',
          compact && 'max-[360px]:sr-only'
        )}
      >
        {t('header.logo')}
      </span>
    </span>
  )
}
