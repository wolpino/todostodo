import { IconButton } from '@chakra-ui/react'
import { headerIconButtonProps } from '@/lib/appTheme'

function PopOutIcon() {
  return (
    <svg width="36" height="36" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <rect
        x="4"
        y="8"
        width="12"
        height="12"
        rx="1.5"
        stroke="currentColor"
        strokeWidth="1.5"
      />
      <path
        d="M14 4h6v6M20 4 10 14"
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function openSmallWindow() {
  const width = 440
  const height = 720
  const left = Math.max(0, window.screenX + window.outerWidth - width - 16)
  const top = Math.max(0, window.screenY + 48)
  const features = [
    `width=${width}`,
    `height=${height}`,
    `left=${left}`,
    `top=${top}`,
    'menubar=no',
    'toolbar=no',
    'location=no',
    'status=no',
    'resizable=yes',
    'scrollbars=yes',
  ].join(',')

  window.open(`${window.location.origin}/`, 'todostodo', features)
}

export function OpenSmallWindowButton() {
  if (window.opener != null) return null

  return (
    <IconButton
      aria-label="Open in small window"
      title="Open in small window"
      onClick={openSmallWindow}
      {...headerIconButtonProps}
    >
      <PopOutIcon />
    </IconButton>
  )
}
