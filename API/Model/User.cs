using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [MaxLength(50)]
        public string UserEmail { get; set; }

        [MaxLength(50)]
        public string UserPassword { get; set; }

        [JsonIgnore]
        public Customer Customer { get; set; }
        [JsonIgnore]
        public SuperUser SuperUser { get; set; }
        [JsonIgnore]
        public Employee Employee { get; set; }

        [JsonIgnore]
        public ICollection<SystemPrivilege> SystemPrivileges { get; set; }

    }
}