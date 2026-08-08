using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Common.Exceptions;
using AssignmentManagementSystem.API.DTOs.Submission;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Interfaces;

namespace AssignmentManagementSystem.API.Services.Implementations;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SubmissionService> _logger;

    public SubmissionService(
        ISubmissionRepository submissionRepository,
        IAssignmentRepository assignmentRepository,
        IUserRepository userRepository,
        ILogger<SubmissionService> logger)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<SubmissionResponseDto> SubmitAssignmentAsync(string assignmentId, string studentId, SubmitAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new NotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (assignment.Status != AssignmentStatus.Published)
        {
            throw new BadRequestException("Submissions are not accepted for draft assignments.");
        }

        var existingSubmission = await _submissionRepository.GetByAssignmentAndStudentAsync(assignmentId, studentId, cancellationToken);
        if (existingSubmission != null)
        {
            throw new ConflictException("A submission already exists for this assignment. Please use the resubmission endpoint to update your answer.");
        }

        var now = DateTime.UtcNow;
        bool isLate = now > assignment.Deadline;
        var status = isLate ? SubmissionStatus.Late : SubmissionStatus.Submitted;

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            AnswerText = dto.AnswerText.Trim(),
            AttachmentUrl = dto.AttachmentUrl?.Trim(),
            SubmittedAt = now,
            Status = status,
            IsLate = isLate,
            IsDeleted = false
        };

        await _submissionRepository.CreateAsync(submission, cancellationToken);
        _logger.LogInformation("Student '{StudentId}' submitted answer for Assignment '{AssignmentId}' (Status: {Status}, IsLate: {IsLate}).", studentId, assignmentId, status, isLate);

        return await GetResponseDtoForSubmissionAsync(submission, cancellationToken);
    }

    public async Task<SubmissionResponseDto> UpdateSubmissionAsync(string submissionId, string studentId, UpdateSubmissionDto dto, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId, cancellationToken);
        if (submission == null)
        {
            throw new NotFoundException($"Submission with ID '{submissionId}' was not found.");
        }

        if (submission.StudentId != studentId)
        {
            _logger.LogWarning("Student '{StudentId}' attempted unauthorized update on Submission '{SubmissionId}'.", studentId, submissionId);
            throw new ForbiddenException("You are not authorized to update this submission.");
        }

        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken);
        if (assignment != null)
        {
            if (!assignment.AllowResubmission)
            {
                throw new BadRequestException("Resubmission not allowed for this assignment.");
            }

            if (DateTime.UtcNow > assignment.Deadline)
            {
                throw new BadRequestException("Cannot update submission after deadline.");
            }
        }

        if (submission.Status == SubmissionStatus.Graded)
        {
            throw new BadRequestException("Cannot update a graded submission.");
        }

        submission.AnswerText = dto.AnswerText.Trim();
        submission.AttachmentUrl = dto.AttachmentUrl?.Trim();
        submission.SubmittedAt = DateTime.UtcNow;

        await _submissionRepository.UpdateAsync(submissionId, submission, cancellationToken);
        _logger.LogInformation("Submission ID '{SubmissionId}' updated by Student '{StudentId}'.", submissionId, studentId);

        return await GetResponseDtoForSubmissionAsync(submission, cancellationToken);
    }

    public async Task<SubmissionResponseDto> GradeSubmissionAsync(string submissionId, string teacherId, GradeSubmissionDto dto, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId, cancellationToken);
        if (submission == null)
        {
            throw new NotFoundException($"Submission with ID '{submissionId}' was not found.");
        }

        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new NotFoundException($"Associated Assignment with ID '{submission.AssignmentId}' was not found.");
        }

        if (assignment.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted unauthorized grading on Submission '{SubmissionId}'.", teacherId, submissionId);
            throw new ForbiddenException("You are not authorized to grade submissions for this assignment.");
        }

        if (dto.Marks > assignment.MaxMarks)
        {
            throw new BadRequestException($"Marks ({dto.Marks}) cannot exceed the maximum allowed marks ({assignment.MaxMarks}).");
        }

        submission.Marks = dto.Marks;
        submission.Feedback = dto.Feedback?.Trim();
        submission.Status = SubmissionStatus.Graded;
        submission.GradedAt = DateTime.UtcNow;
        submission.GradedBy = teacherId;

        await _submissionRepository.UpdateAsync(submissionId, submission, cancellationToken);
        _logger.LogInformation("Submission ID '{SubmissionId}' graded by Teacher '{TeacherId}' (Marks: {Marks}/{MaxMarks}).", submissionId, teacherId, dto.Marks, assignment.MaxMarks);

        return await GetResponseDtoForSubmissionAsync(submission, cancellationToken);
    }

    public async Task<SubmissionResponseDto> UpdateSubmissionStatusAsync(string submissionId, string teacherId, SubmissionStatus newStatus, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId, cancellationToken);
        if (submission == null)
        {
            throw new NotFoundException($"Submission with ID '{submissionId}' was not found.");
        }

        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new NotFoundException($"Associated Assignment with ID '{submission.AssignmentId}' was not found.");
        }

        if (assignment.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted unauthorized status update on Submission '{SubmissionId}'.", teacherId, submissionId);
            throw new ForbiddenException("You are not authorized to modify status for this assignment's submissions.");
        }

        submission.Status = newStatus;
        await _submissionRepository.UpdateAsync(submissionId, submission, cancellationToken);
        _logger.LogInformation("Submission ID '{SubmissionId}' status changed to '{Status}' by Teacher '{TeacherId}'.", submissionId, newStatus, teacherId);

        return await GetResponseDtoForSubmissionAsync(submission, cancellationToken);
    }

    public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsForAssignmentAsync(string assignmentId, string requestingUserId, Role requestingUserRole, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new NotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (requestingUserRole == Role.Teacher && assignment.TeacherId != requestingUserId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted to view submissions for unassigned Assignment '{AssignmentId}'.", requestingUserId, assignmentId);
            throw new ForbiddenException("You are not authorized to view submissions for this assignment.");
        }

        if (requestingUserRole == Role.Student)
        {
            throw new ForbiddenException("Students are not authorized to view all class submissions.");
        }

        var submissions = await _submissionRepository.GetByAssignmentIdAsync(assignmentId, cancellationToken);
        return await MapToDtosAsync(submissions, cancellationToken);
    }

    public async Task<IEnumerable<SubmissionResponseDto>> GetMySubmissionsAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var submissions = await _submissionRepository.GetByStudentIdAsync(studentId, cancellationToken);
        return await MapToDtosAsync(submissions, cancellationToken);
    }

    public async Task<SubmissionResponseDto> GetSubmissionByIdAsync(string submissionId, string requestingUserId, Role requestingUserRole, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId, cancellationToken);
        if (submission == null)
        {
            throw new NotFoundException($"Submission with ID '{submissionId}' was not found.");
        }

        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken);

        if (requestingUserRole == Role.Student && submission.StudentId != requestingUserId)
        {
            _logger.LogWarning("Student '{StudentId}' attempted unauthorized view of Submission '{SubmissionId}'.", requestingUserId, submissionId);
            throw new ForbiddenException("You are not authorized to view this submission.");
        }

        if (requestingUserRole == Role.Teacher && assignment != null && assignment.TeacherId != requestingUserId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted unauthorized view of Submission '{SubmissionId}'.", requestingUserId, submissionId);
            throw new ForbiddenException("You are not authorized to view this submission.");
        }

        return await GetResponseDtoForSubmissionAsync(submission, cancellationToken);
    }

    public async Task<SubmissionResponseDto?> GetSubmissionStatusForStudentAsync(string assignmentId, string studentId, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByAssignmentAndStudentAsync(assignmentId, studentId, cancellationToken);
        if (submission == null) return null;
        return await GetResponseDtoForSubmissionAsync(submission, cancellationToken);
    }

    private async Task<SubmissionResponseDto> GetResponseDtoForSubmissionAsync(Submission submission, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken);
        var student = await _userRepository.GetByIdAsync(submission.StudentId, cancellationToken);
        User? grader = null;
        if (!string.IsNullOrWhiteSpace(submission.GradedBy))
        {
            grader = await _userRepository.GetByIdAsync(submission.GradedBy, cancellationToken);
        }

        return MapToDto(
            submission,
            assignment?.Title ?? "Unknown",
            assignment?.MaxMarks ?? 100,
            student?.FullName ?? "Unknown",
            grader?.FullName);
    }

    private async Task<IEnumerable<SubmissionResponseDto>> MapToDtosAsync(IEnumerable<Submission> submissions, CancellationToken cancellationToken)
    {
        var dtos = new List<SubmissionResponseDto>();
        var assignments = (await _assignmentRepository.GetAllAsync(cancellationToken)).ToDictionary(a => a.Id, a => a);
        var users = (await _userRepository.GetAllAsync(cancellationToken)).ToDictionary(u => u.Id, u => u.FullName);

        foreach (var s in submissions)
        {
            assignments.TryGetValue(s.AssignmentId, out var assignment);
            users.TryGetValue(s.StudentId, out var studentName);
            string? graderName = null;
            if (!string.IsNullOrWhiteSpace(s.GradedBy))
            {
                users.TryGetValue(s.GradedBy, out graderName);
            }

            dtos.Add(MapToDto(
                s,
                assignment?.Title ?? "Unknown",
                assignment?.MaxMarks ?? 100,
                studentName ?? "Unknown",
                graderName));
        }

        return dtos;
    }

    private static SubmissionResponseDto MapToDto(Submission entity, string assignmentTitle, int maxMarks, string studentName, string? graderName)
    {
        return new SubmissionResponseDto
        {
            Id = entity.Id,
            AssignmentId = entity.AssignmentId,
            AssignmentTitle = assignmentTitle,
            MaxMarks = maxMarks,
            StudentId = entity.StudentId,
            StudentName = studentName,
            AnswerText = entity.AnswerText,
            AttachmentUrl = entity.AttachmentUrl,
            SubmittedAt = entity.SubmittedAt,
            Status = entity.Status,
            Marks = entity.Marks,
            Feedback = entity.Feedback,
            GradedAt = entity.GradedAt,
            GradedBy = entity.GradedBy,
            GradedByName = graderName,
            IsLate = entity.IsLate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
