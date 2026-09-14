using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(100)]
        public string Description { get; set; } = "";

        [MaxLength(100)]
        public string Location { get; set; } = "";

        [MaxLength(100)]
        public int Capacity { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; } = "";

    }
}
