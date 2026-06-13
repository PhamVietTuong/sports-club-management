import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, tokenStore, setUnauthorizedHandler } from '../api/client'
import type { AuthResponse, Role } from '../api/types'

interface AuthUser {
  userId: number
  username: string
  role: Role
  fullName: string
}

interface AuthContextValue {
  user: AuthUser | null
  loading: boolean
  login: (username: string, password: string) => Promise<AuthUser>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

const USER_KEY = 'scm_user'

function loadStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw || !tokenStore.get()) return null
  try {
    return JSON.parse(raw) as AuthUser
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(loadStoredUser)
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()

  // Let a 401 (expired/invalid token) drive a clean logout + route change,
  // instead of the client doing a full-page reload.
  useEffect(() => {
    setUnauthorizedHandler(() => {
      localStorage.removeItem(USER_KEY)
      setUser(null)
      navigate('/login', { replace: true })
    })
    return () => setUnauthorizedHandler(null)
  }, [navigate])

  // Verify a stored token is still valid on first load; drop it if not.
  useEffect(() => {
    if (!user) return
    api.get('/auth/me').catch(() => logout())
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function login(username: string, password: string): Promise<AuthUser> {
    setLoading(true)
    try {
      const { data } = await api.post<AuthResponse>('/auth/login', { username, password })
      tokenStore.set(data.token)
      const authUser: AuthUser = {
        userId: data.userId,
        username: data.username,
        role: data.role,
        fullName: data.fullName,
      }
      localStorage.setItem(USER_KEY, JSON.stringify(authUser))
      setUser(authUser)
      return authUser
    } finally {
      setLoading(false)
    }
  }

  function logout() {
    tokenStore.clear()
    localStorage.removeItem(USER_KEY)
    setUser(null)
  }

  const value = useMemo(() => ({ user, loading, login, logout }), [user, loading])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}

export function dashboardPath(role: Role): string {
  switch (role) {
    case 'ADMIN':
      return '/admin/dashboard'
    case 'COACH':
      return '/coach/dashboard'
    case 'MEMBER':
      return '/member/dashboard'
  }
}
