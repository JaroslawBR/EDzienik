using EDzienik.Data;
using EDzienik.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDzienik.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var students = _context.Students.Include(s => s.SchoolClass).Include(s => s.User);
            return View(await students.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name");

            var availableUsers = _context.Users
                .Where(u => u.Student == null && u.Teacher == null)
                .Select(u => new {
                    Id = u.Id,
                    Description = $"{u.LastName} {u.FirstName} ({u.Email})"
                })
                .ToList();

            ViewData["UserId"] = new SelectList(availableUsers, "Id", "Description");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,SchoolClassId")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", student.SchoolClassId);

            var availableUsers = _context.Users
                .Where(u => u.Student == null && u.Teacher == null)
                .Select(u => new { Id = u.Id, Description = $"{u.LastName} {u.FirstName} ({u.Email})" })
                .ToList();
            ViewData["UserId"] = new SelectList(availableUsers, "Id", "Description", student.UserId);

            return View(student);
        }
    }
}
