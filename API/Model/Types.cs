using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    public class Types
    {
        

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }
    }
}
