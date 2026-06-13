import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth, dashboardPath } from './auth/AuthContext'
import ProtectedRoute from './auth/ProtectedRoute'
import Login from './pages/Login'
import Register from './pages/Register'
import Layout from './components/Layout'
import AdminDashboard from './pages/admin/AdminDashboard'
import Members from './pages/admin/Members'
import Coaches from './pages/admin/Coaches'
import Classes from './pages/admin/Classes'
import Packages from './pages/admin/Packages'
import Schedules from './pages/admin/Schedules'
import CoachDashboard from './pages/coach/CoachDashboard'
import CoachClasses from './pages/coach/CoachClasses'
import MemberDashboard from './pages/member/MemberDashboard'
import MemberClasses from './pages/member/MemberClasses'
import MemberProfile from './pages/member/MemberProfile'

export default function App() {
  const { user } = useAuth()

  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      {/* ADMIN */}
      <Route element={<ProtectedRoute role="ADMIN" />}>
        <Route element={<Layout />}>
          <Route path="/admin/dashboard" element={<AdminDashboard />} />
          <Route path="/admin/members" element={<Members />} />
          <Route path="/admin/coaches" element={<Coaches />} />
          <Route path="/admin/classes" element={<Classes />} />
          <Route path="/admin/packages" element={<Packages />} />
          <Route path="/admin/schedules" element={<Schedules />} />
        </Route>
      </Route>

      {/* COACH */}
      <Route element={<ProtectedRoute role="COACH" />}>
        <Route element={<Layout />}>
          <Route path="/coach/dashboard" element={<CoachDashboard />} />
          <Route path="/coach/classes" element={<CoachClasses />} />
        </Route>
      </Route>

      {/* MEMBER */}
      <Route element={<ProtectedRoute role="MEMBER" />}>
        <Route element={<Layout />}>
          <Route path="/member/dashboard" element={<MemberDashboard />} />
          <Route path="/member/classes" element={<MemberClasses />} />
          <Route path="/member/profile" element={<MemberProfile />} />
        </Route>
      </Route>

      <Route
        path="*"
        element={<Navigate to={user ? dashboardPath(user.role) : '/login'} replace />}
      />
    </Routes>
  )
}
