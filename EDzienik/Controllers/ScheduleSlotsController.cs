using EDzienik.Data;
using EDzienik.Entities;
using EDzienik.Models;
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
    public class ScheduleSlotsController : Controller
    {
        private readonly AppDbContext _context;

        public ScheduleSlotsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ScheduleSlots
        // GET: ScheduleSlots
        public async Task<IActionResult> Index()
        {
            var teacher = await GetLoggedTeacherAsync();
            var student = await GetLoggedStudentAsync();

            var query = _context.ScheduleSlots
                .Include(s => s.SchoolClass)
                .Include(s => s.Subject)
                .Include(s => s.Teacher).ThenInclude(t => t.User)
                .AsQueryable();

            if (teacher != null)
            {
                query = query.Where(s => s.TeacherId == teacher.Id);
            }
            else if (student != null)
            {
                query = query.Where(s => s.SchoolClassId == student.SchoolClassId);
            }

            return View(await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartUnix)
                .ToListAsync());
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
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name");
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name");

            var teachers = _context.Teachers
                .Include(t => t.User)
                .Select(t => new {
                    Id = t.Id,
                    FullName = t.User.FirstName + " " + t.User.LastName + " (" + t.User.Email + ")"
                })
                .ToList();

            ViewData["TeacherId"] = new SelectList(teachers, "Id", "FullName");
            return View();
        }

        // POST: ScheduleSlots/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ScheduleSlotViewModel model)
        {
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("EndTime", "Zajęcia muszą kończyć się później niż zaczynają.");
            }

            var scheduleSlot = new ScheduleSlot
            {
                DayOfWeek = model.DayOfWeek,
                Room = model.Room,
                SchoolClassId = model.SchoolClassId,
                SubjectId = model.SubjectId,
                StartUnix = ((DateTimeOffset)model.StartTime).ToUnixTimeSeconds(),
                EndUnix = ((DateTimeOffset)model.EndTime).ToUnixTimeSeconds(),

                TeacherId = model.TeacherId
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
                        if (slot.TeacherId == scheduleSlot.TeacherId)
                            ModelState.AddModelError("TeacherId", "Ten nauczyciel ma już zajęcia w tym czasie!");

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

            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", model.SchoolClassId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", model.SubjectId);

            var teachers = _context.Teachers
               .Include(t => t.User)
               .Select(t => new {
                   Id = t.Id,
                   FullName = t.User.FirstName + " " + t.User.LastName + " (" + t.User.Email + ")"
               })
               .ToList();
            ViewData["TeacherId"] = new SelectList(teachers, "Id", "FullName", model.TeacherId);

            return View(model);
        }

        // GET: ScheduleSlots/Edit/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        // funkcje pomocnicze

        private async Task<Teacher?> GetLoggedTeacherAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.User.Email == userEmail);
        }

        private async Task<Student?> GetLoggedStudentAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.User.Email == userEmail);
        }
    }
}