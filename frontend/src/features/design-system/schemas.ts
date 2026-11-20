import { z } from 'zod'
import {
  accountNoSchema,
  amountSchema,
  emailSchema,
  nameSchema,
  optionalTextSchema,
  passwordSchema,
  phoneSchema,
  tcknSchema,
} from '@/shared/validation'

export const transactionTypes = ['Deposit', 'Withdrawal', 'Transfer'] as const

export const previewFormSchema = z
  .object({
    firstName: nameSchema,
    email: emailSchema,
    nationalId: tcknSchema,
    phone: phoneSchema,
    password: passwordSchema,
    amount: amountSchema,
    accountNo: accountNoSchema,
    description: optionalTextSchema(250),
    type: z.enum(transactionTypes, 'validation.required'),
    from: z.string(),
    to: z.string(),
    approve: z.boolean().refine((value) => value, 'validation.required'),
  })
  .refine((form) => !form.from || !form.to || form.to >= form.from, {
    path: ['to'],
    message: 'validation.dateRangeInvalid',
  })

export type PreviewFormInput = z.input<typeof previewFormSchema>
export type PreviewFormOutput = z.output<typeof previewFormSchema>
