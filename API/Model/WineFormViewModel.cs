namespace API.Model
{
    public class WineFormViewModel
    {
        public int WineID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Vintage { get; set; }
        public int RestockLimit { get; set; }
        public string WineTastingNote { get; set; }
        public double WinePrice { get; set; }
        public Boolean DisplayWine { get; set; }
        public int WineTypeID { get; set; }
        public int VarietalID { get; set; }
        public IFormFile File { get; set; }
    }
}
