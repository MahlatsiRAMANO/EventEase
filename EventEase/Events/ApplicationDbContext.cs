using EventEase.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Events
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<EventType> EventTypes { get; set; }
        protected ApplicationDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Predefined event type categories used for filtering bookings.
            modelBuilder.Entity<EventType>().HasData(
                new EventType { Id = 1, Name = "Wedding" },
                new EventType { Id = 2, Name = "Conference" },
                new EventType { Id = 3, Name = "Birthday Party" },
                new EventType { Id = 4, Name = "Corporate Event" },
                new EventType { Id = 5, Name = "Concert" },
                new EventType { Id = 6, Name = "Workshop" },
                new EventType { Id = 7, Name = "Other" });
        }
    }
}
