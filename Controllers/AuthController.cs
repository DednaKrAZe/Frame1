using System.Security.Claims;
using Engineering.Context;
using Engineering.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication;
using Task = System.Threading.Tasks.Task;

namespace Engineering.Controllers;

[ApiController]
[Route("auth")]
[EnableCors("allow_cors")]
public class AuthController : ControllerBase
{
    private readonly ApplicationContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] Models.Login login)
    {
        if (!_context.Users.Any())
        {
            _context.Users.Add(new User
            {
                Name = "admin",
                Email = "admin@admin.com",
                Login = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(BCrypt.Net.BCrypt.HashPassword("admin") + "admin"),
                Phone = "+78005553535",
                Role = 0
            });
            _context.SaveChanges();
        }

        if (login == null)
        {
            _logger.LogWarning("Null request");
            return UnprocessableEntity("Request cannot be null.");
        }

        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == login.UserLogin);

            if (user == null)
            {
                _logger.LogWarning("User not found");
                return NotFound();
            }

            if (!user.PasswordHash.Equals(BCrypt.Net.BCrypt.HashPassword(login.PasswordHash)))
            {
                _logger.LogWarning("Wrong password");
                return Unauthorized();
            }

            await Authenticate(user.Id.ToString(), user.Role.ToString());
            _logger.LogInformation("Login");
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while login: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }

    private async Task Authenticate(string id, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, id),
            new Claim(ClaimTypes.Role, role)
        };
        ClaimsIdentity identity = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    [HttpPost]
    [Authorize]
    [Route("logout")]
    private async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("Logout");
        return Ok();
    }
}