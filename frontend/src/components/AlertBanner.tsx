export interface BannerState {
  kind: 'success' | 'error'
  message: string
}

interface Props {
  banner: BannerState | null
  onDismiss: () => void
}

export function AlertBanner({ banner, onDismiss }: Props) {
  if (!banner) return null

  const isError = banner.kind === 'error'

  return (
    <div
      role="alert"
      className={`flex items-start justify-between gap-4 rounded-lg border px-4 py-3 text-sm ${
        isError
          ? 'border-red-300 bg-red-50 text-red-800'
          : 'border-green-300 bg-green-50 text-green-800'
      }`}
    >
      <span>{banner.message}</span>
      <button
        type="button"
        onClick={onDismiss}
        className="font-medium underline decoration-dotted underline-offset-2"
      >
        Dismiss
      </button>
    </div>
  )
}
