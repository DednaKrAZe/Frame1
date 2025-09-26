using Engineering.Context;
using Engineering.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Net.Http.Headers;

namespace Engineering.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly ApplicationContext _context;

    public UserController(ApplicationContext context)
    {
        _context = context;
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
    public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
    {
        var users = await _context.Users.Select(u => ConvertUserToResponse(u)).ToListAsync();
        Console.WriteLine("Get all users");
        return Ok(users);
    }
    
    // получить пользователя по id
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            Console.WriteLine("User not found:" + id);
            return NotFound();
        }

        var response = ConvertUserToResponse(user);
        Console.WriteLine("Get user by id:" + id);
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
                Console.WriteLine("User with login already exists:" + user.Login);
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
            Console.WriteLine("Error while deleting user: " + e.Message);
            return StatusCode(500, $"Internal server error");
        }
        
        Console.WriteLine("User created:" + user.Login);
        return Created();
    }
    
    // обновить существующего пользователя
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] Models.UserRequest user)
    {
        try
        {
            user.Id = id;
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existingUser == null)
            {
                Console.WriteLine("User not found:" + id);
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
                existingUser.PasswordHash = user.PasswordHash;
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
            Console.WriteLine("User updated:" + id);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while deleting user: " + e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }

    // удалить пользователя
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("User deleted:" + id);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while deleting user: " + e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }
}