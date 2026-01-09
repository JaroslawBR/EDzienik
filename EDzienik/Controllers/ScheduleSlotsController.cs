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
    public class ScheduleSlotsController : Controller
    {
        private readonly AppDbContext _context;

        public ScheduleSlotsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ScheduleSlots
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ScheduleSlots.Include(s => s.SchoolClass).Include(s => s.Subject).Include(s => s.Teacher);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ScheduleSlots/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleSlot = await _context.ScheduleSlots
                .Include(s => s.SchoolClass)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (scheduleSlot == null)
            {
                return NotFound();
            }

            return View(scheduleSlot);
        }

        // GET: ScheduleSlots/Create
        public IActionResult Create()
        {
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name");
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name");
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId");
            return View();
        }

        // POST: ScheduleSlots/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DayOfWeek,StartUnix,EndUnix,Room,SchoolClassId,SubjectId,TeacherId")] ScheduleSlot scheduleSlot)
        {
            if (ModelState.IsValid)
            {
                _context.Add(scheduleSlot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", scheduleSlot.SchoolClassId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", scheduleSlot.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId", scheduleSlot.TeacherId);
            return View(scheduleSlot);
        }

        // GET: ScheduleSlots/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleSlot = await _context.ScheduleSlots.FindAsync(id);
            if (scheduleSlot == null)
            {
                return NotFound();
            }
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", scheduleSlot.SchoolClassId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", scheduleSlot.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId", scheduleSlot.TeacherId);
            return View(scheduleSlot);
        }

        // POST: ScheduleSlots/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DayOfWeek,StartUnix,EndUnix,Room,SchoolClassId,SubjectId,TeacherId")] ScheduleSlot scheduleSlot)
        {
            if (id != scheduleSlot.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(scheduleSlot);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleSlotExists(scheduleSlot.Id))
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
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", scheduleSlot.SchoolClassId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", scheduleSlot.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId", scheduleSlot.TeacherId);
            return View(scheduleSlot);
        }

        // GET: ScheduleSlots/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleSlot = await _context.ScheduleSlots
                .Include(s => s.SchoolClass)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (scheduleSlot == null)
            {
                return NotFound();
            }

            return View(scheduleSlot);
        }

        // POST: ScheduleSlots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var scheduleSlot = await _context.ScheduleSlots.FindAsync(id);
            if (scheduleSlot != null)
            {
                _context.ScheduleSlots.Remove(scheduleSlot);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleSlotExists(int id)
        {
            return _context.ScheduleSlots.Any(e => e.Id == id);
        }
    }
}
