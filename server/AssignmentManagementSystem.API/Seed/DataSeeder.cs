using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Helpers.Interfaces;
using AssignmentManagementSystem.API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace AssignmentManagementSystem.API.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IMongoDatabase database, IPasswordHasher passwordHasher)
    {
        try
        {
            // Ping check with 1-second timeout to avoid delaying startup when local MongoDB is offline
            using (var pingCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000)))
            {
                await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: pingCts.Token);
            }

            var usersCollection = database.GetCollection<User>("Users");

            // Idempotent check: return early if users collection is not empty
            var existingUserCount = await usersCollection.CountDocumentsAsync(FilterDefinition<User>.Empty);
            if (existingUserCount > 0)
            {
                Log.Information("Database already contains user records ({Count}). Skipping seeding.", existingUserCount);
                return;
            }

            Log.Information("Seeding demo data into MongoDB database '{DatabaseName}'...", database.DatabaseNamespace.DatabaseName);

            var classesCollection = database.GetCollection<ClassEntity>("Classes");
            var subjectsCollection = database.GetCollection<Subject>("Subjects");
            var assignmentsCollection = database.GetCollection<Assignment>("Assignments");
            var submissionsCollection = database.GetCollection<Submission>("Submissions");

            var now = DateTime.UtcNow;

            // 1. Create Demo Users
            var adminUser = new User
            {
                Id = "66a000000000000000000001",
                FullName = "System Admin",
                Email = "admin@school.com",
                PasswordHash = passwordHasher.HashPassword("Admin@123"),
                Role = Role.Admin,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var teacher1 = new User
            {
                Id = "66a000000000000000000002",
                FullName = "Sarah Connor",
                Email = "teacher1@school.com",
                PasswordHash = passwordHasher.HashPassword("Teacher@123"),
                Role = Role.Teacher,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var teacher2 = new User
            {
                Id = "66a000000000000000000003",
                FullName = "Walter White",
                Email = "teacher2@school.com",
                PasswordHash = passwordHasher.HashPassword("Teacher@123"),
                Role = Role.Teacher,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var student1 = new User
            {
                Id = "66a000000000000000000004",
                FullName = "Alex Mercer",
                Email = "student1@school.com",
                PasswordHash = passwordHasher.HashPassword("Student@123"),
                Role = Role.Student,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var student2 = new User
            {
                Id = "66a000000000000000000005",
                FullName = "Emma Watson",
                Email = "student2@school.com",
                PasswordHash = passwordHasher.HashPassword("Student@123"),
                Role = Role.Student,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var student3 = new User
            {
                Id = "66a000000000000000000006",
                FullName = "Peter Parker",
                Email = "student3@school.com",
                PasswordHash = passwordHasher.HashPassword("Student@123"),
                Role = Role.Student,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var users = new[] { adminUser, teacher1, teacher2, student1, student2, student3 };
            await usersCollection.InsertManyAsync(users);

            // 2. Create Demo Classes
            var classA = new ClassEntity
            {
                Id = "66a000000000000000000010",
                Name = "Class 10 - Section A",
                Section = "A",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var classB = new ClassEntity
            {
                Id = "66a000000000000000000011",
                Name = "Class 10 - Section B",
                Section = "B",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            await classesCollection.InsertManyAsync(new[] { classA, classB });

            // 3. Create Demo Subjects
            var mathSubject = new Subject
            {
                Id = "66a000000000000000000020",
                Name = "Mathematics",
                Code = "MATH101",
                ClassId = classA.Id,
                TeacherId = teacher1.Id,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var scienceSubject = new Subject
            {
                Id = "66a000000000000000000021",
                Name = "Science",
                Code = "SCI101",
                ClassId = classA.Id,
                TeacherId = teacher1.Id,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var englishSubject = new Subject
            {
                Id = "66a000000000000000000022",
                Name = "English Literature",
                Code = "ENG201",
                ClassId = classB.Id,
                TeacherId = teacher2.Id,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var historySubject = new Subject
            {
                Id = "66a000000000000000000023",
                Name = "World History",
                Code = "HIS201",
                ClassId = classB.Id,
                TeacherId = teacher2.Id,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            await subjectsCollection.InsertManyAsync(new[] { mathSubject, scienceSubject, englishSubject, historySubject });

            // 4. Create Demo Assignments
            var algebraQuiz = new Assignment
            {
                Id = "66a000000000000000000030",
                Title = "Algebra Quiz #1",
                Description = "Solve polynomial expressions 1-10 on page 42.",
                ClassId = classA.Id,
                SubjectId = mathSubject.Id,
                TeacherId = teacher1.Id,
                Deadline = now.AddDays(7),
                MaxMarks = 100,
                Status = AssignmentStatus.Published,
                AllowResubmission = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            var opticsLab = new Assignment
            {
                Id = "66a000000000000000000031",
                Title = "Physics Optics Lab",
                Description = "Write a comprehensive lab report on light refraction experiment.",
                ClassId = classA.Id,
                SubjectId = scienceSubject.Id,
                TeacherId = teacher1.Id,
                Deadline = now.AddDays(-2), // Past deadline to test late submissions
                MaxMarks = 50,
                Status = AssignmentStatus.Published,
                AllowResubmission = true,
                IsDeleted = false,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5)
            };

            var calculusDraft = new Assignment
            {
                Id = "66a000000000000000000032",
                Title = "Calculus Advanced Equations",
                Description = "Integration techniques chapter 5.",
                ClassId = classA.Id,
                SubjectId = mathSubject.Id,
                TeacherId = teacher1.Id,
                Deadline = now.AddDays(14),
                MaxMarks = 100,
                Status = AssignmentStatus.Draft,
                AllowResubmission = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            var essayAssignment = new Assignment
            {
                Id = "66a000000000000000000033",
                Title = "Shakespeare Essay",
                Description = "Analyze the character arc of Macbeth in Act 3.",
                ClassId = classB.Id,
                SubjectId = englishSubject.Id,
                TeacherId = teacher2.Id,
                Deadline = now.AddDays(5),
                MaxMarks = 100,
                Status = AssignmentStatus.Published,
                AllowResubmission = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            await assignmentsCollection.InsertManyAsync(new[] { algebraQuiz, opticsLab, calculusDraft, essayAssignment });

            // 5. Create Demo Submissions
            var alexSubmission = new Submission
            {
                Id = "66a000000000000000000040",
                AssignmentId = algebraQuiz.Id,
                StudentId = student1.Id,
                AnswerText = "1. x = 5\n2. y = 3x + 2\n3. Factored: (x+2)(x+3)",
                AttachmentUrl = "https://example.com/alex_algebra.pdf",
                SubmittedAt = now.AddDays(-1),
                Status = SubmissionStatus.Graded,
                Marks = 92,
                Feedback = "Excellent work on polynomial factorization!",
                GradedAt = now.AddHours(-12),
                GradedBy = teacher1.Id,
                IsLate = false,
                IsDeleted = false,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddHours(-12)
            };

            var emmaSubmission = new Submission
            {
                Id = "66a000000000000000000041",
                AssignmentId = opticsLab.Id,
                StudentId = student2.Id,
                AnswerText = "Refraction angle measured at 30 degrees using Snell's Law.",
                AttachmentUrl = "https://example.com/emma_optics.pdf",
                SubmittedAt = now.AddHours(-1), // Submitted after deadline (-2 days)
                Status = SubmissionStatus.Late,
                Marks = null,
                Feedback = null,
                GradedAt = null,
                GradedBy = null,
                IsLate = true,
                IsDeleted = false,
                CreatedAt = now.AddHours(-1),
                UpdatedAt = now.AddHours(-1)
            };

            var peterSubmission = new Submission
            {
                Id = "66a000000000000000000042",
                AssignmentId = essayAssignment.Id,
                StudentId = student3.Id,
                AnswerText = "Macbeth's ambition leads to his ultimate downfall due to internal conflict...",
                AttachmentUrl = "https://example.com/peter_macbeth.pdf",
                SubmittedAt = now.AddHours(-3),
                Status = SubmissionStatus.Submitted,
                Marks = null,
                Feedback = null,
                GradedAt = null,
                GradedBy = null,
                IsLate = false,
                IsDeleted = false,
                CreatedAt = now.AddHours(-3),
                UpdatedAt = now.AddHours(-3)
            };

            await submissionsCollection.InsertManyAsync(new[] { alexSubmission, emmaSubmission, peterSubmission });

            Log.Information("==========================================================");
            Log.Information("   DEMO DATASEEDER SUCCESSFULLY EXECUTED   ");
            Log.Information("==========================================================");
            Log.Information(" Demo Credentials:");
            Log.Information("   Admin:     admin@school.com    / Admin@123");
            Log.Information("   Teacher 1: teacher1@school.com / Teacher@123 (Sarah Connor)");
            Log.Information("   Teacher 2: teacher2@school.com / Teacher@123 (Walter White)");
            Log.Information("   Student 1: student1@school.com / Student@123 (Alex Mercer)");
            Log.Information("   Student 2: student2@school.com / Student@123 (Emma Watson)");
            Log.Information("   Student 3: student3@school.com / Student@123 (Peter Parker)");
            Log.Information("==========================================================");
        }
        catch (OperationCanceledException)
        {
            Log.Warning("MongoDB connection unavailable on port 27017. Database seeding bypassed for resilient dev mode.");
        }
        catch (TimeoutException)
        {
            Log.Warning("MongoDB connection timed out on port 27017. Database seeding bypassed for resilient dev mode.");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "An error occurred during database seeding. Application startup will continue.");
        }
    }
}
