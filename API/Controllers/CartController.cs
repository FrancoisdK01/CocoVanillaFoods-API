using API.Data;
using API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;

        public CartController(MyDbContext context, UserManager<User> usermanager)
        {
            _context = context;
            _userManager = usermanager;
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<Cart>> GetCart(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
            {
                return BadRequest();
            }

            var customerID = customer.Id.ToString();

            var cart = await _context.Carts.Include(c => c.CartItems)
                                           .ThenInclude(ci => ci.Wine)
                                           .Where(x => x.CustomerID == customerID)
                                           .FirstOrDefaultAsync();

            if (cart == null)
            {
                return NotFound("Cart not found.");
            }

            return cart;
        }


        [HttpPost("{email}")]
        public async Task<ActionResult<Cart>> AddToCart(string email, [FromBody] CartItem cartItem)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);

            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            var wine = await _context.Wines.FindAsync(cartItem.WineID);

            if (wine == null)
            {
                return NotFound("Wine not found.");
            }

            var cart = await _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Wine).FirstOrDefaultAsync(c => c.CustomerID == customer.Id);

            // If no cart for the user exists, create a new one
            if (cart == null)
            {
                cart = new Cart { CustomerID = customer.Id, CartItems = new List<CartItem>() };
                await _context.Carts.AddAsync(cart);
            }

            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.WineID == cartItem.WineID);

            if (existingCartItem != null)
            {
                // Increment the quantity of the existing cart item
                existingCartItem.Quantity += cartItem.Quantity;
            }
            else
            {
                // Add a new cart item to the cart
                var newCartItem = new CartItem { WineID = cartItem.WineID, Quantity = cartItem.Quantity, CartID = cart.CartID };
                cart.CartItems.Add(newCartItem);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCart), new { email = email }, cart);
        }


        [HttpPut("{email}/increment/{cartItemId}")]
        public async Task<IActionResult> IncrementCartItemQuantity(string email, int cartItemId)
        {

            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);



            var cartItem = await _context.CartItems.Include(ci => ci.Cart).FirstOrDefaultAsync(ci => ci.CartItemID == cartItemId && ci.Cart.CustomerID == customer.Id);

            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            cartItem.Quantity++;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{email}/decrement/{cartItemId}")]
        public async Task<IActionResult> DecrementCartItemQuantity(string email, int cartItemId)
        {


            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);
            var cartItem = await _context.CartItems.Include(ci => ci.Cart).FirstOrDefaultAsync(ci => ci.CartItemID == cartItemId && ci.Cart.CustomerID == customer.Id);

            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            if (cartItem.Quantity <= 1)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity--;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{email}/total")]
        public async Task<ActionResult<double>> GetCartTotal(string email)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);

            var cart = await _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Wine).FirstOrDefaultAsync(c => c.CustomerID == customer.Id);

            if (cart == null)
            {
                return NotFound("Cart not found.");
            }

            var total = cart.CartItems.Sum(ci => ci.Quantity * ci.Wine.WinePrice);

            return Ok(total);
        }
    }
}
