# Assignment & Submission Management System

A full-stack, role-based web application designed for academic institutions to streamline class creation, subject management, assignment publishing, student submissions, evaluation, and grading.

---

## 🚀 Tech Stack Summary

- **Backend**: ASP.NET Core 8 Web API, MongoDB (Official Driver + MongoDB Atlas), Serilog, JWT Authentication, BCrypt, xUnit. *(See [server/README.md](server/README.md) for complete details)*
- **Frontend**: Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4, Axios, Lucide React icons. *(See [client/README.md](client/README.md) for complete details)*

---

## 📂 Project Structure

```text
Assignment-Submission-Management-System/
├── server/     # ASP.NET Core 8 RESTful Web API & xUnit Test Suite (See server/README.md)
├── client/     # Next.js 16 App Router Role-Based Frontend (See client/README.md)
└── README.md   # Root System Architecture & Evaluator Guide
```

- **[server/](server/)**: Contains `AssignmentManagementSystem.API` (Controllers, Repositories, Services, Models, DTOs, DataSeeder) and `AssignmentManagementSystem.Tests` (24 business rule & authorization unit tests).
- **[client/](client/)**: Contains Next.js App Router client with role-based navigation portals for Administrators, Teachers, and Students.

---

## ⚡ Quick Start Guide

Follow these steps to run both backend and frontend applications locally:

### 1. Launch the Backend Web API (`/server`)

Ensure .NET 8 SDK and MongoDB (local or Atlas) are installed/configured.

```bash
# Navigate to the API project directory
cd server/AssignmentManagementSystem.API

# Copy appsettings template and configure MongoDB connection string
cp appsettings.Development.json.example appsettings.Development.json

# Restore, build, and launch API
dotnet run
```
> ℹ️ **Note**: On startup, the API automatically tests connection to MongoDB Atlas and executes `DataSeeder` to populate initial demo data if the database is empty.
>
> 📖 **Swagger API Documentation**: `http://localhost:5071/swagger`

---

### 2. Launch the Frontend Client (`/client`)

In a new terminal window:

```bash
# Navigate to the client project directory
cd client

# Copy environment template
cp .env.local.example .env.local

# Install dependencies and start Next.js dev server
npm install
npm run dev
```
> 🌐 **Client Application**: `http://localhost:3000`

---

## 🔑 Pre-Seeded Demo Credentials

The evaluator can log in immediately using any of the pre-populated demo accounts:

| Role | Email | Password | Details & Context |
| :--- | :--- | :--- | :--- |
| **System Admin** | `admin@school.com` | `Admin@123` | Full access to Classes & Subjects management |
| **Teacher 1** | `teacher1@school.com` | `Teacher@123` | Sarah Connor (Mathematics & Science) |
| **Teacher 2** | `teacher2@school.com` | `Teacher@123` | Walter White (English Literature & World History) |
| **Student 1** | `student1@school.com` | `Student@123` | Alex Mercer (Class 10 - Section A) |
| **Student 2** | `student2@school.com` | `Student@123` | Emma Watson (Class 10 - Section A) |
| **Student 3** | `student3@school.com` | `Student@123` | Peter Parker (Class 10 - Section B) |

---

## 📖 Feature Overview & Role Capabilities

### 👨‍💼 Administrator Module (`/admin`)
- Create, view, update, and soft-delete academic **Classes** (Name, Section).
- Create, view, update, and soft-delete **Subjects** (Code, Name, Class association).
- Assign or reassign Teachers to Subjects.

### 👩‍🏫 Teacher Module (`/teacher`)
- View assignment overview metrics (Drafts vs Published).
- Create, update, publish, and delete **Assignments** for assigned subjects.
- Enforce submission rules (Deadline, Max Marks, Resubmission policy).
- Review student answers, inspect attachment URL links, evaluate submissions, and record marks and feedback.

### 👨‍🎓 Student Module (`/student`)
- View class coursework and assignment status badges (`Not Submitted`, `Submitted`, `Late`, `Graded`).
- Interactive submission portal (`AnswerText` and optional `AttachmentUrl` link).
- Resubmit answers prior to deadline (if permitted by teacher policy).
- View evaluated scores, marks breakdown, and teacher feedback.

---

## 💡 Key Architectural Assumptions & Limitations

- **Authentication**: JWT token authentication with role claims. Server uses HTTP Bearer token headers; Next.js syncs token to both `localStorage` and `auth_token` cookies for Edge Middleware route protection.
- **Attachment Storage**: Attachment inputs accept direct URL links (e.g. Google Drive, GitHub repository link) rather than binary file upload.
- **Database Resilience**: MongoDB Atlas connections use 30-second timeouts with a 3-attempt cold-start retry loop to accommodate free-tier M0 cluster latency.
- **Detailed Docs**: Refer to [server/README.md](server/README.md) and [client/README.md](client/README.md) for detailed folder specs.

---

## 🤖 Development Disclosure

This project was built incrementally using AI-assisted pair programming and development tools (Claude / Gemini Antigravity IDE), adhering to clean architecture, SOLID principles, automated unit testing, and modern UI design practices.
