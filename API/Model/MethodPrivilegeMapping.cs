using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    public class MethodPrivilegeMapping
    {
        [Key]
        public int Id { get; set; }  // Primary Key
        [MaxLength(255)]
        public string ControllerName { get; set; }

        [MaxLength(255)]
        public string MethodName { get; set; }  // Name or identifier of the method or action

        // Optional: If you want to link directly to the SystemPrivilege entity
        public string SystemPrivilegeId { get; set; }
        public SystemPrivilege SystemPrivilege { get; set; }
    }
}
