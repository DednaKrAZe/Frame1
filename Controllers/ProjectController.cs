using Engineering.Context;
using Engineering.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Engineering.Controllers;

[ApiController]
[Authorize(Roles = "Manager, Director")]
[Route("projects")]
public class ProjectController : ControllerBase
{
    private readonly ApplicationContext _context;
    private readonly ILogger<ProjectController> _logger;

    public ProjectController(ApplicationContext context, ILogger<ProjectController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // получить список проектов
    [HttpGet]
    public async Task<ActionResult<List<Project>>> GetAllProjects()
    {
        var projects = await _context.Projects.ToListAsync();

        _logger.LogInformation("Get all projects");
        return Ok(projects);
    }
    
    // получить проект по id
    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProjectById(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(project => project.Id == id);
        if (project == null)
        {
            _logger.LogWarning("Project not found: {ProjectId}", id);
            return NotFound();
        }

        _logger.LogInformation("Get project by id: {ProjectId}", id);
        return Ok(project);
    }

    // создать проект
    [HttpPost]
    public async Task<ActionResult> CreateProject([FromBody] Project project)
    {
        try
        {
            var newProject = new Project
            {
                Name = project.Name,
                Status = project.Status
            };
            await _context.Projects.AddAsync(newProject);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Project created");
            return Created();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while creating project: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }

    // обновить существующий проект
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProject([FromBody] ProjectRequest project, int id)
    {
        try
        {
            var existingProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (existingProject == null)
            {
                _logger.LogWarning("Project not found: {ProjectId}", id);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(project.Name))
            {
                existingProject.Name = project.Name;
            }

            if (project.Status != 0)
            {
                existingProject.Status = project.Status;
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Project updated: {ProjectId}", id);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while updating project: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }
    
    // удалить проект
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        try
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                _logger.LogWarning("Project not found: {ProjectId}", id);
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Project deleted: {ProjectId}", id);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while deleting project: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }
}