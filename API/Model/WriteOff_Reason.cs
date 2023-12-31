﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class WriteOff_Reason
    {
        [Key]
        public int WriteOff_ReasonID { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public int TimesUsed { get; set; }

        public int BottelsLost { get; set; }
        
    }
}
