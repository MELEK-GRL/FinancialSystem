import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '@/shared/api'
import { useFieldError } from '@/shared/hooks'
import { popup } from '@/shared/lib'
import { Button, CenterModal, Textarea } from '@/shared/ui'
import { useChangeAccountStatusMutation } from '../accountsApi'
import { statusReasonSchema, type StatusReasonForm } from '../schemas'
import type { Account } from '../types'

export function AccountStatusToggle({ account }: { account: Account }) {
  const { t } = useTranslation()
  const fieldError = useFieldError()
  const [open, setOpen] = useState(false)
  const [changeStatus, { isLoading }] = useChangeAccountStatusMutation()
  const deactivating = account.status === 'Active'
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<StatusReasonForm>({
    resolver: zodResolver(statusReasonSchema),
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  const close = () => {
    setOpen(false)
    reset()
  }

  const onSubmit = handleSubmit(async ({ reason }) => {
    try {
      await changeStatus({
        id: account.id,
        status: deactivating ? 'Passive' : 'Active',
        reason: reason.trim(),
      }).unwrap()
      close()
      popup.success({
        title: t('accounts.statusChanged'),
        description: t('accounts.statusChangedBody', {
          accountNo: account.accountNo,
          status: t(deactivating ? 'status.Passive' : 'status.Active'),
        }),
      })
    } catch (error) {
      close()
      popup.danger({ title: t('accounts.statusChangeFailed'), description: getErrorMessage(error) })
    }
  })

  return (
    <>
      <Button
        size="sm"
        variant={deactivating ? 'danger' : 'secondary'}
        onClick={() => setOpen(true)}
      >
        {t(deactivating ? 'accounts.deactivate' : 'accounts.activate')}
      </Button>
      <CenterModal
        open={open}
        size="sm"
        title={t(deactivating ? 'accounts.deactivateTitle' : 'accounts.activateTitle')}
        closeLabel={t('common.close')}
        onClose={close}
      >
        <form noValidate onSubmit={onSubmit} className="flex flex-col gap-4">
          <p className="font-mono text-sm text-neutral-700">{account.accountNo}</p>
          <Textarea
            label={t('accounts.reason')}
            hint={t('accounts.reasonHint')}
            required
            maxLength={250}
            autoFocus
            error={fieldError(errors.reason)}
            {...register('reason')}
          />
          <div className="flex justify-end gap-2">
            <Button variant="secondary" onClick={close} disabled={isLoading}>
              {t('common.cancel')}
            </Button>
            <Button type="submit" variant={deactivating ? 'danger' : 'primary'} loading={isLoading}>
              {t(deactivating ? 'accounts.deactivate' : 'accounts.activate')}
            </Button>
          </div>
        </form>
      </CenterModal>
    </>
  )
}
