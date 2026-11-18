using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>Gets all tasks ordered by creation date (newest first).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _taskService.GetAllTasksAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single task by identifier.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);
        return task is null ? NotFound(new { message = $"Task {id} was not found." }) : Ok(task);
    }

    /// <summary>Searches tasks by keyword across title, description, status, and remarks.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponseDto>>> Search(
        [FromQuery] string keyword = "",
        CancellationToken cancellationToken = default)
    {
        var result = await _taskService.SearchTasksAsync(keyword, cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates a new task.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponseDto>> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _taskService.CreateTaskAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing task.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> Update(int id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _taskService.UpdateTaskAsync(id, dto, cancellationToken);
        return updated is null ? NotFound(new { message = $"Task {id} was not found." }) : Ok(updated);
    }

    /// <summary>Deletes a task.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _taskService.DeleteTaskAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = $"Task {id} was not found." });
    }
}
