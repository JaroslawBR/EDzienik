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
    public class SubjectAssignmentsController : Controller
    {
        private readonly AppDbContext _context;

        public SubjectAssignmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SubjectAssignments
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.SubjectAssignments.Include(s => s.SchoolClass).Include(s => s.Subject).Include(s => s.Teacher);
            return View(await appDbContext.ToListAsync());
        }

        // GET: SubjectAssignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectAssignment = await _context.SubjectAssignments
                .Include(s => s.SchoolClass)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subjectAssignment == null)
            {
                return NotFound();
            }

            return View(subjectAssignment);
        }

        // GET: SubjectAssignments/Create
        public IActionResult Create()
        {
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name");
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name");
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId");
            return View();
        }

        // POST: SubjectAssignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TeacherId,SubjectId,SchoolClassId")] SubjectAssignment subjectAssignment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subjectAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", subjectAssignment.SchoolClassId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", subjectAssignment.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId", subjectAssignment.TeacherId);
            return View(subjectAssignment);
        }

        // GET: SubjectAssignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectAssignment = await _context.SubjectAssignments.FindAsync(id);
            if (subjectAssignment == null)
            {
                return NotFound();
            }
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", subjectAssignment.SchoolClassId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", subjectAssignment.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId", subjectAssignment.TeacherId);
            return View(subjectAssignment);
        }

        // POST: SubjectAssignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeacherId,SubjectId,SchoolClassId")] SubjectAssignment subjectAssignment)
        {
            if (id != subjectAssignment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subjectAssignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectAssignmentExists(subjectAssignment.Id))
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
            ViewData["SchoolClassId"] = new SelectList(_context.SchoolClasses, "Id", "Name", subjectAssignment.SchoolClassId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name", subjectAssignment.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "UserId", subjectAssignment.TeacherId);
            return View(subjectAssignment);
        }

        // GET: SubjectAssignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectAssignment = await _context.SubjectAssignments
                .Include(s => s.SchoolClass)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subjectAssignment == null)
            {
                return NotFound();
            }

            return View(subjectAssignment);
        }

        // POST: SubjectAssignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subjectAssignment = await _context.SubjectAssignments.FindAsync(id);
            if (subjectAssignment != null)
            {
                _context.SubjectAssignments.Remove(subjectAssignment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectAssignmentExists(int id)
        {
            return _context.SubjectAssignments.Any(e => e.Id == id);
        }
    }
}
