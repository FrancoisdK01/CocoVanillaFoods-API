using API.Data;
using API.Model;
using API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {

        public readonly MyDbContext _context;
        public readonly UserManager<User> _userManager;

        public AuditLogController(MyDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpPost]
        public async Task<ActionResult<AuditTrail>> writeToAuditLog(AuditLogViewModel alVM)
        {
            var loggedInUser = await _userManager.FindByEmailAsync(alVM.email);

            if(loggedInUser == null)
            {
                return NotFound("Logged in user not found");
            }

            var auditTrail = new AuditTrail
            {
                ButtonPressed = alVM.buttonClicked,
                UserName = loggedInUser.UserName,
                UserEmail = alVM.email,
                TransactionDate = DateTime.UtcNow,
                Quantity = alVM.quantity
            };

            var addRecord = await _context.AuditTrails.AddAsync(auditTrail);
            if(addRecord == null)
            {
                return BadRequest("No data that you want to add in the database");
            }
            await _context.SaveChangesAsync();

            return Ok(auditTrail);

        }
    }
}
