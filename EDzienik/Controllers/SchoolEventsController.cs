using EDzienik.Data;
using EDzienik.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDzienik.Controllers
{
    [Authorize]
    public class SchoolEventsController : Controller
    {
        private readonly AppDbContext _context;

        public SchoolEventsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SchoolEvents
        public async Task<IActionResult> Index(DateTime? from, DateTime? to)
        {
            var teacher = await GetLoggedTeacherAsync();
            var student = await GetLoggedStudentAsync();

            var query = _context.SchoolEvents
                .Include(s => s.SchoolClass)
                .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
                .AsQueryable();

            if (teacher != null)
            {
                query = query.Where(e => e.TeacherId == teacher.Id);
            }
            else if (student != null)
            {
                query = query.Where(e => e.SchoolClassId == student.SchoolClassId);
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.Date >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(e => e.Date <= to.Value);
            }

            ViewData["FromDate"] = from?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = to?.ToString("yyyy-MM-dd");

            return View(await query.OrderByDescending(e => e.Date).ToListAsync());
        }

        // GET: SchoolEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolEvent = await _context.SchoolEvents
                .Include(s => s.SchoolClass)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schoolEvent == null)
            {
                return NotFound();
            }

            return View(schoolEvent);
        }

        // GET: SchoolEvents/Create
        public async Task<IActionResult> Create()
        {
            var teacher = await GetLoggedTeacherAsync();

            if (teacher == null && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (teacher != null)
            {
                var myClassIds = await _context.SubjectAssignments
                    .Where(sa => sa.TeacherId == teacher.Id)
                    .Select(sa => sa.SchoolClassId)
                    .Distinct()
                    .ToListAsync();

                var myClasses = await _context.SchoolClasses
                    .Where(c => myClassIds.Contains(c.Id))
                    .ToListAsync();

                ViewData["SchoolClassId"] = new SelectList(myClasses, "Id", "Name");
            }
            else
            {
                ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name");

                var teachers = _context.Teachers
                    .Include(t => t.User)
                    .Select(t => new {
                        Id = t.Id,
                        FullName = t.User.FirstName + " " + t.User.LastName + " (" + t.User.Email + ")"
                    })
                    .ToList();
                ViewData["TeacherId"] = new SelectList(teachers, "Id", "FullName");
            }

            return View();
        }

        // POST: SchoolEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Date,Type,SchoolClassId")] SchoolEvent schoolEvent)
        {
            var teacher = await GetLoggedTeacherAsync();

            if (teacher != null)
            {
                var myClassIds = await _context.SubjectAssignments
                    .Where(sa => sa.TeacherId == teacher.Id)
                    .Select(sa => sa.SchoolClassId)
                    .Distinct()
                    .ToListAsync();

                if (!myClassIds.Contains(schoolEvent.SchoolClassId))
                    return Forbid();

                schoolEvent.TeacherId = teacher.Id;
            }

            if (teacher == null && !User.IsInRole("Admin")) return Forbid();

            if (ModelState.IsValid)
            {
                _context.Add(schoolEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", schoolEvent.SchoolClassId);
            return View(schoolEvent);
        }

        // GET: SchoolEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolEvent = await _context.SchoolEvents.FindAsync(id);
            if (schoolEvent == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var teacher = await GetLoggedTeacherAsync();
                if (teacher == null || schoolEvent.TeacherId != teacher.Id)
                {
                    return Forbid();
                }
            }


            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", schoolEvent.SchoolClassId);

            var teachers = _context.Teachers
                .Include(t => t.User)
                .Select(t => new {
                    Id = t.Id,
                    FullName = t.User.FirstName + " " + t.User.LastName + " (" + t.User.Email + ")"
                })
                .ToList();
            ViewData["TeacherId"] = new SelectList(teachers, "Id", "FullName", schoolEvent.TeacherId);

            return View(schoolEvent);
        }

        // POST: SchoolEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Date,Type,SchoolClassId,TeacherId")] SchoolEvent schoolEvent)
        {
            if (id != schoolEvent.Id)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var teacher = await GetLoggedTeacherAsync();
                var originalEvent = await _context.SchoolEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

                if (originalEvent == null || teacher == null || originalEvent.TeacherId != teacher.Id)
                {
                    return Forbid();
                }
                schoolEvent.TeacherId = originalEvent.TeacherId;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schoolEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SchoolEventExists(schoolEvent.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", schoolEvent.SchoolClassId);

            var teachers = _context.Teachers
                .Include(t => t.User)
                .Select(t => new {
                    Id = t.Id,
                    FullName = t.User.FirstName + " " + t.User.LastName + " (" + t.User.Email + ")"
                })
                .ToList();
            ViewData["TeacherId"] = new SelectList(teachers, "Id", "FullName", schoolEvent.TeacherId);

            return View(schoolEvent);
        }

        // GET: SchoolEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolEvent = await _context.SchoolEvents
                .Include(s => s.SchoolClass)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schoolEvent == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var teacher = await GetLoggedTeacherAsync();
                if (teacher == null || schoolEvent.TeacherId != teacher.Id)
                {
                    return Forbid();
                }
            }

            return View(schoolEvent);
        }

        // POST: SchoolEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.SchoolEvents.FindAsync(id);
            if (ev == null) return RedirectToAction(nameof(Index));

            if (!User.IsInRole("Admin"))
            {
                var teacher = await GetLoggedTeacherAsync();
                if (teacher == null || ev.TeacherId != teacher.Id)
                    return Forbid();
            }

            _context.SchoolEvents.Remove(ev);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SchoolEventExists(int id)
        {
            return _context.SchoolEvents.Any(e => e.Id == id);
        }

        //funckje pomocnicze

        private async Task<Teacher?> GetLoggedTeacherAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.Email == userEmail);
        }

        private async Task<Student?> GetLoggedStudentAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User.Email == userEmail);
        }
    }
}
