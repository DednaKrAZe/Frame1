namespace Engineering.Models;

public class Login
{
    public required string UserLogin { get; set; }
    public required string PasswordHash { get; set; }
}