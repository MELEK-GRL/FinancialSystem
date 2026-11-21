import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { reportApiError } from '@/shared/api'
import { notify } from '@/shared/lib'
import { Button, ConfirmDialog } from '@/shared/ui'
import { useChangeCustomerStatusMutation } from '../customersApi'
import type { Customer } from '../types'

export function CustomerStatusToggle({ customer }: { customer: Customer }) {
  const { t } = useTranslation()
  const [open, setOpen] = useState(false)
  const [changeStatus, { isLoading }] = useChangeCustomerStatusMutation()
  const deactivating = customer.status === 'Active'
  const name = `${customer.firstName} ${customer.lastName}`

  const confirm = async () => {
    try {
      await changeStatus({ id: customer.id, status: deactivating ? 'Passive' : 'Active' }).unwrap()
      notify.success(t(deactivating ? 'customers.deactivated' : 'customers.activated'))
      setOpen(false)
    } catch (error) {
      reportApiError(error)
    }
  }

  return (
    <>
      <Button variant={deactivating ? 'danger' : 'secondary'} onClick={() => setOpen(true)}>
        {t(deactivating ? 'customers.deactivate' : 'customers.activate')}
      </Button>
      <ConfirmDialog
        open={open}
        tone={deactivating ? 'danger' : 'primary'}
        title={t(deactivating ? 'customers.deactivateTitle' : 'customers.activateTitle')}
        description={t(deactivating ? 'customers.deactivateBody' : 'customers.activateBody', {
          name,
        })}
        confirmLabel={t(deactivating ? 'customers.deactivate' : 'customers.activate')}
        cancelLabel={t('common.cancel')}
        loading={isLoading}
        onConfirm={confirm}
        onCancel={() => setOpen(false)}
      />
    </>
  )
}
