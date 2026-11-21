import { useTranslation } from 'react-i18next'
import type { Account } from '@/features/accounts'
import { useFieldError, useLocale, useFormKeyboard } from '@/shared/hooks'
import { formatMoney } from '@/shared/lib'
import { AmountInput, Button, Select, Textarea, TextInput } from '@/shared/ui'
import { useTransferForm } from '../hooks/useTransferForm'
import { TransferConfirmDialog } from './TransferConfirmDialog'

export function TransferForm({ accounts }: { accounts: Account[] }) {
  const { t } = useTranslation()
  const locale = useLocale()
  const fieldError = useFieldError()
  const keyboard = useFormKeyboard()
  const { form, review, pending, confirm, cancel, isReviewing, isSending } =
    useTransferForm(accounts)
  const {
    register,
    formState: { errors },
  } = form

  return (
    <>
      <form noValidate {...keyboard} onSubmit={review} className="flex flex-col gap-5">
        <Select
          label={t('transfer.source')}
          required
          placeholder={t('transfer.sourcePlaceholder')}
          options={accounts.map((account) => ({
            value: account.id,
            label: t('transfer.sourceOption', {
              accountNo: account.accountNo,
              balance: formatMoney(account.balance, locale),
            }),
          }))}
          error={fieldError(errors.sourceAccountId)}
          {...register('sourceAccountId')}
        />
        <TextInput
          autoFocus
          label={t('transfer.target')}
          hint={t('transfer.targetHint')}
          required
          autoComplete="off"
          placeholder="TR1000000001"
          className="font-mono uppercase"
          error={fieldError(errors.targetAccountNo)}
          {...register('targetAccountNo')}
        />
        <AmountInput
          label={t('transfer.amount')}
          required
          error={fieldError(errors.amount)}
          {...register('amount')}
        />
        <Textarea
          label={t('transfer.description')}
          maxLength={250}
          error={fieldError(errors.description)}
          {...register('description')}
        />
        <Button type="submit" loading={isReviewing} className="w-full sm:w-auto sm:self-end">
          {t('transfer.continue')}
        </Button>
      </form>
      <TransferConfirmDialog
        transfer={pending}
        sending={isSending}
        onConfirm={confirm}
        onCancel={cancel}
      />
    </>
  )
}
