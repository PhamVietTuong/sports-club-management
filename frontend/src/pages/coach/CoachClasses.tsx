import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { ClassDetail, TrainingClass } from '../../api/types'

export default function CoachClasses() {
  const [classes, setClasses] = useState<TrainingClass[]>([])
  const [detail, setDetail] = useState<ClassDetail | null>(null)
  const [error, setError] = useState('')
  const [detailError, setDetailError] = useState('')

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

  return (
    <>
      <div className="page-header"><h1>Lớp học của tôi</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="grid-2">
        <div className="table-wrap">
          <table>
            <thead><tr><th>Tên lớp</th><th>Cấp độ</th><th>Sĩ số</th><th></th></tr></thead>
            <tbody>
              {classes.map((c) => (
                <tr key={c.id}>
                  <td>{c.name}</td><td>{c.level || '—'}</td>
                  <td>{c.currentEnrolled}/{c.capacity}</td>
                  <td><button className="btn btn-ghost btn-sm" onClick={() => viewClass(c.id)}>Xem</button></td>
                </tr>
              ))}
              {classes.length === 0 && <tr><td colSpan={4} className="empty">Chưa có lớp nào.</td></tr>}
            </tbody>
          </table>
        </div>

        <div className="card">
          <div className="card-title">Thành viên đã đăng ký</div>
          {detailError && <div className="alert alert-danger">{detailError}</div>}
          {!detail && !detailError && <p className="text-muted">Chọn một lớp để xem danh sách thành viên.</p>}
          {detail && (
            <>
              <p className="text-muted">{detail.class.name} — {detail.enrolledMembers.length} thành viên</p>
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
        </div>
      </div>
    </>
  )
}
