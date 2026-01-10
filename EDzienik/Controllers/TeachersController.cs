using EDzienik.Data;
using EDzienik.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDzienik.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeachersController : Controller
    {
        private readonly AppDbContext _context;

        public TeachersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var teachers = _context.Teachers.Include(t => t.User);
            return View(await teachers.ToListAsync());
        }

        public IActionResult Create()
        {
            var availableUsers = _context.Users
                .Where(u => u.Teacher == null && u.Student == null)
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
        public async Task<IActionResult> Create([Bind("Id,UserId")] Teacher teacher)
        {
            // Sprawdź czy User istnieje
            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // W razie błędu ponownie załaduj listę
            var availableUsers = _context.Users
               .Where(u => u.Teacher == null && u.Student == null) 
               .Select(u => new {
                   Id = u.Id,
                   Description = $"{u.LastName} {u.FirstName} ({u.Email})"
               })
               .ToList();
            ViewData["UserId"] = new SelectList(availableUsers, "Id", "Description", teacher.UserId);
            return View(teacher);
        }
    }
}