using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.DTOs.Subject;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Interfaces;

namespace AssignmentManagementSystem.API.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IClassRepository _classRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SubjectService> _logger;

    public SubjectService(
        ISubjectRepository subjectRepository,
        IClassRepository classRepository,
        IUserRepository userRepository,
        ILogger<SubjectService> logger)
    {
        _subjectRepository = subjectRepository;
        _classRepository = classRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<SubjectResponseDto> CreateSubjectAsync(CreateSubjectDto dto, CancellationToken cancellationToken = default)
    {
        var classEntity = await _classRepository.GetByIdAsync(dto.ClassId, cancellationToken);
        if (classEntity == null)
        {
            throw new KeyNotFoundException($"Class with ID '{dto.ClassId}' was not found.");
        }

        string? teacherName = null;
        if (!string.IsNullOrWhiteSpace(dto.TeacherId))
        {
            var teacher = await _userRepository.GetByIdAsync(dto.TeacherId, cancellationToken);
            if (teacher == null)
            {
                throw new KeyNotFoundException($"Teacher with ID '{dto.TeacherId}' was not found.");
            }
            if (teacher.Role != Role.Teacher)
            {
                throw new InvalidOperationException($"User '{teacher.FullName}' does not have the Teacher role.");
            }
            teacherName = teacher.FullName;
        }

        var subject = new Subject
        {
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim().ToUpperInvariant(),
            ClassId = dto.ClassId,
            TeacherId = dto.TeacherId,
            IsActive = true
        };

        await _subjectRepository.CreateAsync(subject, cancellationToken);
        _logger.LogInformation("Subject '{Name}' ({Code}) created for Class '{ClassName}'.", subject.Name, subject.Code, classEntity.Name);

        return MapToDto(subject, classEntity.Name, teacherName);
    }

    public async Task<IEnumerable<SubjectResponseDto>> GetAllSubjectsAsync(CancellationToken cancellationToken = default)
    {
        var subjects = await _subjectRepository.GetAllAsync(cancellationToken);
        return await MapToDtosAsync(subjects, cancellationToken);
    }

    public async Task<SubjectResponseDto> GetSubjectByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var subject = await _subjectRepository.GetByIdAsync(id, cancellationToken);
        if (subject == null)
        {
            throw new KeyNotFoundException($"Subject with ID '{id}' was not found.");
        }

        var classEntity = await _classRepository.GetByIdAsync(subject.ClassId, cancellationToken);
        string className = classEntity?.Name ?? "Unknown";

        string? teacherName = null;
        if (!string.IsNullOrWhiteSpace(subject.TeacherId))
        {
            var teacher = await _userRepository.GetByIdAsync(subject.TeacherId, cancellationToken);
            teacherName = teacher?.FullName;
        }

        return MapToDto(subject, className, teacherName);
    }

    public async Task<IEnumerable<SubjectResponseDto>> GetSubjectsByClassAsync(string classId, CancellationToken cancellationToken = default)
    {
        var classEntity = await _classRepository.GetByIdAsync(classId, cancellationToken);
        if (classEntity == null)
        {
            throw new KeyNotFoundException($"Class with ID '{classId}' was not found.");
        }

        var subjects = await _subjectRepository.GetByClassIdAsync(classId, cancellationToken);
        return await MapToDtosAsync(subjects, cancellationToken);
    }

    public async Task<SubjectResponseDto> UpdateSubjectAsync(string id, UpdateSubjectDto dto, CancellationToken cancellationToken = default)
    {
        var subject = await _subjectRepository.GetByIdAsync(id, cancellationToken);
        if (subject == null)
        {
            throw new KeyNotFoundException($"Subject with ID '{id}' was not found.");
        }

        var classEntity = await _classRepository.GetByIdAsync(dto.ClassId, cancellationToken);
        if (classEntity == null)
        {
            throw new KeyNotFoundException($"Class with ID '{dto.ClassId}' was not found.");
        }

        string? teacherName = null;
        if (!string.IsNullOrWhiteSpace(dto.TeacherId))
        {
            var teacher = await _userRepository.GetByIdAsync(dto.TeacherId, cancellationToken);
            if (teacher == null)
            {
                throw new KeyNotFoundException($"Teacher with ID '{dto.TeacherId}' was not found.");
            }
            if (teacher.Role != Role.Teacher)
            {
                throw new InvalidOperationException($"User '{teacher.FullName}' does not have the Teacher role.");
            }
            teacherName = teacher.FullName;
        }

        subject.Name = dto.Name.Trim();
        subject.Code = dto.Code.Trim().ToUpperInvariant();
        subject.ClassId = dto.ClassId;
        subject.TeacherId = dto.TeacherId;
        subject.IsActive = dto.IsActive;

        await _subjectRepository.UpdateAsync(id, subject, cancellationToken);
        _logger.LogInformation("Subject ID '{Id}' updated successfully.", id);

        return MapToDto(subject, classEntity.Name, teacherName);
    }

    public async Task<SubjectResponseDto> AssignTeacherToSubjectAsync(string subjectId, string teacherId, CancellationToken cancellationToken = default)
    {
        var subject = await _subjectRepository.GetByIdAsync(subjectId, cancellationToken);
        if (subject == null)
        {
            throw new KeyNotFoundException($"Subject with ID '{subjectId}' was not found.");
        }

        var teacher = await _userRepository.GetByIdAsync(teacherId, cancellationToken);
        if (teacher == null)
        {
            throw new KeyNotFoundException($"Teacher with ID '{teacherId}' was not found.");
        }

        if (teacher.Role != Role.Teacher)
        {
            throw new InvalidOperationException($"User '{teacher.FullName}' does not have the Teacher role.");
        }

        subject.TeacherId = teacherId;
        await _subjectRepository.UpdateAsync(subjectId, subject, cancellationToken);
        _logger.LogInformation("Assigned Teacher '{TeacherName}' (ID: {TeacherId}) to Subject '{SubjectName}' (ID: {SubjectId}).", teacher.FullName, teacherId, subject.Name, subjectId);

        var classEntity = await _classRepository.GetByIdAsync(subject.ClassId, cancellationToken);
        string className = classEntity?.Name ?? "Unknown";

        return MapToDto(subject, className, teacher.FullName);
    }

    public async Task DeleteSubjectAsync(string id, CancellationToken cancellationToken = default)
    {
        var subject = await _subjectRepository.GetByIdAsync(id, cancellationToken);
        if (subject == null)
        {
            throw new KeyNotFoundException($"Subject with ID '{id}' was not found.");
        }

        subject.IsActive = false;
        await _subjectRepository.UpdateAsync(id, subject, cancellationToken);
        _logger.LogInformation("Subject ID '{Id}' soft-deleted (IsActive set to false).", id);
    }

    private async Task<IEnumerable<SubjectResponseDto>> MapToDtosAsync(IEnumerable<Subject> subjects, CancellationToken cancellationToken)
    {
        var dtos = new List<SubjectResponseDto>();
        var classes = (await _classRepository.GetAllAsync(cancellationToken)).ToDictionary(c => c.Id, c => c.Name);
        var users = (await _userRepository.GetAllAsync(cancellationToken)).ToDictionary(u => u.Id, u => u.FullName);

        foreach (var s in subjects)
        {
            classes.TryGetValue(s.ClassId, out var className);
            string? teacherName = null;
            if (!string.IsNullOrWhiteSpace(s.TeacherId))
            {
                users.TryGetValue(s.TeacherId, out teacherName);
            }
            dtos.Add(MapToDto(s, className ?? "Unknown", teacherName));
        }

        return dtos;
    }

    private static SubjectResponseDto MapToDto(Subject entity, string className, string? teacherName)
    {
        return new SubjectResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            ClassId = entity.ClassId,
            ClassName = className,
            TeacherId = entity.TeacherId,
            TeacherName = teacherName,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
