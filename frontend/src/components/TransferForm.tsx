import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiError, createTransfer, getAccounts } from '../api/client'
import type { BannerState } from './AlertBanner'

interface Props {
  onResult: (banner: BannerState) => void
}

export function TransferForm({ onResult }: Props) {
  const queryClient = useQueryClient()
  const { data: accounts } = useQuery({ queryKey: ['accounts'], queryFn: getAccounts })

  const [senderAccountId, setSenderAccountId] = useState('')
  const [receiverAccountId, setReceiverAccountId] = useState('')
  const [amount, setAmount] = useState('')
  const [validationError, setValidationError] = useState<string | null>(null)

  const mutation = useMutation({
    mutationFn: createTransfer,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
      onResult({ kind: 'success', message: 'Transfer completed successfully.' })
      setAmount('')
    },
    onError: (error: unknown) => {
      const message = error instanceof ApiError ? error.message : 'Transfer failed. Please try again.'
      onResult({ kind: 'error', message })
    },
  })

  const sender = accounts?.find((a) => a.id === senderAccountId)
  const amountValue = Number(amount)

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setValidationError(null)

    if (!senderAccountId || !receiverAccountId) {
      setValidationError('Please select both a sender and a receiver account.')
      return
    }
    if (senderAccountId === receiverAccountId) {
      setValidationError('Sender and receiver accounts must be different.')
      return
    }
    if (!amount || amountValue <= 0) {
      setValidationError('Amount must be greater than zero.')
      return
    }
    if (sender && amountValue > sender.balance) {
      setValidationError('Amount exceeds the sender account balance.')
      return
    }

    mutation.mutate({ senderAccountId, receiverAccountId, amount: amountValue })
  }

  return (
    <section className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
      <h2 className="mb-3 text-lg font-semibold text-gray-900">Transfer Money</h2>
      <form onSubmit={handleSubmit} className="flex flex-col gap-3">
        <label className="flex flex-col gap-1 text-sm text-gray-700">
          From
          <select
            value={senderAccountId}
            onChange={(e) => setSenderAccountId(e.target.value)}
            className="rounded border border-gray-300 px-2 py-1.5"
          >
            <option value="">Select account…</option>
            {accounts?.map((a) => (
              <option key={a.id} value={a.id}>
                {a.accountNumber} — {a.ownerName}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm text-gray-700">
          To
          <select
            value={receiverAccountId}
            onChange={(e) => setReceiverAccountId(e.target.value)}
            className="rounded border border-gray-300 px-2 py-1.5"
          >
            <option value="">Select account…</option>
            {accounts?.map((a) => (
              <option key={a.id} value={a.id}>
                {a.accountNumber} — {a.ownerName}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm text-gray-700">
          Amount
          <input
            type="number"
            min="0.01"
            step="0.01"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            className="rounded border border-gray-300 px-2 py-1.5"
            placeholder="0.00"
          />
        </label>

        {validationError && <p className="text-sm text-red-600">{validationError}</p>}

        <button
          type="submit"
          disabled={mutation.isPending}
          className="rounded bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
        >
          {mutation.isPending ? 'Transferring…' : 'Transfer'}
        </button>
      </form>
    </section>
  )
}
