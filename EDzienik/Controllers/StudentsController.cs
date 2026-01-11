using EDzienik.Data;
using EDzienik.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EDzienik.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public StudentsController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            var students = _context.Students
                .Include(s => s.SchoolClass)
                .Include(s => s.User);
            return View(await students.ToListAsync());
        }

        // GET: Students/Assign
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign()
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Student");

            var existingStudentUserIds = await _context.Students
                .Select(s => s.UserId)
                .ToListAsync();

            var availableUsers = usersInRole
                .Where(u => !existingStudentUserIds.Contains(u.Id))
                .OrderBy(u => u.LastName)
                .ToList();

            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name");
            return View(availableUsers);
        }

        // POST: Students/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign(string userId, int schoolClassId)
        {
            if (string.IsNullOrEmpty(userId) || schoolClassId == 0)
            {
                return RedirectToAction(nameof(Assign));
            }

            var exists = await _context.Students.AnyAsync(s => s.UserId == userId);
            if (exists)
            {
                return RedirectToAction(nameof(Assign));
            }

            var student = new Student
            {
                UserId = userId,
                SchoolClassId = schoolClassId
            };

            _context.Add(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Assign));
        }

        // GET: Students/Manage/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.SchoolClass)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", student.SchoolClassId);
            return View(student);
        }

        // POST: Students/Manage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage(int id, [Bind("Id,SchoolClassId")] Student model)
        {
            if (id != model.Id) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.SchoolClassId = model.SchoolClassId;

            _context.Update(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}