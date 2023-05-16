using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }

        [MaxLength(50)]
        public string First_Name { get; set; }

        [MaxLength(50)]
        public string Last_Name { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(10)]
        public string Phone_Number { get; set; }

        [MaxLength(13)]
        public string ID_Number { get; set; }

        public DateTime Hire_Date { get; set; }

        public int UserID { get; set; }
        [JsonIgnore]
        public User User { get; set; }

        public int SuperUserID { get; set; }
        [JsonIgnore]
        public SuperUser SuperUser { get; set; }
        //public int AddressID { get; set; }

        //[JsonIgnore]
        //public Address Address { get; set; }

        [JsonIgnore]
        public ICollection<Wine> Wines { get; set; }
        [JsonIgnore]
        public ICollection<WriteOff> WriteOffs { get; set; }
        [JsonIgnore]
        public ICollection<Event> Events { get; set; }
    }
}
