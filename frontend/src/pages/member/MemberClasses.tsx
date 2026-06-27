import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { AvailableClass, Schedule } from '../../api/types'
import ScheduleDialog from '../../components/ScheduleDialog'
import Pagination from '../../components/Pagination'
import { useClientPaged } from '../../hooks/useClientPaged'

export default function MemberClasses() {
  const [classes, setClasses] = useState<AvailableClass[]>([])
  const [scheduleDialog, setScheduleDialog] = useState<{ title: string; schedules: Schedule[] } | null>(null)
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')

  const { pageItems: pageClasses, total, page, pageSize, setPage, search, setSearch } =
    useClientPaged(classes, (c, q) =>
      c.class.name.toLowerCase().includes(q)
      || (c.class.coachName ?? '').toLowerCase().includes(q)
      || (c.class.level ?? '').toLowerCase().includes(q))

  function load() {
    api.get<AvailableClass[]>('/member/classes')
      .then((res) => setClasses(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh sách lớp học.')))
  }
  useEffect(load, [])

  async function enroll(id: number) {
    setError(''); setFlash('')
    try { await api.post(`/member/classes/${id}/enroll`); setFlash('Đăng ký thành công!'); load() }
    catch (err) { setError(errorMessage(err)) }
  }
  async function cancel(id: number) {
    setError(''); setFlash('')
    try { await api.post(`/member/classes/${id}/cancel`); setFlash('Đã hủy đăng ký.'); load() }
    catch (err) { setError(errorMessage(err)) }
  }
  async function checkIn(id: number) {
    setError(''); setFlash('')
    try { await api.post(`/member/classes/${id}/checkin`); setFlash('Đã check-in hôm nay.') }
    catch (err) { setError(errorMessage(err)) }
  }

  return (
    <>
      <div className="page-header"><h1>Đăng ký lớp</h1></div>
      <p className="text-muted">
        Chỉ hiển thị các lớp thuộc gói tập của bạn. Đăng ký lớp đầu tiên sẽ kích hoạt gói và
        không thể hủy/đổi gói sau đó.
      </p>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-toolbar">
        <input className="form-control search-input" placeholder="Tìm theo tên lớp / HLV / cấp độ…"
          value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr><th>Tên lớp</th><th>HLV</th><th>Cấp độ</th><th>Lịch học</th><th>Còn trống</th><th></th></tr>
          </thead>
          <tbody>
            {pageClasses.map(({ class: c, isEnrolled, schedules }) => (
              <tr key={c.id}>
                <td>{c.name}</td>
                <td>{c.coachName || '—'}</td>
                <td>{c.level || '—'}</td>
                <td>
                  <button className="btn btn-ghost btn-sm"
                    onClick={() => setScheduleDialog({ title: c.name, schedules })}>
                    Xem lịch
                  </button>
                </td>
                <td>{c.availableSlots}/{c.capacity}</td>
                <td className="actions">
                  {isEnrolled ? (
                    <>
                      <button className="btn btn-primary btn-sm" onClick={() => checkIn(c.id)}>Check-in</button>
                      <button className="btn btn-danger btn-sm" onClick={() => cancel(c.id)}>Hủy</button>
                    </>
                  ) : (
                    <button className="btn btn-primary btn-sm" disabled={c.availableSlots <= 0}
                      onClick={() => enroll(c.id)}>
                      {c.availableSlots <= 0 ? 'Đã đầy' : 'Đăng ký'}
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {pageClasses.length === 0 && <tr><td colSpan={6} className="empty">
              Không có lớp khả dụng. Hãy đăng ký và kích hoạt một gói tập trước.
            </td></tr>}
          </tbody>
        </table>
      </div>
      <Pagination page={page} pageSize={pageSize} total={total} onPage={setPage} />

      <ScheduleDialog open={scheduleDialog !== null} title={scheduleDialog?.title ?? ''}
        schedules={scheduleDialog?.schedules ?? []} onClose={() => setScheduleDialog(null)} />
    </>
  )
}
