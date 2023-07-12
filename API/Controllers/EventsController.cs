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
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromForm] EventFormViewModel eventForm)
        {
            var filePath = await UploadFileToGoogleCloudStorage(eventForm.File.FileName, eventForm.File.OpenReadStream());

            if (string.IsNullOrEmpty(filePath))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload the file to Google Cloud Storage.");
            }

            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            eventItem.EventName = eventForm.EventName;
            eventItem.EventDate = eventForm.EventDate;
            eventItem.Tickets_Available = eventForm.Tickets_Available;
            eventItem.Description = eventForm.Description;
            eventItem.EventPrice = eventForm.EventPrice;
            eventItem.ImagePath = filePath;

            _context.Entry(eventItem).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent([FromForm] EventFormViewModel eventForm)
        {
            var fileName = $"{Guid.NewGuid()}_{eventForm.File.FileName}";
            var filePath = await UploadFileToGoogleCloudStorage(fileName, eventForm.File.OpenReadStream());

            if (string.IsNullOrEmpty(filePath))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload the file to Google Cloud Storage.");
            }

            var eventItem = new Event
            {
                EventName = eventForm.EventName,
                EventDate = eventForm.EventDate,
                Tickets_Available = eventForm.Tickets_Available,
                Description = eventForm.Description,
                EventPrice = eventForm.EventPrice,
                ImagePath = filePath,
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEvent", new { id = eventItem.EventID }, eventItem);
        }


        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventID == id);
        }
    }
}
