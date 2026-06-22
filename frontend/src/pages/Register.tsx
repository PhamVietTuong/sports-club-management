import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api, errorMessage } from '../api/client'

export default function Register() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    fullName: '',
    phone: '',
    gender: '',
  })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  function update(field: keyof typeof form, value: string) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')
    setBusy(true)
    try {
      await api.post('/auth/register', form)
      navigate('/login?registered=true', { replace: true })
    } catch (err) {
      setError(errorMessage(err, 'Đăng ký thất bại. Vui lòng thử lại.'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="login-shell">
      <div className="login-brand">
        <div className="login-brand-icon">🏆</div>
        <div className="login-brand-name">SPORTS CLUB</div>
        <p className="login-brand-tagline">Tham gia câu lạc bộ ngay hôm nay.</p>
      </div>
      <div className="login-form-area">
        <div className="login-form-card">
          <h2>Tạo tài khoản</h2>
          <p className="subtitle">Đăng ký làm thành viên</p>

          {error && <div className="alert alert-danger">{error}</div>}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label className="form-label">Họ và tên *</label>
              <input className="form-control" value={form.fullName}
                onChange={(e) => update('fullName', e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="form-label">Tên đăng nhập *</label>
              <input className="form-control" value={form.username} maxLength={50}
                onChange={(e) => update('username', e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="form-label">Email *</label>
              <input className="form-control" type="email" value={form.email}
                onChange={(e) => update('email', e.target.value)} required />
            </div>
            <div className="form-row">
              <div className="form-group">
                <label className="form-label">Số điện thoại</label>
                <input className="form-control" type="tel" value={form.phone}
                  pattern="(0|\+84)[0-9]{9}"
                  title="Số điện thoại không hợp lệ (định dạng: 0xxxxxxxxx hoặc +84xxxxxxxxx)."
                  onChange={(e) => update('phone', e.target.value)} />
              </div>
              <div className="form-group">
                <label className="form-label">Giới tính</label>
                <select className="form-control" value={form.gender}
                  onChange={(e) => update('gender', e.target.value)}>
                  <option value="">--</option>
                  <option value="MALE">Nam</option>
                  <option value="FEMALE">Nữ</option>
                  <option value="OTHER">Khác</option>
                </select>
              </div>
            </div>
            <div className="form-row">
              <div className="form-group">
                <label className="form-label">Mật khẩu *</label>
                <input className="form-control" type="password" value={form.password}
                  onChange={(e) => update('password', e.target.value)} required />
              </div>
              <div className="form-group">
                <label className="form-label">Xác nhận mật khẩu *</label>
                <input className="form-control" type="password" value={form.confirmPassword}
                  onChange={(e) => update('confirmPassword', e.target.value)} required />
              </div>
            </div>
            <p className="text-muted" style={{ fontSize: 12, marginTop: -4 }}>
              Mật khẩu tối thiểu 8 ký tự, gồm ít nhất một chữ cái và một chữ số.
            </p>
            <button className="btn btn-primary w-100" disabled={busy}>
              {busy ? 'Đang xử lý…' : 'Đăng ký'}
            </button>
          </form>

          <hr className="divider" />
          <p className="text-muted" style={{ textAlign: 'center' }}>
            Đã có tài khoản? <Link to="/login">Đăng nhập</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
