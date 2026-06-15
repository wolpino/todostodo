export type FontId = 'comic-shanns' | 'courier-prime' | 'caveat'

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
    id: 'caveat',
    label: 'Caveat',
    stack: "'Caveat', cursive",
  },
]

export const DEFAULT_FONT: FontId = 'comic-shanns'

export const fontStackFor = (font: string | null | undefined): string =>
  FONT_OPTIONS.find((f) => f.id === font)?.stack ?? FONT_OPTIONS[0].stack
