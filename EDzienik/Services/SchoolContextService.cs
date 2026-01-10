using EDzienik.Data;
using EDzienik.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EDzienik.Services
{
    public class SchoolContextService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SchoolContextService(AppDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<Teacher?> GetCurrentTeacherAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return null;

            return await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<Student?> GetCurrentStudentAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return null;

            return await _context.Students
                .Include(s => s.SchoolClass)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}