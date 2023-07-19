using Microsoft.Build.Graph;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace API.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Title { get; set; }

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

        [Required]
        public string Gender { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^(?=.*([A-Z]){1,})(?=.*[!@#$&*]{1,})(?=.*[0-9]{1,})(?=.*[a-z]{1,}).{8,100}$", ErrorMessage = "Password must consist of the following: 1 Uppercase, 1 lowercase, 1 digit, 1 Non-alphanumeric character, your password must also be at least 8 characters long")]
        public string Password { get; set; }

        [Required]
        public bool EnableTwoFactorAuth { get; set; }
    }
}

