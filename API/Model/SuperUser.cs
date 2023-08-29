using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class SuperUser : IdentityUser
    {
        [MaxLength(50)]
        [Required]
        public string First_Name { get; set; }

        [MaxLength(50)]
        [Required]
        public string Last_Name { get; set; }

        [Required]
        [RegularExpression("^[0-9]{13}$", ErrorMessage = "ID number must be a 13-digit number")]
        public string ID_Number { get; set; }

        [Required]
        public DateTime Hire_Date { get; set; }

        [ForeignKey("Id")]
        public string UserID { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [JsonIgnore]
        public ICollection<Employee> Employees { get; set; }
        //[JsonIgnore]
        //public ICollection<EventLocation> EventLocations { get; set; }
    }
}
