import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { Coach, Schedule, TrainingClass } from '../../api/types'
import Modal from '../../components/Modal'
import ScheduleDialog from '../../components/ScheduleDialog'

const LEVELS = ['BEGINNER', 'INTERMEDIATE', 'ADVANCED']

interface ClassForm {
  id: number
  name: string
  coachId: string
  capacity: string
  level: string
  description: string
  isActive: boolean
}

const emptyForm: ClassForm = {
  id: 0, name: '', coachId: '', capacity: '20', level: 'BEGINNER', description: '', isActive: true,
}

export default function Classes() {
  const [classes, setClasses] = useState<TrainingClass[]>([])
  const [coaches, setCoaches] = useState<Coach[]>([])
  const [schedules, setSchedules] = useState<Schedule[]>([])
  const [scheduleDialog, setScheduleDialog] = useState<{ title: string; schedules: Schedule[] } | null>(null)
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState<ClassForm>(emptyForm)

  function load() {
    api.get<TrainingClass[]>('/admin/classes')
      .then((res) => setClasses(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh sách lớp học.')))
  }
  useEffect(load, [])
  useEffect(() => {
    api.get<Coach[]>('/admin/coaches').then((res) => setCoaches(res.data)).catch(() => {})
    api.get<Schedule[]>('/admin/schedules').then((res) => setSchedules(res.data)).catch(() => {})
  }, [])

  function viewSchedule(c: TrainingClass) {
    setScheduleDialog({ title: c.name, schedules: schedules.filter((s) => s.classId === c.id) })
  }

  function openAdd() { setForm(emptyForm); setOpen(true) }
  function openEdit(c: TrainingClass) {
    setForm({
      id: c.id, name: c.name, coachId: c.coachId ? String(c.coachId) : '',
      capacity: String(c.capacity), level: c.level ?? 'BEGINNER',
      description: c.description ?? '', isActive: c.isActive,
    })
    setOpen(true)
  }

  async function clone(id: number) {
    try { await api.post(`/admin/classes/${id}/clone`); setFlash('Đã nhân bản lớp học.'); load() }
    catch (err) { setError(errorMessage(err)) }
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true); setError('')
    const body = {
      name: form.name, coachId: form.coachId ? Number(form.coachId) : 0,
      capacity: Number(form.capacity), level: form.level,
      description: form.description || null, isActive: form.isActive,
    }
    try {
      if (form.id) await api.put(`/admin/classes/${form.id}`, body)
      else await api.post('/admin/classes', body)
      setOpen(false); setFlash(form.id ? 'Đã cập nhật lớp học.' : 'Đã thêm lớp học.'); load()
    } catch (err) { setError(errorMessage(err)) } finally { setBusy(false) }
  }

  return (
    <>
      <div className="page-header">
        <h1>Lớp học</h1>
        <button className="btn btn-primary" onClick={openAdd}>+ Thêm lớp học</button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>ID</th><th>Tên lớp</th><th>HLV</th><th>Cấp độ</th>
              <th>Sĩ số</th><th>Trạng thái</th><th></th>
            </tr>
          </thead>
          <tbody>
            {classes.map((c) => (
              <tr key={c.id}>
                <td>{c.id}</td>
                <td>{c.name}</td>
                <td>{c.coachName || '—'}</td>
                <td>{c.level || '—'}</td>
                <td>{c.currentEnrolled}/{c.capacity}</td>
                <td>
                  <span className={'badge ' + (c.isActive ? 'badge-active' : 'badge-inactive')}>
                    {c.isActive ? 'Hoạt động' : 'Ẩn'}
                  </span>
                </td>
                <td className="actions">
                  <button className="btn btn-ghost btn-sm" onClick={() => openEdit(c)}>Sửa</button>
                  <button className="btn btn-ghost btn-sm" onClick={() => viewSchedule(c)}>Lịch tập</button>
                  <button className="btn btn-ghost btn-sm" onClick={() => clone(c.id)}>Nhân bản</button>
                </td>
              </tr>
            ))}
            {classes.length === 0 && <tr><td colSpan={7} className="empty">Chưa có lớp học nào.</td></tr>}
          </tbody>
        </table>
      </div>

      <Modal open={open} title={form.id ? 'Sửa lớp học' : 'Thêm lớp học'} onClose={() => setOpen(false)}>
        <form onSubmit={submit}>
          <div className="form-group">
            <label className="form-label">Tên lớp *</label>
            <input className="form-control" value={form.name} required
              onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Huấn luyện viên</label>
              <select className="form-control" value={form.coachId}
                onChange={(e) => setForm({ ...form, coachId: e.target.value })}>
                <option value="">-- Không --</option>
                {coaches.map((c) => <option key={c.id} value={c.id}>{c.fullName}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Sức chứa</label>
              <input className="form-control" type="number" min={1} value={form.capacity}
                onChange={(e) => setForm({ ...form, capacity: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Cấp độ</label>
            <select className="form-control" value={form.level}
              onChange={(e) => setForm({ ...form, level: e.target.value })}>
              {LEVELS.map((l) => <option key={l} value={l}>{l}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Mô tả</label>
            <textarea className="form-control" rows={2} value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })} />
          </div>
          {form.id !== 0 && (
            <div className="form-group">
              <label className="form-label">
                <input type="checkbox" checked={form.isActive}
                  onChange={(e) => setForm({ ...form, isActive: e.target.checked })} /> Đang hoạt động
              </label>
            </div>
          )}
          <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Đang lưu…' : 'Lưu'}</button>
        </form>
      </Modal>

      <ScheduleDialog open={scheduleDialog !== null} title={scheduleDialog?.title ?? ''}
        schedules={scheduleDialog?.schedules ?? []} onClose={() => setScheduleDialog(null)} />
    </>
  )
}
