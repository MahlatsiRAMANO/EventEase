using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class VenueEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Venue Name")]
        public string Name { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = "";

        [StringLength(100)]
        public string? Description { get; set; }

        [Range(1, 10000)]
        public int Capacity { get; set; }

        [Display(Name = "Venue is available for bookings")]
        public bool IsAvailable { get; set; }

        // Shown on the page only, never changed by the form
        public string? ImageFileName { get; set; }
    }
}
