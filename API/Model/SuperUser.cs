using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class SuperUser
    {
        [Key]
        [Required]
        public int SuperUserID { get; set; }

        [MaxLength(50)]
        [Required]
        public string First_Name { get; set; }

        [MaxLength(50)]
        [Required]
        public string Last_Name { get; set; }

        [MaxLength(50)]
        [Required]
        public string Email { get; set; }

        // [MinLength(10)] - REMEMBER TO ADD
        [MaxLength(10)]
        [Required]
        public string Phone_Number { get; set; }

        // [MinLength(13)] - REMEMBER TO ADD
        [MaxLength(13)]
        [Required]
        public string ID_Number { get; set; }

        [Required]
        public DateTime Hire_Date { get; set; }

        public int UserID { get; set; }
        [JsonIgnore]
        public User User { get; set; }

        //public int AddressID { get; set; }
        //[JsonIgnore]
        //public Address Address { get; set; }
        [JsonIgnore]
        public ICollection<Employee> Employees { get; set; }
        [JsonIgnore]
        public ICollection<EventLocation> EventLocations { get; set; }
    }
}
