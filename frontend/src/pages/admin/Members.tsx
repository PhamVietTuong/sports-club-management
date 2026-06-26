import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { Member, TrainingPackage } from '../../api/types'
import Modal from '../../components/Modal'

const STATUS_BADGE: Record<string, string> = {
  ACTIVE: 'badge-active',
  INACTIVE: 'badge-inactive',
  SUSPENDED: 'badge-suspended',
}

const emptyForm = {
  username: '', email: '', password: '', fullName: '',
  phone: '', gender: '', address: '', dateOfBirth: '', expiryDate: '', packageId: '',
}

export default function Members() {
  const [members, setMembers] = useState<Member[]>([])
  const [packages, setPackages] = useState<TrainingPackage[]>([])
  const [statusFilter, setStatusFilter] = useState('')
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')
  const [flash, setFlash] = useState('')
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState(emptyForm)
  const [busy, setBusy] = useState(false)

  function load() {
    const q = statusFilter ? `?status=${statusFilter}` : ''
    api.get<Member[]>('/admin/members' + q)
      .then((res) => setMembers(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh sách thành viên.')))
  }

  useEffect(load, [statusFilter])
  useEffect(() => {
    api.get<TrainingPackage[]>('/admin/packages').then((res) => setPackages(res.data)).catch(() => {})
  }, [])

  async function changeStatus(id: number, status: string) {
    try {
      await api.patch(`/admin/members/${id}/status`, { status })
      load()
    } catch (err) {
      setError(errorMessage(err))
    }
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setFormError('')
    try {
      await api.post('/admin/members', {
        username: form.username,
        email: form.email,
        password: form.password,
        fullName: form.fullName,
        phone: form.phone || null,
        gender: form.gender || null,
        address: form.address || null,
        dateOfBirth: form.dateOfBirth || null,
        expiryDate: form.expiryDate || null,
        packageId: form.packageId ? Number(form.packageId) : 0,
      })
      setOpen(false)
      setForm(emptyForm)
      setFlash('Đã thêm thành viên.')
      load()
    } catch (err) {
      setFormError(errorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  return (
    <>
      <div className="page-header">
        <h1>Thành viên</h1>
        <div className="toolbar">
          <select className="form-control" value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}>
            <option value="">Tất cả trạng thái</option>
            <option value="ACTIVE">ACTIVE</option>
            <option value="INACTIVE">INACTIVE</option>
            <option value="SUSPENDED">SUSPENDED</option>
          </select>
          <button className="btn btn-primary" onClick={() => { setFormError(''); setOpen(true) }}>+ Thêm thành viên</button>
        </div>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>ID</th><th>Họ tên</th><th>Tên đăng nhập</th><th>Email</th>
              <th>SĐT</th><th>Ngày tham gia</th><th>Trạng thái</th><th></th>
            </tr>
          </thead>
          <tbody>
            {members.map((m) => (
              <tr key={m.id}>
                <td>{m.id}</td>
                <td>{m.fullName}</td>
                <td>{m.username}</td>
                <td>{m.email}</td>
                <td>{m.phone || '—'}</td>
                <td>{m.joinDate}</td>
                <td><span className={'badge ' + (STATUS_BADGE[m.status] ?? '')}>{m.status}</span></td>
                <td>
                  <select className="form-control btn-sm" value={m.status}
                    onChange={(e) => changeStatus(m.id, e.target.value)}>
                    <option value="ACTIVE">ACTIVE</option>
                    <option value="INACTIVE">INACTIVE</option>
                    <option value="SUSPENDED">SUSPENDED</option>
                  </select>
                </td>
              </tr>
            ))}
            {members.length === 0 && (
              <tr><td colSpan={8} className="empty">Chưa có thành viên nào.</td></tr>
            )}
          </tbody>
        </table>
      </div>

      <Modal open={open} title="Thêm thành viên" onClose={() => { setFormError(''); setOpen(false) }}>
        <form onSubmit={submit}>
          {formError && <div className="alert alert-danger">{formError}</div>}
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Họ tên *</label>
              <input className="form-control" value={form.fullName} required
                onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Tên đăng nhập *</label>
              <input className="form-control" value={form.username} required
                onChange={(e) => setForm({ ...form, username: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Email *</label>
            <input className="form-control" type="email" value={form.email} required
              onChange={(e) => setForm({ ...form, email: e.target.value })} />
          </div>
          <div className="form-group">
            <label className="form-label">Mật khẩu *</label>
            <input className="form-control" type="password" value={form.password} required
              onChange={(e) => setForm({ ...form, password: e.target.value })} />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">SĐT</label>
              <input className="form-control" type="tel" value={form.phone}
                pattern="(0|\+84)[0-9]{9}"
                title="Số điện thoại không hợp lệ (định dạng: 0xxxxxxxxx hoặc +84xxxxxxxxx)."
                onChange={(e) => setForm({ ...form, phone: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Giới tính</label>
              <select className="form-control" value={form.gender}
                onChange={(e) => setForm({ ...form, gender: e.target.value })}>
                <option value="">--</option>
                <option value="MALE">Nam</option>
                <option value="FEMALE">Nữ</option>
                <option value="OTHER">Khác</option>
              </select>
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Ngày sinh</label>
              <input className="form-control" type="date" value={form.dateOfBirth}
                onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Ngày hết hạn</label>
              <input className="form-control" type="date" value={form.expiryDate}
                onChange={(e) => setForm({ ...form, expiryDate: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Gói tập</label>
            <select className="form-control" value={form.packageId}
              onChange={(e) => setForm({ ...form, packageId: e.target.value })}>
              <option value="">-- Không --</option>
              {packages.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Địa chỉ</label>
            <input className="form-control" value={form.address}
              onChange={(e) => setForm({ ...form, address: e.target.value })} />
          </div>
          <button className="btn btn-primary w-100" disabled={busy}>
            {busy ? 'Đang lưu…' : 'Lưu'}
          </button>
        </form>
      </Modal>
    </>
  )
}
