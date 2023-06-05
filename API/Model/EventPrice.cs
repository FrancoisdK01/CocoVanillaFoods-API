using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace API.Model
{
    public class EventPrice
    {
        [Key]
        public int EventPriceID { get; set; }

        public int Amount { get; set; }

        public DateTime Date { get; set; }


       
        public virtual Event Event { get; set; }
    }
}
