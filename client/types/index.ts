export enum Role {
  Admin = 1,
  Teacher = 2,
  Student = 3,
}

export type RoleType = Role | 'Admin' | 'Teacher' | 'Student' | 1 | 2 | 3;

export enum AssignmentStatus {
  Draft = 1,
  Published = 2,
}

export enum SubmissionStatus {
  Pending = 1,
  Submitted = 2,
  Late = 3,
  Graded = 4,
  ResubmissionRequested = 5,
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: RoleType;
  classId?: string | null;
}

export interface AuthResponse {
  token: string;
  userId: string;
  fullName: string;
  email: string;
  role: RoleType;
  classId?: string | null;
  expiresAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data: T;
  errors?: string[];
  timestamp: string;
}

export interface ClassEntity {
  id: string;
  name: string;
  section?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Subject {
  id: string;
  name: string;
  code: string;
  classId: string;
  className: string;
  teacherId?: string | null;
  teacherName?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Assignment {
  id: string;
  title: string;
  description: string;
  classId: string;
  className: string;
  subjectId: string;
  subjectName: string;
  teacherId: string;
  teacherName: string;
  deadline: string;
  maxMarks: number;
  status: AssignmentStatus | number;
  allowResubmission: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Submission {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  maxMarks: number;
  studentId: string;
  studentName: string;
  answerText: string;
  attachmentUrl?: string | null;
  submittedAt?: string | null;
  status: SubmissionStatus | number;
  marks?: number | null;
  feedback?: string | null;
  gradedAt?: string | null;
  gradedBy?: string | null;
  gradedByName?: string | null;
  isLate: boolean;
  createdAt: string;
  updatedAt: string;
}

// Helper utility to resolve Role enum or string name
export function getRoleName(role?: RoleType | null): string {
  if (!role) return 'User';
  const r = String(role).toLowerCase();
  if (r === 'admin' || r === '1') return 'Admin';
  if (r === 'teacher' || r === '2') return 'Teacher';
  if (r === 'student' || r === '3') return 'Student';
  return 'User';
}

export function getRoleId(role?: RoleType | null): Role {
  if (!role) return Role.Student;
  const r = String(role).toLowerCase();
  if (r === 'admin' || r === '1') return Role.Admin;
  if (r === 'teacher' || r === '2') return Role.Teacher;
  return Role.Student;
}
