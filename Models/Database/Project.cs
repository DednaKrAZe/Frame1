namespace Engineering.Models;

public class Project
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public Status Status { get; set; }
}