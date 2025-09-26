using Engineering.Context;
using Engineering.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using TaskModel = Engineering.Models.Task;

namespace Engineering.Controllers;

[ApiController]
[Authorize(Roles = "Manager, Director")]
[Route("tasks")]
public class TaskController : ControllerBase
{
    private readonly ApplicationContext _context;
    private readonly ILogger<TaskController> _logger;

    public TaskController(ApplicationContext context, ILogger<TaskController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // получить список актуальных задач
    [HttpGet]
    public async Task<ActionResult<List<TaskModel>>> GetAllTasks()
    {
        var tasks = await _context.Tasks.Where(t => t.IsActual == true).ToListAsync();
        _logger.LogInformation("Get all tasks");
        return Ok(tasks);
    }

    // получить задачу по id
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskModel>> GetTaskById([FromRoute] int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Defect)
            .Include(t => t.Executor)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            _logger.LogWarning("Task not found: {TaskId}", id);
            return NotFound();
        }

        _logger.LogInformation("Get task by id: {TaskId}", id);
        return Ok(task);
    }

    // создать задачу (не закончена)
    [HttpPost]
    public async Task<ActionResult> CreateTask([FromBody] TaskModel task)
    {
        try
        {
            var existingDefect = await _context.Defects.FirstOrDefaultAsync(d => d.Id == task.DefectId);
            if (existingDefect != null)
            {
                _logger.LogWarning("Task for defect {DefectId} already exists, must be updated", task.DefectId);
                return Conflict();
            }
            task.PublishedAt = DateTime.Now;
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task created");
            return Created();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while creating task: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }
    
    // обновить существующую задачу
    [HttpPut("{defect_id}")]
    public async Task<ActionResult> UpdateTask([FromBody] TaskRequest task, [FromRoute] int defectId)
    {
        try
        {
            var existingTask = await _context.Tasks
                .OrderByDescending(t => t.PublishedAt)
                .FirstOrDefaultAsync(t => t.DefectId == defectId);
            if (existingTask == null)
            {
                _logger.LogWarning("Task not found: {DefectId}", defectId);
                return NotFound();
            }

            // if (existingTask.IsActual == false)
            // {
            //     _logger.LogWarning("Task is not actual: {DefectId}", defectId);
            //     return BadRequest();
            // }

            var newTask = new TaskModel
            {
                PublishedAt = DateTime.Now,
                ProjectId = task.ProjectId ?? existingTask.ProjectId,
                DefectId = task.DefectId ?? existingTask.DefectId,
                ExecutorId = task.ExecutorId ?? existingTask.ExecutorId,
                Term = task.Term ?? existingTask.Term,
                Status = task.Status ?? existingTask.Status,
                Comments = task.Comments ?? existingTask.Comments,
                Investment = task.Investment ?? existingTask.Investment,
                IsActual = true
            };
            
            _context.Tasks.Add(newTask);
            
            existingTask.IsActual = false;
            _context.Tasks.Update(existingTask);
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Task updated: {DefectId}", defectId);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while updating task: {Error}", e.Message);
            return StatusCode(500, $"Internal server error");
        }
    }
}