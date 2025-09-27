namespace DeliStarter.Data
{
    public class Advert
    {
        public int Id { get; set; } // PK (int is fine)
        public string VendorId { get; set; } = null!;
        public Vendor? Vendor { get; set; }

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; } = 0m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
