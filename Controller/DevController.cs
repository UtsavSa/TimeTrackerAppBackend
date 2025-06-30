//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System;
//using TimeTrackerApi.Models;
//using TimeTrackerApi.Services;

//namespace TimeTrackerApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class TimeEntryController : ControllerBase
//    {
//        private readonly TimeEntryService _service;

//        public TimeEntryController(TimeEntryService service)
//        {
//            _service = service;
//        }

//        [HttpPost("punchin")]
//        public ActionResult<TimeEntry> PunchIn([FromQuery] string taskName, [FromQuery] string userId)
//        {
//            var entry = _service.PunchIn(taskName, userId);
//            return Ok(entry);
//        }

//        [HttpPost("punchout/{id}")]
//        public async Task<ActionResult<TimeEntry>> PunchOut(Guid id, [FromQuery] string userId)
//        {
//            var entry = await _service.PunchOut(id, userId);
//            return entry == null
//                ? NotFound("Entry not found or already punched out.")
//                : Ok(entry);
//        }

//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<TimeEntry>>> GetAll([FromQuery] string userId)
//        {
//            var entries = await _service.GetAll(userId);
//            return Ok(entries);
//        }


//        [HttpDelete("delete/{entryId}")]
//        public async Task<IActionResult> DeleteEntry(Guid entryId, [FromQuery] string userId)
//        {
//            var result = await _service.DeleteEntry(entryId, userId);
//            return result ? Ok(true) : NotFound();
//        }

//        [HttpDelete("delete-all")]
//        public async Task<IActionResult> DeleteAll([FromQuery] string userId)
//        {
//            var deletedCount = await _service.DeleteAll(userId);
//            return Ok(deletedCount);
//        }

//    }

//}




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
