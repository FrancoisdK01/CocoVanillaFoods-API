using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using Microsoft.AspNetCore.Hosting;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WinesController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;
        private readonly IWebHostEnvironment _environment;


        public WinesController(IConfiguration configuration, IWebHostEnvironment environment, MyDbContext dbContext)
        {
            _configuration = configuration;
            _environment = environment;
            _bucketName = _configuration["GoogleCloudStorageBucketName"];
            _context = dbContext;
        }

        private async Task<string> UploadFileToGoogleCloudStorage(string fileName, Stream fileStream)
        {
            try
            {
                var credential = GoogleCredential.FromFile(_configuration["GCPAuthStoargeAuthFile"]);
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

        // GET: api/Wines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Wine>>> GetWines()
        {
            var wines = await _context.Wines.ToListAsync();

            return wines;
        }

        // GET: api/Wines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Wine>> GetWine(int id)
        {
            var wine = await _context.Wines.FindAsync(id);

            if (wine == null)
            {
                return NotFound();
            }

            return wine;
        }

        // PUT: api/Wines/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWine(int id, [FromForm] Wine wine)
        {
            if (id != wine.WineID)
            {
                return BadRequest();
            }

            wine.WineType = await _context.WineTypes.FindAsync(wine.WineTypeID);
            wine.Varietal = await _context.Varietals.FindAsync(wine.VarietalID);

            _context.Entry(wine).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WineExists(id))
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

        // POST: api/Wines
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Wine>> PostWine([FromForm] WineFormViewModel wineForm)
        {
            var fileName = $"{Guid.NewGuid()}_{wineForm.File.FileName}";
            var filePath = await UploadFileToGoogleCloudStorage(fileName, wineForm.File.OpenReadStream());

            if (string.IsNullOrEmpty(filePath))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload the file to Google Cloud Storage.");
            }

            var wine = new Wine
            {
                Name = wineForm.Name,
                Description = wineForm.Description,
                Vintage = wineForm.Vintage,
                RestockLimit = wineForm.RestockLimit,
                WineTastingNote = wineForm.WineTastingNote,
                WinePrice = wineForm.WinePrice,
                WineTypeID = wineForm.WineTypeID,
                VarietalID = wineForm.VarietalID,
                FilePath = filePath,
            };

            _context.Wines.Add(wine);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWine", new { id = wine.WineID }, wine);
        }


        // DELETE: api/Wines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWine(int id)
        {
            var wine = await _context.Wines.FindAsync(id);
            if (wine == null)
            {
                return NotFound();
            }

            _context.Wines.Remove(wine);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WineExists(int id)
        {
            return _context.Wines.Any(e => e.WineID == id);
        }

        
    }

}
