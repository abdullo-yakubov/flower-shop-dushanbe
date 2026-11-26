import { useTranslation } from 'react-i18next'
import { useThemeStore, type ThemeMode } from '@/features/theme/store'
import { Monitor, Moon, Sun, type LucideIcon } from 'lucide-react'
import { cn } from '@/shared/lib/cn'

const OPTIONS: { value: ThemeMode; tKey: string; icon: LucideIcon }[] = [
  { value: 'light', tKey: 'theme.light', icon: Sun },
  { value: 'dark', tKey: 'theme.dark', icon: Moon },
  { value: 'system', tKey: 'theme.system', icon: Monitor },
]

export function ThemeToggle({ compact = false }: { compact?: boolean }) {
  const { t } = useTranslation()
  const { theme, setTheme } = useThemeStore()

  if (compact) {
    const next: ThemeMode = theme === 'light' ? 'dark' : theme === 'dark' ? 'system' : 'light'
    const current = OPTIONS.find((o) => o.value === theme)!
    return (
      <button
        onClick={() => setTheme(next)}
        className="w-10 h-10 flex items-center justify-center rounded-full hover:bg-ink/5 transition-colors text-ink"
        aria-label={t('theme.switch_hint', { theme: t(current.tKey) })}
        title={t(current.tKey)}
      >
        <current.icon className="size-[18px]" aria-hidden />
      </button>
    )
  }

  return (
    <div
      className="flex items-center gap-1 p-1 rounded-[12px] bg-cream"
      role="group"
      aria-label={t('theme.aria')}
    >
      {OPTIONS.map((opt) => (
        <button
          key={opt.value}
          onClick={() => setTheme(opt.value)}
          className={cn(
            'flex items-center gap-1.5 px-3 py-1.5 rounded-[8px] text-xs font-medium transition-all duration-150',
            theme === opt.value ? 'bg-surface text-ink shadow-sm' : 'text-muted hover:text-ink'
          )}
          aria-pressed={theme === opt.value}
        >
          <opt.icon className="size-3.5" aria-hidden />
          {t(opt.tKey)}
        </button>
      ))}
    </div>
  )
}
