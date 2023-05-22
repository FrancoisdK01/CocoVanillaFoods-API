using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    //Add Model for VAT

    public class VAT
    {
        [Key]
        public int VATID { get; set; }

        public double Percentage { get; set; }

        public DateTime Date { get; set; }
    }
}
