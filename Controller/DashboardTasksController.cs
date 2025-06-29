using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackerApi.Data;
using TimeTrackerApi.Models;

namespace TimeTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardTasksController : ControllerBase
    {
        private readonly TimeTrackerContext _context;

        public DashboardTasksController(TimeTrackerContext context)
        {
            _context = context;
        }

        // GET: api/dashboardtasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DashboardTask>>> GetTasks()
        {
            return await _context.DashboardTasks.ToListAsync();
        }

        // POST: api/dashboardtasks
        [HttpPost]
        public async Task<ActionResult<DashboardTask>> CreateTask(DashboardTask task)
        {
            task.Id = Guid.NewGuid(); // Assign new ID
            _context.DashboardTasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
        }

        // PUT: api/dashboardtasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, DashboardTask task)
        {
            if (id != task.Id)
                return BadRequest("Task ID mismatch");

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DashboardTasks.Any(t => t.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/dashboardtasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var task = await _context.DashboardTasks.FindAsync(id);
            if (task == null)
                return NotFound();

            _context.DashboardTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
