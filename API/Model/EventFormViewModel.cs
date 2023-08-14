namespace API.Model
{
    public class EventFormViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public int Tickets_Available { get; set; }
        public int Tickets_Sold { get; set; }  // Added this line
        public string Description { get; set; }
        public int EventPrice { get; set; }
        public IFormFile ImagePath { get; set; }
        public int? EarlyBirdID { get; set; }
        public Boolean DisplayEvent { get; set; }
    }
}
