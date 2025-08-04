import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import RadarScreen from './RadarScreen.tsx'
import { SignalRProvider } from './SignalR/SignalRContext.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <SignalRProvider>
      <RadarScreen />
    </SignalRProvider>
  </StrictMode>,
)
