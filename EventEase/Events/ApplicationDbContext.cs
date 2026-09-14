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
        protected ApplicationDbContext()
        {
        }
    }
}