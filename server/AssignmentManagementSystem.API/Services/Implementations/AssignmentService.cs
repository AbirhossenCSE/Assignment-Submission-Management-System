using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.DTOs.Assignment;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Interfaces;

namespace AssignmentManagementSystem.API.Services.Implementations;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IClassRepository _classRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AssignmentService> _logger;

    public AssignmentService(
        IAssignmentRepository assignmentRepository,
        IClassRepository classRepository,
        ISubjectRepository subjectRepository,
        IUserRepository userRepository,
        ILogger<AssignmentService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _classRepository = classRepository;
        _subjectRepository = subjectRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AssignmentResponseDto> CreateAssignmentAsync(string teacherId, CreateAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var classEntity = await _classRepository.GetByIdAsync(dto.ClassId, cancellationToken);
        if (classEntity == null)
        {
            throw new KeyNotFoundException($"Class with ID '{dto.ClassId}' was not found.");
        }

        var subject = await _subjectRepository.GetByIdAsync(dto.SubjectId, cancellationToken);
        if (subject == null)
        {
            throw new KeyNotFoundException($"Subject with ID '{dto.SubjectId}' was not found.");
        }

        if (subject.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted to create assignment for Subject '{SubjectId}' they do not teach.", teacherId, dto.SubjectId);
            throw new UnauthorizedAccessException("You are not assigned to teach this subject.");
        }

        if (dto.Deadline <= DateTime.UtcNow)
        {
            throw new ArgumentException("Deadline must be a future date.");
        }

        var teacher = await _userRepository.GetByIdAsync(teacherId, cancellationToken);
        string teacherName = teacher?.FullName ?? "Unknown";

        var assignment = new Assignment
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId,
            TeacherId = teacherId,
            Deadline = dto.Deadline.ToUniversalTime(),
            MaxMarks = dto.MaxMarks,
            Status = dto.Status,
            AllowResubmission = dto.AllowResubmission,
            IsDeleted = false
        };

        await _assignmentRepository.CreateAsync(assignment, cancellationToken);
        _logger.LogInformation("Assignment '{Title}' created by Teacher '{TeacherName}' (ID: {TeacherId}).", assignment.Title, teacherName, teacherId);

        return MapToDto(assignment, classEntity.Name, subject.Name, teacherName);
    }

    public async Task<AssignmentResponseDto> UpdateAssignmentAsync(string assignmentId, string teacherId, UpdateAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (assignment.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted unauthorized update on Assignment '{AssignmentId}'.", teacherId, assignmentId);
            throw new UnauthorizedAccessException("You are not authorized to modify this assignment.");
        }

        if (!string.IsNullOrWhiteSpace(dto.ClassId) && dto.ClassId != assignment.ClassId)
        {
            var classEntity = await _classRepository.GetByIdAsync(dto.ClassId, cancellationToken);
            if (classEntity == null) throw new KeyNotFoundException($"Class with ID '{dto.ClassId}' was not found.");
            assignment.ClassId = dto.ClassId;
        }

        if (!string.IsNullOrWhiteSpace(dto.SubjectId) && dto.SubjectId != assignment.SubjectId)
        {
            var subject = await _subjectRepository.GetByIdAsync(dto.SubjectId, cancellationToken);
            if (subject == null) throw new KeyNotFoundException($"Subject with ID '{dto.SubjectId}' was not found.");
            if (subject.TeacherId != teacherId) throw new UnauthorizedAccessException("You are not assigned to teach this subject.");
            assignment.SubjectId = dto.SubjectId;
        }

        if (dto.Deadline.HasValue)
        {
            if (dto.Deadline.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException("Deadline must be a future date.");
            }
            assignment.Deadline = dto.Deadline.Value.ToUniversalTime();
        }

        if (!string.IsNullOrWhiteSpace(dto.Title)) assignment.Title = dto.Title.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Description)) assignment.Description = dto.Description.Trim();
        if (dto.MaxMarks.HasValue && dto.MaxMarks.Value > 0) assignment.MaxMarks = dto.MaxMarks.Value;
        if (dto.AllowResubmission.HasValue) assignment.AllowResubmission = dto.AllowResubmission.Value;
        if (dto.Status.HasValue) assignment.Status = dto.Status.Value;

        await _assignmentRepository.UpdateAsync(assignmentId, assignment, cancellationToken);
        _logger.LogInformation("Assignment ID '{AssignmentId}' updated successfully.", assignmentId);

        return await GetResponseDtoForAssignmentAsync(assignment, cancellationToken);
    }

    public async Task DeleteAssignmentAsync(string assignmentId, string teacherId, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (assignment.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted unauthorized deletion of Assignment '{AssignmentId}'.", teacherId, assignmentId);
            throw new UnauthorizedAccessException("You are not authorized to delete this assignment.");
        }

        await _assignmentRepository.DeleteAsync(assignmentId, cancellationToken);
        _logger.LogInformation("Assignment ID '{AssignmentId}' soft-deleted.", assignmentId);
    }

    public async Task<AssignmentResponseDto> PublishAssignmentAsync(string assignmentId, string teacherId, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (assignment.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted unauthorized publish of Assignment '{AssignmentId}'.", teacherId, assignmentId);
            throw new UnauthorizedAccessException("You are not authorized to publish this assignment.");
        }

        assignment.Status = AssignmentStatus.Published;
        await _assignmentRepository.UpdateAsync(assignmentId, assignment, cancellationToken);
        _logger.LogInformation("Assignment ID '{AssignmentId}' published successfully.", assignmentId);

        return await GetResponseDtoForAssignmentAsync(assignment, cancellationToken);
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsByTeacherAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        var assignments = await _assignmentRepository.GetByTeacherIdAsync(teacherId, cancellationToken);
        return await MapToDtosAsync(assignments, cancellationToken);
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsForClassAsync(string classId, string requestingUserId, Role requestingUserRole, CancellationToken cancellationToken = default)
    {
        var classEntity = await _classRepository.GetByIdAsync(classId, cancellationToken);
        if (classEntity == null)
        {
            throw new KeyNotFoundException($"Class with ID '{classId}' was not found.");
        }

        IEnumerable<Assignment> assignments;
        if (requestingUserRole == Role.Student)
        {
            assignments = await _assignmentRepository.GetPublishedByClassIdAsync(classId, cancellationToken);
        }
        else
        {
            assignments = await _assignmentRepository.GetByClassIdAsync(classId, cancellationToken);
        }

        return await MapToDtosAsync(assignments, cancellationToken);
    }

    public async Task<AssignmentResponseDto> GetAssignmentByIdAsync(string assignmentId, string requestingUserId, Role requestingUserRole, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (requestingUserRole == Role.Student && assignment.Status != AssignmentStatus.Published)
        {
            _logger.LogWarning("Student '{UserId}' attempted to access draft Assignment '{AssignmentId}'.", requestingUserId, assignmentId);
            throw new UnauthorizedAccessException("Draft assignments are not accessible to students.");
        }

        if (requestingUserRole == Role.Teacher && assignment.TeacherId != requestingUserId && assignment.Status != AssignmentStatus.Published)
        {
            _logger.LogWarning("Teacher '{UserId}' attempted to access unpublished draft Assignment '{AssignmentId}' owned by another teacher.", requestingUserId, assignmentId);
            throw new UnauthorizedAccessException("You are not authorized to view this draft assignment.");
        }

        return await GetResponseDtoForAssignmentAsync(assignment, cancellationToken);
    }

    private async Task<AssignmentResponseDto> GetResponseDtoForAssignmentAsync(Assignment assignment, CancellationToken cancellationToken)
    {
        var classEntity = await _classRepository.GetByIdAsync(assignment.ClassId, cancellationToken);
        var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId, cancellationToken);
        var teacher = await _userRepository.GetByIdAsync(assignment.TeacherId, cancellationToken);

        return MapToDto(
            assignment,
            classEntity?.Name ?? "Unknown",
            subject?.Name ?? "Unknown",
            teacher?.FullName ?? "Unknown");
    }

    private async Task<IEnumerable<AssignmentResponseDto>> MapToDtosAsync(IEnumerable<Assignment> assignments, CancellationToken cancellationToken)
    {
        var dtos = new List<AssignmentResponseDto>();
        var classes = (await _classRepository.GetAllAsync(cancellationToken)).ToDictionary(c => c.Id, c => c.Name);
        var subjects = (await _subjectRepository.GetAllAsync(cancellationToken)).ToDictionary(s => s.Id, s => s.Name);
        var users = (await _userRepository.GetAllAsync(cancellationToken)).ToDictionary(u => u.Id, u => u.FullName);

        foreach (var a in assignments)
        {
            classes.TryGetValue(a.ClassId, out var className);
            subjects.TryGetValue(a.SubjectId, out var subjectName);
            users.TryGetValue(a.TeacherId, out var teacherName);

            dtos.Add(MapToDto(
                a,
                className ?? "Unknown",
                subjectName ?? "Unknown",
                teacherName ?? "Unknown"));
        }

        return dtos;
    }

    private static AssignmentResponseDto MapToDto(Assignment entity, string className, string subjectName, string teacherName)
    {
        return new AssignmentResponseDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            ClassId = entity.ClassId,
            ClassName = className,
            SubjectId = entity.SubjectId,
            SubjectName = subjectName,
            TeacherId = entity.TeacherId,
            TeacherName = teacherName,
            Deadline = entity.Deadline,
            MaxMarks = entity.MaxMarks,
            Status = entity.Status,
            AllowResubmission = entity.AllowResubmission,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
