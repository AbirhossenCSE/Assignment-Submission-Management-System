using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Common.Exceptions;
using AssignmentManagementSystem.API.DTOs.Auth;
using AssignmentManagementSystem.API.Helpers.Interfaces;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Interfaces;

namespace AssignmentManagementSystem.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IClassRepository _classRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IClassRepository classRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _classRepository = classRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: Email '{Email}' is already registered.", request.Email);
            throw new ConflictException($"Email '{request.Email}' is already registered.");
        }

        string? validatedClassId = null;
        if (request.Role == Role.Student && !string.IsNullOrWhiteSpace(request.ClassId))
        {
            var classEntity = await _classRepository.GetByIdAsync(request.ClassId, cancellationToken);
            if (classEntity == null)
            {
                throw new NotFoundException($"Class with ID '{request.ClassId}' was not found.");
            }
            validatedClassId = request.ClassId;
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var newUser = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = request.Role,
            ClassId = validatedClassId,
            IsActive = true
        };

        await _userRepository.CreateAsync(newUser, cancellationToken);
        _logger.LogInformation("User '{Email}' registered successfully with role '{Role}'.", newUser.Email, newUser.Role);

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(newUser);

        return new AuthResponseDto
        {
            Token = token,
            UserId = newUser.Id,
            FullName = newUser.FullName,
            Email = newUser.Email,
            Role = newUser.Role,
            ClassId = newUser.ClassId,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User with email '{Email}' not found.", request.Email);
            throw new BadRequestException("Invalid email or password.");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed: Password verification failed for user '{Email}'.", request.Email);
            throw new BadRequestException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: User account '{Email}' is inactive.", request.Email);
            throw new ForbiddenException("Account is inactive. Please contact system administrator.");
        }

        _logger.LogInformation("User '{Email}' logged in successfully.", user.Email);

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ClassId = user.ClassId,
            ExpiresAt = expiresAt
        };
    }
}
