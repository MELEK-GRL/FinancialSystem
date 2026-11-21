import { useState } from 'react'
import { Eye, EyeOff } from 'lucide-react'
import { TextInput, type TextInputProps } from './TextInput'

export type PasswordInputProps = Omit<TextInputProps, 'type' | 'suffix'> & {
  showLabel: string
  hideLabel: string
}

export function PasswordInput({
  showLabel,
  hideLabel,
  maxLength = 16,
  ...props
}: PasswordInputProps) {
  const [visible, setVisible] = useState(false)

  return (
    <TextInput
      type={visible ? 'text' : 'password'}
      maxLength={maxLength}
      suffix={
        <button
          type="button"
          aria-label={visible ? hideLabel : showLabel}
          aria-pressed={visible}
          onClick={() => setVisible((current) => !current)}
          className="rounded text-neutral-500 hover:text-neutral-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
        >
          {visible ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
        </button>
      }
      {...props}
    />
  )
}
