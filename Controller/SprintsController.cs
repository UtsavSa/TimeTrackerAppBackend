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
    [Route("api/sprints")]
    [Authorize]
    public class SprintsController : ControllerBase
    {
        private readonly TimeTrackerContext _context;

        public SprintsController(TimeTrackerContext context)
        {
            _context = context;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ✅ Create a new sprint
        [HttpPost]
        public async Task<ActionResult<SprintDto>> CreateSprint([FromBody] CreateSprintDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            var startUtc = dto.StartDate.Kind == DateTimeKind.Utc ? dto.StartDate : DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);

            var endUtc = dto.EndDate.Kind == DateTimeKind.Utc ? dto.EndDate : DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);



            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                //StartDate = dto.StartDate,
                StartDate = startUtc,
                EndDate = endUtc,
                //EndDate = dto.EndDate,
                CreatedByUserId = user.Id,
                SprintUsers = new List<SprintUser>
                {
                    new SprintUser { UserId = user.Id }
                }
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();


            var sprintDto = new SprintDto
            {
                Id = sprint.Id,
                Name = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                TotalStoryPoints = sprint.TotalStoryPoints,
                TotalHoursTaken = sprint.TotalHoursTaken,
                UserEmails = new List<string> { user.Email! },
                CreatedBy = user.Email!
            };

            return Ok(sprintDto);
        }

        // ✅ Get all sprints for current user
        [HttpGet("mine")]
        public async Task<IActionResult> GetMySprints()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var sprints = await _context.Sprints
                .Where(s => s.SprintUsers.Any(su => su.UserId == userId))
                .Include(s => s.SprintUsers)
                    .ThenInclude(su => su.User)
                .Include(s => s.CreatedByUser)
                .ToListAsync();

            var sprintDtos = sprints.Select(s => new SprintDto
            {
                Id = s.Id,
                Name = s.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                TotalStoryPoints = s.TotalStoryPoints,
                TotalHoursTaken = s.TotalHoursTaken,
                UserEmails = s.SprintUsers.Select(su => su.User.Email!).ToList(),
                CreatedBy = s.CreatedByUser.Email!
            }).ToList();

            return Ok(sprintDtos);
        }


        // ✅ Get details of one sprint + tasks
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSprintDetails(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isPartOfSprint = await _context.SprintUsers
                .AnyAsync(su => su.SprintId == id && su.UserId == userId);

            if (!isPartOfSprint) return Forbid("You are not part of this sprint");

            var sprint = await _context.Sprints
                .Include(s => s.Tasks)
                .Include(s => s.SprintUsers)
                .Include(s => s.CreatedByUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null) return NotFound();

            var dto = new SprintDto
            {
                Id = sprint.Id,
                Name = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                TotalStoryPoints = sprint.TotalStoryPoints,
                TotalHoursTaken = sprint.TotalHoursTaken,
                UserEmails = sprint.SprintUsers.Select(su => su.User.Email!).ToList(),
                CreatedBy = sprint.CreatedByUser.Email!
            };

            return Ok(dto);
        }

        // ✅ Delete sprint (only if user is creator)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSprint(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var sprint = await _context.Sprints.FirstOrDefaultAsync(s => s.Id == id);
            if (sprint == null) return NotFound();

            if (sprint.CreatedByUserId != userId)
                return Forbid("Only the creator can delete this sprint");

            _context.Sprints.Remove(sprint);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Add user to sprint (only by creator)
        [HttpPost("{sprintId}/add-user")]
        public async Task<IActionResult> AddUserToSprint(Guid sprintId, [FromBody] AddUserToSprintDto dto)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized();

            var sprint = await _context.Sprints
                .Include(s => s.SprintUsers)
                .FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint == null) return NotFound("Sprint not found");
            if (sprint.CreatedByUserId != currentUserId)
                return Forbid("Only the sprint creator can add users");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return NotFound("User not found");

            if (sprint.SprintUsers.Any(su => su.UserId == user.Id))
                return BadRequest("User already in sprint");

            sprint.SprintUsers.Add(new SprintUser { UserId = user.Id });
            await _context.SaveChangesAsync();

            return Ok(new { message = "User added to sprint" });
        }

        // ✅ Get all users in a sprint
        [HttpGet("{sprintId}/users")]
        public async Task<IActionResult> GetSprintUsers(Guid sprintId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isPartOfSprint = await _context.SprintUsers
                .AnyAsync(su => su.SprintId == sprintId && su.UserId == userId);

            if (!isPartOfSprint)
                return Forbid("You are not part of this sprint");

            var users = await _context.SprintUsers
                .Where(su => su.SprintId == sprintId)
                .Include(su => su.User)
                .Select(su => su.User.Email!)
                .ToListAsync();

            return Ok(users);
        }

    }
}
