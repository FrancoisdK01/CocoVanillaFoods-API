using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefundsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public RefundsController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost("RequestRefund")]
        public async Task<IActionResult> RequestRefund([FromBody] RefundRequestModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var refundRequest = new RefundRequest
                {
                    WineId = model.WineId,
                    Email = model.Email,
                    RequestedOn = DateTime.UtcNow,
                    Cost = model.Cost,
                    Description = model.Description // Added line
                };

                _context.RefundRequests.Add(refundRequest);
                await _context.SaveChangesAsync();

                // Possibly send a notification about the refund request here...

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception...
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Refunds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RefundRequest>>> GetRefundRequests()
        {
            return await _context.RefundRequests.ToListAsync();
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<IEnumerable<RefundRequest>>> GetUserRefundRequests(string email)
        {
            return await _context.RefundRequests.Where(r => r.Email == email).ToListAsync();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateModel model)
        {
            var refundRequest = await _context.RefundRequests.FindAsync(id);

            if (refundRequest == null)
            {
                return NotFound();
            }

            refundRequest.Status = model.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}
