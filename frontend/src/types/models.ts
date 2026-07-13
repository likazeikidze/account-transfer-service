export interface Account {
  id: string
  accountNumber: string
  ownerName: string
  balance: number
  currency: string
}

export interface TransferHistoryItem {
  id: string
  senderAccountId: string
  senderAccountNumber: string
  receiverAccountId: string
  receiverAccountNumber: string
  amount: number
  currency: string
  status: string
  createdAt: string
}

export interface TransferRequest {
  senderAccountId: string
  receiverAccountId: string
  amount: number
}

export interface TransferResponse {
  id: string
  senderAccountId: string
  receiverAccountId: string
  amount: number
  currency: string
  status: string
  createdAt: string
}

export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errorCode?: string
  traceId?: string
}
