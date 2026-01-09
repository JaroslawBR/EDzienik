using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EDzienik.Data;
using EDzienik.Entities;

namespace EDzienik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectAssignmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubjectAssignmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/SubjectAssignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectAssignment>>> GetSubjectAssignments()
        {
            return await _context.SubjectAssignments.ToListAsync();
        }

        // GET: api/SubjectAssignments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SubjectAssignment>> GetSubjectAssignment(int id)
        {
            var subjectAssignment = await _context.SubjectAssignments.FindAsync(id);

            if (subjectAssignment == null)
            {
                return NotFound();
            }

            return subjectAssignment;
        }

        // PUT: api/SubjectAssignments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubjectAssignment(int id, SubjectAssignment subjectAssignment)
        {
            if (id != subjectAssignment.Id)
            {
                return BadRequest();
            }

            _context.Entry(subjectAssignment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubjectAssignmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SubjectAssignments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SubjectAssignment>> PostSubjectAssignment(SubjectAssignment subjectAssignment)
        {
            _context.SubjectAssignments.Add(subjectAssignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSubjectAssignment", new { id = subjectAssignment.Id }, subjectAssignment);
        }

        // DELETE: api/SubjectAssignments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubjectAssignment(int id)
        {
            var subjectAssignment = await _context.SubjectAssignments.FindAsync(id);
            if (subjectAssignment == null)
            {
                return NotFound();
            }

            _context.SubjectAssignments.Remove(subjectAssignment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SubjectAssignmentExists(int id)
        {
            return _context.SubjectAssignments.Any(e => e.Id == id);
        }
    }
}
