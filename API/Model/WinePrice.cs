namespace API.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class WinePrice
{
    [Key]
    public int WinePriceID { get; set; }

    public double Amount { get; set; }

    public DateTime Date { get; set; }

    public int? WineID { get; set; }  // Make it nullable

    public virtual Wine Wine { get; set; }
}

