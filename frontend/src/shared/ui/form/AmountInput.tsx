import { useLocale } from '@/shared/hooks/useLocale'
import { TextInput, type TextInputProps } from './TextInput'
import { sanitizeAmount, withSanitizer } from './sanitize'

export type AmountInputProps = Omit<TextInputProps, 'type' | 'inputMode' | 'suffix'>

export function AmountInput({ onChange, ...props }: AmountInputProps) {
  const decimalSeparator = useLocale() === 'tr-TR' ? ',' : '.'

  return (
    <TextInput
      type="text"
      inputMode="decimal"
      autoComplete="off"
      suffix="₺"
      className="tabular-nums"
      onChange={withSanitizer((value) => sanitizeAmount(value, decimalSeparator), onChange)}
      {...props}
    />
  )
}
