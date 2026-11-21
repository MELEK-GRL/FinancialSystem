import { useMemo, useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useLazyLookupAccountQuery, type Account, type AccountLookup } from '@/features/accounts'
import { getErrorMessage, getProblemDetails, hasServerResponse } from '@/shared/api'
import { useIdempotencyKey, useLocale } from '@/shared/hooks'
import { formatMoney, popup } from '@/shared/lib'
import { useTransferMutation } from '../transactionsApi'
import { transferSchema, type TransferFormInput, type TransferFormOutput } from '../schemas'

export type PendingTransfer = TransferFormOutput & { source: Account; recipient: AccountLookup }

export function useTransferForm(accounts: Account[]) {
  const { t } = useTranslation()
  const locale = useLocale()
  const idempotency = useIdempotencyKey()
  const [pending, setPending] = useState<PendingTransfer | null>(null)
  const [lookup, lookupState] = useLazyLookupAccountQuery()
  const [transfer, transferState] = useTransferMutation()
  const schema = useMemo(() => transferSchema(accounts), [accounts])
  const form = useForm<TransferFormInput, unknown, TransferFormOutput>({
    resolver: zodResolver(schema),
    mode: 'onBlur',
    reValidateMode: 'onChange',
    defaultValues: {
      sourceAccountId: accounts.length === 1 ? accounts[0].id : '',
      targetAccountNo: '',
      amount: '',
      description: '',
    },
  })

  const review = form.handleSubmit(async (values) => {
    const source = accounts.find((account) => account.id === values.sourceAccountId)
    if (!source) return
    try {
      const recipient = await lookup(values.targetAccountNo).unwrap()
      if (!recipient.isActive) {
        form.setError('targetAccountNo', { type: 'server', message: 'transfer.recipientInactive' })
        return
      }
      setPending({ ...values, source, recipient })
    } catch (error) {
      if (getProblemDetails(error)?.status === 404) {
        form.setError('targetAccountNo', { type: 'server', message: 'transfer.recipientNotFound' })
        return
      }
      popup.danger({ title: t('transfer.failed'), description: getErrorMessage(error) })
    }
  })

  const confirm = async () => {
    if (!pending) return
    const request = {
      sourceAccountId: pending.sourceAccountId,
      targetAccountNo: pending.targetAccountNo,
      amount: pending.amount,
      description: pending.description,
    }
    try {
      const result = await transfer({
        ...request,
        idempotencyKey: idempotency.keyFor(request),
      }).unwrap()
      setPending(null)
      form.reset({
        sourceAccountId: pending.sourceAccountId,
        targetAccountNo: '',
        amount: '',
        description: '',
      })
      popup.success({
        title: t('transfer.done'),
        description: t('transfer.doneBody', {
          amount: formatMoney(result.amount, locale),
          recipient: pending.recipient.ownerName,
          referenceNo: result.referenceNo,
        }),
      })
    } catch (error) {
      if (hasServerResponse(error)) idempotency.reset()
      setPending(null)
      popup.danger({ title: t('transfer.failed'), description: getErrorMessage(error) })
    }
  }

  return {
    form,
    review,
    pending,
    confirm,
    cancel: () => setPending(null),
    isReviewing: lookupState.isFetching,
    isSending: transferState.isLoading,
  }
}
