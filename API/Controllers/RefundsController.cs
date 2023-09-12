using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using API.ViewModels;
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

        //[HttpPost("RequestRefund")]
        //public async Task<IActionResult> RequestRefund([FromBody] RefundRequestModel model)
        //{

        //    var wineOrderToUpdate = await _context.WineOrders.FirstOrDefaultAsync(o => o.OrderRefNum == model.ReferenceNumber);

        //    if (model == null || !ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    TimeSpan difference = DateTime.UtcNow - wineOrderToUpdate.CollectedDate;
        //    if (difference.TotalDays > 7)
        //    {
        //        return BadRequest("Can't request refund after 7 days of purchase.");
        //    }

        //    try
        //    {
        //        var refundRequest = new RefundRequest
        //        {
        //            WineId = model.WineId,
        //            Email = model.Email,
        //            RequestedOn = DateTime.UtcNow,
        //            Cost = model.Cost,
        //            Description = model.Description,
        //            isRefunded = true,
        //            PhoneNumber = model.PhoneNumber
        //        };
        //        if(wineOrderToUpdate != null)
        //        {
        //            wineOrderToUpdate.isRefunded = true;
        //        }

        //        _context.RefundRequests.Add(refundRequest);
        //        await _context.SaveChangesAsync();

        //        // Possibly send a notification about the refund request here...

        //        return Ok();
        //    }
        //    catch (Exception)
        //    {
        //        // Log the exception...
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        // GET: api/Refunds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RefundRequest>>> GetRefundRequests()
        {
            var allRefunds = _context.RefundRequests.Include(ri => ri.RefundItems).Include(wo => wo.WineOrder).ThenInclude(wo => wo.OrderItems).ToList();

            return allRefunds;
        }

        [HttpPost]
        [Route("RequestARefund")]
        public IActionResult RequestRefund([FromBody] RefundRequestViewModel request)
        {
            var wineOrder = _context.WineOrders
                .Include(wo => wo.OrderItems)
                .FirstOrDefault(wo => wo.WineOrderId == request.WineOrderId);

            if (wineOrder == null)
            {
                return NotFound("Order not found.");
            }

            // Check if a refund has already been requested for this order
            if (_context.RefundRequests.Any(rr => rr.WineOrderId == request.WineOrderId))
            {
                return BadRequest("A refund has already been requested for this order.");
            }

            // Check if the order was collected more than a week ago
            if (DateTime.Now > wineOrder.CollectedDate.AddDays(7))
            {
                return BadRequest("Refunds can only be requested within a week of the order's collection date.");
            }

            foreach (var item in request.RefundItems)
            {
                var orderItem = wineOrder.OrderItems.FirstOrDefault(oi => oi.WineOrderItemId == item.WineOrderItemId);

                if (orderItem == null)
                {
                    return BadRequest($"Order item with ID {item.WineOrderItemId} not found in the order.");
                }

                if (item.Quantity > orderItem.Quantity)
                {
                    return BadRequest($"Cannot refund {item.Quantity} units of wine with ID {item.WineOrderItemId}. Only {orderItem.Quantity} units were ordered.");
                }

                if (string.IsNullOrEmpty(item.Reason))
                {
                    return BadRequest($"Reason for refunding wine with ID {item.WineOrderItemId} is required.");
                }
            }

            var refundRequest = new RefundRequest
            {
                WineOrderId = request.WineOrderId,
                Status = "Pending", // Assuming a default status of "Pending" for new requests
                RefundItems = new List<RefundItem>()
            };

            foreach (var item in request.RefundItems)
            {
                var refundItem = new RefundItem
                {
                    WineOrderItemId = item.WineOrderItemId,
                    Quantity = item.Quantity,
                    Reason = item.Reason
                };
                refundRequest.RefundItems.Add(refundItem);
            }

            _context.RefundRequests.Add(refundRequest);
            _context.SaveChanges();

            return Ok(refundRequest);
        }





        [HttpGet]
        [Route("GetWineDetailsForRefund/{refundRequestId}")]
        public IActionResult GetWineDetailsForRefund(int refundRequestId)
        {
            // First, check if the RefundRequest exists
            var refundRequest = _context.RefundRequests.FirstOrDefault(rr => rr.RefundRequestId == refundRequestId);

            if (refundRequest == null)
            {
                return NotFound("Refund request not found.");
            }

            // Fetch the associated refund items and their wine details
            var refundItems = _context.RefundItems
                .Include(ri => ri.WineOrderItem)
                .ThenInclude(woi => woi.Wine)
                .Where(ri => ri.RefundRequestId == refundRequestId)
                .ToList();

            if (!refundItems.Any())
            {
                return NotFound("No wines found for the given refund request.");
            }

            var wineDetails = refundItems.Select(ri => new
            {
                WineId = ri.WineOrderItem.WineId,
                WineName = ri.WineOrderItem.Wine.Name,
                Quantity = ri.Quantity
            }).ToList();

            return Ok(wineDetails);
        }


        [HttpPost]
        [Route("UpdateRefundStatus")]
        public IActionResult UpdateRefundStatus(int refundRequestId, string newStatus)
        {
            var refundRequest = _context.RefundRequests.FirstOrDefault(rr => rr.RefundRequestId == refundRequestId);

            if (refundRequest == null)
            {
                return NotFound("Refund request not found.");
            }

            refundRequest.Status = newStatus;
            _context.SaveChanges();

            return Ok(refundRequest);
        }







        //Customer side stuff

        //[HttpGet("{email}")]
        //public async Task<ActionResult<IEnumerable<RefundRequest>>> GetUserRefundRequests(string email)
        //{
        //    var wineOrder = 

        //    return await _context.RefundRequests.Where(r => r.WineOrderId == email).ToListAsync();
        //}
    }
}



//// Initialize Twilio here, or make sure it's initialized in Startup.cs
//// Ideally, you would read these from a configuration file or environment variables.
//string accountSid = "AC14ac294ab00ac67c898644d8f27851e4";
//string authToken = "b6feebfd098dedb8d5414e067027cb05";
//TwilioClient.Init(accountSid, authToken);

//try
//{
//    // Send SMS
//    var to = $"+27{refundRequest.PhoneNumber.Substring(1)}";  // Assuming phoneNumber is like '0721843438'
//    var from = "+18589018233";
//    var message = $"Your refund request for order number {model.OrderRefNum} has been updated to {model.Status}.";

//    var smsResponse = MessageResource.Create(
//        body: message,
//        from: new Twilio.Types.PhoneNumber(from),
//        to: new Twilio.Types.PhoneNumber(to)
//    );

//    if (smsResponse.ErrorCode != null)
//    {
//        return BadRequest($"SMS failed with error code: {smsResponse.ErrorCode}");
//    }
//}
//catch (Exception ex)
//{
//    // Log the exception, or handle it as per your requirements
//    return BadRequest($"An error occurred while sending SMS: {ex.Message}");
//}
