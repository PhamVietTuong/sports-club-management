import { useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { PtSession } from '../../api/types'
import Pagination from '../../components/Pagination'
import { usePaged } from '../../hooks/usePaged'

const PT_STATUS: Record<string, { label: string; badge: string }> = {
  PENDING: { label: 'Chờ xác nhận', badge: 'badge-review' },
  CONFIRMED: { label: 'Đã xác nhận', badge: 'badge-active' },
  CANCELLED: { label: 'Đã hủy', badge: 'badge-terminated' },
  COMPLETED: { label: 'Hoàn thành', badge: 'badge-inactive' },
}

export default function CoachPt() {
  const [statusFilter, setStatusFilter] = useState('')
  const { items: sessions, total, page, pageSize, setPage, search, setSearch, error, setError, reload } =
    usePaged<PtSession>('/coach/pt-sessions', { status: statusFilter })
  const [flash, setFlash] = useState('')

  async function setStatus(id: number, status: string) {
    setError(''); setFlash('')
    try {
      await api.post(`/coach/pt-sessions/${id}/status`, { status })
      setFlash('Đã cập nhật lịch PT.'); reload()
    } catch (err) { setError(errorMessage(err)) }
  }

  return (
    <>
      <div className="page-header"><h1>Lịch PT</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-toolbar">
        <input className="form-control search-input" placeholder="Tìm theo học viên…"
          value={search} onChange={(e) => setSearch(e.target.value)} />
        <select className="form-control filter-select" value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}>
          <option value="">Tất cả trạng thái</option>
          <option value="PENDING">Chờ xác nhận</option>
          <option value="CONFIRMED">Đã xác nhận</option>
          <option value="COMPLETED">Hoàn thành</option>
          <option value="CANCELLED">Đã hủy</option>
        </select>
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr><th>Học viên</th><th>Ngày</th><th>Giờ</th><th>Ghi chú</th><th>Trạng thái</th><th></th></tr>
          </thead>
          <tbody>
            {sessions.map((s) => (
              <tr key={s.id}>
                <td>{s.memberName}</td>
                <td>{s.sessionDate}</td>
                <td>{s.startTime}–{s.endTime}</td>
                <td>{s.notes || '—'}</td>
                <td><span className={'badge ' + (PT_STATUS[s.status]?.badge ?? '')}>{PT_STATUS[s.status]?.label ?? s.status}</span></td>
                <td className="actions">
                  {s.status === 'PENDING' &&
                    <button className="btn btn-primary btn-sm" onClick={() => setStatus(s.id, 'CONFIRMED')}>Xác nhận</button>}
                  {s.status === 'CONFIRMED' &&
                    <button className="btn btn-ghost btn-sm" onClick={() => setStatus(s.id, 'COMPLETED')}>Hoàn thành</button>}
                  {(s.status === 'PENDING' || s.status === 'CONFIRMED') &&
                    <button className="btn btn-danger btn-sm" onClick={() => setStatus(s.id, 'CANCELLED')}>Hủy</button>}
                </td>
              </tr>
            ))}
            {sessions.length === 0 && <tr><td colSpan={6} className="empty">Chưa có lịch PT nào.</td></tr>}
          </tbody>
        </table>
      </div>
      <Pagination page={page} pageSize={pageSize} total={total} onPage={setPage} />
    </>
  )
}
