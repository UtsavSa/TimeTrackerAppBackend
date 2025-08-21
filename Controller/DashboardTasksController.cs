


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeTrackerApi.Data;
using TimeTrackerApi.Models;
using TimeTrackerApi.Models.Dtos;

namespace TimeTrackerApi.Controllers
{
    [ApiController]
    [Route("api/dashboardtasks")] // ✅ Matches Angular frontend
    [Authorize]
    public class DashboardTasksController : ControllerBase
    {
        private readonly TimeTrackerContext _context;

        public DashboardTasksController(TimeTrackerContext context)
        {
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // GET: api/dashboardtasks
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<DashboardTask>>> GetTasks()
        //{
        //    var userId = GetUserId();
        //    if (userId == null) return Unauthorized();

        //    var tasks = await _context.DashboardTasks
        //        .Where(t => t.UserId == userId)
        //        .ToListAsync();

        //    return Ok(tasks);
        //}

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<DashboardTask>>> GetTasks([FromQuery] Guid? sprintId = null)
        //{
        //    var userId = GetUserId();
        //    if (userId == null) return Unauthorized();

        //    var query = _context.DashboardTasks.Where(t => t.UserId == userId);

        //    if (sprintId.HasValue)
        //        query = query.Where(t => t.SprintId == sprintId.Value);

        //    var tasks = await query.ToListAsync();
        //    return Ok(tasks);
        //}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DashboardTask>>> GetTasks([FromQuery] Guid? sprintId = null)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (sprintId.HasValue)
            {
                // ✅ Confirm user is in the sprint
                var isInSprint = await _context.SprintUsers
                    .AnyAsync(su => su.SprintId == sprintId.Value && su.UserId == userId);

                if (!isInSprint)
                    return Forbid("You are not a member of this sprint");

                // ✅ Return ALL tasks for the sprint (not just user's)
                var sprintTasks = await _context.DashboardTasks
                    .Where(t => t.SprintId == sprintId.Value)
                    .ToListAsync();

                return Ok(sprintTasks);
            }

            // ✅ If no sprint, return user's unassigned personal tasks
            var personalTasks = await _context.DashboardTasks
                .Where(t => t.UserId == userId && t.SprintId == null)
                .ToListAsync();

            return Ok(personalTasks);
        }


        // POST: api/dashboardtasks
        [HttpPost]
        public async Task<ActionResult<DashboardTask>> CreateTask([FromBody] CreateTaskDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var task = new DashboardTask
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                StoryPoints = dto.StoryPoints,
                HoursNeeded = dto.HoursNeeded,
                HoursTaken = dto.HoursTaken,
                Status = dto.Status,
                UserId = userId,
                SprintId = dto.SprintId,
            };

            _context.DashboardTasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
        }

        // PUT: api/dashboardtasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] DashboardTask task)
        {
            var userId = GetUserId();
            if (userId == null || task.UserId != userId || id != task.Id)
                return Unauthorized();

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
        }

        // DELETE: api/dashboardtasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var userId = GetUserId();
            var task = await _context.DashboardTasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null) return NotFound();

            _context.DashboardTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
