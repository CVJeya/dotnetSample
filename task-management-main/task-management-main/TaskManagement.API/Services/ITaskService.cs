using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponseDto>> GetAllTasksAsync(CancellationToken cancellationToken = default);
    Task<TaskResponseDto?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskResponseDto>> SearchTasksAsync(string keyword, CancellationToken cancellationToken = default);
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
}
