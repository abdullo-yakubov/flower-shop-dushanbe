import { useState, useRef, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { motion, AnimatePresence } from 'framer-motion'
import { useAuthStore } from './store'
import { Button } from '@/shared/ui/Button'
import { toast } from '@/shared/ui/toast.store'
import { API_ENABLED } from '@/shared/lib/api/client'
import { sendOtp, verifyOtp } from '@/shared/lib/api/auth'
import { extractError } from '@/shared/lib/api/client'

type Step = 'phone' | 'code'

export function AuthModal() {
  const { t } = useTranslation()
  const { isModalOpen, closeModal, login } = useAuthStore()
  const [step, setStep] = useState<Step>('phone')
  const [phone, setPhone] = useState('+992')
  const [code, setCode] = useState(['', '', '', '', '', ''])
  const [loading, setLoading] = useState(false)
  const [codeError, setCodeError] = useState(false)
  const codeRefs = useRef<(HTMLInputElement | null)[]>([])

  useEffect(() => {
    if (!isModalOpen) {
      requestAnimationFrame(() => {
        setStep('phone')
        setPhone('+992')
        setCode(['', '', '', '', '', ''])
        setCodeError(false)
      })
    }
  }, [isModalOpen])

  useEffect(() => {
    if (!isModalOpen) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') closeModal()
    }
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [isModalOpen, closeModal])

  // Send OTP

  const handlePhone = async (e: React.FormEvent) => {
    e.preventDefault()
    const digits = phone.replace(/\D/g, '')
    if (digits.length < 9) return
    setLoading(true)
    try {
      if (API_ENABLED) {
        await sendOtp(phone)
        toast.info(t('auth.code_sent'))
      } else {
        // Mock: simulate network delay
        await new Promise((r) => setTimeout(r, 800))
        toast.info(t('auth.test_code_toast'))
      }
      setStep('code')
      setTimeout(() => codeRefs.current[0]?.focus(), 50)
    } catch (err) {
      const { message } = extractError(err)
      toast.error(message)
    } finally {
      setLoading(false)
    }
  }

  // OTP input handlers

  const handleCodeInput = (i: number, val: string) => {
    const digit = val.replace(/\D/g, '').slice(-1)
    const next = [...code]
    next[i] = digit
    setCode(next)
    setCodeError(false)
    if (digit && i < 5) setTimeout(() => codeRefs.current[i + 1]?.focus(), 0)
    if (next.every((d) => d)) void submitCode(next.join(''))
  }

  const handleCodeKey = (i: number, e: React.KeyboardEvent) => {
    if (e.key === 'Backspace' && !code[i] && i > 0) {
      codeRefs.current[i - 1]?.focus()
    }
  }

  // Verify OTP

  const submitCode = async (fullCode: string) => {
    setLoading(true)
    try {
      if (API_ENABLED) {
        const result = await verifyOtp(phone, fullCode)
        login(
          {
            id: result.user.id,
            name: result.user.name,
            phone: result.user.phone,
            avatarUrl: result.user.avatarUrl ?? undefined,
            role: result.user.role,
          },
          result.accessToken,
          result.refreshToken
        )
        toast.success(t('auth.logged_in'))
      } else {
        // Mock: only code 123456 works
        await new Promise((r) => setTimeout(r, 600))
        if (fullCode === '123456') {
          login(
            { id: 'mock-1', name: t('auth.default_name'), phone },
            'mock-access',
            'mock-refresh'
          )
          toast.success(t('auth.logged_in'))
        } else {
          throw new Error('wrong_code')
        }
      }
    } catch (err) {
      setCodeError(true)
      setCode(['', '', '', '', '', ''])
      setTimeout(() => codeRefs.current[0]?.focus(), 0)
      const apiErr = extractError(err)
      const isWrongCode = apiErr.code === 'OTP_INVALID' || (err as Error).message === 'wrong_code'
      if (!isWrongCode) toast.error(apiErr.message)
    } finally {
      setLoading(false)
    }
  }

  if (!isModalOpen) return null

  return (
    <div className="fixed inset-0 z-[150] flex items-center justify-center p-4">
      <motion.div
        className="absolute inset-0 bg-black/40"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        onClick={closeModal}
      />
      <motion.div
        initial={{ opacity: 0, scale: 0.95, y: 20 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.95, y: 20 }}
        transition={{ duration: 0.28, ease: [0.22, 1, 0.36, 1] }}
        className="relative bg-surface-elevated border border-border/60 rounded-[24px] p-8 w-full max-w-sm shadow-[0_20px_60px_-12px_rgba(0,0,0,0.35)]"
      >
        <h2 className="font-serif text-2xl font-semibold mb-1 text-ink">
          {step === 'phone' ? t('auth.title') : t('auth.enter_code')}
        </h2>
        <p className="text-sm text-muted mb-6">
          {step === 'phone' ? t('auth.enter_phone') : t('auth.code_sent_to', { phone })}
        </p>

        <AnimatePresence mode="wait">
          {step === 'phone' ? (
            <motion.form
              key="phone"
              onSubmit={handlePhone}
              className="space-y-4"
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 20 }}
            >
              <input
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                placeholder="+992 XX XXX XXXX"
                type="tel"
                className="w-full h-12 px-4 rounded-[12px] border border-border bg-cream text-ink placeholder:text-muted text-sm outline-none transition-colors duration-150 ease-out focus:border-green-deep focus:ring-2 focus:ring-green-deep/15"
                autoFocus
              />
              <Button type="submit" fullWidth size="lg" loading={loading}>
                {t('auth.get_code')}
              </Button>
            </motion.form>
          ) : (
            <motion.div
              key="code"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
            >
              <motion.div
                className="flex gap-2 justify-center mb-4"
                animate={codeError ? { x: [-6, 6, -4, 4, 0] } : {}}
                transition={{ duration: 0.3 }}
              >
                {code.map((d, i) => (
                  <input
                    key={i}
                    ref={(el) => {
                      codeRefs.current[i] = el
                    }}
                    value={d}
                    onChange={(e) => handleCodeInput(i, e.target.value)}
                    onKeyDown={(e) => handleCodeKey(i, e)}
                    maxLength={1}
                    type="text"
                    inputMode="numeric"
                    className={`w-10 h-12 text-center text-lg font-semibold rounded-[10px] border outline-none text-ink transition-all duration-150 ease-out ${
                      codeError
                        ? 'border-error bg-error/5'
                        : 'border-border bg-cream focus:border-green-deep focus:ring-2 focus:ring-green-deep/15'
                    }`}
                  />
                ))}
              </motion.div>
              {codeError && (
                <p className="text-xs text-error text-center mb-3">{t('auth.wrong_code')}</p>
              )}
              {loading && <p className="text-xs text-muted text-center">{t('auth.checking')}</p>}
              <button
                onClick={() => setStep('phone')}
                className="text-xs text-muted hover:text-green-deep transition-colors mt-2 w-full text-center"
              >
                {t('auth.change_number')}
              </button>
            </motion.div>
          )}
        </AnimatePresence>

        <button
          onClick={closeModal}
          aria-label={t('common.close')}
          className="absolute top-4 right-4 w-8 h-8 rounded-full hover:bg-ink/8 flex items-center justify-center text-muted hover:text-ink text-lg transition-colors duration-150"
        >
          ×
        </button>
      </motion.div>
    </div>
  )
}
