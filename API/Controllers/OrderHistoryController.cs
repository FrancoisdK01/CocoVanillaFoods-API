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

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderHistoryController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;

        public OrderHistoryController(MyDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                Received = false,
                OrderTotal = cart.CartTotal,
                CustomerId = customer.Id,
                OrderDate = DateTime.UtcNow,
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

            if (order == null)
            {
                return NotFound();
            }

            order.Received = true;

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

    }




}
