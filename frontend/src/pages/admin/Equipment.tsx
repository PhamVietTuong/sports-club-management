import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { Equipment } from '../../api/types'
import Modal from '../../components/Modal'

const STATUS_LABEL: Record<string, string> = {
  AVAILABLE: 'Sẵn sàng',
  IN_USE: 'Đang dùng',
  MAINTENANCE: 'Bảo trì',
  RETIRED: 'Ngừng dùng',
}
const STATUS_BADGE: Record<string, string> = {
  AVAILABLE: 'badge-active',
  IN_USE: 'badge-inactive',
  MAINTENANCE: 'badge-review',
  RETIRED: 'badge-terminated',
}

interface EquipForm {
  id: number
  name: string
  category: string
  quantity: string
  status: string
  purchaseDate: string
  notes: string
}

const emptyForm: EquipForm = {
  id: 0, name: '', category: '', quantity: '1', status: 'AVAILABLE', purchaseDate: '', notes: '',
}

export default function EquipmentPage() {
  const [items, setItems] = useState<Equipment[]>([])
  const [statusFilter, setStatusFilter] = useState('')
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState<EquipForm>(emptyForm)

  function load() {
    const q = statusFilter ? `?status=${statusFilter}` : ''
    api.get<Equipment[]>('/admin/equipment' + q)
      .then((res) => setItems(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh sách thiết bị.')))
  }
  useEffect(load, [statusFilter])

  function openAdd() { setForm(emptyForm); setFormError(''); setOpen(true) }
  function openEdit(e: Equipment) {
    setForm({
      id: e.id, name: e.name, category: e.category ?? '', quantity: String(e.quantity),
      status: e.status, purchaseDate: e.purchaseDate ?? '', notes: e.notes ?? '',
    })
    setFormError(''); setOpen(true)
  }

  async function remove(id: number) {
    if (!window.confirm('Xóa thiết bị này?')) return
    try { await api.delete(`/admin/equipment/${id}`); setFlash('Đã xóa thiết bị.'); load() }
    catch (err) { setError(errorMessage(err)) }
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true); setFormError('')
    const body = {
      name: form.name, category: form.category || null, quantity: Number(form.quantity),
      status: form.status, purchaseDate: form.purchaseDate || null, notes: form.notes || null,
    }
    try {
      if (form.id) await api.put(`/admin/equipment/${form.id}`, body)
      else await api.post('/admin/equipment', body)
      setOpen(false); setFlash(form.id ? 'Đã cập nhật thiết bị.' : 'Đã thêm thiết bị.'); load()
    } catch (err) { setFormError(errorMessage(err)) } finally { setBusy(false) }
  }

  return (
    <>
      <div className="page-header">
        <h1>Thiết bị</h1>
        <div className="toolbar">
          <select className="form-control" value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}>
            <option value="">Tất cả trạng thái</option>
            <option value="AVAILABLE">Sẵn sàng</option>
            <option value="IN_USE">Đang dùng</option>
            <option value="MAINTENANCE">Bảo trì</option>
            <option value="RETIRED">Ngừng dùng</option>
          </select>
          <button className="btn btn-primary" onClick={openAdd}>+ Thêm thiết bị</button>
        </div>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>ID</th><th>Tên</th><th>Nhóm</th><th>Số lượng</th>
              <th>Trạng thái</th><th>Ngày mua</th><th></th>
            </tr>
          </thead>
          <tbody>
            {items.map((e) => (
              <tr key={e.id}>
                <td>{e.id}</td>
                <td>{e.name}</td>
                <td>{e.category || '—'}</td>
                <td>{e.quantity}</td>
                <td>
                  <span className={'badge ' + (STATUS_BADGE[e.status] ?? '')}>
                    {STATUS_LABEL[e.status] ?? e.status}
                  </span>
                </td>
                <td>{e.purchaseDate || '—'}</td>
                <td className="actions">
                  <button className="btn btn-ghost btn-sm" onClick={() => openEdit(e)}>Sửa</button>
                  <button className="btn btn-danger btn-sm" onClick={() => remove(e.id)}>Xóa</button>
                </td>
              </tr>
            ))}
            {items.length === 0 && <tr><td colSpan={7} className="empty">Chưa có thiết bị nào.</td></tr>}
          </tbody>
        </table>
      </div>

      <Modal open={open} title={form.id ? 'Sửa thiết bị' : 'Thêm thiết bị'} onClose={() => { setFormError(''); setOpen(false) }}>
        <form onSubmit={submit}>
          {formError && <div className="alert alert-danger">{formError}</div>}
          <div className="form-group">
            <label className="form-label">Tên thiết bị *</label>
            <input className="form-control" value={form.name} required
              onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Nhóm</label>
              <input className="form-control" value={form.category}
                onChange={(e) => setForm({ ...form, category: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Số lượng</label>
              <input className="form-control" type="number" min={0} value={form.quantity}
                onChange={(e) => setForm({ ...form, quantity: e.target.value })} />
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Trạng thái</label>
              <select className="form-control" value={form.status}
                onChange={(e) => setForm({ ...form, status: e.target.value })}>
                <option value="AVAILABLE">Sẵn sàng</option>
                <option value="IN_USE">Đang dùng</option>
                <option value="MAINTENANCE">Bảo trì</option>
                <option value="RETIRED">Ngừng dùng</option>
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Ngày mua</label>
              <input className="form-control" type="date" value={form.purchaseDate}
                onChange={(e) => setForm({ ...form, purchaseDate: e.target.value })} />
            </div>
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
