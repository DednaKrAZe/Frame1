namespace Engineering.Models;

public class UserResponse
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Role Role { get; set; }
}