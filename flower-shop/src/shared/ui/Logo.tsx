import { useTranslation } from 'react-i18next'
import { cn } from '@/shared/lib/cn'

interface LogoMarkProps {
  className?: string
  /** `inverse` — for dark/green backgrounds (footer) */
  tone?: 'default' | 'inverse'
}

/** Brand mark: a stylised tulip. Same geometry as /public/favicon.svg. */
export function LogoMark({ className, tone = 'default' }: LogoMarkProps) {
  const stem = tone === 'inverse' ? '#ffffff' : 'var(--green-deep)'
  const leaf = tone === 'inverse' ? 'rgba(255,255,255,0.7)' : 'var(--green-light)'
  return (
    <svg
      viewBox="0 0 32 32"
      aria-hidden="true"
      focusable="false"
      className={cn('shrink-0', className)}
    >
      <path d="M16 17.5V29" stroke={stem} strokeWidth="2.2" strokeLinecap="round" fill="none" />
      <path d="M16 25.5c-4.6-.4-7-3.4-7.4-7 3.9.3 6.4 2.6 7.4 7Z" fill={leaf} />
      <path d="M16 18c-5.6-.4-8.3-4.6-7.6-11 3.3 1.6 6 5 7.6 11Z" fill="#C9606F" />
      <path d="M16 18c5.6-.4 8.3-4.6 7.6-11-3.3 1.6-6 5-7.6 11Z" fill="#C9606F" />
      <path d="M16 3.5c3.7 3.2 4.7 8.2 0 14.5-4.7-6.3-3.7-11.3 0-14.5Z" fill="#E8A0A8" />
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
