using EDzienik.Data;
using EDzienik.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDzienik.Controllers
{
    public class TeachersController : Controller
    {
        private readonly AppDbContext _context;

        public TeachersController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var teachers = _context.Teachers.Include(t => t.User);
            return View(await teachers.ToListAsync());
        }

        [Authorize(Roles = "Admin")] 
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
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Create([Bind("Id,UserId")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

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