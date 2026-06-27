import { useCallback, useEffect, useState } from 'react'
import { api, errorMessage } from '../api/client'
import type { Paged } from '../api/types'

const PAGE_SIZE = 10

/**
 * Drives a server-side paginated + filtered table. Manages page number and a
 * debounced search term, plus any extra query params (e.g. a status filter).
 * Refetches whenever page / search / filters change, and resets to page 1 when
 * the search or filters change.
 */
export function usePaged<T>(path: string, extra: Record<string, string> = {}, pageSize = PAGE_SIZE) {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [debounced, setDebounced] = useState('')
  const [items, setItems] = useState<T[]>([])
  const [total, setTotal] = useState(0)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  // Serialized so the effect deps are stable across re-renders.
  const extraKey = JSON.stringify(extra)

  useEffect(() => {
    const t = setTimeout(() => setDebounced(search.trim()), 300)
    return () => clearTimeout(t)
  }, [search])

  // Any new search/filter starts back at page 1.
  useEffect(() => { setPage(1) }, [debounced, extraKey])

  const reload = useCallback(() => {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
    if (debounced) params.set('search', debounced)
    for (const [k, v] of Object.entries(JSON.parse(extraKey) as Record<string, string>))
      if (v) params.set(k, v)

    setLoading(true)
    api.get<Paged<T>>(`${path}?${params.toString()}`)
      .then((res) => { setItems(res.data.items); setTotal(res.data.total) })
      .catch((err) => setError(errorMessage(err, 'Không thể tải dữ liệu.')))
      .finally(() => setLoading(false))
  }, [path, page, pageSize, debounced, extraKey])

  useEffect(reload, [reload])

  return { items, total, page, pageSize, setPage, search, setSearch, error, setError, loading, reload }
}
