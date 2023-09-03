using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class WriteOff
    {
        [Key]
        public int WriteOffID { get; set; }

        public string WriteOff_Reason { get; set; }

        public DateTime WriteOff_Date { get; set; }

        public string WineName { get; set; }

        public int Quantity { get; set; }
    }
}
