import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { ClassChangeRequest, ClassDetail, CoachAvailableClass, Schedule, TrainingClass } from '../../api/types'
import { formatSchedules } from '../../utils/schedule'
import Modal from '../../components/Modal'
import ScheduleDialog from '../../components/ScheduleDialog'
import Pagination from '../../components/Pagination'
import { useClientPaged } from '../../hooks/useClientPaged'

const ACTION_LABEL: Record<string, string> = { CLAIM: 'Nhận lớp', RELEASE: 'Trả lớp' }
const STATUS_LABEL: Record<string, string> = {
  PENDING: 'Chờ duyệt', APPROVED: 'Đã duyệt', REJECTED: 'Bị từ chối',
}
const STATUS_BADGE: Record<string, string> = {
  PENDING: 'badge-review', APPROVED: 'badge-active', REJECTED: 'badge-inactive',
}

export default function CoachAvailableClasses() {
  const [available, setAvailable] = useState<CoachAvailableClass[]>([])
  const [mine, setMine] = useState<TrainingClass[]>([])
  const [requests, setRequests] = useState<ClassChangeRequest[]>([])
  const [detail, setDetail] = useState<CoachAvailableClass | null>(null)
  const [scheduleDialog, setScheduleDialog] = useState<{ title: string; schedules: Schedule[] } | null>(null)
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')

  function load() {
    api.get<CoachAvailableClass[]>('/coach/available-classes')
      .then((res) => setAvailable(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải lớp khả dụng.')))
    api.get<TrainingClass[]>('/coach/classes').then((res) => setMine(res.data)).catch(() => {})
    api.get<ClassChangeRequest[]>('/coach/class-requests').then((res) => setRequests(res.data)).catch(() => {})
  }
  useEffect(load, [])

  const av = useClientPaged(available, (a, q) =>
    a.class.name.toLowerCase().includes(q) || (a.class.level ?? '').toLowerCase().includes(q))
  const my = useClientPaged(mine, (c, q) =>
    c.name.toLowerCase().includes(q) || (c.level ?? '').toLowerCase().includes(q))
  const rq = useClientPaged(requests, (r, q) =>
    r.className.toLowerCase().includes(q) || r.action.toLowerCase().includes(q)
    || r.status.toLowerCase().includes(q))

  // Classes with an unresolved request can't be re-requested.
  const pendingClassIds = new Set(
    requests.filter((r) => r.status === 'PENDING').map((r) => r.classId))

  async function claim(id: number) {
    setError(''); setFlash('')
    try {
      const res = await api.post<{ message: string }>(`/coach/classes/${id}/claim`)
      setFlash(res.data.message); load()
    } catch (err) { setError(errorMessage(err)) }
  }
  async function release(id: number) {
    if (!window.confirm('Gửi yêu cầu trả lại lớp này?')) return
    setError(''); setFlash('')
    try {
      const res = await api.post<{ message: string }>(`/coach/classes/${id}/release`)
      setFlash(res.data.message); load()
    } catch (err) { setError(errorMessage(err)) }
  }

  // "Lớp của tôi" rows come from /coach/classes (no schedule); fetch the detail
  // (IDOR-guarded to this coach) to show the schedule dialog.
  function viewMineSchedule(c: TrainingClass) {
    api.get<ClassDetail>(`/coach/classes/${c.id}`)
      .then((res) => setScheduleDialog({ title: c.name, schedules: res.data.schedules }))
      .catch((err) => setError(errorMessage(err, 'Không thể tải lịch tập.')))
  }

  return (
    <>
      <div className="page-header"><h1>Nhận lớp</h1></div>
      <p className="text-muted">
        Yêu cầu nhận/trả lớp cần được quản trị viên duyệt. Lớp có lịch trùng với thời khóa biểu
        của bạn sẽ không thể gửi yêu cầu nhận.
      </p>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="card">
        <div className="card-title">Lớp chưa có HLV</div>
        <div className="table-toolbar">
          <input className="form-control search-input" placeholder="Tìm theo tên lớp / cấp độ…"
            value={av.search} onChange={(e) => av.setSearch(e.target.value)} />
        </div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Tên lớp</th><th>Cấp độ</th><th>Sĩ số</th><th></th></tr></thead>
            <tbody>
              {av.pageItems.map(({ class: c, schedules }) => (
                <tr key={c.id}>
                  <td>{c.name}</td><td>{c.level || '—'}</td>
                  <td>{c.currentEnrolled}/{c.capacity}</td>
                  <td className="actions">
                    <button className="btn btn-ghost btn-sm"
                      onClick={() => setScheduleDialog({ title: c.name, schedules })}>
                      Lịch tập
                    </button>
                    <button className="btn btn-ghost btn-sm"
                      onClick={() => setDetail(available.find((a) => a.class.id === c.id) ?? null)}>
                      Xem
                    </button>
                    <button className="btn btn-primary btn-sm" disabled={pendingClassIds.has(c.id)}
                      onClick={() => claim(c.id)}>
                      {pendingClassIds.has(c.id) ? 'Chờ duyệt' : 'Yêu cầu nhận'}
                    </button>
                  </td>
                </tr>
              ))}
              {av.pageItems.length === 0 && <tr><td colSpan={5} className="empty">Không có lớp nào đang chờ nhận.</td></tr>}
            </tbody>
          </table>
        </div>
        <Pagination page={av.page} pageSize={av.pageSize} total={av.total} onPage={av.setPage} />
      </div>

      <div className="card mt-4">
        <div className="card-title">Lớp của tôi</div>
        <div className="table-toolbar">
          <input className="form-control search-input" placeholder="Tìm theo tên lớp / cấp độ…"
            value={my.search} onChange={(e) => my.setSearch(e.target.value)} />
        </div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Tên lớp</th><th>Cấp độ</th><th>Sĩ số</th><th></th></tr></thead>
            <tbody>
              {my.pageItems.map((c) => (
                <tr key={c.id}>
                  <td>{c.name}</td><td>{c.level || '—'}</td><td>{c.currentEnrolled}/{c.capacity}</td>
                  <td className="actions">
                    <button className="btn btn-ghost btn-sm" onClick={() => viewMineSchedule(c)}>Lịch tập</button>
                    <button className="btn btn-ghost btn-sm" disabled={pendingClassIds.has(c.id)}
                      onClick={() => release(c.id)}>
                      {pendingClassIds.has(c.id) ? 'Chờ duyệt' : 'Yêu cầu trả lớp'}
                    </button>
                  </td>
                </tr>
              ))}
              {my.pageItems.length === 0 && <tr><td colSpan={4} className="empty">Bạn chưa phụ trách lớp nào.</td></tr>}
            </tbody>
          </table>
        </div>
        <Pagination page={my.page} pageSize={my.pageSize} total={my.total} onPage={my.setPage} />
      </div>

      <div className="card mt-4">
        <div className="card-title">Yêu cầu của tôi</div>
        <div className="table-toolbar">
          <input className="form-control search-input" placeholder="Tìm theo lớp / hành động / trạng thái…"
            value={rq.search} onChange={(e) => rq.setSearch(e.target.value)} />
        </div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Lớp</th><th>Hành động</th><th>Trạng thái</th><th>Ngày yêu cầu</th><th>Ghi chú</th></tr></thead>
            <tbody>
              {rq.pageItems.map((r) => (
                <tr key={r.id}>
                  <td>{r.className}</td>
                  <td>{ACTION_LABEL[r.action] ?? r.action}</td>
                  <td><span className={'badge ' + (STATUS_BADGE[r.status] ?? '')}>
                    {STATUS_LABEL[r.status] ?? r.status}</span></td>
                  <td>{new Date(r.requestedAt).toLocaleString('vi-VN')}</td>
                  <td>{r.note || '—'}</td>
                </tr>
              ))}
              {rq.pageItems.length === 0 && <tr><td colSpan={5} className="empty">Chưa có yêu cầu nào.</td></tr>}
            </tbody>
          </table>
        </div>
        <Pagination page={rq.page} pageSize={rq.pageSize} total={rq.total} onPage={rq.setPage} />
      </div>

      <Modal open={detail !== null} title={`Chi tiết lớp: ${detail?.class.name ?? ''}`}
        onClose={() => setDetail(null)}>
        {detail && (
          <>
            <p className="text-muted">Cấp độ: <strong>{detail.class.level || '—'}</strong></p>
            <p className="text-muted">Lịch học: <strong>{formatSchedules(detail.schedules)}</strong></p>
            <p className="text-muted">
              Sĩ số: <strong>{detail.class.currentEnrolled}/{detail.class.capacity}</strong>
            </p>
            <div className="card-title" style={{ marginTop: 12 }}>
              Thành viên đã đăng ký ({detail.enrolledMembers.length})
            </div>
            <div className="table-wrap">
              <table>
                <thead><tr><th>Thành viên</th><th>Ngày đăng ký</th></tr></thead>
                <tbody>
                  {detail.enrolledMembers.map((e) => (
                    <tr key={e.id}><td>{e.memberName}</td><td>{e.enrollDate}</td></tr>
                  ))}
                  {detail.enrolledMembers.length === 0 &&
                    <tr><td colSpan={2} className="empty">Chưa có thành viên.</td></tr>}
                </tbody>
              </table>
            </div>
          </>
        )}
      </Modal>

      <ScheduleDialog open={scheduleDialog !== null} title={scheduleDialog?.title ?? ''}
        schedules={scheduleDialog?.schedules ?? []} onClose={() => setScheduleDialog(null)} />
    </>
  )
}
