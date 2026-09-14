using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int VenueId { get; set; }

        public Venue? Venue { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = "";

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(1, 10000)]
        public int NumberOfGuests { get; set; }

        public string? ImageUrl { get; set; }
    }
}