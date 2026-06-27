import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { LessonPlan, TrainingClass } from '../../api/types'
import Modal from '../../components/Modal'
import Pagination from '../../components/Pagination'
import { usePaged } from '../../hooks/usePaged'

export default function CoachLessonPlans() {
  const { items: plans, total, page, pageSize, setPage, search, setSearch, error, setError, reload } =
    usePaged<LessonPlan>('/coach/lesson-plans')
  const [classes, setClasses] = useState<TrainingClass[]>([])
  const [formError, setFormError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState({ classId: '', title: '', content: '' })

  useEffect(() => {
    api.get<TrainingClass[]>('/coach/classes').then((res) => setClasses(res.data)).catch(() => {})
  }, [])

  function openAdd() {
    setForm({ classId: classes[0] ? String(classes[0].id) : '', title: '', content: '' })
    setFormError(''); setOpen(true)
  }

  async function remove(id: number) {
    if (!window.confirm('Xóa giáo án này?')) return
    try { await api.delete(`/coach/lesson-plans/${id}`); setFlash('Đã xóa giáo án.'); reload() }
    catch (err) { setError(errorMessage(err)) }
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true); setFormError('')
    try {
      await api.post('/coach/lesson-plans', {
        classId: Number(form.classId), title: form.title, content: form.content || null,
      })
      setOpen(false); setFlash('Đã tạo giáo án.'); reload()
    } catch (err) { setFormError(errorMessage(err)) } finally { setBusy(false) }
  }

  return (
    <>
      <div className="page-header">
        <h1>Giáo án</h1>
        <button className="btn btn-primary" onClick={openAdd} disabled={classes.length === 0}>+ Tạo giáo án</button>
      </div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}
      {classes.length === 0 && <div className="alert alert-warning">Bạn chưa được phân lớp nào.</div>}

      <div className="table-toolbar">
        <input className="form-control search-input" placeholder="Tìm theo tiêu đề / lớp…"
          value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>

      <div className="table-wrap">
        <table>
          <thead><tr><th>Tiêu đề</th><th>Lớp</th><th>Nội dung</th><th>Ngày tạo</th><th></th></tr></thead>
          <tbody>
            {plans.map((p) => (
              <tr key={p.id}>
                <td>{p.title}</td>
                <td>{p.className}</td>
                <td style={{ maxWidth: 320, whiteSpace: 'pre-wrap' }}>{p.content || '—'}</td>
                <td>{new Date(p.createdAt).toLocaleDateString('vi-VN')}</td>
                <td><button className="btn btn-danger btn-sm" onClick={() => remove(p.id)}>Xóa</button></td>
              </tr>
            ))}
            {plans.length === 0 && <tr><td colSpan={5} className="empty">Chưa có giáo án nào.</td></tr>}
          </tbody>
        </table>
      </div>
      <Pagination page={page} pageSize={pageSize} total={total} onPage={setPage} />

      <Modal open={open} title="Tạo giáo án" onClose={() => { setFormError(''); setOpen(false) }}>
        <form onSubmit={submit}>
          {formError && <div className="alert alert-danger">{formError}</div>}
          <div className="form-group">
            <label className="form-label">Lớp học *</label>
            <select className="form-control" value={form.classId} required
              onChange={(e) => setForm({ ...form, classId: e.target.value })}>
              {classes.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Tiêu đề *</label>
            <input className="form-control" value={form.title} required
              onChange={(e) => setForm({ ...form, title: e.target.value })} />
          </div>
          <div className="form-group">
            <label className="form-label">Nội dung</label>
            <textarea className="form-control" rows={5} value={form.content}
              onChange={(e) => setForm({ ...form, content: e.target.value })} />
          </div>
          <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Đang lưu…' : 'Lưu'}</button>
        </form>
      </Modal>
    </>
  )
}
