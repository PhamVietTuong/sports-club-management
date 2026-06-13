import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { TrainingPackage } from '../../api/types'
import Modal from '../../components/Modal'

interface PkgForm {
  id: number
  name: string
  durationMonths: string
  price: string
  maxClasses: string
  description: string
  isActive: boolean
}

const emptyForm: PkgForm = {
  id: 0, name: '', durationMonths: '1', price: '0', maxClasses: '0', description: '', isActive: true,
}

export default function Packages() {
  const [packages, setPackages] = useState<TrainingPackage[]>([])
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState<PkgForm>(emptyForm)

  function load() {
    api.get<TrainingPackage[]>('/admin/packages')
      .then((res) => setPackages(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh sách gói tập.')))
  }
  useEffect(load, [])

  function openAdd() { setForm(emptyForm); setOpen(true) }
  function openEdit(p: TrainingPackage) {
    setForm({
      id: p.id, name: p.name, durationMonths: String(p.durationMonths), price: String(p.price),
      maxClasses: String(p.maxClasses), description: p.description ?? '', isActive: p.isActive,
    })
    setOpen(true)
  }

  async function clone(id: number) {
    try { await api.post(`/admin/packages/${id}/clone`); setFlash('Đã nhân bản gói tập (+20% giá).'); load() }
    catch (err) { setError(errorMessage(err)) }
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true); setError('')
    const body = {
      name: form.name, durationMonths: Number(form.durationMonths), price: Number(form.price),
      maxClasses: Number(form.maxClasses), description: form.description || null, isActive: form.isActive,
    }
    try {
      if (form.id) await api.put(`/admin/packages/${form.id}`, body)
      else await api.post('/admin/packages', body)
      setOpen(false); setFlash(form.id ? 'Đã cập nhật gói tập.' : 'Đã thêm gói tập.'); load()
    } catch (err) { setError(errorMessage(err)) } finally { setBusy(false) }
  }

  return (
    <>
      <div className="page-header">
        <h1>Gói tập</h1>
        <button className="btn btn-primary" onClick={openAdd}>+ Thêm gói tập</button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>ID</th><th>Tên gói</th><th>Thời hạn</th><th>Giá</th>
              <th>Số lớp tối đa</th><th>Trạng thái</th><th></th>
            </tr>
          </thead>
          <tbody>
            {packages.map((p) => (
              <tr key={p.id}>
                <td>{p.id}</td>
                <td>{p.name}</td>
                <td>{p.durationMonths} tháng</td>
                <td>{p.price.toLocaleString('vi-VN')}</td>
                <td>{p.maxClasses}</td>
                <td>
                  <span className={'badge ' + (p.isActive ? 'badge-active' : 'badge-inactive')}>
                    {p.isActive ? 'Hoạt động' : 'Ẩn'}
                  </span>
                </td>
                <td className="actions">
                  <button className="btn btn-ghost btn-sm" onClick={() => openEdit(p)}>Sửa</button>
                  <button className="btn btn-ghost btn-sm" onClick={() => clone(p.id)}>Nhân bản</button>
                </td>
              </tr>
            ))}
            {packages.length === 0 && <tr><td colSpan={7} className="empty">Chưa có gói tập nào.</td></tr>}
          </tbody>
        </table>
      </div>

      <Modal open={open} title={form.id ? 'Sửa gói tập' : 'Thêm gói tập'} onClose={() => setOpen(false)}>
        <form onSubmit={submit}>
          <div className="form-group">
            <label className="form-label">Tên gói *</label>
            <input className="form-control" value={form.name} required
              onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Thời hạn (tháng)</label>
              <input className="form-control" type="number" min={1} value={form.durationMonths}
                onChange={(e) => setForm({ ...form, durationMonths: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Giá</label>
              <input className="form-control" type="number" min={0} step="0.01" value={form.price}
                onChange={(e) => setForm({ ...form, price: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Số lớp tối đa</label>
            <input className="form-control" type="number" min={0} value={form.maxClasses}
              onChange={(e) => setForm({ ...form, maxClasses: e.target.value })} />
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
    </>
  )
}
