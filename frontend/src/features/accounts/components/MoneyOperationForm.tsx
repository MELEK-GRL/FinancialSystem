import { useMemo } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useDepositMutation, useWithdrawMutation } from '@/features/transactions'
import { getErrorMessage, getProblemDetails } from '@/shared/api'
import { useApiFormErrors, useFieldError, useIdempotencyKey, useLocale } from '@/shared/hooks'
import { formatMoney, popup } from '@/shared/lib'
import { AmountInput, Button, DetailList, MoneyText, Textarea } from '@/shared/ui'
import {
  depositSchema,
  withdrawSchema,
  type MoneyFormInput,
  type MoneyFormOutput,
} from '../schemas'
import type { Account } from '../types'

export type MoneyOperationFormProps = {
  account: Account
  mode: 'deposit' | 'withdraw'
  onDone: () => void
}

const fields = ['amount', 'description'] as const

export function MoneyOperationForm({ account, mode, onDone }: MoneyOperationFormProps) {
  const { t } = useTranslation()
  const locale = useLocale()
  const fieldError = useFieldError()
  const idempotencyKey = useIdempotencyKey()
  const [deposit, depositState] = useDepositMutation()
  const [withdraw, withdrawState] = useWithdrawMutation()
  const schema = useMemo(
    () => (mode === 'withdraw' ? withdrawSchema(account.balance) : depositSchema),
    [mode, account.balance],
  )
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<MoneyFormInput, unknown, MoneyFormOutput>({
    resolver: zodResolver(schema),
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })
  const applyApiErrors = useApiFormErrors(setError, fields)

  const onSubmit = handleSubmit(async ({ amount, description }) => {
    const request = { accountId: account.id, amount, description }
    try {
      const run = mode === 'deposit' ? deposit : withdraw
      const transaction = await run({
        ...request,
        idempotencyKey: idempotencyKey(request),
      }).unwrap()
      onDone()
      popup.success({
        title: t(mode === 'deposit' ? 'accounts.depositDone' : 'accounts.withdrawDone'),
        description: t('accounts.doneBody', {
          amount: formatMoney(transaction.amount, locale),
          referenceNo: transaction.referenceNo,
        }),
      })
    } catch (error) {
      const problem = getProblemDetails(error)
      if (problem && applyApiErrors(problem)) return
      onDone()
      popup.danger({ title: t('accounts.operationFailed'), description: getErrorMessage(error) })
    }
  })

  return (
    <form noValidate onSubmit={onSubmit} className="flex flex-col gap-5">
      <div className="rounded-lg bg-neutral-50 p-4">
        <DetailList
          items={[
            {
              label: t('accounts.account'),
              value: <span className="font-mono">{account.accountNo}</span>,
            },
            { label: t('accounts.available'), value: <MoneyText amount={account.balance} /> },
          ]}
        />
      </div>
      <AmountInput
        label={t('accounts.amount')}
        required
        autoFocus
        error={fieldError(errors.amount)}
        {...register('amount')}
      />
      <Textarea
        label={t('accounts.description')}
        maxLength={250}
        error={fieldError(errors.description)}
        {...register('description')}
      />
      <Button
        type="submit"
        loading={depositState.isLoading || withdrawState.isLoading}
        className="w-full"
      >
        {t(mode === 'deposit' ? 'accounts.confirmDeposit' : 'accounts.confirmWithdraw')}
      </Button>
    </form>
  )
}

export function DepositForm(props: Omit<MoneyOperationFormProps, 'mode'>) {
  return <MoneyOperationForm {...props} mode="deposit" />
}

export function WithdrawForm(props: Omit<MoneyOperationFormProps, 'mode'>) {
  return <MoneyOperationForm {...props} mode="withdraw" />
}
