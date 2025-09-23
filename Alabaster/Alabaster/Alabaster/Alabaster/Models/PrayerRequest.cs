namespace Alabaster.Models
{
    public class PrayerRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string Name { get; set; }
        public string Request { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
