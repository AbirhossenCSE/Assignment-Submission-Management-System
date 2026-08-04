using AssignmentManagementSystem.API.DTOs.Class;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Interfaces;

namespace AssignmentManagementSystem.API.Services.Implementations;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly ILogger<ClassService> _logger;

    public ClassService(IClassRepository classRepository, ILogger<ClassService> logger)
    {
        _classRepository = classRepository;
        _logger = logger;
    }

    public async Task<ClassResponseDto> CreateClassAsync(CreateClassDto dto, CancellationToken cancellationToken = default)
    {
        var classEntity = new ClassEntity
        {
            Name = dto.Name.Trim(),
            Section = dto.Section?.Trim(),
            IsActive = true
        };

        await _classRepository.CreateAsync(classEntity, cancellationToken);
        _logger.LogInformation("Class '{Name}' (Section: {Section}) created with Id {Id}.", classEntity.Name, classEntity.Section, classEntity.Id);

        return MapToDto(classEntity);
    }

    public async Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync(CancellationToken cancellationToken = default)
    {
        var classes = await _classRepository.GetAllAsync(cancellationToken);
        return classes.Select(MapToDto);
    }

    public async Task<ClassResponseDto> GetClassByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var classEntity = await _classRepository.GetByIdAsync(id, cancellationToken);
        if (classEntity == null)
        {
            throw new KeyNotFoundException($"Class with ID '{id}' was not found.");
        }

        return MapToDto(classEntity);
    }

    public async Task<ClassResponseDto> UpdateClassAsync(string id, UpdateClassDto dto, CancellationToken cancellationToken = default)
    {
        var classEntity = await _classRepository.GetByIdAsync(id, cancellationToken);
        if (classEntity == null)
        {
            throw new KeyNotFoundException($"Class with ID '{id}' was not found.");
        }

        classEntity.Name = dto.Name.Trim();
        classEntity.Section = dto.Section?.Trim();
        classEntity.IsActive = dto.IsActive;

        await _classRepository.UpdateAsync(id, classEntity, cancellationToken);
        _logger.LogInformation("Class ID '{Id}' updated successfully.", id);

        return MapToDto(classEntity);
    }

    public async Task DeleteClassAsync(string id, CancellationToken cancellationToken = default)
    {
        var classEntity = await _classRepository.GetByIdAsync(id, cancellationToken);
        if (classEntity == null)
        {
            throw new KeyNotFoundException($"Class with ID '{id}' was not found.");
        }

        classEntity.IsActive = false;
        await _classRepository.UpdateAsync(id, classEntity, cancellationToken);
        _logger.LogInformation("Class ID '{Id}' soft-deleted (IsActive set to false).", id);
    }

    private static ClassResponseDto MapToDto(ClassEntity entity)
    {
        return new ClassResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Section = entity.Section,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
