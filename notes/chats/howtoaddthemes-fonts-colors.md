Given this project uses Chakra UI, here are the main approaches, from simplest to most robust:

## 1. Chakra UI's Built-in Color Mode (Simplest)

Chakra has first-class dark/light mode support via `useColorMode`. For fonts, you extend the theme.

```typescript
// Toggle dark/light
const { colorMode, toggleColorMode } = useColorMode();
```

Persistence is handled automatically via `localStorage`. For fonts, you'd define a set of theme variants and swap the whole theme object.

**Pros:** Zero boilerplate for dark/light. Accessible out of the box.  
**Cons:** Limited to the themes you pre-define; not fully dynamic.

---

## 2. CSS Custom Properties (CSS Variables)

Store the user's preferences and apply them as CSS variables on `document.documentElement`.

```typescript
// On settings change:
document.documentElement.style.setProperty('--app-font', 'Georgia, serif');
document.documentElement.style.setProperty('--brand-color', '#6B46C1');
```

Then in CSS/Chakra theme: reference `var(--app-font)`.

**Pros:** Fast, no re-renders, works with any styling system.  
**Cons:** Bypasses Chakra's theming system slightly; need to map variables to Chakra tokens.

---

## 3. Dynamic Chakra Theme Switching

Store preference in state/context, rebuild the Chakra theme object, and pass it to `ChakraProvider`.

```typescript
const theme = extendTheme({
  fonts: { body: userFontPreference },
  colors: { brand: userColorPalette },
});

return <ChakraProvider theme={theme}>{children}</ChakraProvider>;
```

**Pros:** Fully integrated with Chakra; all components respond to the theme.  
**Cons:** Re-renders the entire app tree on theme change (can use `React.memo` to mitigate).

---

## 4. React Context + localStorage

A `SettingsContext` that persists to `localStorage` and syncs on load. This is the standard pattern regardless of which of the above you use for *applying* the theme.

```typescript
const SettingsContext = createContext<Settings>({ font: 'Inter', colorScheme: 'light' });

// In provider:
const [settings, setSettings] = useState(() =>
  JSON.parse(localStorage.getItem('user-settings') ?? '{}')
);
useEffect(() => localStorage.setItem('user-settings', JSON.stringify(settings)), [settings]);
```

---

## 5. Server-Persisted Settings (Most Robust)

Since this project already has `Settings` model and a backend, you can save preferences to the database and load them on login. This means settings follow the user across devices.

Looking at your existing `Settings` model — you'd add `ColorScheme` and `FontFamily` fields, expose a `PUT /api/settings` endpoint, and hydrate the frontend context on app load after auth.

**Pros:** Cross-device persistence, works with your existing auth + EF Core setup.  
**Cons:** Requires a loading state before the app renders with the correct theme (flash of unstyled content risk).

---

## Recommended Combination for This Project

Given the stack:

1. **Chakra's color mode** for dark/light toggle (already built in)
2. **Dynamic `extendTheme`** for font selection (a few curated options like Inter, Georgia, Mono)
3. **`localStorage`** for instant local persistence (no flash on reload)
4. **Backend `Settings` model** to sync on login/logout for cross-device support

This gives you fast local UX with durable server-side persistence, and stays entirely within Chakra's theming system.

Chakra's color mode system is built around two modes (`light`/`dark`), but you can layer **multiple color palettes** on top of it. Here's how the pieces fit together:

## 1. Define Your Color Palettes in the Theme

```typescript
// theme/index.ts
import { extendTheme, type ThemeConfig } from '@chakra-ui/react';

const config: ThemeConfig = {
  initialColorMode: 'light',
  useSystemColorMode: false,
};

const theme = extendTheme({
  config,
  colors: {
    // Chakra expects a scale of 50–900 for each palette
    ocean: {
      50: '#e0f4ff',
      100: '#b3e0ff',
      500: '#0077cc',
      900: '#003366',
    },
    forest: {
      50: '#e6f4ea',
      100: '#c3e6cb',
      500: '#2e7d32',
      900: '#1b5e20',
    },
    sunset: {
      50: '#fff3e0',
      100: '#ffe0b2',
      500: '#ef6c00',
      900: '#bf360c',
    },
  },
});
```

---

## 2. Semantic Tokens — The Key Concept

Semantic tokens let you map a *named token* (e.g. `brand.primary`) to different values depending on the color mode. This is how you make components automatically adapt without conditional logic everywhere.

