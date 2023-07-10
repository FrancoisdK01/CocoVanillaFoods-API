using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Customer : IdentityUser
    {
        [Required]
        [ForeignKey("User")]
        public string UserID { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }

        [Required]
        [MaxLength(5)]
        public string Title { get; set; }

        [Required]
        [MaxLength(50)]
        public string First_Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Last_Name { get; set; }

        [Required]
        [RegularExpression("^[0-9]{13}$", ErrorMessage = "ID number must be a 13-digit number")]
        public string ID_Number { get; set; }

        [Required]
        [MaxLength(15)]
        public string Gender { get; set; }

        [Required]
        public DateTime Date_Created { get; set; }

        [Required]
        public DateTime Date_of_last_update { get; set; }

        [JsonIgnore]
        public Wishlist Wishlist { get; set; }
        [JsonIgnore]
        public List<Order> Orders { get; set; }
        [JsonIgnore]
        public List<Booking> Bookings { get; set; }
        [JsonIgnore]
        public List<EventReview> EventReviews { get; set; }
    }
}
