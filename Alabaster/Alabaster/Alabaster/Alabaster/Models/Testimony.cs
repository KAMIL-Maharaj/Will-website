namespace Alabaster.Models
{
    public class Testimony
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageBase64 { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public bool IsApproved { get; set; } // ✅ Admin approval flag
    }
}
