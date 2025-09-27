namespace DeliStarter.Data
{
    public class Vendor
    {
        // Use string id to match Identity user id type
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // link to ApplicationUser (FK)
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser? ApplicationUser { get; set; }

        public string DisplayName { get; set; } = "";
        public string Bio { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = true; // change to false if you want admin approval workflow

        // adverts posted by this vendor
        public ICollection<Advert> Adverts { get; set; } = new List<Advert>();
    }
}
