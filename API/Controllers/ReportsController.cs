using API.Data;
using API.Model;
using API.ViewModels.ViewModelsForReports;
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
        [Route("getEventReport/{beginDate}/{endDate}")]
        [DynamicAuthorize]
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
                                             .Include(e => e.EarlyBird) // Why, you don't use it in your report???????
                                             .Where(e => e.EventDate >= beginDate && e.EventDate <= endDate)
                                             .ToListAsync();

            return Ok(listOfEvents);
        }

        [HttpGet]
        [Route("getAllSupplierOrders")]
        [DynamicAuthorize]
        public async Task<IActionResult> GetAllSupplierOrders()
        {
            DateTime currentDate = DateTime.Now;

            // Fetch the latest VAT percentage before (or on) the current date
            var activeVAT = await _context.VATs
                                .Where(v => v.Date <= currentDate)  // Filtering by date
                                .OrderByDescending(v => v.Date)  // Ordering in descending order to get the latest VAT
                                .FirstOrDefaultAsync();

            var listOfSupplierOrders = await _context.SupplierOrders.ToListAsync();

            var fullListOfSupplierDetails = new SupplierOrdersVAT
            {
                SupplierOrders = listOfSupplierOrders,
                VATs = activeVAT
            };

            return Ok(fullListOfSupplierDetails);
        }

    }
}
