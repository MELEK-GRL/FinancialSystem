import { useEffect, useRef, useState } from 'react'
import { Search, X } from 'lucide-react'
import { useDebounce } from '@/shared/hooks/useDebounce'
import { inputClassName } from './styles'

export type SearchInputProps = {
  label: string
  placeholder?: string
  clearLabel: string
  defaultValue?: string
  debounceMs?: number
  onSearch: (value: string) => void
}

export function SearchInput({
  label,
  placeholder,
  clearLabel,
  defaultValue = '',
  debounceMs = 300,
  onSearch,
}: SearchInputProps) {
  const [value, setValue] = useState(defaultValue)
  const debounced = useDebounce(value.trim(), debounceMs)
  const lastSearched = useRef(defaultValue.trim())

  useEffect(() => {
    if (debounced === lastSearched.current) return
    lastSearched.current = debounced
    onSearch(debounced)
  }, [debounced, onSearch])

  return (
    <div className="relative">
      <Search
        className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-neutral-400"
        aria-hidden="true"
      />
      <input
        type="search"
        aria-label={label}
        value={value}
        placeholder={placeholder}
        onChange={(event) => setValue(event.target.value)}
        className={inputClassName(false, 'pl-9 pr-9 [&::-webkit-search-cancel-button]:hidden')}
      />
      {value ? (
        <button
          type="button"
          aria-label={clearLabel}
          onClick={() => setValue('')}
          className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-1 text-neutral-400 hover:text-neutral-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
        >
          <X className="size-4" />
        </button>
      ) : null}
    </div>
  )
}
