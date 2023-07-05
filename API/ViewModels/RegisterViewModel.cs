using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^(?=.*([A-Z]){1,})(?=.*[!@#$&*]{1,})(?=.*[0-9]{1,})(?=.*[a-z]{1,}).{8,100}$", ErrorMessage = "Password must constist of the following: 1 Uppercase, 1 lowercase, 1 digit, 1 Non-alphanumeric character, your password must also be at least 6 characters long")]
        public string Password { get; set; }
    }
}
