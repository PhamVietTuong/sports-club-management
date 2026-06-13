import axios from 'axios'

const TOKEN_KEY = 'scm_token'

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
}

export const api = axios.create({
  baseURL: (import.meta.env.VITE_API_BASE_URL || '') + '/api',
})

// SECURITY — attach the JWT bearer token to every request. The token lives in
// localStorage and is sent via the Authorization header (not a cookie), so
// classic CSRF does not apply.
api.interceptors.request.use((config) => {
  const token = tokenStore.get()
  if (token) {
    config.headers = config.headers ?? {}
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// A handler the AuthProvider registers so a 401 can drive the app's own auth
// state + router navigation instead of a jarring full-page reload.
let onUnauthorized: (() => void) | null = null
export function setUnauthorizedHandler(handler: (() => void) | null) {
  onUnauthorized = handler
}

// On 401 (expired/invalid token) clear it and hand off to the AuthProvider.
api.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      tokenStore.clear()
      if (onUnauthorized) {
        onUnauthorized()
      } else if (!location.pathname.startsWith('/login')) {
        // Fallback only if the app shell hasn't mounted yet.
        location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)

/** Pulls a human-readable message out of an axios error, falling back to a default. */
export function errorMessage(err: unknown, fallback = 'Đã xảy ra lỗi. Vui lòng thử lại.'): string {
  if (axios.isAxiosError(err)) {
    return err.response?.data?.message ?? fallback
  }
  return fallback
}
