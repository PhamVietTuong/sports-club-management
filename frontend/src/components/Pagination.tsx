interface PaginationProps {
  page: number
  pageSize: number
  total: number
  onPage: (page: number) => void
}

/** Prev/next pager with "showing X–Y of N · page P/T" status. */
export default function Pagination({ page, pageSize, total, onPage }: PaginationProps) {
  const pages = Math.max(1, Math.ceil(total / pageSize))
  const from = total === 0 ? 0 : (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, total)

  return (
    <div className="pagination">
      <span className="text-muted">
        {total === 0 ? 'Không có dữ liệu' : `${from}–${to} / ${total} · Trang ${page}/${pages}`}
      </span>
      <div className="pagination-controls">
        <button className="btn btn-ghost btn-sm" disabled={page <= 1} onClick={() => onPage(page - 1)}>
          ‹ Trước
        </button>
        <button className="btn btn-ghost btn-sm" disabled={page >= pages} onClick={() => onPage(page + 1)}>
          Sau ›
        </button>
      </div>
    </div>
  )
}
