using AutoMapper;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskManagement.API.Repositories;

namespace TaskManagement.API.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository repository,
        IMapper mapper,
        ILogger<TaskService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskResponseDto>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all tasks.");
        var items = await _repository.GetAllTasksAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TaskResponseDto>>(items);
    }

    public async Task<TaskResponseDto?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving task {TaskId}.", id);
        var entity = await _repository.GetTaskByIdAsync(id, cancellationToken);
        return entity is null ? null : _mapper.Map<TaskResponseDto>(entity);
    }

    public async Task<IReadOnlyList<TaskResponseDto>> SearchTasksAsync(string keyword, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching tasks with keyword {Keyword}.", keyword);
        var items = await _repository.SearchTasksAsync(keyword ?? string.Empty, cancellationToken);
        return _mapper.Map<IReadOnlyList<TaskResponseDto>>(items);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating task {Title}.", dto.TaskTitle);
        var entity = _mapper.Map<TaskEntity>(dto);
        var created = await _repository.CreateTaskAsync(entity, cancellationToken);
        return _mapper.Map<TaskResponseDto>(created);
    }

    public async Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating task {TaskId}.", id);
        var existing = await _repository.GetTaskByIdForUpdateAsync(id, cancellationToken);
        if (existing is null)
            return null;

        _mapper.Map(dto, existing);
        existing.Id = id;

        var updated = await _repository.UpdateTaskAsync(existing, cancellationToken);
        return _mapper.Map<TaskResponseDto>(updated);
    }

    public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting task {TaskId}.", id);
        return await _repository.DeleteTaskAsync(id, cancellationToken);
    }
}
