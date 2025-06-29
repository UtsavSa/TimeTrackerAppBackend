using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services;

namespace TimeTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeEntryController : ControllerBase
    {
        private readonly TimeEntryService _service;

        public TimeEntryController(TimeEntryService service)
        {
            _service = service;
        }

        [HttpPost("punchin")]
        public ActionResult<TimeEntry> PunchIn([FromQuery] string taskName, [FromQuery] string userId)
        {
            var entry = _service.PunchIn(taskName, userId);
            return Ok(entry);
        }

        [HttpPost("punchout/{id}")]
        public async Task<ActionResult<TimeEntry>> PunchOut(Guid id, [FromQuery] string userId)
        {
            var entry = await _service.PunchOut(id, userId);
            return entry == null
                ? NotFound("Entry not found or already punched out.")
                : Ok(entry);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeEntry>>> GetAll([FromQuery] string userId)
        {
            var entries = await _service.GetAll(userId);
            return Ok(entries);
        }


        [HttpDelete("delete/{entryId}")]
        public async Task<IActionResult> DeleteEntry(Guid entryId, [FromQuery] string userId)
        {
            var result = await _service.DeleteEntry(entryId, userId);
            return result ? Ok(true) : NotFound();
        }

        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAll([FromQuery] string userId)
        {
            var deletedCount = await _service.DeleteAll(userId);
            return Ok(deletedCount);
        }

    }

}
