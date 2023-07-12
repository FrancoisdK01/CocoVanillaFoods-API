using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class SuperUserViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [RegularExpression("^0[0-9]{9}$", ErrorMessage = "Phone number must start with 0 and be 10 characters long")]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression("^[0-9]{13}$", ErrorMessage = "ID number must be a 13-digit number")]
        public string IDNumber { get; set; }
    }
}
