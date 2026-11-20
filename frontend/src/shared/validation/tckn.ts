export function isValidTckn(value: string) {
  if (!/^[1-9][0-9]{10}$/.test(value)) return false

  const digits = [...value].map(Number)
  const oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8]
  const evenSum = digits[1] + digits[3] + digits[5] + digits[7]
  const tenth = (((oddSum * 7 - evenSum) % 10) + 10) % 10
  const eleventh = digits.slice(0, 10).reduce((sum, digit) => sum + digit, 0) % 10

  return digits[9] === tenth && digits[10] === eleventh
}
