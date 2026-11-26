import { useState, useRef, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { motion, AnimatePresence } from 'framer-motion'
import { LANGS, setLang, type Lang } from '@/shared/lib/i18n'
import { FlagIcon } from './flags'

export function LangSwitcher() {
  const { i18n } = useTranslation()
  const [open, setOpen] = useState(false)
  const [activeIndex, setActiveIndex] = useState(0)
  const ref = useRef<HTMLDivElement>(null)
  const triggerRef = useRef<HTMLButtonElement>(null)
  const itemRefs = useRef<(HTMLButtonElement | null)[]>([])
  const current = LANGS.find((l) => l.code === i18n.language) ?? LANGS[0]

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  useEffect(() => {
    if (open) {
      const idx = Math.max(
        0,
        LANGS.findIndex((l) => l.code === i18n.language)
      )
      requestAnimationFrame(() => {
        setActiveIndex(idx)
        itemRefs.current[idx]?.focus()
      })
    }
  }, [open, i18n.language])

  const choose = (code: Lang) => {
    setLang(code)
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
        const next = (i + 1) % LANGS.length
        itemRefs.current[next]?.focus()
        return next
      })
      return
    }
    if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActiveIndex((i) => {
        const next = (i - 1 + LANGS.length) % LANGS.length
        itemRefs.current[next]?.focus()
        return next
      })
      return
    }
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault()
      choose(LANGS[activeIndex].code)
    }
  }

  return (
    <div ref={ref} className="relative">
      <button
        ref={triggerRef}
        onClick={() => setOpen((o) => !o)}
        className="flex items-center gap-1 h-8 px-2 rounded-lg hover:bg-ink/5 transition-colors text-xs font-medium text-ink"
        aria-label="Language"
        aria-haspopup="listbox"
        aria-expanded={open}
      >
        <FlagIcon code={current.code} />
        <span className="hidden sm:inline">{current.label}</span>
        <motion.span
          animate={{ rotate: open ? 180 : 0 }}
          transition={{ duration: 0.2 }}
          className="text-muted text-[10px] ml-0.5"
        >
          ▾
        </motion.span>
      </button>

      <AnimatePresence>
        {open && (
          <motion.div
            role="listbox"
            aria-label="Language"
            tabIndex={-1}
            onKeyDown={onKeyDown}
            initial={{ opacity: 0, y: -8, scale: 0.96 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: -8, scale: 0.96 }}
            transition={{ duration: 0.15, ease: [0.22, 1, 0.36, 1] }}
            className="absolute right-0 top-full mt-2 bg-surface-elevated border border-border/60 rounded-xl shadow-[0_12px_32px_-8px_rgba(0,0,0,0.28)] overflow-hidden z-50 min-w-[9rem] p-1.5"
          >
            {LANGS.map((lang, i) => {
              const isActive = lang.code === i18n.language
              return (
                <button
                  key={lang.code}
                  ref={(el) => {
                    itemRefs.current[i] = el
                  }}
                  role="option"
                  aria-selected={isActive}
                  tabIndex={isActive || i === activeIndex ? 0 : -1}
                  onClick={() => choose(lang.code)}
                  onMouseEnter={() => setActiveIndex(i)}
                  className={`w-full flex items-center gap-2.5 pl-3 pr-2.5 py-2.5 my-0.5 rounded-lg text-sm text-left transition-colors duration-150 ease-out outline-none border-l-2 ${
                    isActive
                      ? 'bg-green-deep/10 border-l-green-deep text-green-deep font-semibold'
                      : 'border-l-transparent text-ink hover:bg-ink/5 focus-visible:bg-ink/5'
                  }`}
                >
                  <FlagIcon code={lang.code} />
                  <span className="flex-1">{lang.label}</span>
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
