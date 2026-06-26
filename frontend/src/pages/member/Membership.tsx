import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { MemberProfile, Payment, TrainingPackage } from '../../api/types'

const METHOD_LABEL: Record<string, string> = {
  CASH: 'Tiền mặt', CARD: 'Thẻ', TRANSFER: 'Chuyển khoản',
}

function vnd(n: number) { return n.toLocaleString('vi-VN') }

export default function Membership() {
  const [packages, setPackages] = useState<TrainingPackage[]>([])
  const [payments, setPayments] = useState<Payment[]>([])
  const [profile, setProfile] = useState<MemberProfile | null>(null)
  const [method, setMethod] = useState('CASH')
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')
  const [busyId, setBusyId] = useState(0)

  function load() {
    api.get<MemberProfile>('/member/profile')
      .then((res) => { setProfile(res.data); setPackages(res.data.packages) })
      .catch((err) => setError(errorMessage(err, 'Không thể tải dữ liệu.')))
    api.get<Payment[]>('/member/payments')
      .then((res) => setPayments(res.data))
      .catch(() => {})
  }
  useEffect(load, [])

  async function buy(pkg: TrainingPackage) {
    if (!window.confirm(`Mua gói "${pkg.name}" với giá ${vnd(pkg.price)}?`)) return
    setBusyId(pkg.id); setError(''); setFlash('')
    try {
      const res = await api.post<{ message: string }>('/member/membership/buy', {
        packageId: pkg.id, method,
      })
      setFlash(res.data.message)
      load()
    } catch (err) { setError(errorMessage(err)) } finally { setBusyId(0) }
  }

  return (
    <>
      <div className="page-header"><h1>Membership & Thanh toán</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}
      {flash && <div className="alert alert-success">{flash}</div>}

      {profile && (
        <div className="card">
          <div className="card-title">Tư cách thành viên hiện tại</div>
          <p className="text-muted">Ngày hết hạn: <strong>{profile.member.expiryDate ?? '—'}</strong></p>
          <p className="text-muted">Trạng thái: <strong>{profile.member.status}</strong></p>
        </div>
      )}

      <div className="card mt-4">
        <div className="card-title">Chọn gói tập</div>
        <div className="form-group" style={{ maxWidth: 240 }}>
          <label className="form-label">Phương thức thanh toán</label>
          <select className="form-control" value={method} onChange={(e) => setMethod(e.target.value)}>
            <option value="CASH">Tiền mặt</option>
            <option value="CARD">Thẻ</option>
            <option value="TRANSFER">Chuyển khoản</option>
          </select>
        </div>
        <div className="table-wrap">
          <table>
            <thead>
              <tr><th>Gói</th><th>Thời hạn</th><th>Số lớp tối đa</th><th>Giá</th><th></th></tr>
            </thead>
            <tbody>
              {packages.map((p) => (
                <tr key={p.id}>
                  <td>{p.name}</td>
                  <td>{p.durationMonths} tháng</td>
                  <td>{p.maxClasses}</td>
                  <td>{vnd(p.price)}</td>
                  <td>
                    <button className="btn btn-primary btn-sm" disabled={busyId === p.id}
                      onClick={() => buy(p)}>
                      {busyId === p.id ? 'Đang xử lý…' : 'Mua'}
                    </button>
                  </td>
                </tr>
              ))}
              {packages.length === 0 && <tr><td colSpan={5} className="empty">Chưa có gói tập.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card mt-4">
        <div className="card-title">Lịch sử thanh toán</div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Số tiền</th><th>Phương thức</th><th>Mô tả</th><th>Thời gian</th></tr></thead>
            <tbody>
              {payments.map((p) => (
                <tr key={p.id}>
                  <td>{vnd(p.amount)}</td>
                  <td>{METHOD_LABEL[p.method] ?? p.method}</td>
                  <td>{p.description || '—'}</td>
                  <td>{new Date(p.paidAt).toLocaleString('vi-VN')}</td>
                </tr>
              ))}
              {payments.length === 0 && <tr><td colSpan={4} className="empty">Chưa có giao dịch.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}
