﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using Microsoft.AspNetCore.Hosting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WinesController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public WinesController(MyDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: api/Wines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Wine>>> GetWines()
        {
            return await _context.Wines.ToListAsync();
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

            // Fetch the WineType and Varietal entities based on the provided IDs
            wine.WineType = await _context.WineTypes.FindAsync(wine.WineTypeID);
            wine.Varietal = await _context.Varietals.FindAsync(wine.VarietalID);

            if (wine.ImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(wine.ImageFile.FileName);
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await wine.ImageFile.CopyToAsync(fileStream);
                }

                // Save the file path (or URL if it's a public folder) in the ImageUrl property
                wine.ImageUrl = fileName;
            }

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
        public async Task<ActionResult<Wine>> PostWine([FromForm] Wine wine, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                // Process and save the image file to a designated folder
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                wine.ImageUrl = fileName;
            }

            // Fetch the WineType and Varietal entities based on the provided IDs
            wine.WineType = await _context.WineTypes.FindAsync(wine.WineTypeID);
            wine.Varietal = await _context.Varietals.FindAsync(wine.VarietalID);

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
