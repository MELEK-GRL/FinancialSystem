import { z } from 'zod'
import { isValidTckn } from './tckn'

export const MAX_TRANSACTION_AMOUNT = Number(import.meta.env.VITE_MAX_TRANSACTION_AMOUNT ?? 100000)

const required = 'validation.required'

export const nameSchema = z
  .string()
  .trim()
  .min(1, required)
  .min(2, 'validation.nameLength')
  .max(50, 'validation.nameLength')

export const emailSchema = z
  .string()
  .trim()
  .min(1, required)
  .max(254, 'validation.emailInvalid')
  .email('validation.emailInvalid')

export const phoneSchema = z
  .string()
  .transform((value) => value.replace(/\D/g, ''))
  .pipe(
    z
      .string()
      .min(1, required)
      .regex(/^5[0-9]{9}$/, 'validation.phoneInvalid'),
  )

export const tcknSchema = z
  .string()
  .trim()
  .min(1, required)
  .refine(isValidTckn, 'validation.tcknInvalid')

export const PASSWORD_MIN_LENGTH = 6
export const PASSWORD_MAX_LENGTH = 16

export const passwordSchema = z
  .string()
  .min(1, required)
  .min(PASSWORD_MIN_LENGTH, 'validation.passwordLength')
  .max(PASSWORD_MAX_LENGTH, 'validation.passwordLength')
  .refine((value) => /[A-Za-z]/.test(value) && /[0-9]/.test(value), 'validation.passwordWeak')

export const accountNoSchema = z
  .string()
  .transform((value) => value.replace(/\s/g, '').toUpperCase())
  .pipe(
    z
      .string()
      .min(1, required)
      .regex(/^TR[0-9]{10}$/, 'validation.accountNoInvalid'),
  )

export const amountSchema = z
  .string()
  .trim()
  .min(1, required)
  .transform((value) => value.replace(',', '.'))
  .refine((value) => /^[0-9]+(\.[0-9]{1,2})?$/.test(value), 'validation.amountInvalid')
  .transform(Number)
  .refine((value) => value > 0, 'validation.amountPositive')
  .refine((value) => value <= MAX_TRANSACTION_AMOUNT, 'validation.amountLimit')

export const optionalTextSchema = (max: number) =>
  z
    .string()
    .trim()
    .max(max, 'validation.textTooLong')
    .transform((value) => (value === '' ? undefined : value))
