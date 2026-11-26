import { useState, useRef, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'

export interface SelectOption<T extends string = string> {
  value: T
  label: string
}

interface SelectProps<T extends string> {
  value: T
  options: SelectOption<T>[]
  onChange: (value: T) => void
  ariaLabel: string
  className?: string
}

/** Custom dropdown replacing native <select>, styled to match the site theme (see LangSwitcher). */
export function Select<T extends string>({
  value,
  options,
  onChange,
  ariaLabel,
  className = '',
}: SelectProps<T>) {
  const [open, setOpen] = useState(false)
  const [activeIndex, setActiveIndex] = useState(0)
  const ref = useRef<HTMLDivElement>(null)
  const triggerRef = useRef<HTMLButtonElement>(null)
  const itemRefs = useRef<(HTMLButtonElement | null)[]>([])
  const current = options.find((o) => o.value === value) ?? options[0]

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  useEffect(() => {
    if (open) requestAnimationFrame(() => itemRefs.current[activeIndex]?.focus())
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  const toggleOpen = () => {
    setOpen((o) => {
      const next = !o
      if (next)
        setActiveIndex(
          Math.max(
            0,
            options.findIndex((opt) => opt.value === value)
          )
        )
      return next
    })
  }

  const choose = (v: T) => {
    onChange(v)
    setOpen(false)
    triggerRef.current?.focus()
  }

  const onKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      e.preventDefault()
      setOpen(false)
      triggerRef.current?.focus()
      return
    }
    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setActiveIndex((i) => {
        const next = (i + 1) % options.length
        itemRefs.current[next]?.focus()
        return next
      })
      return
    }
    if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActiveIndex((i) => {
        const next = (i - 1 + options.length) % options.length
        itemRefs.current[next]?.focus()
        return next
      })
      return
    }
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault()
      choose(options[activeIndex].value)
    }
  }

  return (
    <div ref={ref} className={`relative ${className}`}>
      <button
        ref={triggerRef}
        type="button"
        onClick={toggleOpen}
        className="flex items-center gap-2 h-10 px-3 rounded-[10px] border border-border bg-surface text-sm font-medium text-ink hover:border-green-deep transition-colors"
        aria-label={ariaLabel}
        aria-haspopup="listbox"
        aria-expanded={open}
      >
        <span>{current?.label}</span>
        <motion.span
          animate={{ rotate: open ? 180 : 0 }}
          transition={{ duration: 0.2 }}
          className="text-muted text-[10px]"
        >
          ▾
        </motion.span>
      </button>

      <AnimatePresence>
        {open && (
          <motion.div
            role="listbox"
            aria-label={ariaLabel}
            tabIndex={-1}
            onKeyDown={onKeyDown}
            initial={{ opacity: 0, y: -8, scale: 0.96 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: -8, scale: 0.96 }}
            transition={{ duration: 0.15, ease: [0.22, 1, 0.36, 1] }}
            className="absolute right-0 top-full mt-2 bg-surface-elevated border border-border/60 rounded-xl shadow-[0_12px_32px_-8px_rgba(0,0,0,0.28)] overflow-hidden z-50 min-w-[10rem] p-1.5"
          >
            {options.map((opt, i) => {
              const isActive = opt.value === value
              return (
                <button
                  key={opt.value}
                  ref={(el) => {
                    itemRefs.current[i] = el
                  }}
                  type="button"
                  role="option"
                  aria-selected={isActive}
                  tabIndex={isActive || i === activeIndex ? 0 : -1}
                  onClick={() => choose(opt.value)}
                  onMouseEnter={() => setActiveIndex(i)}
                  className={`w-full flex items-center gap-2.5 pl-3 pr-2.5 py-2.5 my-0.5 rounded-lg text-sm text-left transition-colors duration-150 ease-out outline-none border-l-2 ${
                    isActive
                      ? 'bg-green-deep/10 border-l-green-deep text-green-deep font-semibold'
                      : 'border-l-transparent text-ink hover:bg-ink/5 focus-visible:bg-ink/5'
                  }`}
                >
                  <span className="flex-1">{opt.label}</span>
                  {isActive && (
                    <svg
                      viewBox="0 0 16 16"
                      className="w-3.5 h-3.5 text-green-deep shrink-0"
                      fill="none"
                    >
                      <path
                        d="M3 8.5L6.2 11.5L13 4.5"
                        stroke="currentColor"
                        strokeWidth="1.8"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  )}
                </button>
              )
            })}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}
