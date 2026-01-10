using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDzienik.Data;
using EDzienik.Entities;
using Microsoft.AspNetCore.Authorization;

namespace EDzienik.Controllers
{

    [Authorize]
    public class GradesController : Controller
    {
        private readonly AppDbContext _context;

        public GradesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades
        public async Task<IActionResult> Index()
        {
            var teacher = await GetLoggedTeacherAsync();
            var student = await GetLoggedStudentAsync();

            var query = _context.Grades
                .Include(g => g.Student).ThenInclude(s => s.User)
                .Include(g => g.Subject)
                .Include(g => g.Teacher).ThenInclude(t => t.User)
                .AsQueryable();

            if (teacher != null)
            {
                query = query.Where(g => g.TeacherId == teacher.Id);
            }
            else if (student != null)
            {
                query = query.Where(g => g.StudentId == student.Id);
            }

            return View(await query.OrderByDescending(g => g.CreatedUnix).ToListAsync());
        }

        // GET: Grades/Create
        public async Task<IActionResult> Create()
        {
            var teacher = await GetLoggedTeacherAsync();
            if (teacher == null)
            {
                return Content("Błąd: Tylko nauczyciel może wystawiać oceny (lub brak powiązania konta).");
            }

            var mySubjects = await _context.SubjectAssignments
                .Where(sa => sa.TeacherId == teacher.Id)
                .Include(sa => sa.Subject)
                .Select(sa => sa.Subject)
                .Distinct()
                .ToListAsync();

            var myClassIds = await _context.SubjectAssignments
                .Where(sa => sa.TeacherId == teacher.Id)
                .Select(sa => sa.SchoolClassId)
                .Distinct()
                .ToListAsync();

            var myStudents = await _context.Students
                .Include(s => s.User)
                .Include(s => s.SchoolClass)
                .Where(s => myClassIds.Contains(s.SchoolClassId))
                .OrderBy(s => s.SchoolClass.Name)
                .ThenBy(s => s.User.LastName)
                .Select(s => new
                {
                    Id = s.Id,
                    FullName = $"{s.User.LastName} {s.User.FirstName} (klasa {s.SchoolClass.Name})"
                })
                .ToListAsync();

            ViewData["SubjectId"] = new SelectList(mySubjects, "Id", "Name");
            ViewData["StudentId"] = new SelectList(myStudents, "Id", "FullName");

            return View();
        }

        // POST: Grades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Value,Description,StudentId,SubjectId")] Grade grade)
        {
            var teacher = await GetLoggedTeacherAsync();
            if (teacher == null) return Forbid();

            grade.TeacherId = teacher.Id;
            grade.CreatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var student = await _context.Students.FindAsync(grade.StudentId);
            if (student != null)
            {
                bool hasAssignment = await _context.SubjectAssignments.AnyAsync(sa =>
                   sa.TeacherId == teacher.Id &&
                   sa.SubjectId == grade.SubjectId &&
                   sa.SchoolClassId == student.SchoolClassId);

                if (!hasAssignment)
                {
                    ModelState.AddModelError("", "Nie masz przypisania do klasy tego ucznia z tego przedmiotu.");
                }
            }

            if (grade.Value < 1 || grade.Value > 6)
            {
                ModelState.AddModelError("Value", "Ocena musi być z zakresu 1-6.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(grade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var mySubjects = await _context.SubjectAssignments
                .Where(sa => sa.TeacherId == teacher.Id)
                .Include(sa => sa.Subject)
                .Select(sa => sa.Subject).Distinct().ToListAsync();

            var myClassIds = await _context.SubjectAssignments
                .Where(sa => sa.TeacherId == teacher.Id)
                .Select(sa => sa.SchoolClassId).Distinct().ToListAsync();

            var myStudents = await _context.Students
                .Include(s => s.User).Include(s => s.SchoolClass)
                .Where(s => myClassIds.Contains(s.SchoolClassId))
                .Select(s => new { Id = s.Id, FullName = $"{s.User.LastName} {s.User.FirstName} ({s.SchoolClass.Name})" })
                .ToListAsync();

            ViewData["SubjectId"] = new SelectList(mySubjects, "Id", "Name", grade.SubjectId);
            ViewData["StudentId"] = new SelectList(myStudents, "Id", "FullName", grade.StudentId);

            return View(grade);
        }

        // GET: Grades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                return NotFound();
            }
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "UserId", grade.StudentId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", grade.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId", grade.TeacherId);
            return View(grade);
        }

        // POST: Grades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Value,Description,CreatedUnix,StudentId,SubjectId,TeacherId")] Grade grade)
        {
            if (id != grade.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeExists(grade.Id))
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
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "UserId", grade.StudentId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", grade.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId", grade.TeacherId);
            return View(grade);
        }

        // GET: Grades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .Include(g => g.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade != null)
            {
                _context.Grades.Remove(grade);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.Id == id);
        }


        // funkcje pomocnicze

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
