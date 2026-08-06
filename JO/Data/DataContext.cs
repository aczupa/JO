using Bogus;
using JO.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JO.Data
{
    public class DataContext : IdentityDbContext
    {
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<UserProfile> UserProfile { get; set; }
        public DbSet<QRCodeModel> QRCodeModels { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; } 

        public DataContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Offer>().HasData(GetOffers());

          
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

        
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Offer)
                .WithMany()
                .HasForeignKey(ci => ci.OfferId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cart>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QRCodeModel>().ToTable("QRCodeModels");
        }

        private List<Offer> GetOffers()
        {
            var offers = new List<Offer>();
            var faker = new Faker("fr");

            for (int i = 1; i <= 4; i++)
            {
                var offer = new Offer
                {
                    Id = i,
                    Name = faker.Commerce.ProductName(),
                    TicketCount = faker.Random.Number(1, 100),
                    Price = faker.Random.Decimal(100, 450),
                    Description = faker.Lorem.Paragraph(),
                    ImageUrl = faker.Image.PicsumUrl()
                };

                offers.Add(offer);
            }

            return offers;
        }
    }
}
