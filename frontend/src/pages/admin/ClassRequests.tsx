import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { ClassChangeRequest } from '../../api/types'

const ACTION_LABEL: Record<string, string> = { CLAIM: 'Nhận lớp', RELEASE: 'Trả lớp' }
const STATUS_LABEL: Record<string, string> = {
  PENDING: 'Chờ duyệt', APPROVED: 'Đã duyệt', REJECTED: 'Bị từ chối',
}
const STATUS_BADGE: Record<string, string> = {
  PENDING: 'badge-review', APPROVED: 'badge-active', REJECTED: 'badge-inactive',
}
const FILTERS = ['', 'PENDING', 'APPROVED', 'REJECTED']

export default function ClassRequests() {
  const [requests, setRequests] = useState<ClassChangeRequest[]>([])
  const [status, setStatus] = useState('PENDING')
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')

  function load() {
    const q = status ? `?status=${status}` : ''
    api.get<ClassChangeRequest[]>(`/admin/class-requests${q}`)
      .then((res) => setRequests(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh sách yêu cầu.')))
  }
  useEffect(load, [status])

  async function approve(id: number) {
    setError(''); setFlash('')
    try {
      const res = await api.post<{ message: string }>(`/admin/class-requests/${id}/approve`)
      setFlash(res.data.message); load()
    } catch (err) { setError(errorMessage(err)) }
  }
  async function reject(id: number) {
    const note = window.prompt('Lý do từ chối (tùy chọn):') ?? undefined
    setError(''); setFlash('')
    try {
      const res = await api.post<{ message: string }>(`/admin/class-requests/${id}/reject`, { note })
      setFlash(res.data.message); load()
    } catch (err) { setError(errorMessage(err)) }
  }

  return (
    <>
      <div className="page-header"><h1>Yêu cầu nhận/trả lớp (HLV)</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="form-group" style={{ maxWidth: 240 }}>
        <label className="form-label">Lọc theo trạng thái</label>
        <select className="form-control" value={status} onChange={(e) => setStatus(e.target.value)}>
          {FILTERS.map((f) => (
            <option key={f} value={f}>{f ? (STATUS_LABEL[f] ?? f) : 'Tất cả'}</option>
          ))}
        </select>
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>ID</th><th>HLV</th><th>Lớp</th><th>Hành động</th>
              <th>Trạng thái</th><th>Ngày yêu cầu</th><th>Ghi chú</th><th></th>
            </tr>
          </thead>
          <tbody>
            {requests.map((r) => (
              <tr key={r.id}>
                <td>{r.id}</td>
                <td>{r.coachName}</td>
                <td>{r.className}</td>
                <td>{ACTION_LABEL[r.action] ?? r.action}</td>
                <td><span className={'badge ' + (STATUS_BADGE[r.status] ?? '')}>
                  {STATUS_LABEL[r.status] ?? r.status}</span></td>
                <td>{new Date(r.requestedAt).toLocaleString('vi-VN')}</td>
                <td>{r.note || '—'}</td>
                <td className="actions">
                  {r.status === 'PENDING' ? (
                    <>
                      <button className="btn btn-primary btn-sm" onClick={() => approve(r.id)}>Duyệt</button>
                      <button className="btn btn-danger btn-sm" onClick={() => reject(r.id)}>Từ chối</button>
                    </>
                  ) : '—'}
                </td>
              </tr>
            ))}
            {requests.length === 0 && <tr><td colSpan={8} className="empty">Không có yêu cầu nào.</td></tr>}
          </tbody>
        </table>
      </div>
    </>
  )
}
