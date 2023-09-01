using API.Data;
using API.Model;
using API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditTrail>>> GetAuditTrail()
        {
            return await _context.AuditTrails.ToListAsync();
        }

        [HttpPost]
        [Route("AddAuditLog")]
        public async Task<ActionResult<AuditTrail>> writeToAuditLog(AuditTrail auditLog)
        {
            var loggedInUser = await _userManager.FindByEmailAsync(auditLog.UserEmail);

            if(loggedInUser == null)
            {
                return NotFound("Logged in user not found");
            }

            var auditTrail = new AuditTrail
            {
                ButtonPressed = auditLog.ButtonPressed,
                UserName = loggedInUser.UserName,
                UserEmail = auditLog.UserEmail,
                TransactionDate = DateTime.Now,
                Quantity = auditLog.Quantity
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
