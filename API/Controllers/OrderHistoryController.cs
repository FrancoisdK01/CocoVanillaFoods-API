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
        public async Task<ActionResult<WineOrder>> CreateOrder(string email)
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
                OrderStatus = 0,

                OrderTotal = (int)(cart.DiscountedCart != 0 ? cart.DiscountedCart : cart.CartTotal),
                CustomerId = customer.Id,
                OrderDate = DateTime.Now,
                OrderItems = cart.CartItems.Select(ci => new WineOrderItem { WineId = ci.WineID, Quantity = ci.Quantity }).ToList()
            };


            await _context.WineOrders.AddAsync(order);

            // Clear cart items
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.WineOrderId }, order);
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<IEnumerable<WineOrder>>> GetOrdersForUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
            {
                return BadRequest();
            }

            var orders = await _context.WineOrders.Include(o => o.OrderItems)
                                                  .ThenInclude(oi => oi.Wine)
                                                  .Where(x => x.CustomerId == customer.Id)
                                                  .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found for this user.");
            }

            return orders;
        }

        [HttpGet("Order/{id}")]
        public async Task<ActionResult<WineOrder>> GetOrder(int id)
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

        [HttpPut("UpdateOrder/{id}")]
        public async Task<ActionResult> UpdateOrderStatus(int id)
        {
            var order = await _context.WineOrders.FindAsync(id);
            var customer = await _context.Customers.FindAsync(order.CustomerId);

            var evm = new EmailViewModel();
            if (order == null)
            {
                return NotFound();
            }

            if(order.OrderStatus == 0)
            {
                order.OrderStatus = 1;  // Set status to "Received"
                evm = new EmailViewModel
                {
                    To = order.Customer.Email,
                    Subject = "Your order is at Promenade",
                    Body = $@"
                              <h1>Order Confirmation: Ready for Collection</h1>
                              <p>Dear {customer.First_Name},</p>
                              <p>We are pleased to inform you that your order has been processed and is now ready for collection.</p>
                              <p>Please ensure you have your order reference number with you when you arrive for a seamless collection process.</p>
                              <ul>
                                <li>Order Reference: {order.OrderRefNum}</li>
                              </ul>
                              <p>Our team has ensured that your order is packed with care, awaiting your collection. Should you have any questions, or require any further assistance, please do not hesitate to contact our customer service team.</p>
                              <p>Thank you for choosing us.</p>
                              <p>Warm regards,</p>
                              <p>The Promenade Team</p>
                      "
                };
            }
            else if(order.OrderStatus == 1)
            {
                order.OrderStatus = 2;
                order.CollectedDate = DateTime.Now;

                evm = new EmailViewModel
                {
                    To = order.Customer.Email,
                    Subject = "Your order is at Promenade",
                    Body = $@"
                              <h1>Order Collection Confirmation</h1>
                              <p>Dear {customer.First_Name},</p>
                              <p>We are writing to confirm that your order has been successfully collected.</p>
                              <p>We strive for utmost precision and quality in our products and services. If, for any reason, you find any issues with your order, please be informed that you have a period of 7 days from the date of collection to request a refund.</p>
                              <ul>
                                <li>Order Reference: {order.OrderRefNum}</li>
                              </ul>
                              <p>To initiate a refund, kindly visit the 'Orders' page within your account and follow the prompt to request a refund. Our customer service team will assist you throughout the process.</p>
                              <p>We value your trust in our brand and are committed to ensuring your satisfaction.</p>
                              <p>Warm regards,</p>
                              <p>The Promenade Team</p>
                      "
                };
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                _emailService.SendEmail(evm);
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


        private bool WineOrderExists(int id)
        {
            return _context.WineOrders.Any(e => e.WineOrderId == id);
        }


        [HttpGet("AllOrders")]
        public async Task<ActionResult<IEnumerable<WineOrder>>> GetAllOrders()
        {
            var allOrders = await _context.WineOrders.Include(o => o.OrderItems)
                                                     .ThenInclude(oi => oi.Wine)
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

        //Charts for reporting the sales
        //[HttpGet("SalesReport/{startDate}/{endDate}")]
        //public async Task<ActionResult<IEnumerable<WineOrder>>> GetSalesReport(string startDate, string endDate)
        //{
        //    DateTime startDateTime = DateTime.ParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        //    DateTime endDateTime = DateTime.ParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        //    var orders = await _context.WineOrders
        //        .Include(o => o.OrderItems)
        //        .Where(o => o.OrderDate >= startDateTime && o.OrderDate <= endDateTime)
        //        .ToListAsync();

        //    if (orders == null || !orders.Any())
        //    {
        //        return NotFound("No orders found for the specified date range.");
        //    }

        //    return Ok(orders);
        //}


        [HttpGet("AllSales")]
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
        public async Task<ActionResult<IEnumerable<object>>> GetSalesReport(string? startDate, string? endDate)
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
    }

    



}
