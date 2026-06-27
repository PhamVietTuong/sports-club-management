import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { ClassDetail, Schedule, TrainingClass } from '../../api/types'
import ScheduleDialog from '../../components/ScheduleDialog'
import Pagination from '../../components/Pagination'
import { useClientPaged } from '../../hooks/useClientPaged'

export default function CoachClasses() {
  const [classes, setClasses] = useState<TrainingClass[]>([])
  const [detail, setDetail] = useState<ClassDetail | null>(null)
  const [scheduleDialog, setScheduleDialog] = useState<{ title: string; schedules: Schedule[] } | null>(null)
  const [error, setError] = useState('')
  const [detailError, setDetailError] = useState('')

  const {
    pageItems: pageClasses, total, page, pageSize, setPage, search, setSearch,
  } = useClientPaged(classes, (c, q) =>
    c.name.toLowerCase().includes(q) || (c.level ?? '').toLowerCase().includes(q))

  const {
    pageItems: pageMembers, total: memTotal, page: memPage, pageSize: memPageSize,
    setPage: setMemPage, search: memSearch, setSearch: setMemSearch,
  } = useClientPaged(detail?.enrolledMembers ?? [], (e, q) => e.memberName.toLowerCase().includes(q))

  useEffect(() => {
    api.get<TrainingClass[]>('/coach/classes')
      .then((res) => setClasses(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải lớp học.')))
  }, [])

  function viewClass(id: number) {
    setDetailError('')
    setDetail(null)
    // IDOR is enforced server-side: requesting another coach's class returns 403.
    api.get<ClassDetail>(`/coach/classes/${id}`)
      .then((res) => setDetail(res.data))
      .catch((err) => setDetailError(errorMessage(err, 'Bạn không có quyền xem lớp học này.')))
  }

  function viewSchedule(c: TrainingClass) {
    api.get<ClassDetail>(`/coach/classes/${c.id}`)
      .then((res) => setScheduleDialog({ title: c.name, schedules: res.data.schedules }))
      .catch((err) => setError(errorMessage(err, 'Không thể tải lịch tập.')))
  }

  return (
    <>
      <div className="page-header"><h1>Lớp học của tôi</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="grid-2">
        <div>
          <div className="table-toolbar">
            <input className="form-control search-input" placeholder="Tìm theo tên lớp / cấp độ…"
              value={search} onChange={(e) => setSearch(e.target.value)} />
          </div>
          <div className="table-wrap">
            <table>
              <thead><tr><th>Tên lớp</th><th>Cấp độ</th><th>Sĩ số</th><th></th></tr></thead>
              <tbody>
                {pageClasses.map((c) => (
                  <tr key={c.id}>
                    <td>{c.name}</td><td>{c.level || '—'}</td>
                    <td>{c.currentEnrolled}/{c.capacity}</td>
                    <td className="actions">
                      <button className="btn btn-ghost btn-sm" onClick={() => viewClass(c.id)}>Xem</button>
                      <button className="btn btn-ghost btn-sm" onClick={() => viewSchedule(c)}>Lịch tập</button>
                    </td>
                  </tr>
                ))}
                {pageClasses.length === 0 && <tr><td colSpan={4} className="empty">Chưa có lớp nào.</td></tr>}
              </tbody>
            </table>
          </div>
          <Pagination page={page} pageSize={pageSize} total={total} onPage={setPage} />
        </div>

        <div className="card">
          <div className="card-title">Thành viên đã đăng ký</div>
          {detailError && <div className="alert alert-danger">{detailError}</div>}
          {!detail && !detailError && <p className="text-muted">Chọn một lớp để xem danh sách thành viên.</p>}
          {detail && (
            <>
              <p className="text-muted">{detail.class.name} — {detail.enrolledMembers.length} thành viên</p>
              <div className="table-toolbar">
                <input className="form-control search-input" placeholder="Tìm theo tên thành viên…"
                  value={memSearch} onChange={(e) => setMemSearch(e.target.value)} />
              </div>
              <div className="table-wrap">
                <table>
                  <thead><tr><th>Thành viên</th><th>Ngày đăng ký</th></tr></thead>
                  <tbody>
                    {pageMembers.map((e) => (
                      <tr key={e.id}><td>{e.memberName}</td><td>{e.enrollDate}</td></tr>
                    ))}
                    {pageMembers.length === 0 &&
                      <tr><td colSpan={2} className="empty">Chưa có thành viên.</td></tr>}
                  </tbody>
                </table>
              </div>
              <Pagination page={memPage} pageSize={memPageSize} total={memTotal} onPage={setMemPage} />
            </>
          )}
        </div>
      </div>

      <ScheduleDialog open={scheduleDialog !== null} title={scheduleDialog?.title ?? ''}
        schedules={scheduleDialog?.schedules ?? []} onClose={() => setScheduleDialog(null)} />
    </>
  )
}
