using API.Data;
using API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.ViewModels;
using System.Globalization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderHistoryController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public OrderHistoryController(MyDbContext context, UserManager<User> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpPost("{email}")]
        [DynamicAuthorize]
        public async Task<ActionResult<WineOrder>> UserAddOrder(string email)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);

            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            var cart = await _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Wine).FirstOrDefaultAsync(c => c.CustomerID == customer.Id);

            if (cart == null || cart.CartItems.Count == 0)
            {
                return BadRequest("Cart is empty.");
            }

            // Create new order and map cart items to order items
            var order = new WineOrder
            {
                OrderRefNum = GenerateOrderRefNum(),
                OrderStatusId = (int)OrderStatusEnum.ClientOrderPlaced,  // or another appropriate value

                OrderTotal = (int)(cart.DiscountedCart != 0 ? cart.DiscountedCart : cart.CartTotal),
                CustomerId = customer.Id,
                OrderDate = DateTime.Now,
                OrderItems = cart.CartItems.Select(ci => new WineOrderItem { WineId = ci.WineID, Quantity = ci.Quantity }).ToList()
            };


            await _context.WineOrders.AddAsync(order);

            // Clear cart items
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSingleOrderEntry), new { id = order.WineOrderId }, order);
        }

        [HttpGet("{email}")]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<WineOrder>>> GetAllOrdersForUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            var orders = await _context.WineOrders.Include(o => o.OrderItems)
                                                  .ThenInclude(oi => oi.Wine)
                                                  .Where(x => x.CustomerId == customer.Id)
                                                  .ToListAsync();

            // Return an empty list if no orders are found
            if (orders == null || orders.Count == 0)
            {
                return Ok(new List<WineOrder>());
            }

            return Ok(orders);
        }

        [HttpGet("Order/{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<WineOrder>> GetSingleOrderEntry(int id)
        {
            var order = await _context.WineOrders.Include(o => o.OrderItems)
                                                 .ThenInclude(oi => oi.Wine)
                                                 .FirstOrDefaultAsync(x => x.WineOrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        private bool WineOrderExists(int id)
        {
            return _context.WineOrders.Any(e => e.WineOrderId == id);
        }


        [HttpGet("AllOrders")]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<WineOrder>>> GetAllOrders()
        {
            var allOrders = await _context.WineOrders.Include(o => o.OrderItems)
                                                     .ThenInclude(oi => oi.Wine)
                                                     .OrderBy(o => o.OrderDate)
                                                     .ToListAsync();

            if (allOrders == null || allOrders.Count == 0)
            {
                return NotFound("No orders found for any client.");
            }

            return allOrders;
        }


        private string GenerateOrderRefNum()
        {
            var guid = Guid.NewGuid();
            var str = Convert.ToBase64String(guid.ToByteArray());
            str = str.Replace("=", "").Replace("+", "").Replace("/", "");
            var stringRetrn = str.Substring(1, 7);
            return stringRetrn.ToUpper();
        }

        [HttpGet("AllSales")]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<WineOrder>>> GetAllSales()
        {
            var orders = await _context.WineOrders
                .Include(o => o.OrderItems)
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found.");
            }

            return Ok(orders);
        }

        /////////////////////////////Marco kode om die sales chart te display/////////////////////////////////////
        [HttpGet("SalesReport")]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<object>>> GenerateSalesReport(string? startDate, string? endDate)
        {
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                // Return an empty array with a 200 OK status
                return Ok(new List<object>());
            }

            DateTime startDateTime = DateTime.ParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddHours(23).AddMinutes(59).AddSeconds(59);

            var orders = await _context.WineOrders
                            .Include(o => o.OrderItems)
                            .Where(o => o.OrderDate >= startDateTime && o.OrderDate <= endDateTime)
                            .ToListAsync();

            if (orders == null || !orders.Any())
            {
                // Return an empty array with a 200 OK status
                return Ok(new List<object>());
            }

            // Group orders by OrderDate and sum the total for each day
            var result = orders.GroupBy(o => o.OrderDate.Date)
                               .Select(group => new
                               {
                                   OrderDate = group.Key,
                                   TotalAmount = group.Sum(o => o.OrderTotal)
                               })
                               .OrderBy(item => item.OrderDate)
                               .ToList();

            return Ok(result);
        }

        [HttpPut("UpdateOrder/{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromQuery] int newStatus)
        {
            {
                var order = await _context.WineOrders.FindAsync(id);
                int status = newStatus;

                if (status == 0)
                {
                    return NotFound("Status");
                }

                if (order == null)
                {
                    return NotFound();
                }

                var customer = await _context.Customers.FindAsync(order.CustomerId);
                if (customer == null)
                {
                    return NotFound("Customer not found.");
                }

                EmailViewModel evm = null;

                if (newStatus == 2)
                {
                    order.OrderStatusId = 2; // Set status to "SupplierOrderPlaced"
                    evm = new EmailViewModel
                    {
                        To = customer.Email,
                        Subject = "Your order has been placed",
                        Body = $@"Order Placed Confirmation
                              Dear {customer.First_Name},
                              We are writing to confirm that your order has been successfully placed.
                  
                              Order Reference: {order.OrderRefNum}
                  
                              To initiate a refund, kindly visit the 'Orders' page within your account and follow the prompt to request a refund. Our customer service team will assist you throughout the process.
                              We value your trust in our brand and are committed to ensuring your satisfaction.
                              Warm regards,
                              The Promenade Team
                  "
                    };
                }
                else if (newStatus == 3)
                {
                    order.OrderStatusId = 3;
                    order.CollectedDate = DateTime.Now;
                    evm = new EmailViewModel
                    {
                        To = customer.Email,
                        Subject = "Your order is at Promenade",
                        Body = $@"Order Confirmation: Ready for Collection
                          Dear {customer.First_Name},
                          We are pleased to inform you that your order has been processed and is now ready for collection.
                          Please ensure you have your order reference number with you when you arrive for a seamless collection process.
                  
                          Order Reference: {order.OrderRefNum}
                  
                          Our team has ensured that your order is packed with care, awaiting your collection at the venue. 
                             Should you have any questions, or require any further assistance, please do not hesitate to contact our customer service team.
                          Thank you for choosing us.
                          Warm regards,
                          The Promenade Team
                          "
                    };
                }
                else if (newStatus == 4)  // Assuming 4 corresponds to "Collected"
                {
                    order.OrderStatusId = 4; // Set status to "Collected"
                    evm = new EmailViewModel
                    {
                        To = customer.Email,
                        Subject = "Your order has been Collected",
                        Body = $@"Order Collected
                            Dear {customer.First_Name},
                            We are pleased to inform you that your order has been successfully collected.
                            Thank you for choosing us.
                            Warm regards,
                            The Promenade Team
                            "
                    };
                }

                if (evm != null)
                {
                    await _emailService.SendSimpleMessage(evm);
                    //_emailService.SendEmail(evm);
                }

                _context.Entry(order).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WineOrderExists(id))
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

        }





    }
}