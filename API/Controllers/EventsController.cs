using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

//i like turtles

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;
        private readonly IWebHostEnvironment _environment;


        public EventsController(IConfiguration configuration, IWebHostEnvironment environment, MyDbContext context)
        {
            _configuration = configuration;
            _environment = environment;
            _bucketName = _configuration["GoogleCloudStorageBucketName"];
            _context = context;
        }

        private async Task<string> UploadFileToGoogleCloudStorage(string fileName, Stream fileStream)
        {
            try
            {
                var credential = GoogleCredential.FromFile(_configuration["GCPAuthStorageAuthFile"]);
                var storageClient = StorageClient.Create(credential);
                var bucket = storageClient.GetBucket(_bucketName);
                var objectName = $"images/{fileName}";

                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    storageClient.UploadObject(bucket.Name, objectName, null, memoryStream);
                }

                var filePath = $"https://storage.googleapis.com/{_bucketName}/{objectName}";

                return filePath;
            }
            catch (Exception ex)
            {
                // Log the exception here
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        // GET: api/Events
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            return await _context.Events
                .Include(e => e.EventType) // Include EventType
                .Include(e => e.EarlyBird) // Include EarlyBird
                .ToListAsync();
        }


        // GET: api/Events/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<Event>> GetSingleEventEntry(int id)
        {
            var @event = await _context.Events
                .Include(e => e.EventType) // Include EventType
                .Include(e => e.EarlyBird) // Include EarlyBird
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }


        // PUT: api/Events/5
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> UpdateEventDetails(int id, [FromForm] EventFormViewModel eventForm)
        {
            // Ensure ID matches
            if (id != eventForm.EventID)
            {
                return BadRequest();
            }

            // Get the current event from the database
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            // If a file is included, process and update the image
            if (eventForm.File != null)
            {
                // You might want to delete the existing image here similar to the wine method
                await DeleteImageFromGoogleCloudStorage(eventItem.FilePath);

                var fileName = $"{Guid.NewGuid()}_{eventForm.File.FileName}";
                var filePath = await UploadFileToGoogleCloudStorage(fileName, eventForm.File.OpenReadStream());

                if (string.IsNullOrEmpty(filePath))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload the file to Google Cloud Storage.");
                }

                eventItem.FilePath = filePath;
            }

            // Map other properties from the form to the event object
            eventItem.Name = eventForm.Name;
            eventItem.EventDate = eventForm.EventDate;
            eventItem.Tickets_Available = eventForm.Tickets_Available;
            eventItem.Description = eventForm.Description;
            eventItem.Price = eventForm.Price;
            eventItem.DisplayItem = eventForm.DisplayItem;
            eventItem.EarlyBirdID = eventForm.EarlyBirdID;

            // Update EarlyBirdID
            if (eventForm.EarlyBirdID == 0)
            {
                eventItem.EarlyBirdID = null;
            }
            else
            {
                eventItem.EarlyBirdID = eventForm?.EarlyBirdID;
            }

            // Update EventTypeID
            if (eventForm.EventTypeID == 0)
            {
                eventItem.EventTypeID = null;
            }
            else
            {
                eventItem.EventTypeID = eventForm?.EventTypeID;
            }

            var ticketPurchased = _context.TicketPurchases.FirstOrDefault(tp => tp.EventId == eventItem.EventID);
            ticketPurchased.EventDate = eventForm.EventDate;


            _context.Entry(eventItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id)) // You may want to implement the EventExists method similar to the WineExists method
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


        // POST: api/Events
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<Event>> AddEventDetails([FromForm] EventFormViewModel eventForm)
        {

            var fileName = $"{Guid.NewGuid()}_{eventForm.File.FileName}";
            var filePath = await UploadFileToGoogleCloudStorage(fileName, eventForm.File.OpenReadStream());

            if (string.IsNullOrEmpty(filePath))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload the file to Google Cloud Storage.");
            }


            var eventItem = new Event
            {
                Name = eventForm.Name,
                EventDate = eventForm.EventDate,
                Tickets_Available = eventForm.Tickets_Available,
                Description = eventForm.Description,
                Price = eventForm.Price,
                FilePath = filePath,
                DisplayItem = true,
                EventTypeID = eventForm.EventTypeID
            };

            // Setting EarlyBirdID
            if (eventForm.EarlyBirdID == 0)
            {
                eventItem.EarlyBirdID = null;
            }
            else
            {
                eventItem.EarlyBirdID = eventForm?.EarlyBirdID;
            }

            // Setting EventTypeID
            if (eventForm.EventTypeID == 0)
            {
                eventItem.EventTypeID = null;
            }
            else
            {
                eventItem.EventTypeID = eventForm?.EventTypeID;
            }

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();


            return CreatedAtAction("GetSingleEventEntry", new { id = eventItem.EventID }, eventItem);
        }


        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            var ticketPurchases = _context.TicketPurchases
                .Where(tp => tp.EventId == id)
                .ToList();

            // Check the new conditions
            bool noTicketsPurchased = !ticketPurchases.Any();
            bool eventDatePassed = DateTime.Now > @event.EventDate;

            if (noTicketsPurchased || eventDatePassed)
            {
                try
                {
                    // Additional code like updating TicketPurchasedStatus and deleting the image

                    _context.Events.Remove(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // This will catch EF Core's exception for restricted deletes
                    return BadRequest("Event cannot be deleted as other entities are referencing it.");
                }

                return NoContent();
            }
            else
            {
                return BadRequest("Event cannot be deleted as it doesn't meet the criteria.");
            }
        }



        private async Task DeleteImageFromGoogleCloudStorage(string filePath)
        {
            try
            {
                var credential = GoogleCredential.FromFile(_configuration["GCPAuthStorageAuthFile"]);
                var storageClient = StorageClient.Create(credential);
                var bucket = storageClient.GetBucket(_bucketName);

                // Extract the object name from the file path
                var uri = new Uri(filePath);
                var objectName = uri.PathAndQuery.TrimStart('/');

                // Delete the object from Google Cloud Storage
                storageClient.DeleteObject(bucket.Name, objectName);
            }
            catch (Exception ex)
            {
                // Log the exception here
                Console.WriteLine(ex.ToString());
            }
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventID == id);
        }

        [HttpPost("purchase/{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> PurchaseTicket(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (eventItem.Tickets_Available <= 0)
            {
                return BadRequest(new { success = false, message = "No tickets available." });
            }

            // Subtract one from the available tickets and add one to the amount sold.
            eventItem.Tickets_Available -= 1;
            eventItem.Tickets_Sold += 1;

            // Calculate the price. If the amount sold is less than or equal to the Early Bird limit, apply the discount.
            var earlyBird = await _context.EarlyBird.FindAsync(eventItem.EarlyBirdID);
            var price = eventItem.Price;

            if (earlyBird != null && eventItem.Tickets_Sold <= earlyBird.Limit)
            {
                price *= (1 - earlyBird.Percentage / 100);
            }

            // You would typically create an order or transaction record here and return the price to the client.

            _context.Entry(eventItem).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, price = price });
        }


        [HttpPut("display-toggle/{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> UpdateEventDisplay(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Toggle the EventDisplay attribute
            eventItem.DisplayItem = !eventItem.DisplayItem;

            _context.Entry(eventItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
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
