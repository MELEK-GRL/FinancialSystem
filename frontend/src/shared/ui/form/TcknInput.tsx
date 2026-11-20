import { TextInput, type TextInputProps } from './TextInput'
import { sanitizeDigits, withSanitizer } from './sanitize'

export type TcknInputProps = Omit<TextInputProps, 'type' | 'inputMode' | 'maxLength'>

export function TcknInput({ onChange, ...props }: TcknInputProps) {
  return (
    <TextInput
      type="text"
      inputMode="numeric"
      autoComplete="off"
      maxLength={11}
      className="tabular-nums"
      onChange={withSanitizer((value) => sanitizeDigits(value, 11), onChange)}
      {...props}
    />
  )
}
