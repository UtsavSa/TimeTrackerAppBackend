//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using TimeTrackerApi.Data;
//using TimeTrackerApi.Models;
//using TimeTrackerApi.Models.Dtos;

//namespace TimeTrackerApi.Controllers
//{
//    [ApiController]
//    [Route("api/sprints")]
//    [Authorize]
//    public class SprintsController : ControllerBase
//    {
//        private readonly TimeTrackerContext _context;

//        public SprintsController(TimeTrackerContext context)
//        {
//            _context = context;
//        }

//        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

//        // ✅ Create a new sprint
//        [HttpPost]
//        public async Task<ActionResult<SprintDto>> CreateSprint([FromBody] CreateSprintDto dto)
//        {
//            var userId = GetUserId();
//            if (string.IsNullOrEmpty(userId))
//                return Unauthorized();

//            var user = await _context.Users.FindAsync(userId);
//            if (user == null)
//                return Unauthorized();

//            var startUtc = dto.StartDate.Kind == DateTimeKind.Utc ? dto.StartDate : DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);

//            var endUtc = dto.EndDate.Kind == DateTimeKind.Utc ? dto.EndDate : DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);



//            var sprint = new Sprint
//            {
//                Id = Guid.NewGuid(),
//                Name = dto.Name,
//                //StartDate = dto.StartDate,
//                StartDate = startUtc,
//                EndDate = endUtc,
//                //EndDate = dto.EndDate,
//                CreatedByUserId = user.Id,
//                SprintUsers = new List<SprintUser>
//                {
//                    new SprintUser { UserId = user.Id }
//                }
//            };

//            _context.Sprints.Add(sprint);
//            await _context.SaveChangesAsync();


//            var sprintDto = new SprintDto
//            {
//                Id = sprint.Id,
//                Name = sprint.Name,
//                StartDate = sprint.StartDate,
//                EndDate = sprint.EndDate,
//                TotalStoryPoints = sprint.TotalStoryPoints,
//                TotalHoursTaken = sprint.TotalHoursTaken,
//                UserEmails = new List<string> { user.Email! },
//                CreatedBy = user.Email!
//            };

//            return Ok(sprintDto);
//        }

//        // ✅ Get all sprints for current user
//        [HttpGet("mine")]
//        public async Task<IActionResult> GetMySprints()
//        {
//            var userId = GetUserId();
//            if (userId == null) return Unauthorized();

//            var sprints = await _context.Sprints
//                .Where(s => s.SprintUsers.Any(su => su.UserId == userId))
//                .Include(s => s.SprintUsers)
//                    .ThenInclude(su => su.User)
//                .Include(s => s.CreatedByUser)
//                .ToListAsync();

//            var sprintDtos = sprints.Select(s => new SprintDto
//            {
//                Id = s.Id,
//                Name = s.Name,
//                StartDate = s.StartDate,
//                EndDate = s.EndDate,
//                TotalStoryPoints = s.TotalStoryPoints,
//                TotalHoursTaken = s.TotalHoursTaken,
//                UserEmails = s.SprintUsers.Select(su => su.User.Email!).ToList(),
//                CreatedBy = s.CreatedByUser.Email!
//            }).ToList();

//            return Ok(sprintDtos);
//        }


//        // ✅ Get details of one sprint + tasks
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetSprintDetails(Guid id)
//        {
//            var userId = GetUserId();
//            if (userId == null) return Unauthorized();

//            var isPartOfSprint = await _context.SprintUsers
//                .AnyAsync(su => su.SprintId == id && su.UserId == userId);

//            if (!isPartOfSprint) return Forbid("You are not part of this sprint");

//            var sprint = await _context.Sprints
//                .Include(s => s.Tasks)
//                .Include(s => s.SprintUsers)
//                .Include(s => s.CreatedByUser)
//                .FirstOrDefaultAsync(s => s.Id == id);

//            if (sprint == null) return NotFound();

