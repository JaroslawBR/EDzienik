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
    public class SchoolEventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SchoolEventsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/SchoolEvents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SchoolEvent>>> GetSchoolEvents()
        {
            return await _context.SchoolEvents.ToListAsync();
        }

        // GET: api/SchoolEvents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SchoolEvent>> GetSchoolEvent(int id)
        {
            var schoolEvent = await _context.SchoolEvents.FindAsync(id);

            if (schoolEvent == null)
            {
                return NotFound();
            }

            return schoolEvent;
        }

        // PUT: api/SchoolEvents/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchoolEvent(int id, SchoolEvent schoolEvent)
        {
            if (id != schoolEvent.Id)
            {
                return BadRequest();
            }

            _context.Entry(schoolEvent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolEventExists(id))
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

        // POST: api/SchoolEvents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SchoolEvent>> PostSchoolEvent(SchoolEvent schoolEvent)
        {
            _context.SchoolEvents.Add(schoolEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSchoolEvent", new { id = schoolEvent.Id }, schoolEvent);
        }

        // DELETE: api/SchoolEvents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchoolEvent(int id)
        {
            var schoolEvent = await _context.SchoolEvents.FindAsync(id);
            if (schoolEvent == null)
            {
                return NotFound();
            }

            _context.SchoolEvents.Remove(schoolEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SchoolEventExists(int id)
        {
            return _context.SchoolEvents.Any(e => e.Id == id);
        }
    }
}
