using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Common.Exceptions;
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
            throw new NotFoundException($"Class with ID '{dto.ClassId}' was not found.");
        }

        var subject = await _subjectRepository.GetByIdAsync(dto.SubjectId, cancellationToken);
        if (subject == null)
        {
            throw new NotFoundException($"Subject with ID '{dto.SubjectId}' was not found.");
        }

        if (subject.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted to create assignment for Subject '{SubjectId}' assigned to another teacher.", teacherId, dto.SubjectId);
            throw new ForbiddenException("You are not assigned to teach this subject.");
        }

        if (dto.Deadline <= DateTime.UtcNow)
        {
            throw new BadRequestException("Assignment deadline must be set to a future date and time.");
        }

        var teacher = await _userRepository.GetByIdAsync(teacherId, cancellationToken);

        var assignment = new Assignment
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId,
            TeacherId = teacherId,
            Deadline = dto.Deadline,
            MaxMarks = dto.MaxMarks,
            Status = dto.Status,
            AllowResubmission = dto.AllowResubmission,
            IsDeleted = false
        };

        await _assignmentRepository.CreateAsync(assignment, cancellationToken);
        _logger.LogInformation("Assignment '{Title}' created by Teacher '{TeacherName}' (ID: {TeacherId}).", assignment.Title, teacher?.FullName, teacherId);

        return MapToDto(assignment, classEntity.Name, subject.Name, teacher?.FullName ?? "Unknown");
    }

    public async Task<AssignmentResponseDto> UpdateAssignmentAsync(string assignmentId, string teacherId, UpdateAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new NotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (assignment.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted unauthorized update of Assignment '{AssignmentId}'.", teacherId, assignmentId);
            throw new ForbiddenException("You are not authorized to update this assignment.");
        }

        if (dto.Deadline.HasValue && dto.Deadline.Value <= DateTime.UtcNow)
        {
            throw new BadRequestException("Assignment deadline must be set to a future date and time.");
        }

        var classEntity = await _classRepository.GetByIdAsync(assignment.ClassId, cancellationToken);
        var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId, cancellationToken);
        var teacher = await _userRepository.GetByIdAsync(teacherId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.Title)) assignment.Title = dto.Title.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Description)) assignment.Description = dto.Description.Trim();
        if (dto.Deadline.HasValue) assignment.Deadline = dto.Deadline.Value;
        if (dto.MaxMarks.HasValue) assignment.MaxMarks = dto.MaxMarks.Value;
        if (dto.Status.HasValue) assignment.Status = dto.Status.Value;
        if (dto.AllowResubmission.HasValue) assignment.AllowResubmission = dto.AllowResubmission.Value;

        await _assignmentRepository.UpdateAsync(assignmentId, assignment, cancellationToken);
        _logger.LogInformation("Assignment ID '{Id}' updated by Teacher '{TeacherId}'.", assignmentId, teacherId);

        return MapToDto(assignment, classEntity?.Name ?? "Unknown", subject?.Name ?? "Unknown", teacher?.FullName ?? "Unknown");
    }

    public async Task DeleteAssignmentAsync(string assignmentId, string teacherId, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new NotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (assignment.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted unauthorized deletion of Assignment '{AssignmentId}'.", teacherId, assignmentId);
            throw new ForbiddenException("You are not authorized to delete this assignment.");
        }

        assignment.IsDeleted = true;
        await _assignmentRepository.UpdateAsync(assignmentId, assignment, cancellationToken);
        _logger.LogInformation("Assignment ID '{Id}' soft-deleted by Teacher '{TeacherId}'.", assignmentId, teacherId);
    }

    public async Task<AssignmentResponseDto> PublishAssignmentAsync(string assignmentId, string teacherId, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new NotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (assignment.TeacherId != teacherId)
        {
            _logger.LogWarning("Teacher '{TeacherId}' attempted unauthorized publish of Assignment '{AssignmentId}'.", teacherId, assignmentId);
            throw new ForbiddenException("You are not authorized to publish this assignment.");
        }

        assignment.Status = AssignmentStatus.Published;
        await _assignmentRepository.UpdateAsync(assignmentId, assignment, cancellationToken);
        _logger.LogInformation("Assignment ID '{Id}' published successfully.", assignmentId);

        var classEntity = await _classRepository.GetByIdAsync(assignment.ClassId, cancellationToken);
        var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId, cancellationToken);
        var teacher = await _userRepository.GetByIdAsync(teacherId, cancellationToken);

        return MapToDto(assignment, classEntity?.Name ?? "Unknown", subject?.Name ?? "Unknown", teacher?.FullName ?? "Unknown");
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
            throw new NotFoundException($"Class with ID '{classId}' was not found.");
        }

        var assignments = await _assignmentRepository.GetByClassIdAsync(classId, cancellationToken);

        if (requestingUserRole == Role.Student)
        {
            assignments = assignments.Where(a => a.Status == AssignmentStatus.Published);
        }
        else if (requestingUserRole == Role.Teacher)
        {
            assignments = assignments.Where(a => a.TeacherId == requestingUserId || a.Status == AssignmentStatus.Published);
        }

        return await MapToDtosAsync(assignments, cancellationToken);
    }

    public async Task<AssignmentResponseDto> GetAssignmentByIdAsync(string assignmentId, string requestingUserId, Role requestingUserRole, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new NotFoundException($"Assignment with ID '{assignmentId}' was not found.");
        }

        if (requestingUserRole == Role.Student && assignment.Status != AssignmentStatus.Published)
        {
            _logger.LogWarning("Student '{StudentId}' attempted to view draft Assignment '{AssignmentId}'.", requestingUserId, assignmentId);
            throw new ForbiddenException("Draft assignments are not accessible to students.");
        }

        var classEntity = await _classRepository.GetByIdAsync(assignment.ClassId, cancellationToken);
        var subject = await _subjectRepository.GetByIdAsync(assignment.SubjectId, cancellationToken);
        var teacher = await _userRepository.GetByIdAsync(assignment.TeacherId, cancellationToken);

        return MapToDto(assignment, classEntity?.Name ?? "Unknown", subject?.Name ?? "Unknown", teacher?.FullName ?? "Unknown");
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

            dtos.Add(MapToDto(a, className ?? "Unknown", subjectName ?? "Unknown", teacherName ?? "Unknown"));
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
