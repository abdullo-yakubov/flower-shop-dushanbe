import { motion, AnimatePresence } from 'framer-motion'
import { useEffect, useId, useRef, type ReactNode } from 'react'
import { X } from 'lucide-react'
import { useTranslation } from 'react-i18next'

interface SheetProps {
  open: boolean
  onClose: () => void
  side?: 'right' | 'left' | 'bottom'
  children: ReactNode
  title?: string
}

const variants = {
  right: {
    initial: { x: '100%', opacity: 0 },
    animate: { x: 0, opacity: 1 },
    exit: { x: '100%', opacity: 0 },
    className: 'right-0 top-0 h-full w-full max-w-[420px] rounded-l-[24px]',
  },
  left: {
    initial: { x: '-100%', opacity: 0 },
    animate: { x: 0, opacity: 1 },
    exit: { x: '-100%', opacity: 0 },
    className: 'left-0 top-0 h-full w-full max-w-[380px] rounded-r-[24px]',
  },
  bottom: {
    initial: { y: '100%', opacity: 0 },
    animate: { y: 0, opacity: 1 },
    exit: { y: '100%', opacity: 0 },
    className: 'bottom-0 left-0 right-0 w-full max-h-[90svh] rounded-t-[24px]',
  },
}

export function Sheet({ open, onClose, side = 'right', children, title }: SheetProps) {
  const { t } = useTranslation()
  const v = variants[side]
  const titleId = useId()
  const panel = useRef<HTMLDivElement>(null)
  const latestClose = useRef(onClose)
  useEffect(() => { latestClose.current = onClose }, [onClose])

  useEffect(() => {
    if (open) document.body.style.overflow = 'hidden'
    else document.body.style.overflow = ''
    return () => {
      document.body.style.overflow = ''
    }
  }, [open])

  useEffect(() => {
    if (!open) return
    const previous = document.activeElement
    panel.current?.focus()
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') latestClose.current()
      if (e.key !== 'Tab') return
      const items = Array.from(panel.current?.querySelectorAll<HTMLElement>(
        'button:not(:disabled), input:not(:disabled), select:not(:disabled), a[href], [tabindex="0"]'
      ) ?? [])
      const first = items[0]
      const last = items.at(-1)
      if (!first || !last) return
      if (e.shiftKey && (document.activeElement === first || document.activeElement === panel.current)) {
        e.preventDefault()
        last.focus()
      } else if (!e.shiftKey && document.activeElement === last) {
        e.preventDefault()
        first.focus()
      }
    }
    document.addEventListener('keydown', handler)
    return () => {
      document.removeEventListener('keydown', handler)
      if (previous instanceof HTMLElement) previous.focus()
    }
  }, [open])

  return (
    <AnimatePresence>
      {open && (
        <div className="fixed inset-0 z-[100]" role="dialog" aria-modal aria-labelledby={title ? titleId : undefined}>
          <motion.div
            ref={panel}
            tabIndex={-1}
            className="absolute inset-0 bg-black/40"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
            onClick={onClose}
          />
          <motion.div
            className={`absolute bg-cream shadow-2xl flex flex-col ${v.className}`}
            initial={v.initial}
            animate={v.animate}
            exit={v.exit}
            transition={{ duration: 0.3, ease: [0.22, 1, 0.36, 1] }}
          >
            {side === 'bottom' && (
              <div className="flex justify-center pt-3 pb-1">
                <div className="w-10 h-1 bg-border rounded-full" />
              </div>
            )}
            {title && (
              <div className="flex items-center justify-between px-6 py-4 border-b border-border">
                <h2 id={titleId} className="font-serif text-xl font-semibold">{title}</h2>
                <button
                  onClick={onClose}
                  className="size-11 rounded-full flex items-center justify-center hover:bg-black/5 transition-colors"
                  aria-label={t('common.close')}
                >
                  <X className="size-5" aria-hidden />
                </button>
              </div>
            )}
            <div className="min-h-0 flex-1 overflow-y-auto overscroll-contain pb-[env(safe-area-inset-bottom)]">{children}</div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  )
}
