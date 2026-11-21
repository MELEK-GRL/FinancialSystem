import { ArrowLeft } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useLocation, useParams } from 'react-router'
import { getErrorMessage } from '@/shared/api'
import { maskNationalId } from '@/shared/lib'
import {
  Card,
  DateText,
  DetailList,
  ErrorState,
  formatPhone,
  Link,
  PageHeader,
  Spinner,
} from '@/shared/ui'
import { CustomerAccounts } from '../components/CustomerAccounts'
import { CustomerStatusBadge } from '../components/CustomerStatusBadge'
import { CustomerStatusToggle } from '../components/CustomerStatusToggle'
import { useGetCustomerQuery } from '../customersApi'

export function CustomerDetailPage() {
  const { t } = useTranslation()
  const { id = '' } = useParams()
  const backTo = (useLocation().state as { from?: string } | null)?.from ?? '/customers'
  const { data: customer, isLoading, error, refetch } = useGetCustomerQuery(id)

  const back = (
    <Link to={backTo} className="inline-flex items-center gap-1 text-sm">
      <ArrowLeft className="size-4" aria-hidden="true" />
      {t('customers.back')}
    </Link>
  )

  if (isLoading) return <Spinner label={t('common.loading')} />
  if (error || !customer) {
    return (
      <div className="flex flex-col gap-4">
        {back}
        <ErrorState
          title={t('customers.detailError')}
          description={getErrorMessage(error)}
          retryLabel={t('common.retry')}
          onRetry={refetch}
        />
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-6">
      {back}
      <PageHeader
        title={
          <span className="flex flex-wrap items-center gap-3">
            {customer.firstName} {customer.lastName}
            <CustomerStatusBadge status={customer.status} />
          </span>
        }
        subtitle={customer.customerNo}
        actions={<CustomerStatusToggle customer={customer} />}
      />
      <Card title={t('customers.personalInfo')}>
        <DetailList
          items={[
            { label: t('customers.nationalId'), value: maskNationalId(customer.nationalId) },
            { label: t('customers.email'), value: customer.email },
            { label: t('customers.phone'), value: formatPhone(customer.phone) },
            {
              label: t('customers.createdAt'),
              value: <DateText value={customer.createdAt} withTime={false} />,
            },
          ]}
        />
      </Card>
      <CustomerAccounts customerId={customer.id} />
    </div>
  )
}
