using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class UserRolesViewModel
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsEmployee { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperUser { get; set; }
    }
}