//            var dto = new SprintDto
//            {
//                Id = sprint.Id,
//                Name = sprint.Name,
//                StartDate = sprint.StartDate,
//                EndDate = sprint.EndDate,
//                TotalStoryPoints = sprint.TotalStoryPoints,
//                TotalHoursTaken = sprint.TotalHoursTaken,
//                UserEmails = sprint.SprintUsers.Select(su => su.User.Email!).ToList(),
//                CreatedBy = sprint.CreatedByUser.Email!
//            };

//            return Ok(dto);
//        }

//        // ✅ Delete sprint (only if user is creator)
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteSprint(Guid id)
//        {
//            var userId = GetUserId();
//            if (userId == null) return Unauthorized();

//            var sprint = await _context.Sprints.FirstOrDefaultAsync(s => s.Id == id);
//            if (sprint == null) return NotFound();

//            if (sprint.CreatedByUserId != userId)
//                return Forbid("Only the creator can delete this sprint");

//            _context.Sprints.Remove(sprint);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        // ✅ Add user to sprint (only by creator)
//        [HttpPost("{sprintId}/add-user")]
//        public async Task<IActionResult> AddUserToSprint(Guid sprintId, [FromBody] AddUserToSprintDto dto)
//        {
//            var currentUserId = GetUserId();
//            if (currentUserId == null) return Unauthorized();

//            var sprint = await _context.Sprints
//                .Include(s => s.SprintUsers)
//                .FirstOrDefaultAsync(s => s.Id == sprintId);

//            if (sprint == null) return NotFound("Sprint not found");
//            if (sprint.CreatedByUserId != currentUserId)
//                return Forbid("Only the sprint creator can add users");

//            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
//            if (user == null) return NotFound("User not found");

//            if (sprint.SprintUsers.Any(su => su.UserId == user.Id))
//                return BadRequest("User already in sprint");

//            sprint.SprintUsers.Add(new SprintUser { UserId = user.Id });
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "User added to sprint" });
//        }

//        // ✅ Get all users in a sprint
//        [HttpGet("{sprintId}/users")]
//        public async Task<IActionResult> GetSprintUsers(Guid sprintId)
//        {
//            var userId = GetUserId();
//            if (userId == null) return Unauthorized();

//            var isPartOfSprint = await _context.SprintUsers
//                .AnyAsync(su => su.SprintId == sprintId && su.UserId == userId);

//            if (!isPartOfSprint)
//                return Forbid("You are not part of this sprint");

//            var users = await _context.SprintUsers
//                .Where(su => su.SprintId == sprintId)
//                .Include(su => su.User)
//                .Select(su => su.User.Email!)
//                .ToListAsync();

//            return Ok(users);
//        }

//        // ✅ Get completed story points per sprint for current user
//        [HttpGet("mine/progress")]
//        public async Task<ActionResult<IEnumerable<SprintProgressDto>>> GetMySprintProgress()
//        {
//            var userId = GetUserId();
//            if (string.IsNullOrEmpty(userId)) return Unauthorized();

//            // If your Task entity is called differently (e.g., SprintTask), adjust below.
//            // If Status is an enum, replace the string "Done" comparison with the enum check.
//            var progress = await _context.Sprints
//                .Where(s => s.SprintUsers.Any(su => su.UserId == userId))
//                .Select(s => new SprintProgressDto
//                {
//                    SprintId = s.Id,
//                    DoneStoryPoints = s.Tasks
//                        // If you use a string status:
//                        .Where(t => t.Status == "Done")
//                        // If you use an enum (e.g., TaskStatus.Done), use: .Where(t => t.Status == TaskStatus.Done)
//                        .Sum(t => (int?)t.StoryPoints ?? 0)
//                })
//                .AsNoTracking()
//                .ToListAsync();

//            return Ok(progress);
//        }

//    }
//}


