import { z } from 'zod'
import { amountSchema, optionalTextSchema } from '@/shared/validation'

export const depositSchema = z.object({
  amount: amountSchema,
  description: optionalTextSchema(250),
})

export const withdrawSchema = (available: number) =>
  depositSchema.extend({
    amount: amountSchema.refine((amount) => amount <= available, 'validation.insufficientBalance'),
  })

export const statusReasonSchema = z.object({
  reason: z.string().trim().min(5, 'validation.reasonLength').max(250, 'validation.reasonLength'),
})

export type MoneyFormInput = z.input<typeof depositSchema>
export type MoneyFormOutput = z.output<typeof depositSchema>
export type StatusReasonForm = z.input<typeof statusReasonSchema>
