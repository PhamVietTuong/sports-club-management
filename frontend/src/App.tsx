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
import Equipment from './pages/admin/Equipment'
import Payments from './pages/admin/Payments'
import CoachDashboard from './pages/coach/CoachDashboard'
import CoachClasses from './pages/coach/CoachClasses'
import CoachAttendance from './pages/coach/CoachAttendance'
import CoachLessonPlans from './pages/coach/CoachLessonPlans'
import CoachProgress from './pages/coach/CoachProgress'
import CoachPt from './pages/coach/CoachPt'
import CoachAvailableClasses from './pages/coach/CoachAvailableClasses'
import CoachRatings from './pages/coach/CoachRatings'
import MemberDashboard from './pages/member/MemberDashboard'
import MemberClasses from './pages/member/MemberClasses'
import MemberProfile from './pages/member/MemberProfile'
import Membership from './pages/member/Membership'
import MemberTraining from './pages/member/MemberTraining'
import MemberCoaches from './pages/member/MemberCoaches'
import MemberHealth from './pages/member/MemberHealth'
import Chat from './pages/Chat'

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
          <Route path="/admin/equipment" element={<Equipment />} />
          <Route path="/admin/payments" element={<Payments />} />
        </Route>
      </Route>

      {/* COACH */}
      <Route element={<ProtectedRoute role="COACH" />}>
        <Route element={<Layout />}>
          <Route path="/coach/dashboard" element={<CoachDashboard />} />
          <Route path="/coach/classes" element={<CoachClasses />} />
          <Route path="/coach/available-classes" element={<CoachAvailableClasses />} />
          <Route path="/coach/attendance" element={<CoachAttendance />} />
          <Route path="/coach/lesson-plans" element={<CoachLessonPlans />} />
          <Route path="/coach/progress" element={<CoachProgress />} />
          <Route path="/coach/pt" element={<CoachPt />} />
          <Route path="/coach/ratings" element={<CoachRatings />} />
          <Route path="/coach/chat" element={<Chat />} />
        </Route>
      </Route>

      {/* MEMBER */}
      <Route element={<ProtectedRoute role="MEMBER" />}>
        <Route element={<Layout />}>
          <Route path="/member/dashboard" element={<MemberDashboard />} />
          <Route path="/member/classes" element={<MemberClasses />} />
          <Route path="/member/membership" element={<Membership />} />
          <Route path="/member/coaches" element={<MemberCoaches />} />
          <Route path="/member/training" element={<MemberTraining />} />
          <Route path="/member/health" element={<MemberHealth />} />
          <Route path="/member/chat" element={<Chat />} />
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
