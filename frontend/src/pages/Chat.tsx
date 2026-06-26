import { useEffect, useRef, useState, type FormEvent } from 'react'
import { HubConnectionBuilder, HubConnectionState, type HubConnection } from '@microsoft/signalr'
import { api, errorMessage, tokenStore } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import type { ChatContact, ChatMessage } from '../api/types'

export default function Chat() {
  const { user } = useAuth()
  const myId = user?.userId ?? 0

  const [contacts, setContacts] = useState<ChatContact[]>([])
  const [activeId, setActiveId] = useState<number | null>(null)
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [draft, setDraft] = useState('')
  const [error, setError] = useState('')
  const [connected, setConnected] = useState(false)

  const endRef = useRef<HTMLDivElement>(null)
  const connRef = useRef<HubConnection | null>(null)
  // Refs so the (long-lived) ReceiveMessage handler always sees current values.
  const activeIdRef = useRef<number | null>(null)
  activeIdRef.current = activeId

  function loadContacts() {
    api.get<ChatContact[]>('/chat/contacts')
      .then((res) => setContacts(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh bạ.')))
  }
  function loadConversation(userId: number) {
    // GET marks the thread read server-side and returns the full history.
    api.get<ChatMessage[]>(`/chat/conversation/${userId}`)
      .then((res) => setMessages(res.data))
      .catch((err) => setError(errorMessage(err)))
  }

  // Open one SignalR connection for the lifetime of the page.
  useEffect(() => {
    // `disposed` guards against React 18 StrictMode's mount→unmount→mount in dev:
    // the first connection's cleanup aborts its own start(), which must not surface
    // as an error on the (real) second connection.
    let disposed = false
    loadContacts()
    const conn = new HubConnectionBuilder()
      .withUrl('/hubs/chat', { accessTokenFactory: () => tokenStore.get() ?? '' })
      .withAutomaticReconnect()
      .build()
    connRef.current = conn

    conn.on('ReceiveMessage', (m: { senderUserId: number; recipientUserId: number }) => {
      const other = m.senderUserId === myId ? m.recipientUserId : m.senderUserId
      // If the message belongs to the open thread, refresh it (also marks read).
      if (other === activeIdRef.current) loadConversation(other)
      // Always refresh contacts so unread badges stay current.
      loadContacts()
    })
    conn.onreconnected(() => { setConnected(true); setError('') })
    conn.onreconnecting(() => setConnected(false))
    conn.onclose(() => setConnected(false))

    conn.start()
      .then(() => { if (!disposed) { setConnected(true); setError('') } })
      .catch(() => { if (!disposed) setError('Không thể kết nối máy chủ trò chuyện.') })

    return () => { disposed = true; conn.stop() }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (activeId !== null) loadConversation(activeId)
  }, [activeId])

  useEffect(() => { endRef.current?.scrollIntoView({ behavior: 'smooth' }) }, [messages])

  async function send(e: FormEvent) {
    e.preventDefault()
    const body = draft.trim()
    if (!body || activeId === null) return
    if (connRef.current?.state !== HubConnectionState.Connected) {
      setError('Chưa kết nối. Vui lòng thử lại.')
      return
    }
    setDraft('')
    try {
      // Server persists and pushes ReceiveMessage back to both parties.
      await connRef.current.invoke('SendMessage', activeId, body)
    } catch (err) {
      setError(errorMessage(err, 'Không gửi được tin nhắn.'))
      setDraft(body)
    }
  }

  const active = contacts.find((c) => c.userId === activeId) ?? null

  return (
    <>
      <div className="page-header"><h1>Tin nhắn</h1></div>
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="chat-layout">
        <div className="chat-contacts">
          {contacts.map((c) => (
            <button key={c.userId}
              className={'chat-contact' + (c.userId === activeId ? ' active' : '')}
              onClick={() => setActiveId(c.userId)}>
              <span className="chat-contact-name">{c.name}</span>
              {c.unreadCount > 0 && <span className="chat-unread">{c.unreadCount}</span>}
            </button>
          ))}
          {contacts.length === 0 && <p className="text-muted" style={{ padding: 12 }}>Chưa có liên hệ nào.</p>}
        </div>

        <div className="chat-thread">
          {active === null ? (
            <p className="text-muted" style={{ padding: 16 }}>Chọn một liên hệ để bắt đầu trò chuyện.</p>
          ) : (
            <>
              <div className="chat-thread-header">
                {active.name}
                {!connected && <span className="text-muted" style={{ fontSize: 12, fontWeight: 400 }}> · đang kết nối…</span>}
              </div>
              <div className="chat-messages">
                {messages.map((m) => (
                  <div key={m.id} className={'chat-bubble' + (m.mine ? ' mine' : '')}>
                    <div>{m.body}</div>
                    <div className="chat-time">{new Date(m.sentAt).toLocaleString('vi-VN')}</div>
                  </div>
                ))}
                {messages.length === 0 && <p className="text-muted">Chưa có tin nhắn.</p>}
                <div ref={endRef} />
              </div>
              <form className="chat-input" onSubmit={send}>
                <input className="form-control" placeholder="Nhập tin nhắn…" value={draft}
                  maxLength={2000} onChange={(e) => setDraft(e.target.value)} />
                <button className="btn btn-primary" disabled={!draft.trim()}>Gửi</button>
              </form>
            </>
          )}
        </div>
      </div>
    </>
  )
}
