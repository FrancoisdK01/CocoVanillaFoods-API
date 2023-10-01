using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading.Tasks;
using System;
using API.Model;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DamageImagesController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;

        public DamageImagesController(IConfiguration configuration, MyDbContext dbContext)
        {
            _configuration = configuration;
            _bucketName = _configuration["GoogleCloudStorageBucketName"];
            _context = dbContext;
        }

        // Your existing UploadFileToGoogleCloudStorage code here

        [HttpPost]
        public async Task<ActionResult> UploadDamageImage([FromForm] IFormFile file)
        {
           

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = await UploadFileToGoogleCloudStorage(fileName, file.OpenReadStream());

            if (string.IsNullOrEmpty(filePath))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload the file to Google Cloud Storage.");
            }

            var damageImage = new DamageImage
            {
                FilePath = filePath,
                // Additional fields like UserId, Description, Timestamp, etc.
            };

            _context.DamageImages.Add(damageImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(UploadDamageImage), new { id = damageImage.Id }, damageImage);
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DamageImage>>> GetAllDamageImages()
        {
            var images = await _context.DamageImages.ToListAsync();
            return Ok(images);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDamageImage(int id)
        {
            var damageImage = await _context.DamageImages.FindAsync(id);
            if (damageImage == null)
            {
                return NotFound();
            }

            // Delete the image from Google Cloud Storage
            if (!string.IsNullOrEmpty(damageImage.FilePath))
            {
                await DeleteImageFromGoogleCloudStorage(damageImage.FilePath);
            }

            _context.DamageImages.Remove(damageImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

