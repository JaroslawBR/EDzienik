using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDzienik.Data;
using EDzienik.Entities;

namespace EDzienik.Controllers
{
    public class SchoolEventsController : Controller
    {
        private readonly AppDbContext _context;

        public SchoolEventsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SchoolEvents
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.SchoolEvents.Include(s => s.SchoolClass).Include(s => s.Teacher);
            return View(await appDbContext.ToListAsync());
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
        public IActionResult Create()
        {
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name");
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId");
            return View();
        }

        // POST: SchoolEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Date,Type,SchoolClassId,TeacherId")] SchoolEvent schoolEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schoolEvent);
                await _context.SaveChangesAsync();
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

            return View(schoolEvent);
        }

        // POST: SchoolEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schoolEvent = await _context.SchoolEvents.FindAsync(id);
            if (schoolEvent != null)
            {
                _context.SchoolEvents.Remove(schoolEvent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SchoolEventExists(int id)
        {
            return _context.SchoolEvents.Any(e => e.Id == id);
        }
    }
}
