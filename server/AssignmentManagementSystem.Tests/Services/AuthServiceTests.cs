using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Common.Exceptions;
using AssignmentManagementSystem.API.DTOs.Auth;
using AssignmentManagementSystem.API.Helpers.Interfaces;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Implementations;
using AssignmentManagementSystem.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AssignmentManagementSystem.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            FullName = "Jane Doe",
            Email = "existing@school.com",
            Password = "Password123!",
            Role = Role.Student
        };

        var existingUser = TestDataBuilder.CreateTestUser(email: request.Email);
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> action = async () => await _authService.RegisterAsync(request);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage($"Email '{request.Email}' is already registered.");
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailIsNew_ReturnsAuthResponseDtoWithToken()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            FullName = "Jane Doe",
            Email = "newuser@school.com",
            Password = "Password123!",
            Role = Role.Student
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(p => p.HashPassword(request.Password))
            .Returns("hashed_password_xyz");

        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(("generated_jwt_token", DateTime.UtcNow.AddMinutes(60)));

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("generated_jwt_token");
        result.Email.Should().Be(request.Email);
        result.FullName.Should().Be(request.FullName);
        result.Role.Should().Be(Role.Student);

        _userRepositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u => u.Email == request.Email), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreInvalid_ThrowsBadRequestException()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "nonexistent@school.com",
            Password = "WrongPassword123!"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> action = async () => await _authService.LoginAsync(request);

        // Assert
        await action.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WhenAccountIsInactive_ThrowsForbiddenException()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "inactive@school.com",
            Password = "Password123!"
        };

        var inactiveUser = TestDataBuilder.CreateTestUser(email: request.Email, isActive: false);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveUser);

        _passwordHasherMock
            .Setup(p => p.VerifyPassword(request.Password, inactiveUser.PasswordHash))
            .Returns(true);

        // Act
        Func<Task> action = async () => await _authService.LoginAsync(request);

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Account is inactive. Please contact system administrator.");
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "student@school.com",
            Password = "CorrectPassword123!"
        };

        var activeUser = TestDataBuilder.CreateTestUser(email: request.Email, isActive: true);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeUser);

        _passwordHasherMock
            .Setup(p => p.VerifyPassword(request.Password, activeUser.PasswordHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateToken(activeUser))
            .Returns(("valid_jwt_token", DateTime.UtcNow.AddMinutes(60)));

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("valid_jwt_token");
        result.Email.Should().Be(request.Email);
    }
}
