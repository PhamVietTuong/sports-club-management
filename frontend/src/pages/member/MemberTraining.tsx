import { useEffect, useState } from 'react'
import { api } from '../../api/client'
import type { Attendance, LessonPlan, ProgressNote } from '../../api/types'
import Pagination from '../../components/Pagination'
import { usePaged } from '../../hooks/usePaged'
import { useClientPaged } from '../../hooks/useClientPaged'

const ATT_LABEL: Record<string, string> = { PRESENT: 'Có mặt', ABSENT: 'Vắng', LATE: 'Muộn' }
const ATT_BADGE: Record<string, string> = {
  PRESENT: 'badge-active', ABSENT: 'badge-terminated', LATE: 'badge-review',
}

export default function MemberTraining() {
  const {
    items: plans, total: plansTotal, page: plansPage, pageSize: plansPageSize,
    setPage: setPlansPage, search: plansSearch, setSearch: setPlansSearch, error,
  } = usePaged<LessonPlan>('/member/lesson-plans')
  const {
    items: notes, total: notesTotal, page: notesPage, pageSize: notesPageSize,
    setPage: setNotesPage, search: notesSearch, setSearch: setNotesSearch,
  } = usePaged<ProgressNote>('/member/progress')
  const [attendance, setAttendance] = useState<Attendance[]>([])
  const {
    pageItems: pageAttendance, total: attTotal, page: attPage, pageSize: attPageSize,
    setPage: setAttPage, search: attSearch, setSearch: setAttSearch,
  } = useClientPaged(attendance, (a, q) =>
    a.className.toLowerCase().includes(q) || a.status.toLowerCase().includes(q))

  useEffect(() => {
    api.get<Attendance[]>('/member/attendance').then((res) => setAttendance(res.data)).catch(() => {})
  }, [])

  return (
    <>
      <div className="page-header"><h1>Giáo án & Tiến độ</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="card">
        <div className="card-title">Giáo án từ huấn luyện viên</div>
        <div className="table-toolbar">
          <input className="form-control search-input" placeholder="Tìm theo tiêu đề / lớp…"
            value={plansSearch} onChange={(e) => setPlansSearch(e.target.value)} />
        </div>
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
        <Pagination page={plansPage} pageSize={plansPageSize} total={plansTotal} onPage={setPlansPage} />
      </div>

      <div className="card mt-4">
        <div className="card-title">Đánh giá tiến độ của tôi</div>
        <div className="table-toolbar">
          <input className="form-control search-input" placeholder="Tìm theo nội dung…"
            value={notesSearch} onChange={(e) => setNotesSearch(e.target.value)} />
        </div>
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
        <Pagination page={notesPage} pageSize={notesPageSize} total={notesTotal} onPage={setNotesPage} />
      </div>

      <div className="card mt-4">
        <div className="card-title">Lịch sử điểm danh</div>
        <div className="table-toolbar">
          <input className="form-control search-input" placeholder="Tìm theo lớp / trạng thái…"
            value={attSearch} onChange={(e) => setAttSearch(e.target.value)} />
        </div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Lớp</th><th>Ngày</th><th>Trạng thái</th></tr></thead>
            <tbody>
              {pageAttendance.map((a) => (
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
              {pageAttendance.length === 0 && <tr><td colSpan={3} className="empty">Chưa có lịch sử điểm danh.</td></tr>}
            </tbody>
          </table>
        </div>
        <Pagination page={attPage} pageSize={attPageSize} total={attTotal} onPage={setAttPage} />
      </div>
    </>
  )
}
