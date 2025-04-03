import {StrictMode} from 'react'
import {createRoot} from 'react-dom/client'
import App from "./components/App.tsx";
import {Toaster} from "react-hot-toast";
import './style.css';

createRoot(document.getElementById('root')!).render(
  <>
          <App />
      <Toaster />
  </>,
)
