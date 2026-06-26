import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { PtSession, RateableCoach } from '../../api/types'
import Modal from '../../components/Modal'

const PT_STATUS: Record<string, { label: string; badge: string }> = {
  PENDING: { label: 'Chờ xác nhận', badge: 'badge-review' },
  CONFIRMED: { label: 'Đã xác nhận', badge: 'badge-active' },
  CANCELLED: { label: 'Đã hủy', badge: 'badge-terminated' },
  COMPLETED: { label: 'Hoàn thành', badge: 'badge-inactive' },
}

function stars(n: number) { return '★'.repeat(Math.round(n)) + '☆'.repeat(5 - Math.round(n)) }
function today() { return new Date().toISOString().slice(0, 10) }

export default function MemberCoaches() {
  const [coaches, setCoaches] = useState<RateableCoach[]>([])
  const [sessions, setSessions] = useState<PtSession[]>([])
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)

  const [rateFor, setRateFor] = useState<RateableCoach | null>(null)
  const [rateForm, setRateForm] = useState({ rating: '5', comment: '' })
  const [rateError, setRateError] = useState('')

  const [bookFor, setBookFor] = useState<RateableCoach | null>(null)
  const [bookForm, setBookForm] = useState({ sessionDate: today(), startTime: '08:00', endTime: '09:00', notes: '' })
  const [bookError, setBookError] = useState('')

  function load() {
    api.get<RateableCoach[]>('/member/coaches')
      .then((res) => setCoaches(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh sách huấn luyện viên.')))
    api.get<PtSession[]>('/member/pt-sessions').then((res) => setSessions(res.data)).catch(() => {})
  }
  useEffect(load, [])

  function openRate(c: RateableCoach) {
    setRateFor(c)
    setRateForm({ rating: String(c.myRating ?? 5), comment: c.myComment ?? '' })
    setRateError('')
  }
  async function submitRate(e: FormEvent) {
    e.preventDefault()
    if (!rateFor) return
    setBusy(true); setRateError('')
    try {
      await api.post(`/member/coaches/${rateFor.id}/rating`, {
        rating: Number(rateForm.rating), comment: rateForm.comment || null,
      })
      setRateFor(null); setFlash('Đã gửi đánh giá.'); load()
    } catch (err) { setRateError(errorMessage(err)) } finally { setBusy(false) }
  }

  function openBook(c: RateableCoach) {
    setBookFor(c)
    setBookForm({ sessionDate: today(), startTime: '08:00', endTime: '09:00', notes: '' })
    setBookError('')
  }
  async function submitBook(e: FormEvent) {
    e.preventDefault()
    if (!bookFor) return
    setBusy(true); setBookError('')
    try {
      await api.post('/member/pt-sessions', {
        coachId: bookFor.id, sessionDate: bookForm.sessionDate,
        startTime: bookForm.startTime, endTime: bookForm.endTime, notes: bookForm.notes || null,
      })
      setBookFor(null); setFlash('Đã đặt lịch PT. Chờ HLV xác nhận.'); load()
    } catch (err) { setBookError(errorMessage(err)) } finally { setBusy(false) }
  }

  async function cancelSession(id: number) {
    if (!window.confirm('Hủy lịch PT này?')) return
    try { await api.post(`/member/pt-sessions/${id}/cancel`); setFlash('Đã hủy lịch PT.'); load() }
    catch (err) { setError(errorMessage(err)) }
  }

  return (
    <>
      <div className="page-header"><h1>Huấn luyện viên</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr><th>Tên</th><th>Chuyên môn</th><th>Đánh giá</th><th>Của tôi</th><th></th></tr>
          </thead>
          <tbody>
            {coaches.map((c) => (
              <tr key={c.id}>
                <td>{c.fullName}</td>
                <td>{c.specialization || '—'}</td>
                <td>
                  {c.ratingCount > 0
                    ? <span title={`${c.averageRating}/5`}>{stars(c.averageRating)} ({c.ratingCount})</span>
                    : <span className="text-muted">Chưa có</span>}
                </td>
                <td>{c.myRating ? `${c.myRating}/5` : '—'}</td>
                <td className="actions">
                  <button className="btn btn-ghost btn-sm" disabled={!c.canRate} title={c.canRate ? '' : 'Chỉ đánh giá HLV của lớp bạn đã tham gia'}
                    onClick={() => openRate(c)}>Đánh giá</button>
                  <button className="btn btn-primary btn-sm" onClick={() => openBook(c)}>Đặt lịch PT</button>
                </td>
              </tr>
            ))}
            {coaches.length === 0 && <tr><td colSpan={5} className="empty">Chưa có huấn luyện viên.</td></tr>}
          </tbody>
        </table>
      </div>

      <div className="card mt-4">
        <div className="card-title">Lịch PT của tôi</div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>HLV</th><th>Ngày</th><th>Giờ</th><th>Trạng thái</th><th></th></tr></thead>
            <tbody>
              {sessions.map((s) => (
                <tr key={s.id}>
                  <td>{s.coachName}</td>
                  <td>{s.sessionDate}</td>
                  <td>{s.startTime}–{s.endTime}</td>
                  <td><span className={'badge ' + (PT_STATUS[s.status]?.badge ?? '')}>{PT_STATUS[s.status]?.label ?? s.status}</span></td>
                  <td>
                    {(s.status === 'PENDING' || s.status === 'CONFIRMED') &&
                      <button className="btn btn-danger btn-sm" onClick={() => cancelSession(s.id)}>Hủy</button>}
                  </td>
                </tr>
              ))}
              {sessions.length === 0 && <tr><td colSpan={5} className="empty">Chưa có lịch PT.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      <Modal open={!!rateFor} title={`Đánh giá ${rateFor?.fullName ?? ''}`} onClose={() => setRateFor(null)}>
        <form onSubmit={submitRate}>
          {rateError && <div className="alert alert-danger">{rateError}</div>}
          <div className="form-group">
            <label className="form-label">Số sao</label>
            <select className="form-control" value={rateForm.rating}
              onChange={(e) => setRateForm({ ...rateForm, rating: e.target.value })}>
              {[5, 4, 3, 2, 1].map((r) => <option key={r} value={r}>{r} sao</option>)}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Nhận xét</label>
            <textarea className="form-control" rows={3} value={rateForm.comment}
              onChange={(e) => setRateForm({ ...rateForm, comment: e.target.value })} />
          </div>
          <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Đang gửi…' : 'Gửi đánh giá'}</button>
        </form>
      </Modal>

      <Modal open={!!bookFor} title={`Đặt lịch PT với ${bookFor?.fullName ?? ''}`} onClose={() => setBookFor(null)}>
        <form onSubmit={submitBook}>
          {bookError && <div className="alert alert-danger">{bookError}</div>}
          <div className="form-group">
            <label className="form-label">Ngày *</label>
            <input className="form-control" type="date" value={bookForm.sessionDate} required
              onChange={(e) => setBookForm({ ...bookForm, sessionDate: e.target.value })} />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Giờ bắt đầu *</label>
              <input className="form-control" type="time" value={bookForm.startTime} required
                onChange={(e) => setBookForm({ ...bookForm, startTime: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Giờ kết thúc *</label>
              <input className="form-control" type="time" value={bookForm.endTime} required
                onChange={(e) => setBookForm({ ...bookForm, endTime: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Ghi chú</label>
            <textarea className="form-control" rows={2} value={bookForm.notes}
              onChange={(e) => setBookForm({ ...bookForm, notes: e.target.value })} />
          </div>
          <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Đang đặt…' : 'Đặt lịch'}</button>
        </form>
      </Modal>
    </>
  )
}
