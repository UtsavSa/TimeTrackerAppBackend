



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrackerApi.Models;
using TimeTrackerApi.Models.Dtos;
using TimeTrackerApi.Services;

namespace TimeTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimeEntryController : ControllerBase
    {
        private readonly TimeEntryService _service;

        public TimeEntryController(TimeEntryService service)
        {
            _service = service;
        }

        // 🔐 Helper to get current user ID from token
        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // ✅ Punch In
        [HttpPost("punchin")]
        public async Task<ActionResult<TimeEntry>> PunchIn([FromBody] PunchInDto dto)
        {
            try
            {
                var entry = await _service.PunchInAsync(dto.TaskName); // ✅ only task name
                return Ok(entry);
            }
            catch (Exception ex)
            {
                return BadRequest($"Punch-in failed: {ex.Message}");
            }
        }

        // ✅ Punch Out
        [HttpPut("punchout/{id}")]
        public async Task<ActionResult<TimeEntry>> PunchOut(Guid id)
        {
            var entry = await _service.PunchOutAsync(id);
            return entry == null
                ? NotFound("Entry not found or already punched out.")
                : Ok(entry);
        }

        // ✅ Get All
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeEntry>>> GetAll()
        {
            var entries = await _service.GetAllAsync();
            return Ok(entries);
        }

        // ✅ Delete Entry
        [HttpDelete("delete/{entryId}")]
        public async Task<IActionResult> DeleteEntry(Guid entryId)
        {
            var success = await _service.DeleteEntryAsync(entryId);
            return success ? NoContent() : NotFound("Entry not found.");
        }

        // ✅ Delete All
        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAll()
        {
            var deletedCount = await _service.DeleteAllAsync();
            return Ok(new { deleted = deletedCount });
        }
    }
}
