import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { client } from './api/generated/client.gen'

// Route all API calls through the Vite dev proxy (/api/* → localhost:5162)
client.setConfig({ baseUrl: '' })

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
