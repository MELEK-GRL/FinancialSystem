import { z } from 'zod'
import { emailSchema, phoneSchema } from '@/shared/validation'

export const profileSchema = z.object({
  email: emailSchema,
  phone: phoneSchema,
})

export type ProfileFormInput = z.input<typeof profileSchema>
export type ProfileFormOutput = z.output<typeof profileSchema>