```typescript
const theme = extendTheme({
  // ...
  semanticTokens: {
    colors: {
      'brand.primary': {
        default: 'ocean.500',   // light mode
        _dark: 'ocean.200',     // dark mode
      },
      'brand.bg': {
        default: 'ocean.50',
        _dark: 'gray.800',
      },
      'brand.text': {
        default: 'gray.800',
        _dark: 'gray.100',
      },
    },
  },
});
```

Then in components you just use `color="brand.primary"` and Chakra handles mode-switching automatically.

---

## 3. Swapping the Active Palette

Since you want *multiple* color schemes (not just dark/light), you combine Chakra's color mode with a second piece of state — the active palette name. Store this in a context:

```typescript
// context/ThemeContext.tsx
type Palette = 'ocean' | 'forest' | 'sunset';

const ThemeContext = createContext<{
  palette: Palette;
  setPalette: (p: Palette) => void;
}>({ palette: 'ocean', setPalette: () => {} });

export const ThemeProvider = ({ children }: { children: React.ReactNode }) => {
  const [palette, setPalette] = useState<Palette>(
    () => (localStorage.getItem('palette') as Palette) ?? 'ocean'
  );

  // Rebuild the theme whenever palette changes
  const theme = useMemo(() => buildTheme(palette), [palette]);

  const handleSetPalette = (p: Palette) => {
    setPalette(p);
    localStorage.setItem('palette', p);
  };

  return (
    <ThemeContext.Provider value={{ palette, setPalette: handleSetPalette }}>
      <ChakraProvider theme={theme}>{children}</ChakraProvider>
    </ThemeContext.Provider>
  );
};
```

```typescript
// theme/buildTheme.ts
const paletteMap = {
  ocean: { primary: 'ocean', accent: '#0077cc' },
  forest: { primary: 'forest', accent: '#2e7d32' },
  sunset: { primary: 'sunset', accent: '#ef6c00' },
};

export const buildTheme = (palette: Palette) =>
  extendTheme({
    config,
    colors: { /* full color scales */ },
    semanticTokens: {
      colors: {
        'brand.primary': {
          default: `${paletteMap[palette].primary}.500`,
          _dark: `${paletteMap[palette].primary}.200`,
        },
        'brand.bg': {
          default: `${paletteMap[palette].primary}.50`,
          _dark: 'gray.900',
        },
      },
    },
  });
```

---

## 4. The Settings UI

```typescript
// components/ThemeSettings.tsx
const ThemeSettings = () => {
  const { colorMode, toggleColorMode } = useColorMode();
  const { palette, setPalette } = useContext(ThemeContext);

  return (
    <Stack>
      {/* Dark / Light toggle */}
      <FormLabel>Mode</FormLabel>
      <Switch isChecked={colorMode === 'dark'} onChange={toggleColorMode}>
        Dark mode
      </Switch>

      {/* Palette picker */}
      <FormLabel>Color scheme</FormLabel>
      <HStack>
        {(['ocean', 'forest', 'sunset'] as const).map((p) => (
          <Box
            key={p}
            w={8} h={8} borderRadius="full"
            bg={`${p}.500`}
            border={palette === p ? '3px solid' : 'none'}
            borderColor="white"
            cursor="pointer"
            onClick={() => setPalette(p)}
          />
        ))}
      </HStack>
    </Stack>
  );
};
```

---

## How It All Fits Together

```
User picks "forest" palette + "dark" mode
        ↓
ThemeContext rebuilds theme with forest semantic tokens
        ↓
Chakra's _dark variant activates on all tokens
        ↓
Every component using brand.* tokens updates automatically
        ↓
localStorage saves both preferences across sessions
```

The key insight is: **Chakra's `colorMode` handles light/dark within a palette, and your own context handles which palette is active.** Semantic tokens are what tie the two axes together so component code stays clean.

Simpler — by a meaningful margin. Here's why:

## Font Switching vs. Color Switching

**Color switching** requires:
- A full color scale (50–900) per palette
- Semantic tokens mapping each color role to light *and* dark variants
- Two axes of state (palette × mode) that interact
- Rebuilding the theme object when palette changes

**Font switching** requires:
- A list of font family strings
- One field in the theme (`fonts.body` and optionally `fonts.heading`)
- One axis of state

