import type { Lang } from '@/shared/lib/i18n'

const RuFlag = () => (
  <svg viewBox="0 0 20 14" width="20" height="14" className="rounded-[2px] shrink-0">
    <rect width="20" height="14" fill="#FFFFFF" />
    <rect y="4.67" width="20" height="4.67" fill="#0039A6" />
    <rect y="9.33" width="20" height="4.67" fill="#D52B1E" />
  </svg>
)

const GbFlag = () => (
  <svg viewBox="0 0 20 14" width="20" height="14" className="rounded-[2px] shrink-0">
    <rect width="20" height="14" fill="#012169" />
    <path d="M0 0L20 14M20 0L0 14" stroke="#FFFFFF" strokeWidth="2.8" />
    <path d="M0 0L20 14M20 0L0 14" stroke="#C8102E" strokeWidth="1" />
    <path d="M10 0V14M0 7H20" stroke="#FFFFFF" strokeWidth="4.6" />
    <path d="M10 0V14M0 7H20" stroke="#C8102E" strokeWidth="2.6" />
  </svg>
)

const TjFlag = () => (
  <svg viewBox="0 0 20 14" width="20" height="14" className="rounded-[2px] shrink-0">
    <rect width="20" height="14" fill="#FFFFFF" />
    <rect width="20" height="4.2" fill="#CC0000" />
    <rect y="9.8" width="20" height="4.2" fill="#006600" />
    <circle cx="10" cy="7" r="1.7" fill="#F8C300" />
  </svg>
)

const FLAG_COMPONENTS: Record<Lang, () => React.JSX.Element> = {
  ru: RuFlag,
  en: GbFlag,
  tg: TjFlag,
}

export function FlagIcon({ code }: { code: Lang }) {
  const Flag = FLAG_COMPONENTS[code]
  return <Flag />
}
