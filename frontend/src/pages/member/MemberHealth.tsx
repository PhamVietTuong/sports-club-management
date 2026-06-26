import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { HealthMetric } from '../../api/types'
import Modal from '../../components/Modal'

function today() { return new Date().toISOString().slice(0, 10) }

const emptyForm = { recordedDate: today(), weightKg: '', heightCm: '', bodyFatPct: '', notes: '' }

export default function MemberHealth() {
  const [metrics, setMetrics] = useState<HealthMetric[]>([])
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState(emptyForm)

  function load() {
    api.get<HealthMetric[]>('/member/health')
      .then((res) => setMetrics(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải chỉ số sức khỏe.')))
  }
  useEffect(load, [])

  function openAdd() { setForm({ ...emptyForm, recordedDate: today() }); setFormError(''); setOpen(true) }

  async function remove(id: number) {
    if (!window.confirm('Xóa bản ghi này?')) return
    try { await api.delete(`/member/health/${id}`); setFlash('Đã xóa bản ghi.'); load() }
    catch (err) { setError(errorMessage(err)) }
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true); setFormError('')
    try {
      await api.post('/member/health', {
        recordedDate: form.recordedDate,
        weightKg: form.weightKg ? Number(form.weightKg) : null,
        heightCm: form.heightCm ? Number(form.heightCm) : null,
        bodyFatPct: form.bodyFatPct ? Number(form.bodyFatPct) : null,
        notes: form.notes || null,
      })
      setOpen(false); setFlash('Đã lưu chỉ số sức khỏe.'); load()
    } catch (err) { setFormError(errorMessage(err)) } finally { setBusy(false) }
  }

  return (
    <>
      <div className="page-header">
        <h1>Theo dõi sức khỏe</h1>
        <button className="btn btn-primary" onClick={openAdd}>+ Thêm chỉ số</button>
      </div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr><th>Ngày</th><th>Cân nặng (kg)</th><th>Chiều cao (cm)</th><th>Mỡ cơ thể (%)</th><th>Ghi chú</th><th></th></tr>
          </thead>
          <tbody>
            {metrics.map((m) => (
              <tr key={m.id}>
                <td>{m.recordedDate}</td>
                <td>{m.weightKg ?? '—'}</td>
                <td>{m.heightCm ?? '—'}</td>
                <td>{m.bodyFatPct ?? '—'}</td>
                <td>{m.notes || '—'}</td>
                <td><button className="btn btn-danger btn-sm" onClick={() => remove(m.id)}>Xóa</button></td>
              </tr>
            ))}
            {metrics.length === 0 && <tr><td colSpan={6} className="empty">Chưa có dữ liệu sức khỏe.</td></tr>}
          </tbody>
        </table>
      </div>

      <Modal open={open} title="Thêm chỉ số sức khỏe" onClose={() => { setFormError(''); setOpen(false) }}>
        <form onSubmit={submit}>
          {formError && <div className="alert alert-danger">{formError}</div>}
          <div className="form-group">
            <label className="form-label">Ngày *</label>
            <input className="form-control" type="date" value={form.recordedDate} required
              onChange={(e) => setForm({ ...form, recordedDate: e.target.value })} />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Cân nặng (kg)</label>
              <input className="form-control" type="number" min={0} step="0.1" value={form.weightKg}
                onChange={(e) => setForm({ ...form, weightKg: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Chiều cao (cm)</label>
              <input className="form-control" type="number" min={0} step="0.1" value={form.heightCm}
                onChange={(e) => setForm({ ...form, heightCm: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Tỷ lệ mỡ cơ thể (%)</label>
            <input className="form-control" type="number" min={0} max={100} step="0.1" value={form.bodyFatPct}
              onChange={(e) => setForm({ ...form, bodyFatPct: e.target.value })} />
          </div>
          <div className="form-group">
            <label className="form-label">Ghi chú</label>
            <textarea className="form-control" rows={2} value={form.notes}
              onChange={(e) => setForm({ ...form, notes: e.target.value })} />
          </div>
          <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Đang lưu…' : 'Lưu'}</button>
        </form>
      </Modal>
    </>
  )
}
