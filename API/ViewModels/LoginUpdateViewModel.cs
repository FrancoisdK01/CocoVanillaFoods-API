namespace API.ViewModels
{
    public class LoginUpdateViewModel
    {
        public string UserName{ get; set; }
        public string CurrentPassword { get; set; }
        public string NewEmail { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
