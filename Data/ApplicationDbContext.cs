using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeliStarter.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Vendor> Vendors { get; set; } = null!;
        public DbSet<Advert> Adverts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // configure Vendor PK and relationship to Adverts
            builder.Entity<Vendor>()
                .HasKey(v => v.Id);

            builder.Entity<Vendor>()
                .HasMany(v => v.Adverts)
                .WithOne(a => a.Vendor)
                .HasForeignKey(a => a.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            // optional: small index to speed lookups
            builder.Entity<Vendor>().HasIndex(v => v.ApplicationUserId);
        }
    }
}
