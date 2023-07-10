using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class LoginUpdateViewModel
    {
        [Required]
        public string UserName{ get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewEmail { get; set; }
        
        [Required]
        [RegularExpression("^(?=.*([A-Z]){1,})(?=.*[!@#$&*]{1,})(?=.*[0-9]{1,})(?=.*[a-z]{1,}).{8,100}$", ErrorMessage = "Password must consist of the following: 1 Uppercase, 1 lowercase, 1 digit, 1 Non-alphanumeric character, your password must also be at least 6 characters long")]
        public string NewPassword { get; set; }

        [Required]
        [RegularExpression("^(?=.*([A-Z]){1,})(?=.*[!@#$&*]{1,})(?=.*[0-9]{1,})(?=.*[a-z]{1,}).{8,100}$", ErrorMessage = "Password must consist of the following: 1 Uppercase, 1 lowercase, 1 digit, 1 Non-alphanumeric character, your password must also be at least 6 characters long")]
        public string ConfirmPassword { get; set; }
    }
}
