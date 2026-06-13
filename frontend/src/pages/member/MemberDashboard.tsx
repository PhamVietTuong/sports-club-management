import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { MemberDashboard } from '../../api/types'

export default function MemberDashboard() {
  const [data, setData] = useState<MemberDashboard | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get<MemberDashboard>('/member/dashboard')
      .then((res) => setData(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải bảng điều khiển.')))
  }, [])

  if (error) return <div className="alert alert-warning">{error}</div>
  if (!data) return <div className="loading">Đang tải…</div>

  const active = data.enrollments.filter((e) => e.status === 'ACTIVE')

  return (
    <>
      <div className="page-header"><h1>Xin chào, {data.member.fullName}</h1></div>

      <div className="grid-3">
        <div className="stat-card">
          <div className="stat-icon blue">🏋️</div>
          <div className="stat-number">{active.length}</div>
          <div className="stat-label">Lớp đang tham gia</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon green">📅</div>
          <div className="stat-number">{data.schedules.length}</div>
          <div className="stat-label">Buổi tập trong tuần</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon orange">📌</div>
          <div className="stat-number">{data.member.status}</div>
          <div className="stat-label">Trạng thái thành viên</div>
        </div>
      </div>

      <div className="grid-2 mt-4">
        <div className="card">
          <div className="card-title">Lớp đã đăng ký</div>
          <div className="table-wrap">
            <table>
              <thead><tr><th>Lớp</th><th>Ngày đăng ký</th><th>Trạng thái</th></tr></thead>
              <tbody>
                {data.enrollments.map((e) => (
                  <tr key={e.id}>
                    <td>{e.className}</td><td>{e.enrollDate}</td>
                    <td><span className={'badge ' + (e.status === 'ACTIVE' ? 'badge-active' : 'badge-inactive')}>{e.status}</span></td>
                  </tr>
                ))}
                {data.enrollments.length === 0 && <tr><td colSpan={3} className="empty">Chưa đăng ký lớp nào.</td></tr>}
              </tbody>
            </table>
          </div>
        </div>

        <div className="card">
          <div className="card-title">Lịch tập của tôi</div>
          <div className="table-wrap">
            <table>
              <thead><tr><th>Lớp</th><th>Ngày</th><th>Giờ</th><th>Phòng</th></tr></thead>
              <tbody>
                {data.schedules.map((s) => (
                  <tr key={s.id}>
                    <td>{s.className}</td><td>{s.dayOfWeek}</td>
                    <td>{s.startTime}–{s.endTime}</td><td>{s.room || '—'}</td>
                  </tr>
                ))}
                {data.schedules.length === 0 && <tr><td colSpan={4} className="empty">Chưa có lịch.</td></tr>}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </>
  )
}
