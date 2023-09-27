using API.Data;
using Microsoft.Data.SqlClient;

namespace API.Services
{
    public class BackupService : IBackupService
    {
        private readonly string connectionString;
        private readonly string databaseName;

        public BackupService(IConfiguration configuration)
        {
            // Retrieve connection string and database name from configuration
            this.connectionString = configuration.GetConnectionString("MyConnection");
            this.databaseName = "Promenade";
        }

        public async Task CreateBackupAsync()
        {
            string backupFileName = $"{databaseName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.bak";
            string backupFilePath = Path.Combine("C:\\SQLBackups", backupFileName);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = $"BACKUP DATABASE {databaseName} TO DISK='{backupFilePath}'";

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Backup successfully created at {backupFilePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating backup: {ex.Message}");
                    }
                }
            }
        }
    }
}