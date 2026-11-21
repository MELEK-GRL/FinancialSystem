import { useTranslation } from 'react-i18next'
import { maskNationalId } from '@/shared/lib'
import { Card, DateText, DetailList, ErrorState, PageHeader, Spinner } from '@/shared/ui'
import { CustomerStatusBadge } from '../components/CustomerStatusBadge'
import { ProfileForm } from '../components/ProfileForm'
import { useGetMyProfileQuery } from '../customersApi'

export function ProfilePage() {
  const { t } = useTranslation()
  const { data: customer, isLoading, isError, refetch } = useGetMyProfileQuery()

  if (isLoading) return <Spinner label={t('common.loading')} />
  if (isError || !customer) {
    return (
      <ErrorState
        title={t('errors.INTERNAL_ERROR')}
        retryLabel={t('common.retry')}
        onRetry={refetch}
      />
    )
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader title={t('customers.profileTitle')} subtitle={t('customers.profileSubtitle')} />
      <Card title={t('customers.personalInfo')}>
        <DetailList
          items={[
            { label: t('customers.fullName'), value: `${customer.firstName} ${customer.lastName}` },
            { label: t('customers.customerNo'), value: customer.customerNo },
            { label: t('customers.nationalId'), value: maskNationalId(customer.nationalId) },
            {
              label: t('customers.status'),
              value: <CustomerStatusBadge status={customer.status} />,
            },
            {
              label: t('customers.createdAt'),
              value: <DateText value={customer.createdAt} withTime={false} />,
            },
          ]}
        />
      </Card>
      <Card title={t('customers.contactInfo')}>
        <ProfileForm customer={customer} />
      </Card>
    </div>
  )
}
