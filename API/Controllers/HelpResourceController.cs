﻿using API.Data;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.EntityFrameworkCore;
using API.Model;
using System.Diagnostics;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpResourceController : ControllerBase
    {
        private readonly MyDbContext _context;

        public HelpResourceController(MyDbContext context)
        {
            _context = context;
        }

        // Endpoint to retrieve video
        [HttpGet("getHelpPaths")]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<HelpResource>>> HelpPaths()
        {
            return await _context.HelpResources.ToListAsync();
        }
    }
}
