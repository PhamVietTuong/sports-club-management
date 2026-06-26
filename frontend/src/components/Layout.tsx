import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import type { Role } from '../api/types'

interface NavItem {
  to: string
  label: string
}

const NAV: Record<Role, NavItem[]> = {
  ADMIN: [
    { to: '/admin/dashboard', label: 'Bảng điều khiển' },
    { to: '/admin/members', label: 'Thành viên' },
    { to: '/admin/coaches', label: 'Huấn luyện viên' },
    { to: '/admin/classes', label: 'Lớp học' },
    { to: '/admin/schedules', label: 'Lịch tập' },
    { to: '/admin/packages', label: 'Gói tập' },
    { to: '/admin/equipment', label: 'Thiết bị' },
    { to: '/admin/payments', label: 'Thanh toán & Doanh thu' },
  ],
  COACH: [
    { to: '/coach/dashboard', label: 'Bảng điều khiển' },
    { to: '/coach/classes', label: 'Lớp học của tôi' },
    { to: '/coach/available-classes', label: 'Nhận lớp' },
    { to: '/coach/attendance', label: 'Điểm danh' },
    { to: '/coach/lesson-plans', label: 'Giáo án' },
    { to: '/coach/progress', label: 'Đánh giá học viên' },
    { to: '/coach/pt', label: 'Lịch PT' },
    { to: '/coach/ratings', label: 'Đánh giá của tôi' },
    { to: '/coach/chat', label: 'Tin nhắn' },
  ],
  MEMBER: [
    { to: '/member/dashboard', label: 'Bảng điều khiển' },
    { to: '/member/classes', label: 'Đăng ký lớp' },
    { to: '/member/membership', label: 'Membership & Thanh toán' },
    { to: '/member/coaches', label: 'Huấn luyện viên' },
    { to: '/member/training', label: 'Giáo án & Tiến độ' },
    { to: '/member/health', label: 'Theo dõi sức khỏe' },
    { to: '/member/chat', label: 'Tin nhắn' },
    { to: '/member/profile', label: 'Hồ sơ' },
  ],
}

const ROLE_LABEL: Record<Role, string> = {
  ADMIN: 'Quản trị viên',
  COACH: 'Huấn luyện viên',
  MEMBER: 'Thành viên',
}

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  if (!user) return null

  const items = NAV[user.role]

  function handleLogout() {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <span className="sidebar-brand-icon">🏆</span>
          <span>SPORTS CLUB</span>
        </div>
        <nav className="sidebar-nav">
          {items.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => 'nav-link' + (isActive ? ' active' : '')}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
        <button className="btn btn-ghost sidebar-logout" onClick={handleLogout}>
          Đăng xuất
        </button>
      </aside>

      <div className="main-content">
        <header className="top-bar">
          <span className="top-bar-title">{ROLE_LABEL[user.role]}</span>
          <div className="top-bar-user">
            <span>{user.fullName || user.username}</span>
            <div className="user-avatar">
              {(user.fullName || user.username).slice(0, 1).toUpperCase()}
            </div>
          </div>
        </header>
        <div className="page-body">
          <Outlet />
        </div>
      </div>
    </div>
  )
}
