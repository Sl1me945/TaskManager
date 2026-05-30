using Microsoft.AspNetCore.Mvc;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;

namespace ToDoApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskRepository taskRepository, ILogger<TasksController> logger)
        {
            _taskRepository = taskRepository;
            _logger = logger;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<List<BaseTask>>> GetAll()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return Ok(tasks);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseTask>> GetById(Guid id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return NotFound($"Task with id {id} not found");

            return Ok(task);
        }

        // GET: api/tasks/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<BaseTask>>> GetByUserId(Guid userId)
        {
            var tasks = await _taskRepository.GetByUserIdAsync(userId);
            return Ok(tasks);
        }

        // POST: api/tasks/simple
        [HttpPost("simple")]
        public async Task<ActionResult<SimpleTask>> CreateSimpleTask([FromBody] CreateSimpleTaskRequest request)
        {
            try
            {
                var task = new SimpleTask(
                    request.Title,
                    request.Description ?? "",
                    request.DueDate,
                    request.Priority
                );
                task.AssignToUser(request.UserId);

                await _taskRepository.AddAsync(task);
                await _taskRepository.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/tasks/work
        [HttpPost("work")]
        public async Task<ActionResult<WorkTask>> CreateWorkTask([FromBody] CreateWorkTaskRequest request)
        {
            try
            {
                var task = new WorkTask(
                    request.Title,
                    request.Description ?? "",
                    request.DueDate,
                    request.Priority,
                    request.ProjectName
                );
                task.AssignToUser(request.UserId);

                await _taskRepository.AddAsync(task);
                await _taskRepository.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return NotFound($"Task with id {id} not found");

            try
            {
                if (!string.IsNullOrWhiteSpace(request.Title))
                    task.UpdateTitle(request.Title);

                if (request.Description != null)
                    task.UpdateDescription(request.Description);

                if (request.DueDate.HasValue)
                    task.SetDueDate(request.DueDate.Value);

                if (request.Priority.HasValue)
                    task.SetPriority(request.Priority.Value);

                if (request.IsCompleted && !task.IsCompleted)
                    task.MarkAsCompleted();

                // WorkTask specific
                if (task is WorkTask workTask && !string.IsNullOrWhiteSpace(request.ProjectName))
                    workTask.UpdateProjectName(request.ProjectName);

                await _taskRepository.UpdateAsync(task);
                await _taskRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _taskRepository.DeleteAsync(id);
                await _taskRepository.SaveChangesAsync();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }

    // DTO classes
    public record CreateSimpleTaskRequest(
        Guid UserId,
        string Title,
        string? Description,
        DateTime DueDate,
        Priority Priority
    );

    public record CreateWorkTaskRequest(
        Guid UserId,
        string Title,
        string? Description,
        DateTime DueDate,
        Priority Priority,
        string ProjectName
    );

    public record UpdateTaskRequest(
        string? Title,
        string? Description,
        DateTime? DueDate,
        Priority? Priority,
        bool IsCompleted,
        string? ProjectName
    );
}