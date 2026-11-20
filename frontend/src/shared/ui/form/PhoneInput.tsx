import { TextInput, type TextInputProps } from './TextInput'
import { formatPhone, withSanitizer } from './sanitize'

export type PhoneInputProps = Omit<TextInputProps, 'type' | 'inputMode' | 'maxLength'>

export function PhoneInput({ onChange, ...props }: PhoneInputProps) {
  return (
    <TextInput
      type="tel"
      inputMode="numeric"
      autoComplete="tel-national"
      maxLength={13}
      placeholder="5XX XXX XX XX"
      className="tabular-nums"
      onChange={withSanitizer(formatPhone, onChange)}
      {...props}
    />
  )
}
