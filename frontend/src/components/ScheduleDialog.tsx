import type { Schedule } from '../api/types'
import { DAY_LABEL } from '../utils/schedule'
import Modal from './Modal'

interface ScheduleDialogProps {
  open: boolean
  title: string
  schedules: Schedule[]
  onClose: () => void
}

/** A reusable dialog that shows a class's weekly workout schedule (session times). */
export default function ScheduleDialog({ open, title, schedules, onClose }: ScheduleDialogProps) {
  return (
    <Modal open={open} title={`Lịch tập: ${title}`} onClose={onClose}>
      <div className="table-wrap">
        <table>
          <thead><tr><th>Ngày</th><th>Bắt đầu</th><th>Kết thúc</th><th>Phòng</th></tr></thead>
          <tbody>
            {schedules.map((s) => (
              <tr key={s.id}>
                <td>{DAY_LABEL[s.dayOfWeek] ?? s.dayOfWeek}</td>
                <td>{s.startTime}</td>
                <td>{s.endTime}</td>
                <td>{s.room || '—'}</td>
              </tr>
            ))}
            {schedules.length === 0 &&
              <tr><td colSpan={4} className="empty">Lớp này chưa có lịch tập.</td></tr>}
          </tbody>
        </table>
      </div>
    </Modal>
  )
}
