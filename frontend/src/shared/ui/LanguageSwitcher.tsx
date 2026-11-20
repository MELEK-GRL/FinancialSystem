import clsx from 'clsx'

export type LanguageOption = { value: string; label: string }

export type LanguageSwitcherProps = {
  value: string
  options: LanguageOption[]
  label: string
  onChange: (value: string) => void
}

export function LanguageSwitcher({ value, options, label, onChange }: LanguageSwitcherProps) {
  return (
    <div
      role="group"
      aria-label={label}
      className="inline-flex rounded-lg border border-neutral-200 bg-white p-0.5"
    >
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          aria-pressed={option.value === value}
          onClick={() => onChange(option.value)}
          className={clsx(
            'rounded-md px-2.5 py-1 text-xs font-medium transition-colors duration-150',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary',
            option.value === value
              ? 'bg-primary text-white'
              : 'text-neutral-600 hover:bg-neutral-100',
          )}
        >
          {option.label}
        </button>
      ))}
    </div>
  )
}
