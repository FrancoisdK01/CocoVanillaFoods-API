using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;



namespace API.Model
{
    public class ShopItem
    {
        //[Key]
        //public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public string FilePath { get; set; }

        public bool DisplayItem { get; set; }

        public double Price { get; set; }


    }
}
