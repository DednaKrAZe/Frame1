using System.ComponentModel;
using System.Data.SqlTypes;

namespace Engineering.Models;

public class Task
{
    public int Id { get; set; }
    public required DateTime PublishedAt { get; set; }
    public int ProjectId { get; set; }
    public required  int DefectId { get; set; }
    public int? ExecutorId { get; set; }
    public DateOnly? Term { get; set; }
    [DefaultValue(0)]
    public required Status Status { get; set; }
    public string? Comments { get; set; }
    public SqlMoney Investment { get; set; }
    [DefaultValue(true)]
    public bool IsActual { get; set; }
    public Defect? Defect { get; set; }
    public User? Executor { get; set; }
    public Project? Project { get; set; }
}