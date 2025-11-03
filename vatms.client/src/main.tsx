import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import RadarScreen from './RadarScreen.tsx'
import { SignalRProvider } from './SignalR/SignalRContext.tsx'
import SelectionWindow from './iFACTS/SelectionWindow.tsx';
import MainMenu from './MainMenu.tsx';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Router>
      <Routes>
        <Route path="/" element={
          <MainMenu />
        } />
        <Route path="/radar-screen" element={
          <SignalRProvider>
            <RadarScreen />
          </SignalRProvider>
        } />
        <Route path="/ifacts-selection-window" element={
          <SignalRProvider>
            <SelectionWindow />
          </SignalRProvider>
        } />
      </Routes>
    </Router>
  </StrictMode>
)
