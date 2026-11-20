import { colors } from '@/shared/theme/colors'
import { Text } from '@/shared/ui'

const swatches = [
  { name: 'primary', value: colors.primary.DEFAULT },
  { name: 'primary-soft', value: colors.primary.soft },
  { name: 'success', value: colors.success.DEFAULT },
  { name: 'danger', value: colors.danger.DEFAULT },
  { name: 'warning', value: colors.warning.DEFAULT },
  { name: 'neutral-50', value: colors.neutral[50] },
  { name: 'neutral-200', value: colors.neutral[200] },
  { name: 'neutral-500', value: colors.neutral[500] },
  { name: 'neutral-900', value: colors.neutral[900] },
]

export function ColorPalette() {
  return (
    <div className="grid grid-cols-3 gap-3 sm:grid-cols-5 lg:grid-cols-9">
      {swatches.map((swatch) => (
        <div key={swatch.name} className="flex flex-col gap-1.5">
          <span
            className="h-12 rounded-lg border border-neutral-200"
            style={{ backgroundColor: swatch.value }}
          />
          <Text variant="small">{swatch.name}</Text>
          <Text variant="small" className="font-mono">
            {swatch.value}
          </Text>
        </div>
      ))}
    </div>
  )
}
