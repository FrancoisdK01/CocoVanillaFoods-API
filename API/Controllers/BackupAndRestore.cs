﻿using API.Data;
using API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackupAndRestore : ControllerBase
    {
        public readonly MyDbContext _context;
        public readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public BackupAndRestore(MyDbContext context, UserManager<User> userManager, IConfiguration config)
        {
            _config = config;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("BackupDatabase")]
        public IActionResult Backup()
        {
            string connectionString = _config.GetConnectionString("MyConnection");
            string databaseName = "Promenade";
            string dateTimeString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupFileName = $"{databaseName}_backup_{dateTimeString}.bak";
            string backupFilePath = Path.Combine("C:\\", backupFileName);


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = String.Format("BACKUP DATABASE {0} TO DISK='{1}'", databaseName, backupFilePath);
                    try
                    {
                        command.ExecuteNonQuery();
                        connection.Close();
                        return Ok("Backup successfully created at " + backupFilePath);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error creating backup: " + ex.Message);
                    }
                }
            }
        }

        [HttpPost]
        [Route("RestoreDatabase")]
        public IActionResult RestoreDatabase(string backupFilePath)
        {
            string connectionString = _config.GetConnectionString("MyConnection");
            string databaseName = "Promenade";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;

                        // Set to single user mode
                        command.CommandText = $"USE master; ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                        command.ExecuteNonQuery();

                        if (command.ExecuteNonQuery() > 0)
                        {
                            Console.WriteLine(command.CommandText);
                        }

                        // Perform the restore operation
                        command.CommandText = $"RESTORE DATABASE {databaseName} FROM DISK='{backupFilePath}' WITH REPLACE";
                        command.ExecuteNonQuery();

                        // Set back to multi-user mode
                        command.CommandText = $"ALTER DATABASE {databaseName} SET MULTI_USER;";
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }

                // Return success response
                return Ok("Database successfully restored.");
            }
            catch (Exception ex)
            {
                // Log the exception message to the console (consider using a logging framework)
                Console.WriteLine("Error restoring database: " + ex.Message);

                // Return error response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("UpdateTimer")]
        public async Task<IActionResult> UpdateTimer(int id, int timerFrequency)
        {
            var timerData = await _context.TimerFrequency.FirstOrDefaultAsync(x => x.Id == id);
            if (timerData == null)
            {
                return BadRequest();
            }

            timerData.HourFrequency = timerFrequency;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(timerData);
        }

    }
}
