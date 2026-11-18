using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.API.Models;

/// <summary>
/// Domain entity mapped to the Tasks table. Named TaskEntity to avoid collision with System.Threading.Tasks.Task.
/// </summary>
[Table("Tasks")]
public class TaskEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string TaskTitle { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? TaskDescription { get; set; }

    [Required]
    public DateTime TaskDueDate { get; set; }

    [Required]
    [StringLength(50)]
    public string TaskStatus { get; set; } = TaskStatusConstants.Pending;

    [StringLength(1000)]
    public string? TaskRemarks { get; set; }

    [Required]
    public DateTime CreatedOn { get; set; }

    [Required]
    public DateTime LastUpdatedOn { get; set; }

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [Required]
    [StringLength(100)]
    public string LastUpdatedBy { get; set; } = "System";
}

/// <summary>
/// Centralized status values for validation and UI consistency.
/// </summary>
public static class TaskStatusConstants
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All =
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    };
}
