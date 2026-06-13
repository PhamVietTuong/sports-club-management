import { Navigate, Outlet } from 'react-router-dom'
import { useAuth, dashboardPath } from './AuthContext'
import type { Role } from '../api/types'

/**
 * SECURITY (client-side guard) — blocks routes when there is no session or the
 * role does not match. This is UX only; the .NET API enforces authorization on
 * every request via [Authorize(Roles = ...)].
 */
export default function ProtectedRoute({ role }: { role: Role }) {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  if (user.role !== role) return <Navigate to={dashboardPath(user.role)} replace />
  return <Outlet />
}
