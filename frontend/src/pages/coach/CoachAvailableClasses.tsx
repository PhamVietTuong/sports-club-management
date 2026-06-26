import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { TrainingClass } from '../../api/types'

export default function CoachAvailableClasses() {
  const [available, setAvailable] = useState<TrainingClass[]>([])
  const [mine, setMine] = useState<TrainingClass[]>([])
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')

  function load() {
    api.get<TrainingClass[]>('/coach/available-classes')
      .then((res) => setAvailable(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải lớp khả dụng.')))
    api.get<TrainingClass[]>('/coach/classes').then((res) => setMine(res.data)).catch(() => {})
  }
  useEffect(load, [])

  async function claim(id: number) {
    setError(''); setFlash('')
    try { await api.post(`/coach/classes/${id}/claim`); setFlash('Đã nhận lớp.'); load() }
    catch (err) { setError(errorMessage(err)) }
  }
  async function release(id: number) {
    if (!window.confirm('Trả lại lớp này?')) return
    setError(''); setFlash('')
    try { await api.post(`/coach/classes/${id}/release`); setFlash('Đã trả lớp.'); load() }
    catch (err) { setError(errorMessage(err)) }
  }

  return (
    <>
      <div className="page-header"><h1>Nhận lớp</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="card">
        <div className="card-title">Lớp chưa có HLV</div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Tên lớp</th><th>Cấp độ</th><th>Sĩ số</th><th></th></tr></thead>
            <tbody>
              {available.map((c) => (
                <tr key={c.id}>
                  <td>{c.name}</td><td>{c.level || '—'}</td><td>{c.currentEnrolled}/{c.capacity}</td>
                  <td><button className="btn btn-primary btn-sm" onClick={() => claim(c.id)}>Nhận lớp</button></td>
                </tr>
              ))}
              {available.length === 0 && <tr><td colSpan={4} className="empty">Không có lớp nào đang chờ nhận.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card mt-4">
        <div className="card-title">Lớp của tôi</div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Tên lớp</th><th>Cấp độ</th><th>Sĩ số</th><th></th></tr></thead>
            <tbody>
              {mine.map((c) => (
                <tr key={c.id}>
                  <td>{c.name}</td><td>{c.level || '—'}</td><td>{c.currentEnrolled}/{c.capacity}</td>
                  <td><button className="btn btn-ghost btn-sm" onClick={() => release(c.id)}>Trả lớp</button></td>
                </tr>
              ))}
              {mine.length === 0 && <tr><td colSpan={4} className="empty">Bạn chưa phụ trách lớp nào.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}
