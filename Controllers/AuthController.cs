using System.Security.Claims;
using Engineering.Context;
using Engineering.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
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
        if (login == null)
        {
            _logger.LogWarning("Null request");
            return UnprocessableEntity("Request cannot be null.");
        }

        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login.UserLogin);

            if (user == null)
            {
                _logger.LogWarning("User not found: {Login}", login.UserLogin);
                return NotFound();
            }

            if (!BCrypt.Net.BCrypt.Verify(login.PasswordHash + login.UserLogin, user.PasswordHash))
            {
                _logger.LogWarning("Wrong password for user: {Login}", login.UserLogin);
                return Unauthorized();
            }

            await Authenticate(user.Id.ToString(), user.Role.ToString());
            _logger.LogInformation("Login user: {User}", user.Login);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while login: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }

    [HttpPost]
    [Authorize]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("Logout");
        return Ok();
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
}