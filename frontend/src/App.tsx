import { useState } from 'react'
import { AccountList } from './components/AccountList'
import { TransactionList } from './components/TransactionList'
import { TransferForm } from './components/TransferForm'
import { AlertBanner, type BannerState } from './components/AlertBanner'

function App() {
  const [banner, setBanner] = useState<BannerState | null>(null)

  return (
    <div className="mx-auto flex min-h-screen max-w-3xl flex-col gap-4 bg-gray-50 p-6">
      <h1 className="text-2xl font-semibold text-gray-900">Account Transfer Service</h1>

      <AlertBanner banner={banner} onDismiss={() => setBanner(null)} />

      <AccountList />
      <TransferForm onResult={setBanner} />
      <TransactionList />
    </div>
  )
}

export default App
