using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskEntity>> GetAllTasksAsync(CancellationToken cancellationToken = default);
    Task<TaskEntity?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Tracked entity for updates (EF change tracking).</summary>
    Task<TaskEntity?> GetTaskByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskEntity>> SearchTasksAsync(string keyword, CancellationToken cancellationToken = default);
    Task<TaskEntity> CreateTaskAsync(TaskEntity entity, CancellationToken cancellationToken = default);
    Task<TaskEntity> UpdateTaskAsync(TaskEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
