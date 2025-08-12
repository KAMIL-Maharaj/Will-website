namespace Alabaster.Models
{
    public class UpcomingEvent
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string ImageUrl { get; set; }
    }
}
