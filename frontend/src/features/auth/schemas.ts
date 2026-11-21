import { z } from 'zod'
import {
  emailSchema,
  nameSchema,
  passwordSchema,
  phoneSchema,
  tcknSchema,
} from '@/shared/validation'

export const loginSchema = z.object({
  email: emailSchema,
  password: z.string().min(1, 'validation.required'),
})

export const registerSchema = z
  .object({
    firstName: nameSchema,
    lastName: nameSchema,
    nationalId: tcknSchema,
    email: emailSchema,
    phone: phoneSchema,
    password: passwordSchema,
    passwordConfirm: z.string().min(1, 'validation.required'),
  })
  .refine((form) => form.password === form.passwordConfirm, {
    path: ['passwordConfirm'],
    message: 'validation.passwordMismatch',
  })

export type LoginForm = z.input<typeof loginSchema>
export type RegisterFormInput = z.input<typeof registerSchema>
export type RegisterFormOutput = z.output<typeof registerSchema>
