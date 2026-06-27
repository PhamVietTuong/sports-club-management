import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { AttendanceRoster, TrainingClass } from '../../api/types'
import { formatSchedules, isScheduledDay } from '../../utils/schedule'
import Pagination from '../../components/Pagination'
import { useClientPaged } from '../../hooks/useClientPaged'

const STATUSES: { value: string; label: string }[] = [
  { value: 'PRESENT', label: 'Có mặt' },
  { value: 'ABSENT', label: 'Vắng' },
  { value: 'LATE', label: 'Muộn' },
]
const STATUS_BADGE: Record<string, string> = {
  PRESENT: 'badge-active', ABSENT: 'badge-terminated', LATE: 'badge-review',
}

function today() { return new Date().toISOString().slice(0, 10) }

export default function CoachAttendance() {
  const [classes, setClasses] = useState<TrainingClass[]>([])
  const [classId, setClassId] = useState<number>(0)
  const [date, setDate] = useState(today())
  const [roster, setRoster] = useState<AttendanceRoster | null>(null)
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')

  const {
    pageItems: pageRoster, total, page, pageSize, setPage, search, setSearch,
  } = useClientPaged(roster?.roster ?? [], (r, q) => r.memberName.toLowerCase().includes(q))

  useEffect(() => {
    api.get<TrainingClass[]>('/coach/classes')
      .then((res) => {
        setClasses(res.data)
        if (res.data.length > 0) setClassId(res.data[0].id)
      })
      .catch((err) => setError(errorMessage(err, 'Không thể tải lớp học.')))
  }, [])

  function loadRoster() {
    if (!classId) return
    setError('')
    // IDOR enforced server-side: another coach's class returns 403.
    api.get<AttendanceRoster>(`/coach/classes/${classId}/attendance?date=${date}`)
      .then((res) => setRoster(res.data))
      .catch((err) => { setRoster(null); setError(errorMessage(err, 'Không thể tải điểm danh.')) })
  }
  useEffect(loadRoster, [classId, date])

  async function mark(memberId: number, status: string) {
    setFlash('')
    try {
      await api.post(`/coach/classes/${classId}/attendance`, { memberId, sessionDate: date, status })
      setFlash('Đã điểm danh.')
      loadRoster()
    } catch (err) { setError(errorMessage(err)) }
  }

  return (
    <>
      <div className="page-header"><h1>Điểm danh</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="card">
        <div className="toolbar">
          <div className="form-group" style={{ marginBottom: 0 }}>
            <label className="form-label">Lớp học</label>
            <select className="form-control" value={classId}
              onChange={(e) => setClassId(Number(e.target.value))}>
              {classes.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <div className="form-group" style={{ marginBottom: 0 }}>
            <label className="form-label">Ngày</label>
            <input className="form-control" type="date" value={date}
              onChange={(e) => setDate(e.target.value)} />
          </div>
        </div>
        {roster && (
          <p className="text-muted" style={{ marginTop: 12, marginBottom: 0 }}>
            Lịch cố định của lớp: <strong>{formatSchedules(roster.schedules)}</strong>
          </p>
        )}
      </div>

      {roster && !isScheduledDay(date, roster.schedules) && (
        <div className="alert alert-warning mt-4">
          Ngày bạn chọn không nằm trong lịch cố định của lớp. Bạn vẫn có thể điểm danh cho buổi bù,
          nhưng hãy kiểm tra lại ngày.
        </div>
      )}

      {roster && roster.roster.length > 0 && (
        <div className="table-toolbar mt-4">
          <input className="form-control search-input" placeholder="Tìm theo tên thành viên…"
            value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
      )}

      <div className="table-wrap mt-4">
        <table>
          <thead><tr><th>Thành viên</th><th>Trạng thái</th><th>Check-in</th><th>Điểm danh</th></tr></thead>
          <tbody>
            {pageRoster.map((r) => (
              <tr key={r.memberId}>
                <td>{r.memberName}</td>
                <td>
                  {r.status
                    ? <span className={'badge ' + (STATUS_BADGE[r.status] ?? '')}>
                        {STATUSES.find((s) => s.value === r.status)?.label ?? r.status}
                      </span>
                    : <span className="text-muted">Chưa điểm danh</span>}
                </td>
                <td>{r.checkedInAt ? new Date(r.checkedInAt).toLocaleTimeString('vi-VN') : '—'}</td>
                <td className="actions">
                  {STATUSES.map((s) => (
                    <button key={s.value} className="btn btn-ghost btn-sm"
                      onClick={() => mark(r.memberId, s.value)}>{s.label}</button>
                  ))}
                </td>
              </tr>
            ))}
            {roster && pageRoster.length === 0 &&
              <tr><td colSpan={4} className="empty">Lớp chưa có thành viên đăng ký.</td></tr>}
            {!roster && <tr><td colSpan={4} className="empty">Chọn lớp để điểm danh.</td></tr>}
          </tbody>
        </table>
      </div>
      {roster && <Pagination page={page} pageSize={pageSize} total={total} onPage={setPage} />}
    </>
  )
}
