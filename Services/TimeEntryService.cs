//using TimeTrackerApi.Data;
//using TimeTrackerApi.Models;
//using Microsoft.EntityFrameworkCore;

//namespace TimeTrackerApi.Services
//{
//    public class TimeEntryService
//    {
//        private readonly TimeTrackerContext _context;

//        public TimeEntryService(TimeTrackerContext context)
//        {
//            _context = context;
//        }

//        // ✅ Punch In
//        public TimeEntry PunchIn(string taskName, string userId)
//        {
//            var entry = new TimeEntry
//            {
//                Id = Guid.NewGuid(), // make sure TimeEntry.Id is Guid in your model
//                TaskName = taskName,
//                UserId = userId,
//                PunchInTime = DateTime.Now
//            };

//            _context.TimeEntries.Add(entry);
//            _context.SaveChanges();
//            return entry;
//        }

//        // ✅ Punch Out
//        public async Task<TimeEntry?> PunchOut(Guid entryId, string userId)
//        {
//            var entry = await _context.TimeEntries
//                .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId && e.PunchOutTime == null);

//            if (entry is null)
//                return null;

//            entry.PunchOutTime = DateTime.Now;
//            await _context.SaveChangesAsync();
//            return entry;
//        }

//        // ✅ Get All Entries for a User
//        public async Task<List<TimeEntry>> GetAll(string userId)
//        {
//            return await _context.TimeEntries
//                .AsNoTracking()
//                .Where(e => e.UserId == userId)
//                .OrderByDescending(e => e.PunchInTime)
//                .ToListAsync();
//        }

//        // ✅ Delete a Single Entry
//        public async Task<bool> DeleteEntry(Guid entryId, string userId)
//        {
//            var entry = await _context.TimeEntries
//                .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId);

//            if (entry is null)
//                return false;

//            _context.TimeEntries.Remove(entry);
//            await _context.SaveChangesAsync();
//            return true;
//        }

//        // ✅ Delete All Entries for a User
//        public async Task<int> DeleteAll(string userId)
//        {
//            var entries = _context.TimeEntries
//                .Where(e => e.UserId == userId);

//            _context.TimeEntries.RemoveRange(entries);
//            return await _context.SaveChangesAsync(); // number of rows deleted
//        }
//    }
//}


using TimeTrackerApi.Data;
using TimeTrackerApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TimeTrackerApi.Services
{
    public class TimeEntryService
    {
        private readonly TimeTrackerContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TimeEntryService(TimeTrackerContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // 🔐 Helper: Extract user ID from JWT
        private string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // ✅ Punch In
        public async Task<TimeEntry?> PunchInAsync(string taskName)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is missing from token");

            var entry = new TimeEntry
            {
                Id = Guid.NewGuid(),
                TaskName = taskName,
                UserId = userId,
                PunchInTime = DateTime.Now
            };

            _context.TimeEntries.Add(entry);

            try
            {
                await _context.SaveChangesAsync();
                return entry;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("EF Core Save Error (PunchIn): " + ex.InnerException?.Message);
                throw;
            }
        }

        // ✅ Punch Out
        public async Task<TimeEntry?> PunchOutAsync(Guid entryId)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is missing from token");

            var entry = await _context.TimeEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId && e.PunchOutTime == null);

            if (entry is null)
                return null;

            entry.PunchOutTime = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                return entry;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("EF Core Save Error (PunchOut): " + ex.InnerException?.Message);
                throw;
            }
        }

        // ✅ Get All Entries for Current User
        public async Task<List<TimeEntry>> GetAllAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is missing from token");

            return await _context.TimeEntries
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.PunchInTime)
                .ToListAsync();
        }

        // ✅ Delete a Single Entry
        public async Task<bool> DeleteEntryAsync(Guid entryId)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is missing from token");

            var entry = await _context.TimeEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId);

            if (entry is null)
                return false;

            _context.TimeEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Delete All Entries for Current User
        public async Task<int> DeleteAllAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is missing from token");

            var entries = _context.TimeEntries.Where(e => e.UserId == userId);

            _context.TimeEntries.RemoveRange(entries);
            return await _context.SaveChangesAsync();
        }
    }
}
