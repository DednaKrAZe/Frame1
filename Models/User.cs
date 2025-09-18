using System.ComponentModel.DataAnnotations;

namespace Engineering.Models;

public class User
{
    public int Id { get; set; }
    [MaxLength(256)]
    public required string Name { get; set; }
    [MaxLength(128)]
    public required string Login { get; set; }
    public required string PasswordHash { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public Role Role { get; set; }
}