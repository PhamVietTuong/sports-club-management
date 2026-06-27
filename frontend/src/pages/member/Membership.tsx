import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { MembershipRequest, MemberProfile, Payment, TrainingPackage } from '../../api/types'
import Pagination from '../../components/Pagination'
import { usePaged } from '../../hooks/usePaged'
import { useClientPaged } from '../../hooks/useClientPaged'

const METHOD_LABEL: Record<string, string> = {
  CASH: 'Tiền mặt', CARD: 'Thẻ', TRANSFER: 'Chuyển khoản',
}

const STATUS_LABEL: Record<string, string> = {
  PENDING: 'Chờ duyệt', APPROVED: 'Đã duyệt', ACTIVE: 'Đang hoạt động',
  REJECTED: 'Bị từ chối', CANCELLED: 'Đã hủy',
}

const STATUS_BADGE: Record<string, string> = {
  PENDING: 'badge-review', APPROVED: 'badge-active', ACTIVE: 'badge-active',
  REJECTED: 'badge-inactive', CANCELLED: 'badge-inactive',
}

function vnd(n: number) { return n.toLocaleString('vi-VN') }

export default function Membership() {
  const [packages, setPackages] = useState<TrainingPackage[]>([])
  const {
    items: payments, total: payTotal, page: payPage, pageSize: payPageSize,
    setPage: setPayPage, search: paySearch, setSearch: setPaySearch, reload: reloadPayments,
  } = usePaged<Payment>('/member/payments')
  const [requests, setRequests] = useState<MembershipRequest[]>([])
  const [profile, setProfile] = useState<MemberProfile | null>(null)
  const [method, setMethod] = useState('CASH')
  const [error, setError] = useState('')
  const [flash, setFlash] = useState('')
  const [busyId, setBusyId] = useState(0)

  function load() {
    api.get<MemberProfile>('/member/profile')
      .then((res) => { setProfile(res.data); setPackages(res.data.packages) })
      .catch((err) => setError(errorMessage(err, 'Không thể tải dữ liệu.')))
    api.get<MembershipRequest[]>('/member/membership/requests')
      .then((res) => setRequests(res.data))
      .catch(() => {})
    reloadPayments()
  }
  useEffect(load, [])

  // A member may only have one in-flight request (PENDING/APPROVED) at a time.
  const hasOpen = requests.some((r) => r.status === 'PENDING' || r.status === 'APPROVED')

  const {
    pageItems: pageRequests, total: reqTotal, page: reqPage, pageSize: reqPageSize,
    setPage: setReqPage, search: reqSearch, setSearch: setReqSearch,
  } = useClientPaged(requests, (r, q) =>
    r.packageName.toLowerCase().includes(q) || r.status.toLowerCase().includes(q))

  async function request(pkg: TrainingPackage) {
    if (!window.confirm(`Gửi yêu cầu đăng ký gói "${pkg.name}" (${vnd(pkg.price)})?`)) return
    setBusyId(pkg.id); setError(''); setFlash('')
    try {
      const res = await api.post<{ message: string }>('/member/membership/request', {
        packageId: pkg.id, method,
      })
      setFlash(res.data.message)
      load()
    } catch (err) { setError(errorMessage(err)) } finally { setBusyId(0) }
  }

  async function act(url: string, confirmMsg?: string) {
    if (confirmMsg && !window.confirm(confirmMsg)) return
    setError(''); setFlash('')
    try {
      const res = await api.post<{ message: string }>(url)
      setFlash(res.data.message)
      load()
    } catch (err) { setError(errorMessage(err)) }
  }

  async function change(r: MembershipRequest) {
    const input = window.prompt(
      'Đổi sang gói nào? Nhập ID gói:\n' +
        packages.map((p) => `${p.id} — ${p.name} (${vnd(p.price)})`).join('\n'),
      String(r.packageId),
    )
    if (!input) return
    const packageId = Number(input)
    if (!packageId) { setError('ID gói không hợp lệ.'); return }
    setError(''); setFlash('')
    try {
      const res = await api.post<{ message: string }>(
        `/member/membership/requests/${r.id}/change`, { packageId })
      setFlash(res.data.message)
      load()
    } catch (err) { setError(errorMessage(err)) }
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
        <div className="card-title">Yêu cầu gói tập của tôi</div>
        <p className="text-muted" style={{ marginTop: -8 }}>
          Sau khi quản trị viên duyệt, bạn có 24 giờ để hủy hoặc đổi gói — trước khi
          kích hoạt hoặc đăng ký lớp đầu tiên.
        </p>
        <div className="table-toolbar">
          <input className="form-control search-input" placeholder="Tìm theo gói / trạng thái…"
            value={reqSearch} onChange={(e) => setReqSearch(e.target.value)} />
        </div>
        <div className="table-wrap">
          <table>
            <thead>
              <tr><th>Gói</th><th>Số tiền</th><th>Trạng thái</th><th>Ngày yêu cầu</th><th>Ghi chú</th><th></th></tr>
            </thead>
            <tbody>
              {pageRequests.map((r) => (
                <tr key={r.id}>
                  <td>{r.packageName}</td>
                  <td>{vnd(r.amount)}</td>
                  <td>
                    <span className={'badge ' + (STATUS_BADGE[r.status] ?? '')}>
                      {STATUS_LABEL[r.status] ?? r.status}
                    </span>
                  </td>
                  <td>{new Date(r.requestedAt).toLocaleString('vi-VN')}</td>
                  <td>{r.note || '—'}</td>
                  <td className="actions">
                    {r.status === 'APPROVED' && (
                      <button className="btn btn-primary btn-sm"
                        onClick={() => act(`/member/membership/requests/${r.id}/activate`,
                          'Kích hoạt gói tập ngay? Sau khi kích hoạt sẽ không thể hủy/đổi.')}>
                        Kích hoạt
                      </button>
                    )}
                    {r.canModify && (
                      <>
                        <button className="btn btn-ghost btn-sm" onClick={() => change(r)}>Đổi gói</button>
                        <button className="btn btn-danger btn-sm"
                          onClick={() => act(`/member/membership/requests/${r.id}/cancel`, 'Hủy yêu cầu này?')}>
                          Hủy
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
              {pageRequests.length === 0 && <tr><td colSpan={6} className="empty">Chưa có yêu cầu nào.</td></tr>}
            </tbody>
          </table>
        </div>
        <Pagination page={reqPage} pageSize={reqPageSize} total={reqTotal} onPage={setReqPage} />
      </div>

      <div className="card mt-4">
        <div className="card-title">Đăng ký gói tập</div>
        {hasOpen && (
          <div className="alert alert-danger">
            Bạn đang có một yêu cầu chờ xử lý. Hãy hoàn tất hoặc hủy yêu cầu đó trước khi đăng ký gói mới.
          </div>
        )}
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
                    <button className="btn btn-primary btn-sm" disabled={busyId === p.id || hasOpen}
                      onClick={() => request(p)}>
                      {busyId === p.id ? 'Đang xử lý…' : 'Gửi yêu cầu'}
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
        <div className="table-toolbar">
          <input className="form-control search-input" placeholder="Tìm theo mô tả / phương thức…"
            value={paySearch} onChange={(e) => setPaySearch(e.target.value)} />
        </div>
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
        <Pagination page={payPage} pageSize={payPageSize} total={payTotal} onPage={setPayPage} />
      </div>
    </>
  )
}