```typescript
// That's basically the entire implementation
const fonts = {
  sans: `'Inter', sans-serif`,
  serif: `'Georgia', serif`,
  mono: `'JetBrains Mono', monospace`,
};

const theme = extendTheme({
  fonts: {
    body: fonts[userFontPreference],
    heading: fonts[userFontPreference],
  },
});
```

The context and localStorage pattern is identical to what you'd already write for colors — so if you build the color system first, font switching is just one more field on the same settings object and one more property in `buildTheme`.

## The Only Wrinkle

If you want **web fonts** (Google Fonts, etc.) rather than system fonts, you need to load the font files. Two options:

- Add `<link>` tags in `index.html` for all possible fonts upfront (simple, slight perf cost)
- Dynamically inject `<link>` tags when the user selects a font (slightly more complex, loads on demand)

System fonts (Inter is often already available, Georgia and monospace are always available) require zero loading logic.

## Summary

| Concern | Colors | Fonts |
|---|---|---|
| State axes | 2 (palette + dark/light) | 1 |
| Theme fields affected | Many (`colors`, semantic tokens) | 1–2 (`fonts.body`, `fonts.heading`) |
| Asset loading | None | Only for web fonts |
| Relative complexity | Baseline | ~30% of the color work |

If you're building both, do colors first — fonts will slot right in as a small addition.


Good, I have a clear picture. There are two important things to note before giving recommendations:

**1. Chakra UI isn't wired up yet.** There's no `ChakraProvider` in `main.tsx` or `App.tsx`, so that's step zero before any theming works.

**2. The `Settings` model already has a `font` field**, and you have a great pattern to follow in `AuthProvider` for building a `SettingsProvider`.

---

## MVP Recommendation: Font Switching in 4 Steps

### Step 1 — Add ChakraProvider to `main.tsx`

Wrap the app at the root. Without this, no Chakra theming works at all.

```tsx
// main.tsx
import { ChakraProvider } from '@chakra-ui/react';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <BrowserRouter>
      <ChakraProvider>
        <App />
      </ChakraProvider>
    </BrowserRouter>
  </React.StrictMode>
);
```

---

### Step 2 — Create a `SettingsProvider` (modeled on your existing `AuthProvider`)

Your `AuthProvider` is the right pattern — context + localStorage. Font follows the same shape:

```tsx
// components/SettingsProvider.tsx
type FontChoice = 'sans' | 'serif' | 'mono';

const fontMap: Record<FontChoice, string> = {
  sans: `'Inter', sans-serif`,
  serif: `'Georgia', serif`,
  mono: `'JetBrains Mono', monospace`,
};

export default function SettingsProvider({ children }: { children: React.ReactNode }) {
  const [font, setFont] = useState<FontChoice>(
    () => (localStorage.getItem('font') as FontChoice) ?? 'sans'
  );

  const theme = useMemo(() =>
    extendTheme({ fonts: { body: fontMap[font], heading: fontMap[font] } }),
    [font]
  );

  const handleSetFont = (f: FontChoice) => {
    setFont(f);
    localStorage.setItem('font', f);
  };

  return (
    <SettingsContext.Provider value={{ font, setFont: handleSetFont }}>
      <ChakraProvider theme={theme}>{children}</ChakraProvider>
    </SettingsContext.Provider>
  );
}
```

This replaces the bare `ChakraProvider` from Step 1 — `SettingsProvider` owns it.

---

### Step 3 — Place `SettingsProvider` in `main.tsx`

```tsx
<BrowserRouter>
  <SettingsProvider>   {/* owns ChakraProvider + theme */}
    <App />
  </SettingsProvider>
</BrowserRouter>
```

---

### Step 4 — A simple picker UI wherever settings live

```tsx
const { font, setFont } = useContext(SettingsContext);

<Select value={font} onChange={(e) => setFont(e.target.value as FontChoice)}>
  <option value="sans">Sans-serif (Inter)</option>
  <option value="serif">Serif (Georgia)</option>
  <option value="mono">Monospace</option>
</Select>
```

---

## What to Skip for MVP

- **Backend sync** — `localStorage` is enough for now; the `Settings` model and `font` field are already there for when you want to persist cross-device later
- **Google Fonts / web font loading** — use system fonts (Georgia, monospace) + Inter if it's already in your CSS; zero loading complexity
- **Color scheme** — that can come after fonts are working

The whole font feature is roughly ~50 lines of new code across 2 files, and it slots naturally into your existing `AuthProvider` pattern.