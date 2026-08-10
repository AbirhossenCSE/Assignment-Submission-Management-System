# Assignment & Submission Management System - Frontend Client

A role-based web interface built with **Next.js 16 (App Router)**, **TypeScript**, and **Tailwind CSS**. Designed for school administrators, teachers, and students to manage classes, subjects, assignments, submissions, and grading.

---

## 🛠️ Tech Stack

- **Framework**: [Next.js 16.3.0](https://nextjs.org/) (App Router, Server & Client Components)
- **UI Library & Styling**: [Tailwind CSS v4](https://tailwindcss.com/) with modern dark-mode aesthetics & glassmorphism
- **Icons**: [Lucide React v1.30.0](https://lucide.dev/)
- **HTTP Client**: [Axios v1.19.0](https://axios-http.com/) (Interceptors for JWT Bearer token & 401 handling)
- **State & Notifications**: React Context Toast Provider (`/components/Toast.tsx`)
- **Language**: TypeScript v5 (Strict mode)

---

## 📂 Project Structure

```text
client/
├── app/                        # Next.js App Router Pages & Layouts
│   ├── (auth)/
│   │   └── login/             # Login Page (Controlled Form + Role Redirect)
│   ├── admin/                  # Admin Module (Classes, Subjects, Teacher Assignment)
│   │   ├── classes/            # Class CRUD Management
│   │   ├── subjects/           # Subject CRUD & Teacher Assignment
│   │   └── layout.tsx          # Shared Admin Sidebar & Top Bar Layout
│   ├── teacher/                # Teacher Module (Assignments & Submission Grading)
│   │   ├── assignments/        # Assignment Creation, Editing, Publishing & Detail View
│   │   │   └── [id]/
│   │   │       └── submissions/# Student Submissions & Grading Portal
│   │   └── layout.tsx          # Shared Teacher Sidebar & Top Bar Layout
│   ├── student/                # Student Module (Coursework & Submissions)
│   │   ├── assignments/        # Class Coursework List & Interactive Submission Form
│   │   │   └── [id]/           # Assignment Detail & Submission / Resubmission Form
│   │   ├── submissions/        # Complete Submission History & Feedback View
│   │   └── layout.tsx          # Shared Student Sidebar & Top Bar Layout
│   ├── layout.tsx              # Root HTML & Inter Font Layout
│   └── page.tsx                # Smart Root Page (Role-Based Auto-Redirect)
├── components/                 # Reusable Modern UI Components
│   ├── Button.tsx              # Variant & State Button Primitive
│   ├── ConfirmDialog.tsx       # Reusable Confirmation Modal
│   ├── Input.tsx               # Styled Label & Text Input Primitive
│   ├── Modal.tsx               # Accessible Backdrop Overlay Modal
│   ├── Select.tsx              # Custom Dropdown Select Primitive
│   ├── Textarea.tsx            # Styled Textarea Input Primitive
│   └── Toast.tsx               # Custom Toast Notification Provider & Hook
├── lib/                        # Utility & Core Helpers
│   ├── api.ts                  # Axios Instance & Interceptors
│   └── auth.ts                 # JWT Token & User Cookie/LocalStorage Sync
├── types/                      # TypeScript Interfaces (DTO Shapes)
│   └── index.ts                # API Response & Domain Entity Shapes
├── middleware.ts               # Next.js Edge Middleware for Role & Auth Protection
├── .env.local.example          # Environment Configuration Template
└── package.json                # Dependencies & Build Scripts
```

---

## 📋 Prerequisites

Before running the client application, ensure you have installed:
- **Node.js**: `v18.0.0` or higher
- **npm**: `v9.0.0` or higher

> ⚠️ **Important Backend Prerequisite**:
> The backend Web API must be running for authentication and data operations to work. Refer to [server/README.md](../server/README.md) for instructions on starting the backend Web API at `http://localhost:5071`.

---

## 🚀 Setup & Getting Started

### 1. Navigate to the client folder
```bash
cd client
```

### 2. Configure Environment Variables
Copy `.env.local.example` to `.env.local`:
```bash
cp .env.local.example .env.local
```

Verify `NEXT_PUBLIC_API_BASE_URL` in `.env.local` points to your backend Web API:
```env
NEXT_PUBLIC_API_BASE_URL=http://localhost:5071/api
```

### 3. Install Dependencies
```bash
npm install
```

### 4. Start the Development Server
```bash
npm run dev
```
Open your browser and navigate to `http://localhost:3000`.

---

## 🏗️ Production Build & Execution

To test or deploy a production bundle:

```bash
# 1. Build optimized Next.js production bundle
npm run build

# 2. Start the production server
npm start
```

---

## 🔑 Demo Credentials

You can log in using any of the following pre-seeded database accounts:

| Role | Email | Password | Assigned Section / Details |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@school.com` | `Admin@123` | Full Administrative Portal |
| **Teacher 1** | `teacher1@school.com` | `Teacher@123` | Sarah Connor (Mathematics & Science) |
| **Teacher 2** | `teacher2@school.com` | `Teacher@123` | Walter White (English & History) |
| **Student 1** | `student1@school.com` | `Student@123` | Alex Mercer (Class 10 - Section A) |
| **Student 2** | `student2@school.com` | `Student@123` | Emma Watson (Class 10 - Section A) |
| **Student 3** | `student3@school.com` | `Student@123` | Peter Parker (Class 10 - Section B) |

---

## 🗺️ Route Map & Access Matrix

| Route | Accessible Roles | Description |
| :--- | :--- | :--- |
| `/login` | Public | Login form with client-side validation |
| `/` | Public | Smart root auto-redirecting authenticated users to their portal |
| `/admin` | `Admin` | Admin Dashboard overview & quick stats |
| `/admin/classes` | `Admin` | Class creation, editing, section management, & deletion |
| `/admin/subjects` | `Admin` | Subject CRUD & Teacher assignment mapping |
| `/teacher` | `Teacher` | Teacher Dashboard overview & assignment stats |
| `/teacher/assignments` | `Teacher` | Assignment list, modal creation/editing, publish & delete actions |
| `/teacher/assignments/[id]` | `Teacher` | Assignment detail view & submission summary |
| `/teacher/assignments/[id]/submissions` | `Teacher` | Student submission review, answer inspection, & grading form |
| `/student` | `Student` | Student Dashboard overview & coursework stats |
| `/student/assignments` | `Student` | Published class coursework list & submission status badges |
| `/student/assignments/[id]` | `Student` | Assignment detail view & interactive submission / resubmission form |
| `/student/submissions` | `Student` | Complete submission history, scores, & teacher feedback |

---

## 💡 Key Architectural Assumptions

1. **Authentication & Route Protection**:
   - On successful login, the JWT token and user object are saved to both `localStorage` (for client API requests) and a browser cookie named `auth_token` (for Next.js Edge Middleware server-side route protection).
2. **Student Class Context**:
   - The student's `classId` is populated on the `User` entity during registration/login response, allowing immediate query filtering for published class assignments (`GET /api/assignments/class/{classId}`).
3. **Attachment Submission**:
   - Submissions support an optional `AttachmentUrl` text string field (e.g. Google Drive, GitHub repository link) rather than multipart binary file upload.
4. **Resubmission Policy**:
   - If an assignment has `allowResubmission: true` and the deadline has not passed, students can update their submission until it is graded by the teacher.

---

## 📌 Known Limitations

- **Attachment File Upload**: File submission is accepted via URL string input (`AttachmentUrl`) rather than raw binary multipart upload.
- **List Pagination**: Table views display full data sets; client-side/server-side pagination control is not yet implemented for large historical record sets.
- **Real-time Updates**: Status updates require page navigation or standard HTTP refetching (WebSocket / SignalR real-time push notifications are not included).
