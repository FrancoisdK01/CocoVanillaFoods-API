using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using Twilio.Rest.Api.V2010.Account;
using Twilio;

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

            var wineOrderToUpdate = await _context.WineOrders.FirstOrDefaultAsync(o => o.OrderRefNum == model.ReferenceNumber);

            if (model == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            TimeSpan difference = DateTime.UtcNow - wineOrderToUpdate.CollectedDate;
            if (difference.TotalDays > 7)
            {
                return BadRequest("Can't request refund after 7 days of purchase.");
            }

            try
            {
                var refundRequest = new RefundRequest
                {
                    WineId = model.WineId,
                    Email = model.Email,
                    RequestedOn = DateTime.UtcNow,
                    Cost = model.Cost,
                    Description = model.Description,
                    isRefunded = true,
                    PhoneNumber = model.PhoneNumber
                };
                if(wineOrderToUpdate != null)
                {
                    wineOrderToUpdate.isRefunded = true;
                }

                _context.RefundRequests.Add(refundRequest);
                await _context.SaveChangesAsync();

                // Possibly send a notification about the refund request here...

                return Ok();
            }
            catch (Exception)
            {
                // Log the exception...
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Refunds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RefundRequest>>> GetRefundRequests()
        {
            var refundRequests = await _context.RefundRequests
        .Include(r => r.WineOrder) // Include the related WineOrder object
        .ToListAsync();

            foreach (var request in refundRequests)
            {
                request.OrderRefNum = request.WineOrder?.OrderRefNum; // Set the OrderRefNum property
            }

            return refundRequests;
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

            Console.WriteLine($"OrderRefNum before SMS send: {refundRequest.OrderRefNum}");  // Debug log

            refundRequest.Status = model.Status;
            await _context.SaveChangesAsync();

            // Initialize Twilio here, or make sure it's initialized in Startup.cs
            // Ideally, you would read these from a configuration file or environment variables.
            string accountSid = "AC14ac294ab00ac67c898644d8f27851e4";
            string authToken = "b6feebfd098dedb8d5414e067027cb05";
            TwilioClient.Init(accountSid, authToken);

            try
            {
                // Send SMS
                var to = $"+27{refundRequest.PhoneNumber.Substring(1)}";  // Assuming phoneNumber is like '0721843438'
                var from = "+18589018233";
                var message = $"Your refund request for order number {model.OrderRefNum} has been updated to {model.Status}.";

                var smsResponse = MessageResource.Create(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(from),
                    to: new Twilio.Types.PhoneNumber(to)
                );

                if (smsResponse.ErrorCode != null)
                {
                    return BadRequest($"SMS failed with error code: {smsResponse.ErrorCode}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception, or handle it as per your requirements
                return BadRequest($"An error occurred while sending SMS: {ex.Message}");
            }

            return NoContent();
        }




    }
}
