using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HalloDoc;

[Table("Task")]
public partial class Task
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public string? TaskName { get; set; }

    public string? Assignee { get; set; }

    public int CategoryId { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? DueDate { get; set; }

    public string? City { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Tasks")]
    public virtual Category Category { get; set; } = null!;
}
