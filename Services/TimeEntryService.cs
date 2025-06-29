using TimeTrackerApi.Data;
using TimeTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace TimeTrackerApi.Services
{
    public class TimeEntryService
    {
        private readonly TimeTrackerContext _context;

        public TimeEntryService(TimeTrackerContext context)
        {
            _context = context;
        }

        // ✅ PUNCH IN - Sync OK since no async is used here
        public TimeEntry PunchIn(string taskName, string userId)
        {
            var entry = new TimeEntry
            {
                Id = Guid.NewGuid(),
                TaskName = taskName,
                UserId = userId,
                PunchInTime = DateTime.Now
            };

            _context.TimeEntries.Add(entry);
            _context.SaveChanges();

            return entry;
        }

        // ✅ PUNCH OUT - Async version
        public async Task<TimeEntry?> PunchOut(Guid entryId, string userId)
        {
            // Get the existing entry
            var entry = await _context.TimeEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId && e.PunchOutTime == null);

            if (entry is null)
                return null;

            // Set punch out time and save
            entry.PunchOutTime = DateTime.Now;

            await _context.SaveChangesAsync();

            return entry;
        }

        // ✅ GET ALL ENTRIES FOR A USER
        public async Task<List<TimeEntry>> GetAll(string userId)
        {
            return await _context.TimeEntries
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.PunchInTime)
                .ToListAsync();
        }

        // delete a single entry

        public async Task<bool> DeleteEntry(Guid entryId, string userId)
        {

            var entry = await _context.TimeEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId);

            if (entry is null)
                return false;

            _context.TimeEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;



        }

        // delete everything
        public async Task<int> DeleteAll(string userId)
        {
            var entries = _context.TimeEntries.Where(e => e.UserId == userId);
            _context.TimeEntries.RemoveRange(entries);
            return await _context.SaveChangesAsync(); // Returns number of rows affected
        }

    }
}
