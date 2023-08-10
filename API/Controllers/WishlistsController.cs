using API.Data;
using API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;

        public WishlistController(MyDbContext context, UserManager<User> usermanager)
        {
            _context = context;
            _userManager = usermanager;
        }

        [HttpGet("{email}")]
        [Authorize(Roles = "Customer")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Wishlist>> GetWishlist(string email)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            var wishlist = await _context.Wishlists.Include(w => w.WishlistItems)
                                                   .ThenInclude(wi => wi.Wine)
                                                   .FirstOrDefaultAsync(w => w.CustomerID == customer.Id);

            if (wishlist == null)
            {
                return NotFound("Wishlist not found.");
            }

            return wishlist;
        }

        [HttpPost("{email}")]
        public async Task<ActionResult<Wishlist>> AddToWishlist(string email, [FromBody] WishlistItem wishlistItem)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            var wine = await _context.Wines.FindAsync(wishlistItem.WineID);

            if (wine == null)
            {
                return NotFound("Wine not found.");
            }

            var wishlist = await _context.Wishlists.Include(w => w.WishlistItems).ThenInclude(wi => wi.Wine).FirstOrDefaultAsync(w => w.CustomerID == customer.Id);

            // If no wishlist for the user exists, create a new one
            if (wishlist == null)
            {
                wishlist = new Wishlist { CustomerID = customer.Id, WishlistItems = new List<WishlistItem>() };
                await _context.Wishlists.AddAsync(wishlist);
            }

            var existingWishlistItem = wishlist.WishlistItems.FirstOrDefault(wi => wi.WineID == wishlistItem.WineID);

            // In wishlist, we are not considering the quantity of an item. Hence, no increment logic.
            if (existingWishlistItem == null)
            {
                // Add a new wishlist item to the wishlist
                var newWishlistItem = new WishlistItem { WineID = wishlistItem.WineID, WishlistID = wishlist.WishlistID };
                wishlist.WishlistItems.Add(newWishlistItem);

            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWishlist), new { email = email }, wishlist);
        }

        [HttpDelete("{email}/{wishlistItemId}")]
        public async Task<IActionResult> RemoveFromWishlist(string email, int wishlistItemId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            var wishlistItem = await _context.WishlistItems.Include(wi => wi.Wishlist).FirstOrDefaultAsync(wi => wi.WishlistItemID == wishlistItemId && wi.Wishlist.CustomerID == customer.Id);

            if (wishlistItem == null)
            {
                return NotFound("Wishlist item not found.");
            }

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
