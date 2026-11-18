using System.ComponentModel.DataAnnotations;
using TaskManagement.API.Models;

namespace TaskManagement.API.DTOs;

public class TaskResponseDto
{
    public int Id { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string? TaskDescription { get; set; }
    public DateTime TaskDueDate { get; set; }
    public string TaskStatus { get; set; } = string.Empty;
    public string? TaskRemarks { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime LastUpdatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string LastUpdatedBy { get; set; } = string.Empty;
}

public class CreateTaskDto
{
    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters.")]
    public string TaskTitle { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? TaskDescription { get; set; }

    [Required(ErrorMessage = "Due date is required.")]
    public DateTime TaskDueDate { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [RegularExpression("^(Pending|InProgress|Completed|Cancelled)$",
        ErrorMessage = "Status must be Pending, InProgress, Completed, or Cancelled.")]
    public string TaskStatus { get; set; } = TaskStatusConstants.Pending;

    [StringLength(1000)]
    public string? TaskRemarks { get; set; }

    [Required(ErrorMessage = "CreatedBy is required.")]
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";
}

public class UpdateTaskDto
{
    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(200, MinimumLength = 2)]
    public string TaskTitle { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? TaskDescription { get; set; }

    [Required(ErrorMessage = "Due date is required.")]
    public DateTime TaskDueDate { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [RegularExpression("^(Pending|InProgress|Completed|Cancelled)$",
        ErrorMessage = "Status must be Pending, InProgress, Completed, or Cancelled.")]
    public string TaskStatus { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? TaskRemarks { get; set; }

    [Required(ErrorMessage = "LastUpdatedBy is required.")]
    [StringLength(100)]
    public string LastUpdatedBy { get; set; } = "System";
}
