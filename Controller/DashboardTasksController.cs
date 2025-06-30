//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using TimeTrackerApi.Data;
//using TimeTrackerApi.Models;
//using TimeTrackerApi.Models.Dtos;
//using System.IdentityModel.Tokens.Jwt;


//namespace TimeTrackerApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    public class DashboardTasksController : ControllerBase
//    {
//        private readonly TimeTrackerContext _context;

//        public DashboardTasksController(TimeTrackerContext context)
//        {
//            _context = context;
//        }

//        // 🔐 Helper: Get user ID from JWT
//        private string? GetUserId()
//        {
//            return User.FindFirstValue(ClaimTypes.NameIdentifier)
//                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
//        }

//        // GET: api/dashboardtasks
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<DashboardTask>>> GetTasks()
//        {
//            var userId = GetUserId();
//            if (userId == null) return Unauthorized();

//            var tasks = await _context.DashboardTasks
//                .Where(t => t.UserId == userId)
//                .ToListAsync();

//            return Ok(tasks);
//        }

//        // POST: api/dashboardtasks
//        [HttpPost]
//        public async Task<ActionResult<DashboardTask>> CreateTask([FromBody] CreateTaskDto dto)
//        {
//            var userId = GetUserId();
//            if (userId == null) return Unauthorized();

//            var task = new DashboardTask
//            {
//                Id = Guid.NewGuid(),
//                Name = dto.Name,
//                Description = dto.Description,
//                StoryPoints = dto.StoryPoints,
//                HoursNeeded = dto.HoursNeeded,
//                HoursTaken = dto.HoursTaken,
//                Status = dto.Status,
//                UserId = userId
//            };

//            _context.DashboardTasks.Add(task);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
//        }

//        // PUT: api/dashboardtasks/{id}
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] DashboardTask task)
//        {
//            var userId = GetUserId();
//            if (userId == null) return Unauthorized();

//            if (id != task.Id) return BadRequest("Task ID mismatch.");
//            if (task.UserId != userId) return Forbid("You are not allowed to modify this task.");

//            _context.Entry(task).State = EntityState.Modified;

//            try
//            {
//                await _context.SaveChangesAsync();
//                return NoContent();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!await _context.DashboardTasks.AnyAsync(t => t.Id == id && t.UserId == userId))
//                    return NotFound("Task not found.");
//                throw;
//            }
//        }

//        // DELETE: api/dashboardtasks/{id}
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteTask(Guid id)
//        {
//            var userId = GetUserId();
//            if (userId == null) return Unauthorized();

//            var task = await _context.DashboardTasks
//                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

//            if (task == null)
//                return NotFound("Task not found or access denied.");

//            _context.DashboardTasks.Remove(task);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }
//    }
//}

// File: Controllers/DashboardTasksController.cs

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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DashboardTask>>> GetTasks()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var tasks = await _context.DashboardTasks
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return Ok(tasks);
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
                UserId = userId
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
