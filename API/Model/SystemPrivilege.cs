using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class SystemPrivilege : IdentityRole
    {
        //[Key]
        //public int SystemPrivilegeID { get; set; }
        //public string UserID { get; set; }

        //[ForeignKey("UserID")]
        //[JsonIgnore]
        //public User User { get; set; }

        public string RoleId { get; set; }

        //[MaxLength(50)]
        //public string Privilege_Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }
    }
}
