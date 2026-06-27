import { useEffect, useState } from 'react'
import { api } from '../../api/client'
import type { Payment, Revenue } from '../../api/types'
import Pagination from '../../components/Pagination'
import { usePaged } from '../../hooks/usePaged'

const METHOD_LABEL: Record<string, string> = {
  CASH: 'Tiền mặt', CARD: 'Thẻ', TRANSFER: 'Chuyển khoản',
}
const STATUS_BADGE: Record<string, string> = {
  COMPLETED: 'badge-active', PENDING: 'badge-review', REFUNDED: 'badge-terminated',
}
const MONTHS = ['', 'Th1', 'Th2', 'Th3', 'Th4', 'Th5', 'Th6', 'Th7', 'Th8', 'Th9', 'Th10', 'Th11', 'Th12']

function vnd(n: number) { return n.toLocaleString('vi-VN') }

export default function Payments() {
  const [statusFilter, setStatusFilter] = useState('')
  const { items: payments, total, page, pageSize, setPage, search, setSearch, error } =
    usePaged<Payment>('/admin/payments', { status: statusFilter })
  const [revenue, setRevenue] = useState<Revenue | null>(null)

  useEffect(() => {
    api.get<Revenue>('/admin/payments/revenue')
      .then((res) => setRevenue(res.data))
      .catch(() => {})
  }, [])

  return (
    <>
      <div className="page-header"><h1>Thanh toán & Doanh thu</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="grid-3">
        <div className="stat-card">
          <div className="stat-icon green">💰</div>
          <div className="stat-number">{revenue ? vnd(revenue.total) : '—'}</div>
          <div className="stat-label">Tổng doanh thu</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon blue">🧾</div>
          <div className="stat-number">{revenue?.paymentCount ?? '—'}</div>
          <div className="stat-label">Số giao dịch</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon orange">📈</div>
          <div className="stat-number">
            {revenue && revenue.monthly.length > 0 ? vnd(revenue.monthly[0].total) : '—'}
          </div>
          <div className="stat-label">Doanh thu tháng gần nhất</div>
        </div>
      </div>

      <div className="card mt-4">
        <div className="card-title">Doanh thu theo tháng</div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Tháng</th><th>Số giao dịch</th><th>Doanh thu</th></tr></thead>
            <tbody>
              {revenue?.monthly.map((m) => (
                <tr key={`${m.year}-${m.month}`}>
                  <td>{MONTHS[m.month]}/{m.year}</td>
                  <td>{m.count}</td>
                  <td>{vnd(m.total)}</td>
                </tr>
              ))}
              {(!revenue || revenue.monthly.length === 0) &&
                <tr><td colSpan={3} className="empty">Chưa có doanh thu.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card mt-4">
        <div className="card-title">Lịch sử thanh toán</div>
        <div className="table-toolbar">
          <input className="form-control search-input" placeholder="Tìm theo thành viên / mô tả / phương thức…"
            value={search} onChange={(e) => setSearch(e.target.value)} />
          <select className="form-control filter-select" value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}>
            <option value="">Tất cả trạng thái</option>
            <option value="COMPLETED">Hoàn tất</option>
            <option value="PENDING">Chờ xử lý</option>
            <option value="REFUNDED">Hoàn tiền</option>
          </select>
        </div>
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>ID</th><th>Thành viên</th><th>Số tiền</th><th>Phương thức</th>
                <th>Trạng thái</th><th>Mô tả</th><th>Thời gian</th>
              </tr>
            </thead>
            <tbody>
              {payments.map((p) => (
                <tr key={p.id}>
                  <td>{p.id}</td>
                  <td>{p.memberName}</td>
                  <td>{vnd(p.amount)}</td>
                  <td>{METHOD_LABEL[p.method] ?? p.method}</td>
                  <td><span className={'badge ' + (STATUS_BADGE[p.status] ?? '')}>{p.status}</span></td>
                  <td>{p.description || '—'}</td>
                  <td>{new Date(p.paidAt).toLocaleString('vi-VN')}</td>
                </tr>
              ))}
              {payments.length === 0 && <tr><td colSpan={7} className="empty">Chưa có thanh toán nào.</td></tr>}
            </tbody>
          </table>
        </div>
        <Pagination page={page} pageSize={pageSize} total={total} onPage={setPage} />
      </div>
    </>
  )
}
