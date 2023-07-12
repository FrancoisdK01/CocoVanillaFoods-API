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

        [Required]
        [MaxLength(50)]
        public string First_Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Last_Name { get; set; }

        //[MaxLength(50)]
        //public string Email { get; set; }

        // [MinLength(10)] - REMEMBER TO ADD
        //[MaxLength(10)]
        //public string Phone_Number { get; set; }

        // [MinLength(13)] - REMEMBER TO ADD
        [Required]
        [RegularExpression("^[0-9]{13}$", ErrorMessage = "ID number must be a 13-digit number")]
        public string ID_Number { get; set; }

        [Required]
        public DateTime Hire_Date { get; set; }

        [Required]
        [ForeignKey("Id")]
        public string UserId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [Required]
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
