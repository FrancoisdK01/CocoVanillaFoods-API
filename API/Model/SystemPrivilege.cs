using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class SystemPrivilege
    {
        [Key]
        public string Id { get; set; } // This property will act as the primary key

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

    }

}
