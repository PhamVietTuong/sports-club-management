import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { ClassDetail, ProgressNote, TrainingClass } from '../../api/types'
import Modal from '../../components/Modal'

export default function CoachProgress() {
  const [notes, setNotes] = useState<ProgressNote[]>([])
  const [classes, setClasses] = useState<TrainingClass[]>([])
  const [members, setMembers] = useState<{ id: number; name: string }[]>([])
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState({ classId: '', memberId: '', note: '', rating: '' })

  function load() {
    api.get<ProgressNote[]>('/coach/progress')
      .then((res) => setNotes(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải đánh giá.')))
  }
  useEffect(() => {
    load()
    api.get<TrainingClass[]>('/coach/classes').then((res) => setClasses(res.data)).catch(() => {})
  }, [])

  // When the chosen class changes, pull its enrolled members for the picker.
  function onClassChange(classId: string) {
    setForm((f) => ({ ...f, classId, memberId: '' }))
    setMembers([])
    if (!classId) return
    api.get<ClassDetail>(`/coach/classes/${classId}`)
      .then((res) => setMembers(res.data.enrolledMembers.map((e) => ({ id: e.memberId, name: e.memberName }))))
      .catch((err) => setFormError(errorMessage(err)))
  }

  function openAdd() {
    setForm({ classId: '', memberId: '', note: '', rating: '' })
    setMembers([]); setFormError(''); setOpen(true)
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true); setFormError('')
    try {
      await api.post('/coach/progress', {
        memberId: Number(form.memberId),
        classId: form.classId ? Number(form.classId) : null,
        note: form.note,
        rating: form.rating ? Number(form.rating) : null,
      })
      setOpen(false); setFlash('Đã lưu đánh giá tiến độ.'); load()
    } catch (err) { setFormError(errorMessage(err)) } finally { setBusy(false) }
  }

  return (
    <>
      <div className="page-header">
        <h1>Đánh giá học viên</h1>
        <button className="btn btn-primary" onClick={openAdd} disabled={classes.length === 0}>+ Thêm đánh giá</button>
      </div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}
      {classes.length === 0 && <div className="alert alert-warning">Bạn chưa được phân lớp nào.</div>}

      <div className="table-wrap">
        <table>
          <thead><tr><th>Học viên</th><th>Đánh giá</th><th>Điểm</th><th>Thời gian</th></tr></thead>
          <tbody>
            {notes.map((n) => (
              <tr key={n.id}>
                <td>{n.memberName}</td>
                <td style={{ maxWidth: 360, whiteSpace: 'pre-wrap' }}>{n.note}</td>
                <td>{n.rating ? `${n.rating}/5` : '—'}</td>
                <td>{new Date(n.recordedAt).toLocaleDateString('vi-VN')}</td>
              </tr>
            ))}
            {notes.length === 0 && <tr><td colSpan={4} className="empty">Chưa có đánh giá nào.</td></tr>}
          </tbody>
        </table>
      </div>

      <Modal open={open} title="Thêm đánh giá tiến độ" onClose={() => { setFormError(''); setOpen(false) }}>
        <form onSubmit={submit}>
          {formError && <div className="alert alert-danger">{formError}</div>}
          <div className="form-group">
            <label className="form-label">Lớp học *</label>
            <select className="form-control" value={form.classId} required
              onChange={(e) => onClassChange(e.target.value)}>
              <option value="">-- Chọn lớp --</option>
              {classes.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Học viên *</label>
            <select className="form-control" value={form.memberId} required disabled={!form.classId}
              onChange={(e) => setForm({ ...form, memberId: e.target.value })}>
              <option value="">-- Chọn học viên --</option>
              {members.map((m) => <option key={m.id} value={m.id}>{m.name}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Nội dung đánh giá *</label>
            <textarea className="form-control" rows={4} value={form.note} required
              onChange={(e) => setForm({ ...form, note: e.target.value })} />
          </div>
          <div className="form-group">
            <label className="form-label">Điểm tiến độ (1–5)</label>
            <select className="form-control" value={form.rating}
              onChange={(e) => setForm({ ...form, rating: e.target.value })}>
              <option value="">-- Không --</option>
              {[1, 2, 3, 4, 5].map((r) => <option key={r} value={r}>{r}</option>)}
            </select>
          </div>
          <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Đang lưu…' : 'Lưu'}</button>
        </form>
      </Modal>
    </>
  )
}
