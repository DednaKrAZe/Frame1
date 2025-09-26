using Engineering.Context;
using Engineering.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Net.Http.Headers;

namespace Engineering.Controllers;

[ApiController]
[Authorize(Roles = "Manager, Director")]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly ApplicationContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(ApplicationContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private UserResponse ConvertUserToResponse(User user)
    {
        return new UserResponse
        {
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Login = user.Login,
            Role = user.Role
        };
    }
    
    // получить список пользователей
    [HttpGet]
    [Authorize(Roles = "Manager, Director")]
    public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
    {
        var users = await _context.Users.Select(u => ConvertUserToResponse(u)).ToListAsync();
        _logger.LogInformation("Get all users");
        return Ok(users);
    }
    
    // получить пользователя по id
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById([FromRoute] int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return NotFound();
        }

        var response = ConvertUserToResponse(user);
        _logger.LogInformation("Get user by id: {UserId}", id);
        return Ok(response);
    }
    
    // создать пользователя
    // TODO: на фронтенде пароль должен хешироваться перед передачей
    [HttpPost]
    public async Task<ActionResult> CreateUser([FromBody] User user)
    {
        try
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
            if (existingUser != null)
            {
                _logger.LogWarning("User with login already exists {UserLogin}", user.Login);
                return Conflict();
            }
            
            var hashed = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash + user.Login);
            
            var newUser = new User
            {
                Name = user.Name,
                Email = user.Email,
                PasswordHash = hashed,
                Phone = user.Phone,
                Login = user.Login,
                Role = user.Role
            };
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while creating user: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
        
        _logger.LogInformation("User created");
        return Created();
    }
    
    // обновить существующего пользователя
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser([FromRoute] int id, [FromBody] Models.UserRequest user)
    {
        try
        {
            user.Id = id;
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existingUser == null)
            {
                _logger.LogWarning("User not found: {UserId}", id);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(user.Name))
            {
                existingUser.Name = user.Name;
            }

            if (!string.IsNullOrEmpty(user.Login))
            {
                existingUser.Login = user.Login;
            }

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash + existingUser.Login);
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                existingUser.Email = user.Email;
            }

            if (!string.IsNullOrEmpty(user.Phone))
            {
                existingUser.Phone = user.Phone;
            }

            if (user.Role != 0)
            {
                existingUser.Role = user.Role;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User updated: {UserId}", id);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while updating user: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }

    // удалить пользователя
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("User deleted: {UserId}", id);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while deleting user: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }
}