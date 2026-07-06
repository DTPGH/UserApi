using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserApi.Models.Common;

namespace UserApi.Models;

[Table("TaskItems")]
public class TaskItem : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column(TypeName = "nvarchar(255)")]
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(1000)")]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    public DateTime? DueDate { get; set; }

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;
}
public enum TaskItemStatus
{
    Todo = 1,
    InProgress = 2,
    Done = 3
}