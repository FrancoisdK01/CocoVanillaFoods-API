namespace API.Model
{
    public class EventFormViewModel
    {
        public int EventID { get; set; }
        public string Name { get; set; }
        public DateTime EventDate { get; set; }
        public int Tickets_Available { get; set; }
        public int Tickets_Sold { get; set; }  // Added this line
        public string Description { get; set; }
        public int Price { get; set; }
        public IFormFile File { get; set; }
        public int? EarlyBirdID { get; set; }
        public Boolean DisplayItem { get; set; }

        public int? EventTypeID { get; set; }

    }
}
