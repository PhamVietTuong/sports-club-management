import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { CoachDashboard } from '../../api/types'

export default function CoachDashboard() {
  const [data, setData] = useState<CoachDashboard | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get<CoachDashboard>('/coach/dashboard')
      .then((res) => setData(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải bảng điều khiển.')))
  }, [])

  if (error) return <div className="alert alert-warning">{error}</div>
  if (!data) return <div className="loading">Đang tải…</div>

  return (
    <>
      <div className="page-header"><h1>Xin chào, {data.coach.fullName}</h1></div>

      <div className="grid-3">
        <div className="stat-card">
          <div className="stat-icon blue">📚</div>
          <div className="stat-number">{data.classes.length}</div>
          <div className="stat-label">Lớp đang phụ trách</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon green">📅</div>
          <div className="stat-number">{data.schedules.length}</div>
          <div className="stat-label">Buổi tập trong tuần</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon orange">⭐</div>
          <div className="stat-number">{data.coach.experience}</div>
          <div className="stat-label">Năm kinh nghiệm</div>
        </div>
      </div>

      <div className="card mt-4">
        <div className="card-title">Lịch tập của tôi</div>
        <div className="table-wrap">
          <table>
            <thead>
              <tr><th>Lớp</th><th>Ngày</th><th>Bắt đầu</th><th>Kết thúc</th><th>Phòng</th></tr>
            </thead>
            <tbody>
              {data.schedules.map((s) => (
                <tr key={s.id}>
                  <td>{s.className}</td><td>{s.dayOfWeek}</td>
                  <td>{s.startTime}</td><td>{s.endTime}</td><td>{s.room || '—'}</td>
                </tr>
              ))}
              {data.schedules.length === 0 && <tr><td colSpan={5} className="empty">Chưa có lịch.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}
