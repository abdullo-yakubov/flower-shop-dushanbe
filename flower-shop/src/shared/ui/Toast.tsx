import { AnimatePresence, motion } from 'framer-motion'
import { CircleCheck, CircleX, Info, X, type LucideIcon } from 'lucide-react'
import { useToastStore } from './toast.store'

const ICONS: Record<string, LucideIcon> = { success: CircleCheck, error: CircleX, info: Info }
const COLORS: Record<string, string> = {
  success: 'bg-green-deep text-white',
  error: 'bg-error text-white',
  info: 'bg-ink text-white',
}

function ToastIcon({ type }: { type: string }) {
  const Icon = ICONS[type] ?? Info
  return <Icon className="size-5 shrink-0" aria-hidden />
}

export function ToastContainer() {
  const { toasts, remove } = useToastStore()
  return (
    <div className="fixed bottom-4 right-4 md:bottom-4 md:top-auto top-4 z-[200] flex flex-col gap-2 items-end pointer-events-none">
      <AnimatePresence>
        {toasts.map((t) => (
          <motion.div
            key={t.id}
            initial={{ opacity: 0, x: 40, scale: 0.9 }}
            animate={{ opacity: 1, x: 0, scale: 1 }}
            exit={{ opacity: 0, x: 40, scale: 0.9 }}
            transition={{ duration: 0.28, ease: [0.22, 1, 0.36, 1] }}
            className={`pointer-events-auto flex items-center gap-3 px-4 py-3 rounded-xl shadow-lg min-w-[260px] max-w-[340px] ${COLORS[t.type]}`}
          >
            <ToastIcon type={t.type} />
            <span className="flex-1 text-sm">{t.message}</span>
            {t.action && (
              <button
                onClick={() => {
                  t.action!.onClick()
                  remove(t.id)
                }}
                className="text-sm font-semibold underline underline-offset-2 shrink-0"
              >
                {t.action.label}
              </button>
            )}
            <button
              onClick={() => remove(t.id)}
              className="ml-1 opacity-60 hover:opacity-100"
              aria-label="Close"
            >
              <X className="size-4" />
            </button>
          </motion.div>
        ))}
      </AnimatePresence>
    </div>
  )
}
