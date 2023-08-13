using API.Data;
using API.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ReportsController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("getRefundReport/{beginDate}/{endDate}")]
        [Authorize(Roles = "Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllRefunds(DateTime beginDate, DateTime endDate)
        {
            // Ensure dates are in correct order (swap if necessary)
            if (beginDate > endDate)
            {
                var temp = beginDate;
                beginDate = endDate;
                endDate = temp;
            }


            var listOfWriteOffs = await _context.RefundRequests
                                             .Include(w => w.WineOrder).Include(wo => wo.WineOrder)
                                             .Where(r => r.RequestedOn >= beginDate && r.RequestedOn <= endDate)
                                             .ToListAsync();

            foreach (var request in listOfWriteOffs)
            {
                request.OrderRefNum = request.WineOrder?.OrderRefNum; // Set the OrderRefNum property
            }

            return Ok(listOfWriteOffs);
        }

        [HttpGet]
        [Route("getEventReport/{beginDate}/{endDate}")]
        [Authorize(Roles = "Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetEventsReport(DateTime beginDate, DateTime endDate)
        {
            // Ensure dates are in correct order (swap if necessary)
            if (beginDate > endDate)
            {
                var temp = beginDate;
                beginDate = endDate;
                endDate = temp;
            }

            var listOfEvents = await _context.Events
                                             .Include(e => e.EarlyBird)
                                             .Where(e => e.EventDate >= beginDate && e.EventDate <= endDate)
                                             .ToListAsync();

            return Ok(listOfEvents);
        }

        [HttpGet]
        [Route("getAllSupplierOrders")]
        [Authorize(Roles = "Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllSupplierOrders()
        {
            var listOfSupplierOrders = await _context.SupplierOrders.ToListAsync();

            return Ok(listOfSupplierOrders);
        }
    }
}
