using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.DTOs.Submission;

namespace AssignmentManagementSystem.API.Services.Interfaces;

public interface ISubmissionService
{
    Task<SubmissionResponseDto> SubmitAssignmentAsync(string assignmentId, string studentId, SubmitAssignmentDto dto, CancellationToken cancellationToken = default);
    Task<SubmissionResponseDto> UpdateSubmissionAsync(string submissionId, string studentId, UpdateSubmissionDto dto, CancellationToken cancellationToken = default);
    Task<SubmissionResponseDto> GradeSubmissionAsync(string submissionId, string teacherId, GradeSubmissionDto dto, CancellationToken cancellationToken = default);
    Task<SubmissionResponseDto> UpdateSubmissionStatusAsync(string submissionId, string teacherId, SubmissionStatus newStatus, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsForAssignmentAsync(string assignmentId, string requestingUserId, Role requestingUserRole, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubmissionResponseDto>> GetMySubmissionsAsync(string studentId, CancellationToken cancellationToken = default);
    Task<SubmissionResponseDto> GetSubmissionByIdAsync(string submissionId, string requestingUserId, Role requestingUserRole, CancellationToken cancellationToken = default);
    Task<SubmissionResponseDto?> GetSubmissionStatusForStudentAsync(string assignmentId, string studentId, CancellationToken cancellationToken = default);
}
