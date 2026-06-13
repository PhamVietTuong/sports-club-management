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
}

export interface ClassDetail {
  class: TrainingClass
  enrolledMembers: Enrollment[]
}

export interface MemberProfile {
  member: Member
  packages: TrainingPackage[]
}
