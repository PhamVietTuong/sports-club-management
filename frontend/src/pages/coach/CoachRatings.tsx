import { useEffect, useState } from 'react'
import { api, errorMessage } from '../../api/client'
import type { CoachRatingSummary } from '../../api/types'

function stars(n: number) { return '★'.repeat(Math.round(n)) + '☆'.repeat(5 - Math.round(n)) }

export default function CoachRatings() {
  const [data, setData] = useState<CoachRatingSummary | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get<CoachRatingSummary>('/coach/ratings')
      .then((res) => setData(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải đánh giá.')))
  }, [])

  return (
    <>
      <div className="page-header"><h1>Đánh giá của tôi</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="grid-3">
        <div className="stat-card">
          <div className="stat-icon green">⭐</div>
          <div className="stat-number">{data && data.count > 0 ? `${data.average}/5` : '—'}</div>
          <div className="stat-label">Điểm trung bình</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon blue">💬</div>
          <div className="stat-number">{data?.count ?? '—'}</div>
          <div className="stat-label">Số lượt đánh giá</div>
        </div>
      </div>

      <div className="card mt-4">
        <div className="card-title">Nhận xét từ học viên</div>
        <div className="table-wrap">
          <table>
            <thead><tr><th>Học viên</th><th>Số sao</th><th>Nhận xét</th><th>Ngày</th></tr></thead>
            <tbody>
              {data?.ratings.map((r) => (
                <tr key={r.id}>
                  <td>{r.memberName}</td>
                  <td title={`${r.rating}/5`}>{stars(r.rating)}</td>
                  <td style={{ maxWidth: 360, whiteSpace: 'pre-wrap' }}>{r.comment || '—'}</td>
                  <td>{new Date(r.createdAt).toLocaleDateString('vi-VN')}</td>
                </tr>
              ))}
              {(!data || data.ratings.length === 0) &&
                <tr><td colSpan={4} className="empty">Chưa có đánh giá nào.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}
