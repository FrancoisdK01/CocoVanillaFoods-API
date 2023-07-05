using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class EventLocation
    {
        [Key]
        public int EventLocationID { get; set; }

        [MaxLength(100)]
        public string Street_Address { get; set; }

        [MaxLength(4)]
        public string Postal_Code { get; set; }

        public DateTime Date_of_last_update { get; set; }

        public Event Event { get; set; }

        //[ForeignKey("SuperUser")]
        //public int? SuperUserID { get; set; }
        //public SuperUser SuperUser { get; set; }
    }
}
