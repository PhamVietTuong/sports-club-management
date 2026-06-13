import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { AvailableClass } from '../../api/types'

export default function MemberClasses() {
  const [classes, setClasses] = useState<AvailableClass[]>([])
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')

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

  return (
    <>
      <div className="page-header"><h1>Đăng ký lớp</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr><th>Tên lớp</th><th>HLV</th><th>Cấp độ</th><th>Còn trống</th><th></th></tr>
          </thead>
          <tbody>
            {classes.map(({ class: c, isEnrolled }) => (
              <tr key={c.id}>
                <td>{c.name}</td>
                <td>{c.coachName || '—'}</td>
                <td>{c.level || '—'}</td>
                <td>{c.availableSlots}/{c.capacity}</td>
                <td>
                  {isEnrolled ? (
                    <button className="btn btn-danger btn-sm" onClick={() => cancel(c.id)}>Hủy</button>
                  ) : (
                    <button className="btn btn-primary btn-sm" disabled={c.availableSlots <= 0}
                      onClick={() => enroll(c.id)}>
                      {c.availableSlots <= 0 ? 'Đã đầy' : 'Đăng ký'}
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {classes.length === 0 && <tr><td colSpan={5} className="empty">Không có lớp khả dụng.</td></tr>}
          </tbody>
        </table>
      </div>
    </>
  )
}
