# Assignment & Submission Management System - Backend API

An ASP.NET Core 8 Web API backend for an educational **Assignment & Submission Management System** built with clean layered architecture, MongoDB, JWT-based authentication, role-based authorization (Admin, Teacher, Student), centralized error handling, automatic input validation, automated database seeding, and comprehensive unit tests.

---

## 🛠️ Tech Stack & Dependencies

- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: MongoDB (using `MongoDB.Driver` v2.23.0) with resilient in-memory fallback for offline development
- **Authentication**: JWT Bearer Tokens (`Microsoft.AspNetCore.Authentication.JwtBearer` v8.0.0)
- **Password Security**: BCrypt (`BCrypt.Net-Next` v4.0.3)
- **Logging**: Serilog with Console & Rolling File sinks (`Serilog.AspNetCore` v8.0.0, `Serilog.Sinks.File` v5.0.0)
- **API Documentation**: OpenAPI / Swagger UI (`Swashbuckle.AspNetCore` v6.4.0)
- **Unit Testing**: xUnit (`xunit` v2.5.3), Moq (`Moq` v4.20.70), FluentAssertions (`FluentAssertions` v6.12.0)

---

## 📁 Project Structure

```
server/
├── AssignmentManagementSystem.API/           # Main Web API Project
│   ├── Common/                               # Shared enums, custom exceptions, API response envelopes
│   │   ├── Enums/                            # Role, AssignmentStatus, SubmissionStatus
│   │   ├── Exceptions/                       # NotFoundException, BadRequestException, ForbiddenException, ConflictException
│   │   └── ApiResponse.cs                    # Standardized JSON response wrapper { success, statusCode, message, data, errors }
│   ├── Configurations/                       # MongoDbSettings, JwtSettings options classes
│   ├── Controllers/                          # REST API controllers (Auth, Class, Subject, Assignment, Submission, Health)
│   ├── DTOs/                                 # Data Transfer Objects with DataAnnotations validation
│   ├── Helpers/                              # ClaimsPrincipalExtensions, CurrentUserService, JwtTokenGenerator, PasswordHasher
│   ├── Middlewares/                          # GlobalExceptionMiddleware (standardized error & 500 safety handling)
│   ├── Models/                               # Domain entities (BaseEntity, User, ClassEntity, Subject, Assignment, Submission)
│   ├── Repositories/                         # Generic IMongoRepository and specialized repositories
│   ├── Seed/                                 # DataSeeder for automatic demo dataset population
│   ├── Services/                             # Business logic layer interfaces and implementations
│   ├── Program.cs                            # Application startup, DI container registration, middleware pipeline
│   └── appsettings.Development.json.example  # Configuration template with placeholder values
│
└── AssignmentManagementSystem.Tests/         # xUnit Unit Test Suite Project
    ├── Helpers/                              # TestDataBuilder fixture factory
    └── Services/                             # AuthServiceTests, SubjectServiceTests, AssignmentServiceTests, SubmissionServiceTests
```

---

## 📋 Prerequisites

Before running the application, ensure you have the following installed:

1. **.NET 8 SDK** (v8.0.x or higher) -> Download from [.NET Official Site](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **MongoDB** (Local instance running on `mongodb://localhost:27017` or a MongoDB Atlas connection string).
   * *Note*: If MongoDB is not running locally, the API automatically operates in **Resilient Dev Mode** (in-memory storage fallback), allowing immediate development and testing without crashing!

---

## ⚙️ Setup & Configuration Instructions

### 1. Clone & Navigate
Navigate to the `server/AssignmentManagementSystem.API` directory:
```bash
cd server/AssignmentManagementSystem.API
```

### 2. Configure Settings
Copy the configuration template `appsettings.Development.json.example` to `appsettings.Development.json`:
```bash
# On PowerShell (Windows):
Copy-Item appsettings.Development.json.example appsettings.Development.json

# On Linux / macOS / Bash:
cp appsettings.Development.json.example appsettings.Development.json
```

Update `appsettings.Development.json` with your actual MongoDB connection string and a secure JWT secret key (minimum 32 characters):
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "AssignmentManagementDb"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SUPER_SECRET_STRONG_KEY_AT_LEAST_32_CHARACTERS_LONG",
    "Issuer": "AssignmentManagementApi",
    "Audience": "AssignmentManagementClient",
    "ExpiryInMinutes": 60
  }
}
```

### 3. Restore, Build, and Run
```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run Web API server
dotnet run
```

---

## 📖 API Documentation & Swagger UI

Once the application is running, open your web browser and navigate to:
```
http://localhost:5000/swagger
```
or
```
http://localhost:5005/swagger
```
*(depending on your configured port)*.

### Swagger Features:
- Complete interactive OpenAPI documentation for all Auth, Class, Subject, Assignment, and Submission endpoints.
- Built-in **JWT Bearer Authorization**: Click the **Authorize** button in Swagger UI and enter `Bearer <YOUR_JWT_TOKEN>` to test role-protected endpoints.

---

## 🌱 Database Seeding & Demo Credentials

When the Web API starts for the first time on an empty MongoDB database, the `DataSeeder` automatically populates realistic demo accounts, classes, subjects, assignments (drafts & published, future & past deadlines), and student submissions (graded & ungraded).

### Pre-populated Demo Logins:

| Role | Full Name | Email | Password |
|---|---|---|---|
| **Admin** | System Admin | `admin@school.com` | `Admin@123` |
| **Teacher 1** | Sarah Connor | `teacher1@school.com` | `Teacher@123` |
| **Teacher 2** | Walter White | `teacher2@school.com` | `Teacher@123` |
| **Student 1** | Alex Mercer | `student1@school.com` | `Student@123` |
| **Student 2** | Emma Watson | `student2@school.com` | `Student@123` |
| **Student 3** | Peter Parker | `student3@school.com` | `Student@123` |

---

## 🧪 Running Unit Tests

The repository includes 23 automated xUnit unit tests covering business logic, authorization rules, and submission workflows in `AssignmentManagementSystem.Tests`.

To run all unit tests:
```bash
cd server/AssignmentManagementSystem.Tests
dotnet test
```

---

## 💡 Key Architectural Assumptions

1. **Late Submissions**: Submissions made after an assignment's deadline are allowed but automatically assigned a `SubmissionStatus.Late` status and `IsLate = true` flag for teacher review, rather than being hard-blocked.
2. **Soft Deletions**: Deleting classes, subjects, or assignments sets `IsActive = false` or `IsDeleted = true` flag to preserve historical audit trails and relational integrity.
3. **Teacher Subject Ownership**: Teachers can only create, update, publish, or delete assignments for subjects to which they have been explicitly assigned by an Admin (`Subject.TeacherId == teacherId`).
4. **Resubmission Restrictions**: Students can resubmit answers before the deadline only if `Assignment.AllowResubmission == true` and the submission has not already been graded by a teacher.

---

## ⚠️ Known Limitations

- **File Attachment Placeholder**: `AttachmentUrl` stored on student submissions is currently a string URL placeholder (no cloud file storage provider integrated).
- **No Email Notifications**: System actions (grading, publishing assignments) do not dispatch external SMTP/email notifications.
- **Unpaginated List Endpoints**: List endpoints return full filtered collections without pagination parameters.
