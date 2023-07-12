using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }
}
