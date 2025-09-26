using System.ComponentModel.DataAnnotations;

namespace Engineering.Models;

public class UserRequest
{
    public int? Id { get; set; }

    [MaxLength(256)]
    public string? Name { get; set; }
    [MaxLength(128)]
    public string? Login { get; set; }
    public string? PasswordHash { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Role Role { get; set; }
}