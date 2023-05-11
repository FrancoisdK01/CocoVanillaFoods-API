using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Address
    {
        //Love you

        [Key]
        public int AddressID { get; set; }

        [MaxLength(100)]
        public string Street_Address { get; set; }

        [MaxLength(4)]
        public string Postal_Code { get; set; }

        public DateTime Date_of_last_update { get; set; }

        public string City { get; set; }

        [JsonIgnore]
        [ForeignKey("CustomerID")]
        public int? CustomerID { get; set; }
        public Customer Customer { get; set; }

        [ForeignKey("SuperUser")]
        public int SuperUserID { get; set; }
        public SuperUser SuperUser { get; set; }

        [ForeignKey("EmployeeID")]
        public int? EmployeeID { get; set; }
        public Employee Employee { get; set; }

        [ForeignKey("ProvinceID")]
        public int? ProvinceID { get; set; }
        public Province Province { get; set; }

        [JsonIgnore]
        public EventLocation EventLocation { get; set; }
    }
}
