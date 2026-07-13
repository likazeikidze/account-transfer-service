import type {
  Account,
  ProblemDetails,
  TransferHistoryItem,
  TransferRequest,
  TransferResponse,
} from '../types/models'

export class ApiError extends Error {
  status: number
  errorCode?: string

  constructor(problem: ProblemDetails, status: number) {
    super(problem.detail ?? problem.title ?? 'Request failed')
    this.status = status
    this.errorCode = problem.errorCode
  }
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
  })

  if (!response.ok) {
    const problem: ProblemDetails = await response.json().catch(() => ({}))
    throw new ApiError(problem, response.status)
  }

  return response.json() as Promise<T>
}

export const getAccounts = () => request<Account[]>('/api/accounts')

export const getTransfers = () => request<TransferHistoryItem[]>('/api/transfers')

export const createTransfer = (payload: TransferRequest) =>
  request<TransferResponse>('/api/transfers', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
