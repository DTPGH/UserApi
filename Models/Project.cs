using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserApi.Models.Common;

namespace UserApi.Models;

[Table("Projects")]
public class Project : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column(TypeName = "nvarchar(255)")]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(1000)")]
    public string? Description { get; set; }

    public int OwnerId { get; set; }

    public User Owner { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}