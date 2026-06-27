import type { Schedule } from '../api/types'

// Backend day-of-week constants → short Vietnamese labels.
export const DAY_LABEL: Record<string, string> = {
  MONDAY: 'T2', TUESDAY: 'T3', WEDNESDAY: 'T4', THURSDAY: 'T5',
  FRIDAY: 'T6', SATURDAY: 'T7', SUNDAY: 'CN',
}

// JS Date.getDay() (0=Sun..6=Sat) → backend day-of-week constant.
const JS_DAY: string[] = [
  'SUNDAY', 'MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY', 'SATURDAY',
]

/** "T2 07:00–08:00 (Studio A), T4 07:00–08:00 (Studio A)" — or "—" when empty. */
export function formatSchedules(schedules: Schedule[]): string {
  if (!schedules || schedules.length === 0) return '—'
  return schedules
    .map((s) => `${DAY_LABEL[s.dayOfWeek] ?? s.dayOfWeek} ${s.startTime}–${s.endTime}` +
      (s.room ? ` (${s.room})` : ''))
    .join(', ')
}

/** The backend day-of-week constant for an ISO date string (yyyy-mm-dd). */
export function weekdayOf(isoDate: string): string {
  return JS_DAY[new Date(isoDate + 'T00:00:00').getDay()]
}

/** Whether the given ISO date falls on one of the class's scheduled weekdays. */
export function isScheduledDay(isoDate: string, schedules: Schedule[]): boolean {
  if (!schedules || schedules.length === 0) return true // no fixed schedule → don't warn
  const wd = weekdayOf(isoDate)
  return schedules.some((s) => s.dayOfWeek === wd)
}
