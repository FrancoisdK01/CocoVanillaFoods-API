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
        private readonly IConfiguration _config;
        private string userPhoneNumber;

        public RefundsController(MyDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;   
        }

        // GET: api/Refunds
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<RefundRequest>>> GetRefundRequests()
        {
            var allRefunds = _context.RefundRequests.Include(ri => ri.RefundItems).Include(wo => wo.WineOrder).ThenInclude(wo => wo.OrderItems).ToList();

            return allRefunds;
        }

        [HttpGet]
        [Route("allRefundsResponses")]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<RefundResponse>>> GetRefundReponses()
        {
            var allRefundsResponses = _context.RefundResponses.ToList();

            return allRefundsResponses;
        }

        [HttpGet]
        [Route("getResponse/{id}")]
        [DynamicAuthorize]
        public IActionResult GetResponse(int id)
        {
            // First, find the RefundItems with the matching RefundRequestId
            var refundItems = _context.RefundItems.Where(ri => ri.RefundRequestId == id).ToList();

            if (!refundItems.Any())
            {
                return NotFound($"No refund items found for RefundRequestId: {id}");
            }

            // Create a list to hold the result
            List<RefundResponseViewModel> responseList = new List<RefundResponseViewModel>();

            foreach (var item in refundItems)
            {
                var response = _context.RefundResponses.Find(item.ResponseID);
                if (response != null)
                {
                    responseList.Add(new RefundResponseViewModel
                    {
                        RefundItemId = item.RefundItemId,
                        Description = response.Description
                    });
                }
            }

            return Ok(responseList);
        }

        [HttpPost]
        [Route("RequestARefund")]
        [DynamicAuthorize]
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
                Status = "Incomplete", 
                RefundItems = new List<RefundItem>(),
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

            wineOrder.isRefunded = true;

            _context.RefundRequests.Add(refundRequest);
            _context.SaveChanges();

            return Ok(refundRequest);
        }





        [HttpGet]
        [Route("GetWineDetailsForRefund/{refundRequestId}")]
        [DynamicAuthorize]
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
                Quantity = ri.Quantity,
                Cost = ri.Quantity * ri.WineOrderItem.Wine.Price,
                Reason = ri.Reason
            }).ToList();

            return Ok(wineDetails);
        }


        [HttpPut]
        [Route("UpdateRefundStatus/{refundRequestId}")]
        [DynamicAuthorize]
        public IActionResult UpdateRefundStatus(int refundRequestId, [FromBody] List<RefundItemUpdateViewModel> itemsStatuses)
        {
            try
            {
                var refundRequest = _context.RefundRequests
                                           .Include(rr => rr.RefundItems)  // Load associated RefundItems
                                           .FirstOrDefault(rr => rr.RefundRequestId == refundRequestId);

                if (refundRequest == null)
                {
                    return NotFound("Refund request not found.");
                }

                foreach (var itemStatus in itemsStatuses)
                {
                    var refundItem = refundRequest.RefundItems.FirstOrDefault(ri => ri.RefundItemId == itemStatus.RefundItemId);
                    if (refundItem != null)
                    {
                        var responseId = _context.RefundResponses.FirstOrDefault(r => r.Description == itemStatus.Status)?.RefundResponseID;
                        if (responseId.HasValue)
                        {
                            refundItem.ResponseID = responseId.Value;
                        }
                        else
                        {
                            return BadRequest($"Invalid status value: {itemStatus.Status}");
                        }
                    }
                    else
                    {
                        return NotFound($"Refund item with ID {itemStatus.RefundItemId} not found.");
                    }
                }
                if (refundRequest.RefundItems.All(ri => ri.ResponseID == 2 || ri.ResponseID == 3))
                {
                    refundRequest.Status = "Completed";
                }
                else
                {
                    refundRequest.Status = "Incomplete";
                }

                var wineOrder = _context.WineOrders.FirstOrDefault(wo => wo.WineOrderId == refundRequest.WineOrderId);
                if (wineOrder == null)
                {
                    return NotFound();
                }

                var user = _context.Customers.FirstOrDefault(c => c.Id == wineOrder.CustomerId);
                if (user == null)
                {
                    return NotFound();
                }

                userPhoneNumber = user.PhoneNumber;

                // SMS HERE

                string accountSid = _config["Twilio:AccountSid"];
                string authToken = _config["Twilio:AuthToken"];
                TwilioClient.Init(accountSid, authToken);

                try
                {
                    // Send SMS
                    var to = $"+27{userPhoneNumber.Substring(1)}";  // Assuming phoneNumber is like '0721843438'
                    var from = "+18589018233";
                    var message = $"Your refund request for order number {wineOrder.OrderRefNum} has been completed. \nShould you wish to see more information, please few your refunds page.";

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

                _context.SaveChanges();

                return Ok(refundRequest);
            }
            catch (DbUpdateException ex)
            {
                // Handle database update exceptions
                return StatusCode(500, $"Internal server error 1. Failed to update the database. Error details: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(500, $"Internal server error. Error details: {ex.Message}");
            }
        }

        //Customer side stuff
        [HttpGet("CustomerRefunds/{email}")]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<RefundRequest>>> GetUserRefundRequests(string email)
        {
            return await _context.RefundRequests.Where(r => r.WineOrder.Customer.Email == email).Include(wo => wo.WineOrder).ThenInclude(w => w.OrderItems).ThenInclude(oi => oi.Wine).ToListAsync();
        }
    }
}