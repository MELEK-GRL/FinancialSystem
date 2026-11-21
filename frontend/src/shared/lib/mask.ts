export function maskNationalId(value: string) {
  return value.length === 11 ? `${value.slice(0, 3)}******${value.slice(9)}` : value
}
