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
    }
}
