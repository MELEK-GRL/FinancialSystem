export const colors = {
  primary: { DEFAULT: '#1f4e79', hover: '#173b5c', soft: '#e8eff6' },
  success: { DEFAULT: '#15803d', soft: '#dcfce7' },
  danger: { DEFAULT: '#b91c1c', hover: '#991b1b', soft: '#fee2e2' },
  warning: { DEFAULT: '#b45309', soft: '#fef3c7' },
  neutral: {
    50: '#fafafa',
    100: '#f4f4f5',
    200: '#e4e4e7',
    300: '#d4d4d8',
    400: '#a1a1aa',
    500: '#71717a',
    600: '#52525b',
    700: '#3f3f46',
    800: '#27272a',
    900: '#18181b',
  },
} as const

export const statusColors = {
  Success: colors.success.DEFAULT,
  Failed: colors.danger.DEFAULT,
  Pending: colors.warning.DEFAULT,
} as const

export const chartColors = {
  inflow: colors.success.DEFAULT,
  outflow: colors.danger.DEFAULT,
  net: colors.primary.DEFAULT,
  grid: colors.neutral[200],
  axis: colors.neutral[500],
} as const
