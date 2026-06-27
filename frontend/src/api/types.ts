// TypeScript shapes mirroring the backend DTOs (ASP.NET Core serializes to camelCase).

export type Role = 'ADMIN' | 'COACH' | 'MEMBER'

export interface AuthResponse {
  token: string
  expiresAt: string
  userId: number
  username: string
  role: Role
  fullName: string
}

export interface MessageResponse {
  message: string
}

export interface Member {
  id: number
  userId: number
  username: string
  email: string
  phone?: string | null
  fullName: string
  gender?: string | null
  dateOfBirth?: string | null
  address?: string | null
  packageId: number
  joinDate: string
  expiryDate?: string | null
  status: string
}

export interface Coach {
  id: number
  userId: number
  username: string
  email: string
  phone?: string | null
  fullName: string
  specialization?: string | null
  bio?: string | null
  experience: number
  salary: number
  status: string
}

export interface TrainingClass {
  id: number
  name: string
  coachId?: number | null
  coachName?: string | null
  capacity: number
  currentEnrolled: number
  availableSlots: number
  level?: string | null
  description?: string | null
  isActive: boolean
}

export interface TrainingPackage {
  id: number
  name: string
  durationMonths: number
  price: number
  maxClasses: number
  description?: string | null
  isActive: boolean
}

export interface Schedule {
  id: number
  classId: number
  className: string
  dayOfWeek: string
  startTime: string
  endTime: string
  room?: string | null
  repeatWeekly: boolean
}

export interface Enrollment {
  id: number
  memberId: number
  memberName: string
  classId: number
  className: string
  enrollDate: string
  status: string
}

export interface AdminStats {
  totalMembers: number
  totalCoaches: number
  totalClasses: number
}

export interface CoachDashboard {
  coach: Coach
  classes: TrainingClass[]
  schedules: Schedule[]
}

export interface MemberDashboard {
  member: Member
  enrollments: Enrollment[]
  schedules: Schedule[]
}

export interface AvailableClass {
  class: TrainingClass
  isEnrolled: boolean
  schedules: Schedule[]
}

export interface ClassDetail {
  class: TrainingClass
  enrolledMembers: Enrollment[]
  schedules: Schedule[]
}

export interface MemberProfile {
  member: Member
  packages: TrainingPackage[]
}

// ── Module 1: Equipment ──────────────────────────────────────────────────────
export interface Equipment {
  id: number
  name: string
  category?: string | null
  quantity: number
  status: string
  purchaseDate?: string | null
  notes?: string | null
}

// ── Module 2: Payments & Revenue ─────────────────────────────────────────────
export interface Payment {
  id: number
  memberId: number
  memberName: string
  packageId?: number | null
  amount: number
  method: string
  status: string
  description?: string | null
  paidAt: string
}

export interface MonthlyRevenue {
  year: number
  month: number
  total: number
  count: number
}

export interface Revenue {
  total: number
  paymentCount: number
  monthly: MonthlyRevenue[]
}

// ── Module 3: Attendance ─────────────────────────────────────────────────────
export interface Attendance {
  id: number
  classId: number
  className: string
  memberId: number
  memberName: string
  sessionDate: string
  status: string
  checkedInAt?: string | null
}

export interface AttendanceRosterEntry {
  memberId: number
  memberName: string
  status?: string | null
  checkedInAt?: string | null
}

export interface AttendanceRoster {
  date: string
  class: TrainingClass
  schedules: Schedule[]
  roster: AttendanceRosterEntry[]
}

// ── Module 4: Lesson plans & Progress ────────────────────────────────────────
export interface LessonPlan {
  id: number
  classId: number
  className: string
  coachId: number
  title: string
  content?: string | null
  createdAt: string
}

export interface ProgressNote {
  id: number
  memberId: number
  memberName: string
  coachId: number
  classId?: number | null
  note: string
  rating?: number | null
  recordedAt: string
}

// ── Module 5: Coach ratings ──────────────────────────────────────────────────
export interface RateableCoach {
  id: number
  fullName: string
  specialization?: string | null
  experience: number
  averageRating: number
  ratingCount: number
  myRating?: number | null
  myComment?: string | null
  canRate: boolean
}

export interface CoachRatingItem {
  id: number
  memberId: number
  memberName: string
  rating: number
  comment?: string | null
  createdAt: string
}

export interface CoachRatingSummary {
  average: number
  count: number
  ratings: CoachRatingItem[]
}

// ── Module 6: Health metrics ─────────────────────────────────────────────────
export interface HealthMetric {
  id: number
  recordedDate: string
  weightKg?: number | null
  heightCm?: number | null
  bodyFatPct?: number | null
  notes?: string | null
}

// ── Module 7: PT sessions ────────────────────────────────────────────────────
export interface PtSession {
  id: number
  memberId: number
  memberName: string
  coachId: number
  coachName: string
  sessionDate: string
  startTime: string
  endTime: string
  status: string
  notes?: string | null
}

// ── Module 10: Membership requests (register package → admin approval) ───────
export interface MembershipRequest {
  id: number
  memberId: number
  memberName: string
  packageId: number
  packageName: string
  amount: number
  method: string
  status: string // PENDING | APPROVED | ACTIVE | REJECTED | CANCELLED
  requestedAt: string
  approvedAt?: string | null
  startDate?: string | null
  activatedAt?: string | null
  note?: string | null
  canModify: boolean
}

// A coach-facing unassigned class with its weekly schedule and enrolled members.
export interface CoachAvailableClass {
  class: TrainingClass
  schedules: Schedule[]
  enrolledMembers: Enrollment[]
}

// ── Module 10: Coach class-change requests (claim/release → admin approval) ───
export interface ClassChangeRequest {
  id: number
  coachId: number
  coachName: string
  classId: number
  className: string
  action: string // CLAIM | RELEASE
  status: string // PENDING | APPROVED | REJECTED
  requestedAt: string
  decidedAt?: string | null
  note?: string | null
}

// ── Module 9: Chat ───────────────────────────────────────────────────────────
export interface ChatContact {
  userId: number
  name: string
  role: string
  unreadCount: number
}

export interface ChatMessage {
  id: number
  senderUserId: number
  recipientUserId: number
  body: string
  sentAt: string
  isRead: boolean
  mine: boolean
}
