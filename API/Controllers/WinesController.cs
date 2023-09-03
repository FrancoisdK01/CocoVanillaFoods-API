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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Data;

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

        private async Task<string> UploadFileToGoogleCloudStorage(string fileName, Stream fileStream, string previousFilePath = null)
        {

            // Delete the previous image if there is one
            if (!string.IsNullOrEmpty(previousFilePath))
            {
                await DeleteImageFromGoogleCloudStorage(previousFilePath);
            }

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

        // GET: api/Wines
        [HttpGet]
        [Authorize(Roles = "Admin, Superuser, Customer")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Wine>>> GetWines()
        {
            var wines = await _context.Wines.ToListAsync();

            return wines;
        }


        [HttpGet]
        [Route("CustomerWines")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Wine>>> GetWinesForCustomers()
        {
            var wines = await _context.Wines.ToListAsync();

            return wines;
        }

        // GET: api/Wines/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Superuser, Customer")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
        [Authorize(Roles = "Admin, Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutWine(int id, [FromForm] WineFormViewModel wineForm)
        {
            if (id != wineForm.WineID)
            {
                return BadRequest();
            }

            // Get the current wine from the database
            var wineToUpdate = await _context.Wines.FindAsync(id);
            if (wineToUpdate == null)
            {
                return NotFound();
            }

            // If a file is included, process and update the image
            if (wineForm.File != null)
            {
                await DeleteImageFromGoogleCloudStorage(wineForm.File.FileName);

                var fileName = $"{Guid.NewGuid()}_{wineForm.File.FileName}";
                var filePath = await UploadFileToGoogleCloudStorage(fileName, wineForm.File.OpenReadStream(), wineToUpdate.FilePath);

                if (string.IsNullOrEmpty(filePath))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload the file to Google Cloud Storage.");
                }

                wineToUpdate.FilePath = filePath;
            }

            // Map other properties from the form to the wine object
            wineToUpdate.Name = wineForm.Name;
            //wineToUpdate.Description = wineForm.Description;
            wineToUpdate.Vintage = wineForm.Vintage;
            //wineToUpdate.RestockLimit = wineForm.RestockLimit;
            wineToUpdate.WineTastingNote = wineForm.WineTastingNote;
            wineToUpdate.Price = wineForm.Price;
            wineToUpdate.WineTypeID = wineForm.WineTypeID;
            wineToUpdate.VarietalID = wineForm.VarietalID;
            wineToUpdate.DisplayItem = wineForm.DisplayItem;

            _context.Entry(wineToUpdate).State = EntityState.Modified;

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
        [Authorize(Roles = "Admin, Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
                //Description = wineForm.Description,
                Vintage = wineForm.Vintage,
                //RestockLimit = wineForm.RestockLimit,
                WineTastingNote = wineForm.WineTastingNote,
                Price = wineForm.Price,
                DisplayItem = wineForm.DisplayItem,
                WineTypeID = wineForm.WineTypeID,
                VarietalID = wineForm.VarietalID,
                FilePath = filePath,
            };

            var winePrice = new WinePrice
            {
                Amount = wineForm.Price,
                Date = DateTime.Now,
                Wine = wine
            };

            _context.Wines.Add(wine);
            _context.WinePrice.Add(winePrice);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWine", new { id = wine.WineID }, wine);
        }


        // DELETE: api/Wines/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteWine(int id)
        {
            var wine = await _context.Wines.FindAsync(id);
            if (wine == null)
            {
                return NotFound();
            }

            // Delete the image from Google Cloud Storage
            if (!string.IsNullOrEmpty(wine.FilePath))
            {
                await DeleteImageFromGoogleCloudStorage(wine.FilePath);
            }

            _context.Wines.Remove(wine);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin, Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        private async Task DeleteImageFromGoogleCloudStorage(string filePath)
        {
            try
            {
                var credential = GoogleCredential.FromFile(_configuration["GCPAuthStorageAuthFile"]);
                var storageClient = StorageClient.Create(credential);
                var bucket = storageClient.GetBucket(_bucketName);

                // Extract the object name from the file path
                var uri = new Uri(filePath);
                var objectNameParts = uri.LocalPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);
                var finalObjectName = string.Join("/", objectNameParts);

                Console.WriteLine("Deleting object: " + finalObjectName); // Log the object name

                // Delete the object from Google Cloud Storage
                await storageClient.DeleteObjectAsync(bucket.Name, finalObjectName); // Use the correct object name
            }
            catch (Exception ex)
            {
                // Log the exception here
                Console.WriteLine("An error occurred while deleting the image from Google Cloud Storage:");
                Console.WriteLine(ex.ToString());
            }
        }

        [Authorize(Roles = "Admin, Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        private bool WineExists(int id)
        {
            return _context.Wines.Any(e => e.WineID == id);
        }

        
    }

}
