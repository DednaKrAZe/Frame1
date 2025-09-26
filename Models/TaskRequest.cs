using System.Data.SqlTypes;

namespace Engineering.Models;

public class TaskRequest
{
    public int? ProjectId { get; set; }
    public int? DefectId { get; set; }
    public int? ExecutorId { get; set; }
    public DateOnly? Term { get; set; }
    public Status? Status { get; set; }
    public string? Comments { get; set; }
    public SqlMoney? Investment { get; set; }
}