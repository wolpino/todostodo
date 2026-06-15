import { useEffect } from 'react'
import { useSettings } from '@/hooks/useSettings'
import { fontStackFor } from '@/lib/fonts'

/** Syncs the active font from settings to CSS variables used by the page and Chakra. */
export function FontApplier() {
  const { data } = useSettings()

  useEffect(() => {
    const stack = fontStackFor(data?.font)
    document.documentElement.style.setProperty('--app-font', stack)
    document.documentElement.style.setProperty('--sans', stack)
  }, [data?.font])

  return null
}
