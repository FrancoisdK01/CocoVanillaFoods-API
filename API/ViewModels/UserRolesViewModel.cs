namespace API.ViewModels
{
    public class UserRolesViewModel
    {
        public string UserEmail { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsEmployee { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperUser { get; set; }
    }
}
