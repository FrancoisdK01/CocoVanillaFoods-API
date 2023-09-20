using API.Model;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class SystemPrivilegeViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
        public List<ControllerMethodMapping> ControllerMethods { get; set; }
    }
}
