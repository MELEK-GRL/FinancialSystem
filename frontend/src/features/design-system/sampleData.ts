export type SampleTransaction = {
  id: string
  referenceNo: string
  type: 'Deposit' | 'Withdrawal' | 'Transfer'
  status: 'Success' | 'Failed' | 'Pending'
  direction: 'in' | 'out'
  amount: number
  createdAt: string
}

export const sampleTransactions: SampleTransaction[] = [
  {
    id: '1',
    referenceNo: 'TX2026112000000041',
    type: 'Deposit',
    status: 'Success',
    direction: 'in',
    amount: 12500,
    createdAt: '2026-11-20T09:15:00Z',
  },
  {
    id: '2',
    referenceNo: 'TX2026112000000042',
    type: 'Transfer',
    status: 'Success',
    direction: 'out',
    amount: 1250.5,
    createdAt: '2026-11-20T10:42:00Z',
  },
  {
    id: '3',
    referenceNo: 'TX2026112000000043',
    type: 'Withdrawal',
    status: 'Failed',
    direction: 'out',
    amount: 99999,
    createdAt: '2026-11-20T12:03:00Z',
  },
  {
    id: '4',
    referenceNo: 'TX2026112000000044',
    type: 'Transfer',
    status: 'Pending',
    direction: 'out',
    amount: 300,
    createdAt: '2026-11-20T13:30:00Z',
  },
]
