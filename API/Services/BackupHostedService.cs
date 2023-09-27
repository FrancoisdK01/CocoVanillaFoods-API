using API.Data;
using API.Model;

namespace API.Services
{
    public class BackupHostedService : IHostedService, IDisposable
    {
        private readonly IBackupService backupService;
        private readonly IServiceProvider serviceProvider;
        private Timer timer;

        public BackupHostedService(IBackupService backupService, IServiceProvider serviceProvider)
        {
            this.backupService = backupService;
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var backupTimer = context.Set<TimerFrequency>().FirstOrDefault();
                timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(backupTimer.HourFrequency));
            }// Check every hour
            return Task.CompletedTask;
        }

        private void DoWork(object state)
       {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();

                var backupTimer = context.Set<BackupTimer>().FirstOrDefault(); // Assuming there's only one entry
                var timerFreq = context.Set<TimerFrequency>().FirstOrDefault();
                if (backupTimer == null)
                {
                    Console.WriteLine("BackupTimer", backupTimer);
                    return;
                }

                var lastBackup = backupTimer.LastBackup;
                Console.WriteLine("lastBackup", lastBackup);
                var nextBackupTime = lastBackup.AddHours(timerFreq.HourFrequency);
                Console.WriteLine("nextBackupTimer", nextBackupTime);

                if (DateTime.Now >= nextBackupTime)
                {
                    backupService.CreateBackupAsync().Wait();
                    backupTimer.LastBackup = DateTime.Now;
                    context.SaveChanges();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
