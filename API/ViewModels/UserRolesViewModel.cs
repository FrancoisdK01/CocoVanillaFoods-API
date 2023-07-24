using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class UserRolesViewModel
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }
        public List<string> Privileges { get; set; } = new List<string>();
    }
}
