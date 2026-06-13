import { useEffect, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../../api/client'
import type { MemberProfile as Profile } from '../../api/types'

export default function MemberProfile() {
  const [profile, setProfile] = useState<Profile | null>(null)
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')
  const [busy, setBusy] = useState(false)
  const [form, setForm] = useState({
    fullName: '', phone: '', address: '', currentPassword: '', newPassword: '',
  })

  useEffect(() => {
    api.get<Profile>('/member/profile')
      .then((res) => {
        setProfile(res.data)
        setForm((f) => ({
          ...f,
          fullName: res.data.member.fullName,
          phone: res.data.member.phone ?? '',
          address: res.data.member.address ?? '',
        }))
      })
      .catch((err) => setError(errorMessage(err, 'Không thể tải hồ sơ.')))
  }, [])

  async function submit(e: FormEvent) {
    e.preventDefault()
    setBusy(true); setError(''); setFlash('')
    try {
      await api.put('/member/profile', {
        fullName: form.fullName,
        // Always send phone (empty string = explicit clear); the API leaves it
        // unchanged only when the field is omitted/null.
        phone: form.phone,
        address: form.address || null,
        currentPassword: form.currentPassword || null,
        newPassword: form.newPassword || null,
      })
      setFlash('Cập nhật hồ sơ thành công.')
      setForm((f) => ({ ...f, currentPassword: '', newPassword: '' }))
    } catch (err) { setError(errorMessage(err)) } finally { setBusy(false) }
  }

  if (!profile) return <div className="loading">Đang tải…</div>

  return (
    <>
      <div className="page-header"><h1>Hồ sơ của tôi</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      <div className="grid-2">
        <div className="card">
          <div className="card-title">Thông tin cá nhân</div>
          <form onSubmit={submit}>
            <div className="form-group">
              <label className="form-label">Tên đăng nhập</label>
              <input className="form-control" value={profile.member.username} disabled />
            </div>
            <div className="form-group">
              <label className="form-label">Email</label>
              <input className="form-control" value={profile.member.email} disabled />
            </div>
            <div className="form-group">
              <label className="form-label">Họ tên *</label>
              <input className="form-control" value={form.fullName} required
                onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Số điện thoại</label>
              <input className="form-control" value={form.phone}
                onChange={(e) => setForm({ ...form, phone: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Địa chỉ</label>
              <input className="form-control" value={form.address}
                onChange={(e) => setForm({ ...form, address: e.target.value })} />
            </div>

            <hr className="divider" />
            <div className="card-title">Đổi mật khẩu</div>
            <p className="text-muted" style={{ fontSize: 12, marginTop: -8 }}>
              Để trống nếu không muốn đổi. Mật khẩu mới tối thiểu 8 ký tự, gồm chữ và số.
            </p>
            <div className="form-group">
              <label className="form-label">Mật khẩu hiện tại</label>
              <input className="form-control" type="password" value={form.currentPassword}
                onChange={(e) => setForm({ ...form, currentPassword: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">Mật khẩu mới</label>
              <input className="form-control" type="password" value={form.newPassword}
                onChange={(e) => setForm({ ...form, newPassword: e.target.value })} />
            </div>

            <button className="btn btn-primary w-100" disabled={busy}>
              {busy ? 'Đang lưu…' : 'Lưu thay đổi'}
            </button>
          </form>
        </div>

        <div className="card">
          <div className="card-title">Tư cách thành viên</div>
          <p className="text-muted">Ngày tham gia: <strong>{profile.member.joinDate}</strong></p>
          <p className="text-muted">Ngày hết hạn: <strong>{profile.member.expiryDate ?? '—'}</strong></p>
          <p className="text-muted">Trạng thái: <strong>{profile.member.status}</strong></p>

          <hr className="divider" />
          <div className="card-title">Gói tập hiện có</div>
          <div className="table-wrap">
            <table>
              <thead><tr><th>Gói</th><th>Thời hạn</th><th>Giá</th></tr></thead>
              <tbody>
                {profile.packages.map((p) => (
                  <tr key={p.id}>
                    <td>{p.name}</td><td>{p.durationMonths} tháng</td>
                    <td>{p.price.toLocaleString('vi-VN')}</td>
                  </tr>
                ))}
                {profile.packages.length === 0 && <tr><td colSpan={3} className="empty">Chưa có gói tập.</td></tr>}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </>
  )
}
