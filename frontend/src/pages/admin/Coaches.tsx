import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { Coach } from '../../api/types'
import Modal from '../../components/Modal'

const emptyCreate = {
  username: '', email: '', password: '', fullName: '',
  phone: '', specialization: '', bio: '', experience: '0', salary: '0',
}

export default function Coaches() {
  const [coaches, setCoaches] = useState<Coach[]>([])
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState(emptyCreate)
  const [editing, setEditing] = useState<Coach | null>(null)

  function load() {
    api.get<Coach[]>('/admin/coaches')
      .then((res) => setCoaches(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh sách huấn luyện viên.')))
  }
  useEffect(load, [])

  async function submitCreate(e: FormEvent) {
    e.preventDefault()
    setBusy(true); setError('')
    try {
      await api.post('/admin/coaches', {
        username: createForm.username, email: createForm.email, password: createForm.password,
        fullName: createForm.fullName, phone: createForm.phone || null,
        specialization: createForm.specialization || null, bio: createForm.bio || null,
        experience: Number(createForm.experience), salary: Number(createForm.salary),
      })
      setCreateOpen(false); setCreateForm(emptyCreate); setFlash('Đã thêm huấn luyện viên.'); load()
    } catch (err) { setError(errorMessage(err)) } finally { setBusy(false) }
  }

  async function submitEdit(e: FormEvent) {
    e.preventDefault()
    if (!editing) return
    setBusy(true); setError('')
    try {
      await api.put(`/admin/coaches/${editing.id}`, {
        fullName: editing.fullName, specialization: editing.specialization,
        bio: editing.bio, experience: editing.experience, salary: editing.salary,
      })
      setEditing(null); setFlash('Đã cập nhật huấn luyện viên.'); load()
    } catch (err) { setError(errorMessage(err)) } finally { setBusy(false) }
  }

  return (
    <>
      <div className="page-header">
        <h1>Huấn luyện viên</h1>
        <button className="btn btn-primary" onClick={() => setCreateOpen(true)}>+ Thêm HLV</button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>ID</th><th>Họ tên</th><th>Chuyên môn</th><th>Kinh nghiệm</th>
              <th>Lương</th><th>Email</th><th></th>
            </tr>
          </thead>
          <tbody>
            {coaches.map((c) => (
              <tr key={c.id}>
                <td>{c.id}</td>
                <td>{c.fullName}</td>
                <td>{c.specialization || '—'}</td>
                <td>{c.experience} năm</td>
                <td>{c.salary.toLocaleString('vi-VN')}</td>
                <td>{c.email}</td>
                <td>
                  <button className="btn btn-ghost btn-sm" onClick={() => setEditing({ ...c })}>Sửa</button>
                </td>
              </tr>
            ))}
            {coaches.length === 0 && <tr><td colSpan={7} className="empty">Chưa có HLV nào.</td></tr>}
          </tbody>
        </table>
      </div>

      <Modal open={createOpen} title="Thêm huấn luyện viên" onClose={() => setCreateOpen(false)}>
        <form onSubmit={submitCreate}>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Họ tên *</label>
              <input className="form-control" value={createForm.fullName} required
                onChange={(e) => setCreateForm({ ...createForm, fullName: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Tên đăng nhập *</label>
              <input className="form-control" value={createForm.username} required
                onChange={(e) => setCreateForm({ ...createForm, username: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Email *</label>
            <input className="form-control" type="email" value={createForm.email} required
              onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })} />
          </div>
          <div className="form-group">
            <label className="form-label">Mật khẩu *</label>
            <input className="form-control" type="password" value={createForm.password} required
              onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })} />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">SĐT</label>
              <input className="form-control" value={createForm.phone}
                onChange={(e) => setCreateForm({ ...createForm, phone: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Chuyên môn</label>
              <input className="form-control" value={createForm.specialization}
                onChange={(e) => setCreateForm({ ...createForm, specialization: e.target.value })} />
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Kinh nghiệm (năm)</label>
              <input className="form-control" type="number" min={0} value={createForm.experience}
                onChange={(e) => setCreateForm({ ...createForm, experience: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Lương</label>
              <input className="form-control" type="number" min={0} step="0.01" value={createForm.salary}
                onChange={(e) => setCreateForm({ ...createForm, salary: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Giới thiệu</label>
            <textarea className="form-control" rows={2} value={createForm.bio}
              onChange={(e) => setCreateForm({ ...createForm, bio: e.target.value })} />
          </div>
          <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Đang lưu…' : 'Lưu'}</button>
        </form>
      </Modal>

      <Modal open={!!editing} title="Sửa huấn luyện viên" onClose={() => setEditing(null)}>
        {editing && (
          <form onSubmit={submitEdit}>
            <div className="form-group">
              <label className="form-label">Họ tên *</label>
              <input className="form-control" value={editing.fullName} required
                onChange={(e) => setEditing({ ...editing, fullName: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Chuyên môn</label>
              <input className="form-control" value={editing.specialization ?? ''}
                onChange={(e) => setEditing({ ...editing, specialization: e.target.value })} />
            </div>
            <div className="form-row">
              <div className="form-group">
                <label className="form-label">Kinh nghiệm (năm)</label>
                <input className="form-control" type="number" min={0} value={editing.experience}
                  onChange={(e) => setEditing({ ...editing, experience: Number(e.target.value) })} />
              </div>
              <div className="form-group">
                <label className="form-label">Lương</label>
                <input className="form-control" type="number" min={0} step="0.01" value={editing.salary}
                  onChange={(e) => setEditing({ ...editing, salary: Number(e.target.value) })} />
              </div>
            </div>
            <div className="form-group">
              <label className="form-label">Giới thiệu</label>
              <textarea className="form-control" rows={2} value={editing.bio ?? ''}
                onChange={(e) => setEditing({ ...editing, bio: e.target.value })} />
            </div>
            <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Đang lưu…' : 'Cập nhật'}</button>
          </form>
        )}
      </Modal>
    </>
  )
}
