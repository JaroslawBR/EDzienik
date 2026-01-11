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
        public async Task<IActionResult> Index(int? schoolClassId, int? subjectId, int? studentId, int? teacherId)
        {
            var teacher = await GetLoggedTeacherAsync();
            var student = await GetLoggedStudentAsync();

            var query = _context.Grades
                .Include(g => g.Student).ThenInclude(s => s.User)
                .Include(g => g.Student).ThenInclude(s => s.SchoolClass)
                .Include(g => g.Subject)
                .Include(g => g.Teacher).ThenInclude(t => t.User)
                .AsQueryable();


            if (teacher != null)
            {
                query = query.Where(g => g.TeacherId == teacher.Id);

                var myClassIds = await _context.SubjectAssignments
                    .Where(sa => sa.TeacherId == teacher.Id)
                    .Select(sa => sa.SchoolClassId).Distinct().ToListAsync();

                var myClasses = await _context.SchoolClasses
                    .Where(c => myClassIds.Contains(c.Id))
                    .OrderBy(c => c.Name).ToListAsync();

                var myStudentsQuery = _context.Students
                    .Include(s => s.User).Include(s => s.SchoolClass)
                    .Where(s => myClassIds.Contains(s.SchoolClassId));

                if (schoolClassId.HasValue)
                {
                    myStudentsQuery = myStudentsQuery.Where(s => s.SchoolClassId == schoolClassId.Value);
                }

                var myStudents = await myStudentsQuery
                    .OrderBy(s => s.User.LastName)
                    .Select(s => new { Id = s.Id, FullName = $"{s.User.LastName} {s.User.FirstName} ({s.SchoolClass.Name})" })
                    .ToListAsync();

                var mySubjectIds = await _context.SubjectAssignments
                    .Where(sa => sa.TeacherId == teacher.Id)
                    .Select(sa => sa.SubjectId).Distinct().ToListAsync();

                var mySubjects = await _context.Subjects
                    .Where(s => mySubjectIds.Contains(s.Id))
                    .OrderBy(s => s.Name).ToListAsync();

                ViewData["SchoolClassId"] = new SelectList(myClasses, "Id", "Name", schoolClassId);
                ViewData["StudentId"] = new SelectList(myStudents, "Id", "FullName", studentId);
                ViewData["SubjectId"] = new SelectList(mySubjects, "Id", "Name", subjectId);
            }
            else if (student != null)
            {
                query = query.Where(g => g.StudentId == student.Id);

                var teachers = await _context.Teachers.Include(t => t.User)
                    .OrderBy(t => t.User.LastName)
                    .Select(t => new { Id = t.Id, FullName = $"{t.User.LastName} {t.User.FirstName}" })
                    .ToListAsync();

                var subjects = await _context.Subjects.OrderBy(s => s.Name).ToListAsync();

                ViewData["TeacherId"] = new SelectList(teachers, "Id", "FullName", teacherId);
                ViewData["SubjectId"] = new SelectList(subjects, "Id", "Name", subjectId);
            }
            else if (User.IsInRole("Admin"))
            {
                var classes = await _context.SchoolClasses.OrderBy(c => c.Name).ToListAsync();
                var subjects = await _context.Subjects.OrderBy(s => s.Name).ToListAsync();
                var teachers = await _context.Teachers.Include(t => t.User)
                    .OrderBy(t => t.User.LastName)
                    .Select(t => new { Id = t.Id, FullName = $"{t.User.LastName} {t.User.FirstName}" })
                    .ToListAsync();

                var studentsQuery = _context.Students.Include(s => s.User).Include(s => s.SchoolClass).AsQueryable();
                if (schoolClassId.HasValue)
                {
                    studentsQuery = studentsQuery.Where(s => s.SchoolClassId == schoolClassId.Value);
                }
                var students = await studentsQuery
                    .OrderBy(s => s.User.LastName)
                    .Select(s => new { Id = s.Id, FullName = $"{s.User.LastName} {s.User.FirstName} ({s.SchoolClass.Name})" })
                    .ToListAsync();

                ViewData["SchoolClassId"] = new SelectList(classes, "Id", "Name", schoolClassId);
                ViewData["StudentId"] = new SelectList(students, "Id", "FullName", studentId);
                ViewData["SubjectId"] = new SelectList(subjects, "Id", "Name", subjectId);
                ViewData["TeacherId"] = new SelectList(teachers, "Id", "FullName", teacherId);
            }


            if (schoolClassId.HasValue)
            {
                query = query.Where(g => g.Student.SchoolClassId == schoolClassId.Value);
            }
            if (studentId.HasValue)
            {
                query = query.Where(g => g.StudentId == studentId.Value);
            }
            if (subjectId.HasValue)
            {
                query = query.Where(g => g.SubjectId == subjectId.Value);
            }
            if (teacherId.HasValue)
            {
                query = query.Where(g => g.TeacherId == teacherId.Value);
            }

            return View(await query.OrderByDescending(g => g.CreatedUnix).ToListAsync());
        }

        // GET: Grades/Create
        public async Task<IActionResult> Create()
        {
            var teacher = await GetLoggedTeacherAsync();

            if (teacher == null && !User.IsInRole("Admin"))
            {
                return Content("Błąd: Tylko nauczyciel może wystawiać oceny (lub brak powiązania konta).");
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name");

                var allStudents = await _context.Students
                    .Include(s => s.User).Include(s => s.SchoolClass)
                    .Select(s => new { Id = s.Id, FullName = $"{s.User.LastName} {s.User.FirstName} ({s.SchoolClass.Name})" })
                    .ToListAsync();
                ViewData["StudentId"] = new SelectList(allStudents, "Id", "FullName");

                var allTeachers = await _context.Teachers.Include(t => t.User)
                    .Select(t => new { Id = t.Id, FullName = $"{t.User.LastName} {t.User.FirstName}" })
                    .ToListAsync();
                ViewData["TeacherId"] = new SelectList(allTeachers, "Id", "FullName");
            }
            else
            {
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
            }

            return View();
        }

        // POST: Grades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Value,Description,StudentId,SubjectId,TeacherId")] Grade grade)
        {
            var teacher = await GetLoggedTeacherAsync();

            if (teacher != null)
            {
                grade.TeacherId = teacher.Id;
            }
            else if (User.IsInRole("Admin"))
            {
                if (grade.TeacherId == 0) ModelState.AddModelError("TeacherId", "Admin musi wskazać nauczyciela wystawiającego ocenę.");
            }
            else
            {
                return Forbid();
            }

            grade.CreatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (!User.IsInRole("Admin") && teacher != null)
            {
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
            }

            if (grade.Value < 1 || grade.Value > 6)
            {
                ModelState.AddModelError("Value", "Ocena musi być z zakresu 1-6.");
            }

            ModelState.Remove("Student");
            ModelState.Remove("Subject");
            ModelState.Remove("Teacher");


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

            var grade = await _context.Grades
                .Include(g => g.Student).ThenInclude(s => s.User)
                .Include(g => g.Student).ThenInclude(s => s.SchoolClass)
                .Include(g => g.Subject)
                .Include(g => g.Teacher).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (grade == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", grade.SubjectId);

                var allStudents = await _context.Students
                    .Include(s => s.User).Include(s => s.SchoolClass)
                    .Select(s => new { Id = s.Id, FullName = $"{s.User.LastName} {s.User.FirstName} ({s.SchoolClass.Name})" })
                    .ToListAsync();
                ViewData["StudentId"] = new SelectList(allStudents, "Id", "FullName", grade.StudentId);

                var allTeachers = await _context.Teachers.Include(t => t.User)
                    .Select(t => new { Id = t.Id, FullName = $"{t.User.LastName} {t.User.FirstName}" })
                    .ToListAsync();
                ViewData["TeacherId"] = new SelectList(allTeachers, "Id", "FullName", grade.TeacherId);
            }
            else
            {
                var teacher = await GetLoggedTeacherAsync();

                if (teacher == null || grade.TeacherId != teacher.Id)
                {
                    return Forbid();
                }

                var mySubjects = await _context.SubjectAssignments
                    .Where(sa => sa.TeacherId == teacher.Id)
                    .Include(sa => sa.Subject)
                    .Select(sa => sa.Subject)
                    .Distinct()
                    .ToListAsync();

                if (!mySubjects.Any(s => s.Id == grade.SubjectId))
                {
                    mySubjects.Add(grade.Subject);
                }

                var myClassIds = await _context.SubjectAssignments
                    .Where(sa => sa.TeacherId == teacher.Id)
                    .Select(sa => sa.SchoolClassId)
                    .Distinct()
                    .ToListAsync();

                var myStudents = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.SchoolClass)
                    .Where(s => myClassIds.Contains(s.SchoolClassId) || s.Id == grade.StudentId)
                    .OrderBy(s => s.SchoolClass.Name)
                    .ThenBy(s => s.User.LastName)
                    .Select(s => new
                    {
                        Id = s.Id,
                        FullName = $"{s.User.LastName} {s.User.FirstName} (klasa {s.SchoolClass.Name})"
                    })
                    .ToListAsync();

                ViewData["SubjectId"] = new SelectList(mySubjects, "Id", "Name", grade.SubjectId);
                ViewData["StudentId"] = new SelectList(myStudents, "Id", "FullName", grade.StudentId);

            }

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

            var teacher = await GetLoggedTeacherAsync();

            if (!User.IsInRole("Admin"))
            {

                var originalGrade = await _context.Grades.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);

                if (originalGrade == null || teacher == null || originalGrade.TeacherId != teacher.Id)
                {
                    return Forbid();
                }

                grade.TeacherId = teacher.Id;

                var student = await _context.Students.FindAsync(grade.StudentId);
                if (student != null)
                {
                    bool hasAssignment = await _context.SubjectAssignments.AnyAsync(sa =>
                       sa.TeacherId == teacher.Id &&
                       sa.SubjectId == grade.SubjectId &&
                       sa.SchoolClassId == student.SchoolClassId);

                    if (!hasAssignment)
                    {
 
                    }
                }
            }

            if (grade.Value < 1 || grade.Value > 6)
            {
                ModelState.AddModelError("Value", "Ocena musi być z zakresu 1-6.");
            }

            ModelState.Remove("Student");
            ModelState.Remove("Subject");
            ModelState.Remove("Teacher");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeExists(grade.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

    
            if (User.IsInRole("Admin"))
            {
                ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", grade.SubjectId);
                ViewData["StudentId"] = new SelectList(_context.Students.Include(s => s.User).Select(s => new { Id = s.Id, Fn = $"{s.User.LastName}" }), "Id", "Fn", grade.StudentId);
                ViewData["TeacherId"] = new SelectList(_context.Teachers.Include(t => t.User).Select(t => new { Id = t.Id, Fn = $"{t.User.LastName}" }), "Id", "Fn", grade.TeacherId);
            }
            else
            {
         
                ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", grade.SubjectId);
                ViewData["StudentId"] = new SelectList(_context.Students.Include(s => s.User).Select(s => new { Id = s.Id, Fn = $"{s.User.LastName}" }), "Id", "Fn", grade.StudentId);
            }

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

            if (!User.IsInRole("Admin"))
            {
                var teacher = await GetLoggedTeacherAsync();
                if (teacher == null || grade.TeacherId != teacher.Id)
                {
                    return Forbid();
                }
            }

            return View(grade);
        }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return RedirectToAction(nameof(Index));

            if (!User.IsInRole("Admin"))
            {
                var teacher = await GetLoggedTeacherAsync();
                if (teacher == null || grade.TeacherId != teacher.Id)
                    return Forbid();
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.Id == id);
        }


        //  funkcje pomocnicze

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
