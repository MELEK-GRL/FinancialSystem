import { z } from 'zod'
import type { Account } from '@/features/accounts'
import { accountNoSchema, amountSchema, optionalTextSchema } from '@/shared/validation'

export const transferSchema = (accounts: Account[]) =>
  z
    .object({
      sourceAccountId: z.string().min(1, 'validation.required'),
      targetAccountNo: accountNoSchema,
      amount: amountSchema,
      description: optionalTextSchema(250),
    })
    .superRefine((form, context) => {
      const source = accounts.find((account) => account.id === form.sourceAccountId)
      if (!source) return
      if (source.accountNo === form.targetAccountNo) {
        context.addIssue({
          code: 'custom',
          path: ['targetAccountNo'],
          message: 'validation.sameAccount',
        })
      }
      if (form.amount > source.balance) {
        context.addIssue({
          code: 'custom',
          path: ['amount'],
          message: 'validation.insufficientBalance',
        })
      }
    })

export type TransferFormInput = z.input<ReturnType<typeof transferSchema>>
export type TransferFormOutput = z.output<ReturnType<typeof transferSchema>>

const optionalText = z.string().transform((value) => value || undefined)

const optionalAmount = z
  .string()
  .trim()
  .transform((value) => value || undefined)
  .pipe(amountSchema.optional())

export const historyFilterSchema = z
  .object({
    from: optionalText,
    to: optionalText,
    type: optionalText,
    status: optionalText,
    minAmount: optionalAmount,
    maxAmount: optionalAmount,
  })
  .superRefine((form, context) => {
    if (form.from && form.to && form.to < form.from) {
      context.addIssue({ code: 'custom', path: ['to'], message: 'validation.dateRangeInvalid' })
    }
    if (
      form.minAmount !== undefined &&
      form.maxAmount !== undefined &&
      form.maxAmount < form.minAmount
    ) {
      context.addIssue({
        code: 'custom',
        path: ['maxAmount'],
        message: 'validation.amountRangeInvalid',
      })
    }
  })

export type HistoryFilterInput = z.input<typeof historyFilterSchema>
export type HistoryFilterOutput = z.output<typeof historyFilterSchema>
