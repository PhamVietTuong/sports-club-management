import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, errorMessage } from '../../api/client'
import type { AdminStats } from '../../api/types'

export default function AdminDashboard() {
  const [stats, setStats] = useState<AdminStats | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api
      .get<AdminStats>('/admin/dashboard')
      .then((res) => setStats(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải dữ liệu bảng điều khiển.')))
  }, [])

  return (
    <>
      <div className="page-header">
        <h1>Bảng điều khiển</h1>
      </div>
      {error && <div className="alert alert-warning">{error}</div>}

      <div className="grid-3">
        <div className="stat-card">
          <div className="stat-icon blue">👥</div>
          <div className="stat-number">{stats?.totalMembers ?? '—'}</div>
          <div className="stat-label">Tổng số thành viên</div>
          <Link to="/admin/members">Quản lý →</Link>
        </div>
        <div className="stat-card">
          <div className="stat-icon green">⭐</div>
          <div className="stat-number">{stats?.totalCoaches ?? '—'}</div>
          <div className="stat-label">Tổng số huấn luyện viên</div>
          <Link to="/admin/coaches">Quản lý →</Link>
        </div>
        <div className="stat-card">
          <div className="stat-icon orange">📅</div>
          <div className="stat-number">{stats?.totalClasses ?? '—'}</div>
          <div className="stat-label">Lớp học đang hoạt động</div>
          <Link to="/admin/classes">Quản lý →</Link>
        </div>
      </div>

      <div className="grid-2 mt-4">
        <div className="card">
          <div className="card-title">Thao tác nhanh</div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <Link className="btn btn-ghost" to="/admin/schedules">Quản lý lịch tập</Link>
            <Link className="btn btn-ghost" to="/admin/packages">Quản lý gói tập</Link>
          </div>
        </div>
        <div className="card">
          <div className="card-title">Tổng quan</div>
          <p className="text-muted">
            Quản lý thành viên, huấn luyện viên và các lớp tập luyện của câu lạc bộ từ bảng điều
            khiển này. Sử dụng thanh bên để di chuyển giữa các mục.
          </p>
        </div>
      </div>
    </>
  )
}
