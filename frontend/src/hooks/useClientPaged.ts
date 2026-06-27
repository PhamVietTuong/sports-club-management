import { useMemo, useState } from 'react'

const PAGE_SIZE = 10

/**
 * Client-side pagination + filtering for an already-loaded array. Same UX as the
 * server-side `usePaged` (10 rows/page, search box, prev/next) — used for
 * per-user / contextual tables that are fetched in full and don't warrant their
 * own paged endpoint.
 *
 * @param items   the full list
 * @param matches predicate: does an item match the lowercased search query?
 */
export function useClientPaged<T>(
  items: T[],
  matches: (item: T, query: string) => boolean,
  pageSize = PAGE_SIZE,
) {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)

  const q = search.trim().toLowerCase()
  const filtered = useMemo(
    () => (q ? items.filter((it) => matches(it, q)) : items),
    [items, q, matches],
  )

  const total = filtered.length
  const pages = Math.max(1, Math.ceil(total / pageSize))
  const safePage = Math.min(page, pages)
  const pageItems = filtered.slice((safePage - 1) * pageSize, safePage * pageSize)

  function onSearch(value: string) { setSearch(value); setPage(1) }

  return { pageItems, total, page: safePage, pageSize, setPage, search, setSearch: onSearch }
}
