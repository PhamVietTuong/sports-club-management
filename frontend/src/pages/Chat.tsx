import { useEffect, useRef, useState, type FormEvent } from 'react'
import { api, errorMessage } from '../api/client'
import type { ChatContact, ChatMessage } from '../api/types'

export default function Chat() {
  const [contacts, setContacts] = useState<ChatContact[]>([])
  const [activeId, setActiveId] = useState<number | null>(null)
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [draft, setDraft] = useState('')
  const [error, setError] = useState('')
  const endRef = useRef<HTMLDivElement>(null)

  function loadContacts() {
    api.get<ChatContact[]>('/chat/contacts')
      .then((res) => setContacts(res.data))
      .catch((err) => setError(errorMessage(err, 'Không thể tải danh bạ.')))
  }
  function loadConversation(userId: number) {
    api.get<ChatMessage[]>(`/chat/conversation/${userId}`)
      .then((res) => setMessages(res.data))
      .catch((err) => setError(errorMessage(err)))
  }

  // Poll contacts (for unread badges) every 5s.
  useEffect(() => {
    loadContacts()
    const t = setInterval(loadContacts, 5000)
    return () => clearInterval(t)
  }, [])

  // Poll the open conversation every 4s.
  useEffect(() => {
    if (activeId === null) return
    loadConversation(activeId)
    const t = setInterval(() => loadConversation(activeId), 4000)
    return () => clearInterval(t)
  }, [activeId])

  useEffect(() => { endRef.current?.scrollIntoView({ behavior: 'smooth' }) }, [messages])

  async function send(e: FormEvent) {
    e.preventDefault()
    if (!draft.trim() || activeId === null) return
    const body = draft
    setDraft('')
    try {
      await api.post('/chat/send', { recipientUserId: activeId, body })
      loadConversation(activeId)
      loadContacts()
    } catch (err) { setError(errorMessage(err)); setDraft(body) }
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
              <div className="chat-thread-header">{active.name}</div>
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