//-----------------


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
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            // Ensure UTC
            var startUtc = dto.StartDate.Kind == DateTimeKind.Utc
                ? dto.StartDate
                : DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
            var endUtc = dto.EndDate.Kind == DateTimeKind.Utc
                ? dto.EndDate
                : DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);

            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StartDate = startUtc,
                EndDate = endUtc,
                CreatedByUserId = user.Id,
                SprintUsers = new List<SprintUser>
                {
                    new SprintUser { UserId = user.Id }
                }
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            // Return minimal DTO; TotalStoryPoints will be computed on reads
            var sprintDto = new SprintDto
            {
                Id = sprint.Id,
                Name = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                TotalStoryPoints = sprint.TotalStoryPoints, // likely 0 now; read endpoints compute it
                TotalHoursTaken = sprint.TotalHoursTaken,
                UserEmails = new List<string> { user.Email! },
                CreatedBy = user.Email!
            };

            return Ok(sprintDto);
        }

        // ✅ Get all sprints for current user (computes TotalStoryPoints from tasks)
        [HttpGet("mine")]
        public async Task<IActionResult> GetMySprints()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var sprintDtos = await _context.Sprints
                .Where(s => s.SprintUsers.Any(su => su.UserId == userId))
                .Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,

                    // 👇 Compute from tasks so UI isn't stuck at 0
                    TotalStoryPoints = s.Tasks.Sum(t => (int?)t.StoryPoints ?? 0),

                    TotalHoursTaken = s.TotalHoursTaken,
                    UserEmails = s.SprintUsers.Select(su => su.User.Email!).ToList(),
                    CreatedBy = s.CreatedByUser.Email!
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(sprintDtos);
        }

        // ✅ Get details of one sprint + tasks (computes TotalStoryPoints from tasks)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSprintDetails(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isPartOfSprint = await _context.SprintUsers
                .AnyAsync(su => su.SprintId == id && su.UserId == userId);

            if (!isPartOfSprint) return Forbid("You are not part of this sprint");

            var dto = await _context.Sprints
                .Where(s => s.Id == id)
                .Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,

                    // 👇 Compute from tasks
                    TotalStoryPoints = s.Tasks.Sum(t => (int?)t.StoryPoints ?? 0),

                    TotalHoursTaken = s.TotalHoursTaken,
                    UserEmails = s.SprintUsers.Select(su => su.User.Email!).ToList(),
                    CreatedBy = s.CreatedByUser.Email!
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
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
                .Select(su => su.User.Email!)
                .AsNoTracking()
                .ToListAsync();

            return Ok(users);
        }

        // ✅ Get completed story points per sprint for current user
        //[HttpGet("mine/progress")]
        //public async Task<ActionResult<IEnumerable<SprintProgressDto>>> GetMySprintProgress()
        //{
        //    var userId = GetUserId();
        //    if (string.IsNullOrEmpty(userId)) return Unauthorized();

        //    var progress = await _context.Sprints
        //        .Where(s => s.SprintUsers.Any(su => su.UserId == userId))
        //        .Select(s => new SprintProgressDto
        //        {
        //            SprintId = s.Id,
        //            DoneStoryPoints = s.Tasks
        //                // If you use an enum for Status, replace with the enum check
        //                .Where(t => t.Status == "Done")
        //                .Sum(t => (int?)t.StoryPoints ?? 0)
        //        })
        //        .AsNoTracking()
        //        .ToListAsync();

        //    return Ok(progress);
        //}

        // ✅ Get completed story points per sprint for current user (robust status check)
        [HttpGet("mine/progress")]
        public async Task<ActionResult<IEnumerable<SprintProgressDto>>> GetMySprintProgress()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // If you use an enum, switch to the enum-based filter shown below.
            var doneLabels = new[] { "done", "completed" }; // expand if you have more

            var progress = await _context.Sprints
                .Where(s => s.SprintUsers.Any(su => su.UserId == userId))
                .Select(s => new SprintProgressDto
                {
                    SprintId = s.Id,

                    // STRING STATUS VERSION (case/space tolerant):
                    DoneStoryPoints = s.Tasks
                        .Where(t =>
                            t.Status != null &&
                            doneLabels.Contains(
                                // normalize " Done  " -> "done"
                                (t.Status).Trim().ToLower()
                            )
                        )
                        .Sum(t => (int?)t.StoryPoints ?? 0)

                    /*  ENUM STATUS VERSION (uncomment if t.Status is an enum, e.g., TaskStatus.Done):
                    DoneStoryPoints = s.Tasks
                        .Where(t => t.Status == TaskStatus.Done)
                        .Sum(t => (int?)t.StoryPoints ?? 0)
                    */
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(progress);
        }

    }
}
