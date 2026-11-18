using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _db;

    public TaskRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TaskEntity>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskEntity?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<TaskEntity?> GetTaskByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskEntity>> SearchTasksAsync(string keyword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return await GetAllTasksAsync(cancellationToken);

        var k = keyword.Trim();
        return await _db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.TaskTitle.Contains(k)
                || (t.TaskDescription != null && t.TaskDescription.Contains(k))
                || t.TaskStatus.Contains(k)
                || (t.TaskRemarks != null && t.TaskRemarks.Contains(k)))
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskEntity> CreateTaskAsync(TaskEntity entity, CancellationToken cancellationToken = default)
    {
        _db.Tasks.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TaskEntity> UpdateTaskAsync(TaskEntity entity, CancellationToken cancellationToken = default)
    {
        _db.Tasks.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Tasks.FindAsync(new object[] { id }, cancellationToken);
        if (existing is null)
            return false;

        _db.Tasks.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tasks.AnyAsync(t => t.Id == id, cancellationToken);
    }
}
