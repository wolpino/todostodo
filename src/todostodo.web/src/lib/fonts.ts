export type FontId = 'comic-shanns' | 'courier-prime' | 'patrick-hand'

/** Legacy font ids still stored for some users. */
const LEGACY_FONT_ALIASES: Record<string, FontId> = {
  caveat: 'patrick-hand',
}

export const FONT_OPTIONS: { id: FontId; label: string; stack: string }[] = [
  {
    id: 'comic-shanns',
    label: 'Comic Shanns',
    stack: "'Comic Shanns', system-ui, sans-serif",
  },
  {
    id: 'courier-prime',
    label: 'Courier Prime',
    stack: "'Courier Prime', ui-monospace, monospace",
  },
  {
    id: 'patrick-hand',
    label: 'Patrick Hand',
    stack: "'Patrick Hand', cursive",
  },
]

export const DEFAULT_FONT: FontId = 'comic-shanns'

export const normalizeFontId = (font: string | null | undefined): FontId | undefined => {
  if (!font) return undefined
  const aliased = LEGACY_FONT_ALIASES[font] ?? font
  return FONT_OPTIONS.find((f) => f.id === aliased)?.id
}

export const fontStackFor = (font: string | null | undefined): string =>
  FONT_OPTIONS.find((f) => f.id === normalizeFontId(font))?.stack ?? FONT_OPTIONS[0].stack
