namespace API.Model
{
    public class EventFormViewModel
    {

        public string EventName { get; set; }

        public DateTime EventDate { get; set; }

        public int Tickets_Available { get; set; }

        public string Description { get; set; }

        public int EventPrice { get; set; }

        public IFormFile ImagePath { get; set; }
    }
}
