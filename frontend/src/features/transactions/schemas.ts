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
