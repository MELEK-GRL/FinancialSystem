import { z } from 'zod'

const optionalText = z.string().transform((value) => value || undefined)

export const auditFilterSchema = z
  .object({ from: optionalText, to: optionalText, action: optionalText })
  .superRefine((form, context) => {
    if (form.from && form.to && form.to < form.from) {
      context.addIssue({ code: 'custom', path: ['to'], message: 'validation.dateRangeInvalid' })
    }
  })

export type AuditFilterInput = z.input<typeof auditFilterSchema>
export type AuditFilterOutput = z.output<typeof auditFilterSchema>
