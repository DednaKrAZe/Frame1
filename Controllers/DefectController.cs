using Engineering.Context;
using Engineering.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Engineering.Controllers;

[ApiController]
[Authorize(Roles = "Manager, Director")]
[Route("defects")]
public class DefectController : ControllerBase
{
    private readonly ApplicationContext _context;
    private readonly ILogger<UserController> _logger;

    public DefectController(ApplicationContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // получить список дефектов
    [HttpGet]
    public async Task<ActionResult<List<Defect>>> GetAllDefects()
    {
        var defects = await _context.Defects.ToListAsync();

        _logger.LogInformation("Get all defects");
        return Ok(defects);
    }

    // получить дефект по id
    [HttpGet("{id}")]
    public async Task<ActionResult<Defect>> GetDefectById(int id)
    {
        var defect = await _context.Defects.FirstOrDefaultAsync(defect => defect.Id == id);
        if (defect == null)
        {
            _logger.LogWarning("Defect not found: {DefectId}", id);
            return NotFound();
        }

        _logger.LogInformation("Get defect by id: {DefectId}", id);
        return Ok(defect);
    }

    // создать дефект
    [HttpPost]
    public async Task<ActionResult> CreateDefect([FromBody] Defect defect)
    {
        try
        {
            var newDefect = new Defect
            {
                Name = defect.Name,
                Description = defect.Description,
                Priority = defect.Priority
            };
            await _context.Defects.AddAsync(newDefect);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Defect created");
            return Created();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while creating defect: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }

    // обновить существующий дефект
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateDefect([FromBody] DefectRequest defect, int id)
    {
        try
        {
            var existingDefect = await _context.Defects.FirstOrDefaultAsync(d => d.Id == id);
            if (existingDefect == null)
            {
                _logger.LogWarning("Defect not found: {DefectId}", id);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(defect.Name))
            {
                existingDefect.Name = defect.Name;
            }

            if (!string.IsNullOrEmpty(defect.Description))
            {
                existingDefect.Description = defect.Description;
            }

            if (defect.Priority != 0)
            {
                existingDefect.Priority = defect.Priority;
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Defect updated: {DefectId}", id);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while updating defect: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }
    
    // удалить дефект
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDefect(int id)
    {
        try
        {
            var defect = await _context.Defects.FirstOrDefaultAsync(d => d.Id == id);
            if (defect == null)
            {
                _logger.LogWarning("Defect not found: {DefectId}", id);
                return NotFound();
            }

            _context.Defects.Remove(defect);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Defect deleted: {DefectId}", id);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while deleting defect: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }
}
