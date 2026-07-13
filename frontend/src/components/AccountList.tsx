import { useQuery } from '@tanstack/react-query'
import { getAccounts } from '../api/client'

function formatCurrency(amount: number, currency: string) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount)
}

export function AccountList() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['accounts'],
    queryFn: getAccounts,
  })

  return (
    <section className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-gray-900">Accounts</h2>
      {isLoading && <p className="text-sm text-gray-500">Loading accounts…</p>}
      {isError && <p className="text-sm text-red-600">Failed to load accounts.</p>}
      {data && (
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-b border-gray-200 text-gray-500">
              <th className="py-2 font-medium">Account #</th>
              <th className="py-2 font-medium">Owner</th>
              <th className="py-2 pr-0 text-right font-medium">Balance</th>
            </tr>
          </thead>
          <tbody>
            {data.map((account) => (
              <tr key={account.id} className="border-b border-gray-100 last:border-0">
                <td className="py-2 font-mono text-gray-700">{account.accountNumber}</td>
                <td className="py-2 text-gray-700">{account.ownerName}</td>
                <td className="py-2 text-right font-medium text-gray-900">
                  {formatCurrency(account.balance, account.currency)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
