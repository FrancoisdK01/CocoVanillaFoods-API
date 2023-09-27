

using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    public class BackupTimer
    {
        [Key]
        public int Id { get; set; }
        public DateTime LastBackup { get; set; }
    }
}
