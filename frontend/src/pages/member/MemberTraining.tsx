import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { Attendance, LessonPlan, ProgressNote } from '../../api/types'

const ATT_LABEL: Record<string, string> = { PRESENT: 'Có mặt', ABSENT: 'Vắng', LATE: 'Muộn' }
const ATT_BADGE: Record<string, string> = {
  PRESENT: 'badge-active', ABSENT: 'badge-terminated', LATE: 'badge-review',
}

export default function MemberTraining() {
  const [plans, setPlans] = useState<LessonPlan[]>([])
  const [notes, setNotes] = useState<ProgressNote[]>([])
  const [attendance, setAttendance] = useState<Attendance[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    api.get<LessonPlan[]>('/member/lesson-plans')
      .then((res) => setPlans(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải dữ liệu tập luyện.')))
    api.get<ProgressNote[]>('/member/progress').then((res) => setNotes(res.data)).catch(() => {})
    api.get<Attendance[]>('/member/attendance').then((res) => setAttendance(res.data)).catch(() => {})
  }, [])

  return (
    <>
      <div className="page-header"><h1>Giáo án & Tiến độ</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="card">
        <div className="card-title">Giáo án từ huấn luyện viên</div>
        {plans.length === 0 ? (
          <p className="text-muted">Chưa có giáo án cho lớp bạn đang theo học.</p>
        ) : (
          plans.map((p) => (
            <div key={p.id} style={{ marginBottom: 16 }}>
              <strong>{p.title}</strong> <span className="text-muted">— {p.className}</span>
              <p className="text-muted" style={{ whiteSpace: 'pre-wrap', marginTop: 4 }}>{p.content || '—'}</p>
            </div>
          ))
        )}
      </div>

      <div className="card mt-4">
        <div className="card-title">Đánh giá tiến độ của tôi</div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Nội dung</th><th>Điểm</th><th>Ngày</th></tr></thead>
            <tbody>
              {notes.map((n) => (
                <tr key={n.id}>
                  <td style={{ maxWidth: 420, whiteSpace: 'pre-wrap' }}>{n.note}</td>
                  <td>{n.rating ? `${n.rating}/5` : '—'}</td>
                  <td>{new Date(n.recordedAt).toLocaleDateString('vi-VN')}</td>
                </tr>
              ))}
              {notes.length === 0 && <tr><td colSpan={3} className="empty">Chưa có đánh giá.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card mt-4">
        <div className="card-title">Lịch sử điểm danh</div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Lớp</th><th>Ngày</th><th>Trạng thái</th></tr></thead>
            <tbody>
              {attendance.map((a) => (
                <tr key={a.id}>
                  <td>{a.className}</td>
                  <td>{a.sessionDate}</td>
                  <td>
                    <span className={'badge ' + (ATT_BADGE[a.status] ?? '')}>
                      {ATT_LABEL[a.status] ?? a.status}
                    </span>
                  </td>
                </tr>
              ))}
              {attendance.length === 0 && <tr><td colSpan={3} className="empty">Chưa có lịch sử điểm danh.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}
