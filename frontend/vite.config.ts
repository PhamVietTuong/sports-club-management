import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The dev server proxies /api to the .NET backend so the SPA can call the API
// with same-origin relative URLs during development.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5097',
        changeOrigin: true,
      },
      // SignalR chat hub — needs the WebSocket upgrade proxied to the backend.
      '/hubs': {
        target: 'http://localhost:5097',
        changeOrigin: true,
        ws: true,
      },
    },
  },
})
