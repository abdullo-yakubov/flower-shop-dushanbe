import type { LucideIcon } from 'lucide-react'
import { cn } from '@/shared/lib/cn'

interface EmptyIconProps {
  icon: LucideIcon
  className?: string
  tone?: 'green' | 'rose' | 'muted'
}

const TONES = {
  green: 'bg-green-deep/10 text-green-deep',
  rose: 'bg-rose-dust/20 text-rose-dust',
  muted: 'bg-ink/5 text-muted',
}

export function EmptyIcon({ icon: Icon, className, tone = 'green' }: EmptyIconProps) {
  return (
    <span
      className={cn(
        'mx-auto flex size-16 items-center justify-center rounded-full',
        TONES[tone],
        className
      )}
    >
      <Icon className="size-7" strokeWidth={1.5} aria-hidden />
    </span>
  )
}
