using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Employee : IdentityUser
    {
        //[Key]
        //public int EmployeeID { get; set; }

        [MaxLength(50)]
        public string First_Name { get; set; }

        [MaxLength(50)]
        public string Last_Name { get; set; }

        //[MaxLength(50)]
        //public string Email { get; set; }

        // [MinLength(10)] - REMEMBER TO ADD
        //[MaxLength(10)]
        //public string Phone_Number { get; set; }

        // [MinLength(13)] - REMEMBER TO ADD
        [MaxLength(13)]
        public string ID_Number { get; set; }

        public DateTime Hire_Date { get; set; }

        [ForeignKey("Id")]
        public string UserId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [ForeignKey("Id")]
        public string SuperUserID { get; set; }
        [JsonIgnore]
        public virtual SuperUser SuperUser { get; set; }

        [JsonIgnore]
        public ICollection<Wine> Wines { get; set; }
        [JsonIgnore]
        public ICollection<WriteOff> WriteOffs { get; set; }
        [JsonIgnore]
        public ICollection<Event> Events { get; set; }
    }
}
