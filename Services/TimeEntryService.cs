// Services/TimeEntryService.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TimeTrackerApi.Data;
using TimeTrackerApi.Models;
using System.IdentityModel.Tokens.Jwt;

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

        private string? GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            // Try standard NameIdentifier first (if you've mapped it), otherwise fallback to sub
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user?.FindFirst("sub")?.Value; // final fallback
        }

        // Treat all DateTime as UTC when returning JSON
        private static DateTime EnsureUtc(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc) return dt;
            if (dt.Kind == DateTimeKind.Local) return dt.ToUniversalTime();
            // Unspecified -> assume UTC (fits your current prod data)
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }

        // ✅ Punch In (store UTC)
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
                PunchInTime = DateTime.UtcNow
            };

            _context.TimeEntries.Add(entry);
            await _context.SaveChangesAsync();

            // normalize on return so JSON has 'Z'
            entry.PunchInTime = EnsureUtc(entry.PunchInTime);
            return entry;
        }

        // ✅ Punch Out (store UTC)
        public async Task<TimeEntry?> PunchOutAsync(Guid entryId)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is missing from token");

            var entry = await _context.TimeEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId && e.PunchOutTime == null);

            if (entry is null) return null;

            entry.PunchOutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            entry.PunchInTime = EnsureUtc(entry.PunchInTime);
            if (entry.PunchOutTime.HasValue) entry.PunchOutTime = EnsureUtc(entry.PunchOutTime.Value);
            return entry;
        }

        // ✅ Get all (normalize to UTC **after** query; do NOT call custom methods inside LINQ)
        public async Task<List<TimeEntry>> GetAllAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is missing from token");

            var list = await _context.TimeEntries
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.PunchInTime)
                .ToListAsync();

            foreach (var e in list)
            {
                e.PunchInTime = EnsureUtc(e.PunchInTime);
                if (e.PunchOutTime.HasValue) e.PunchOutTime = EnsureUtc(e.PunchOutTime.Value);
            }

            return list;
        }

        public async Task<bool> DeleteEntryAsync(Guid entryId)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is missing from token");

            var entry = await _context.TimeEntries.FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId);
            if (entry is null) return false;

            _context.TimeEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }

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
