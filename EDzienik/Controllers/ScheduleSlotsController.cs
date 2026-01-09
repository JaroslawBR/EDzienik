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
    public class ScheduleSlotsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScheduleSlotsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ScheduleSlots
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleSlot>>> GetScheduleSlots()
        {
            return await _context.ScheduleSlots.ToListAsync();
        }

        // GET: api/ScheduleSlots/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleSlot>> GetScheduleSlot(int id)
        {
            var scheduleSlot = await _context.ScheduleSlots.FindAsync(id);

            if (scheduleSlot == null)
            {
                return NotFound();
            }

            return scheduleSlot;
        }

        // PUT: api/ScheduleSlots/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScheduleSlot(int id, ScheduleSlot scheduleSlot)
        {
            if (id != scheduleSlot.Id)
            {
                return BadRequest();
            }

            _context.Entry(scheduleSlot).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleSlotExists(id))
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

        // POST: api/ScheduleSlots
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ScheduleSlot>> PostScheduleSlot(ScheduleSlot scheduleSlot)
        {
            _context.ScheduleSlots.Add(scheduleSlot);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetScheduleSlot", new { id = scheduleSlot.Id }, scheduleSlot);
        }

        // DELETE: api/ScheduleSlots/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheduleSlot(int id)
        {
            var scheduleSlot = await _context.ScheduleSlots.FindAsync(id);
            if (scheduleSlot == null)
            {
                return NotFound();
            }

            _context.ScheduleSlots.Remove(scheduleSlot);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScheduleSlotExists(int id)
        {
            return _context.ScheduleSlots.Any(e => e.Id == id);
        }
    }
}
