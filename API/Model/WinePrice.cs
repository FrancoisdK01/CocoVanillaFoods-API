namespace API.Model;
using System.ComponentModel.DataAnnotations;

public class WinePrice
{
    [Key]
    public int WinePriceID { get; set; }

    public double Amount { get; set; }

    public DateTime Date { get; set; }
    public virtual Wine Wine { get; set; }
}

