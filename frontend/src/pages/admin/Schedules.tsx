import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { Schedule, TrainingClass } from '../../api/types'
import Modal from '../../components/Modal'

const DAYS = ['MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY', 'SATURDAY', 'SUNDAY']

const emptyForm = { classId: '', dayOfWeek: 'MONDAY', startTime: '07:00', endTime: '08:00', room: '' }

export default function Schedules() {
  const [schedules, setSchedules] = useState<Schedule[]>([])
  const [classes, setClasses] = useState<TrainingClass[]>([])
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState(emptyForm)

  function load() {
    api.get<Schedule[]>('/admin/schedules')
      .then((res) => setSchedules(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải lịch tập.')))
  }
  useEffect(load, [])
  useEffect(() => {
    api.get<TrainingClass[]>('/admin/classes').then((res) => setClasses(res.data)).catch(() => {})
  }, [])

  async function clone(id: number) {
    try { await api.post(`/admin/schedules/${id}/clone`); setFlash('Đã nhân bản lịch tập.'); load() }
    catch (err) { setError(errorMessage(err)) }
  }
  async function remove(id: number) {
    if (!confirm('Xóa lịch tập này?')) return
    try { await api.delete(`/admin/schedules/${id}`); setFlash('Đã xóa lịch tập.'); load() }
    catch (err) { setError(errorMessage(err)) }
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    if (!form.classId) { setError('Vui lòng chọn lớp học.'); return }
    setBusy(true); setError('')
    try {
      await api.post('/admin/schedules', {
        classId: Number(form.classId), dayOfWeek: form.dayOfWeek,
        startTime: form.startTime, endTime: form.endTime, room: form.room || null,
      })
      setOpen(false); setForm(emptyForm); setFlash('Đã thêm lịch tập.'); load()
    } catch (err) { setError(errorMessage(err)) } finally { setBusy(false) }
  }

  return (
    <>
      <div className="page-header">
        <h1>Lịch tập</h1>
        <button className="btn btn-primary" onClick={() => setOpen(true)}>+ Thêm lịch</button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr><th>ID</th><th>Lớp học</th><th>Ngày</th><th>Bắt đầu</th><th>Kết thúc</th><th>Phòng</th><th></th></tr>
          </thead>
          <tbody>
            {schedules.map((s) => (
              <tr key={s.id}>
                <td>{s.id}</td>
                <td>{s.className}</td>
                <td>{s.dayOfWeek}</td>
                <td>{s.startTime}</td>
                <td>{s.endTime}</td>
                <td>{s.room || '—'}</td>
                <td className="actions">
                  <button className="btn btn-ghost btn-sm" onClick={() => clone(s.id)}>Nhân bản</button>
                  <button className="btn btn-danger btn-sm" onClick={() => remove(s.id)}>Xóa</button>
                </td>
              </tr>
            ))}
            {schedules.length === 0 && <tr><td colSpan={7} className="empty">Chưa có lịch tập nào.</td></tr>}
          </tbody>
        </table>
      </div>

      <Modal open={open} title="Thêm lịch tập" onClose={() => setOpen(false)}>
        <form onSubmit={submit}>
          <div className="form-group">
            <label className="form-label">Lớp học *</label>
            <select className="form-control" value={form.classId} required
              onChange={(e) => setForm({ ...form, classId: e.target.value })}>
              <option value="">-- Chọn lớp --</option>
              {classes.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Ngày trong tuần</label>
            <select className="form-control" value={form.dayOfWeek}
              onChange={(e) => setForm({ ...form, dayOfWeek: e.target.value })}>
              {DAYS.map((d) => <option key={d} value={d}>{d}</option>)}
            </select>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Giờ bắt đầu</label>
              <input className="form-control" type="time" value={form.startTime}
                onChange={(e) => setForm({ ...form, startTime: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Giờ kết thúc</label>
              <input className="form-control" type="time" value={form.endTime}
                onChange={(e) => setForm({ ...form, endTime: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Phòng</label>
            <input className="form-control" value={form.room}
              onChange={(e) => setForm({ ...form, room: e.target.value })} />
          </div>
          <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Đang lưu…' : 'Lưu'}</button>
        </form>
      </Modal>
    </>
  )
}
