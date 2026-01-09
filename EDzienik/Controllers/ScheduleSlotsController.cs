using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDzienik.Data;
using EDzienik.Entities;
using EDzienik.Models;

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
            return View();
        }

        // POST: ScheduleSlots/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleSlotViewModel model) // Przyjmujemy ViewModel
        {
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("EndTime", "Zajęcia muszą kończyć się później niż zaczynają.");
            }

            // Konwersja ViewModel -> Entity
            var scheduleSlot = new ScheduleSlot
            {
                DayOfWeek = model.DayOfWeek,
                Room = model.Room,
                SchoolClassId = model.SchoolClassId,
                SubjectId = model.SubjectId,

                // Przeliczenie DateTime na Unix Timestamp
                StartUnix = ((DateTimeOffset)model.StartTime).ToUnixTimeSeconds(),
                EndUnix = ((DateTimeOffset)model.EndTime).ToUnixTimeSeconds(),

                // HARDCODED TEACHER ID (Tryb testowy taki sam jak w Grades)
                TeacherId = 1
            };

            if (ModelState.IsValid)
            {
                // Walidacja konfliktów (ta, którą już miałeś, ale dostosowana do nowego obiektu)
                var conflicts = await _context.ScheduleSlots
                    .Where(s => s.DayOfWeek == scheduleSlot.DayOfWeek)
                    .ToListAsync();

                foreach (var slot in conflicts)
                {
                    bool overlap = (scheduleSlot.StartUnix < slot.EndUnix) && (scheduleSlot.EndUnix > slot.StartUnix);
                    if (overlap)
                    {
                        // Tutaj sprawdzamy TeacherId=1 "na sztywno" bo taki przypisujemy
                        if (slot.TeacherId == 1)
                            ModelState.AddModelError("", "Masz już zajęcia w tym czasie!");

                        if (slot.SchoolClassId == scheduleSlot.SchoolClassId)
                            ModelState.AddModelError("SchoolClassId", "Ta klasa ma już zajęcia w tym czasie.");

                        if (!string.IsNullOrEmpty(scheduleSlot.Room) && scheduleSlot.Room == slot.Room)
                            ModelState.AddModelError("Room", $"Sala {slot.Room} jest zajęta.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(scheduleSlot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Jeśli błąd, przywracamy listy rozwijane
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", model.SchoolClassId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", model.SubjectId);
            return View(model);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DayOfWeek,StartUnix,EndUnix,Room,SchoolClassId,SubjectId,TeacherId")] ScheduleSlot scheduleSlot)
        {
            if (id != scheduleSlot.Id) return NotFound();

            if (scheduleSlot.EndUnix <= scheduleSlot.StartUnix)
                ModelState.AddModelError("EndUnix", "Koniec musi być po początku.");

            if (ModelState.IsValid)
            {
                var conflicts = await _context.ScheduleSlots
                    .Where(s => s.DayOfWeek == scheduleSlot.DayOfWeek && s.Id != id)
                    .ToListAsync();

                foreach (var slot in conflicts)
                {
                    bool overlap = (scheduleSlot.StartUnix < slot.EndUnix) && (scheduleSlot.EndUnix > slot.StartUnix);
                    if (overlap)
                    {
                        if (slot.TeacherId == scheduleSlot.TeacherId)
                            ModelState.AddModelError("TeacherId", "Ten nauczyciel jest zajęty.");
                        if (slot.SchoolClassId == scheduleSlot.SchoolClassId)
                            ModelState.AddModelError("SchoolClassId", "Ta klasa jest zajęta.");
                        if (!string.IsNullOrEmpty(scheduleSlot.Room) && scheduleSlot.Room == slot.Room)
                            ModelState.AddModelError("Room", "Sala jest zajęta.");
                    }
                }
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
                    if (!ScheduleSlotExists(scheduleSlot.Id)) return NotFound();
                    else throw;
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