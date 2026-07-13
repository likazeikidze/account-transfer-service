import { useQuery } from '@tanstack/react-query'
import { getTransfers } from '../api/client'

function formatCurrency(amount: number, currency: string) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount)
}

function formatTimestamp(iso: string) {
  return new Date(iso).toLocaleString()
}

export function TransactionList() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['transfers'],
    queryFn: getTransfers,
  })

  return (
    <section className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-gray-900">Completed Transactions</h2>
      {isLoading && <p className="text-sm text-gray-500">Loading transactions…</p>}
      {isError && <p className="text-sm text-red-600">Failed to load transactions.</p>}
      {data && data.length === 0 && (
        <p className="text-sm text-gray-500">No transfers yet.</p>
      )}
      {data && data.length > 0 && (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-b border-gray-200 text-gray-500">
              <th className="py-2 font-medium">From</th>
              <th className="py-2 font-medium">To</th>
              <th className="py-2 text-right font-medium">Amount</th>
              <th className="py-2 text-right font-medium">When</th>
            </tr>
          </thead>
          <tbody>
            {data.map((transfer) => (
              <tr key={transfer.id} className="border-b border-gray-100 last:border-0">
                <td className="py-2 font-mono text-gray-700">{transfer.senderAccountNumber}</td>
                <td className="py-2 font-mono text-gray-700">{transfer.receiverAccountNumber}</td>
                <td className="py-2 text-right font-medium text-gray-900">
                  {formatCurrency(transfer.amount, transfer.currency)}
                </td>
                <td className="py-2 text-right text-gray-500">
                  {formatTimestamp(transfer.createdAt)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
