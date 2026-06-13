import { useState, type FormEvent } from 'react'
import { useNavigate, useSearchParams, Link } from 'react-router-dom'
import { useAuth, dashboardPath } from '../auth/AuthContext'
import { errorMessage } from '../api/client'

export default function Login() {
  const { login, loading } = useAuth()
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')
    try {
      const user = await login(username, password)
      navigate(dashboardPath(user.role), { replace: true })
    } catch (err) {
      setError(errorMessage(err, 'Tên đăng nhập hoặc mật khẩu không đúng.'))
    }
  }

  return (
    <div className="login-shell">
      <div className="login-brand">
        <div className="login-brand-icon">🏆</div>
        <div className="login-brand-name">SPORTS CLUB</div>
        <p className="login-brand-tagline">Vượt qua giới hạn. Theo dõi tiến bộ.</p>
      </div>
      <div className="login-form-area">
        <div className="login-form-card">
          <h2>Chào mừng trở lại</h2>
          <p className="subtitle">Đăng nhập vào tài khoản của bạn</p>

          {error && <div className="alert alert-danger">{error}</div>}
          {params.get('registered') === 'true' && (
            <div className="alert alert-success">Đăng ký thành công! Vui lòng đăng nhập.</div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label className="form-label">Tên đăng nhập</label>
              <input
                className="form-control"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                maxLength={50}
                autoComplete="username"
                placeholder="Nhập tên đăng nhập"
              />
            </div>
            <div className="form-group">
              <label className="form-label">Mật khẩu</label>
              <input
                className="form-control"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                autoComplete="current-password"
                placeholder="Nhập mật khẩu"
              />
            </div>
            <button className="btn btn-primary w-100" disabled={loading}>
              {loading ? 'Đang đăng nhập…' : 'Đăng nhập'}
            </button>
          </form>

          <hr className="divider" />
          <p className="text-muted" style={{ textAlign: 'center' }}>
            Chưa có tài khoản? <Link to="/register">Đăng ký</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
