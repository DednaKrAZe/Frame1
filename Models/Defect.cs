namespace Engineering.Models;

public class Defect
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; }
}