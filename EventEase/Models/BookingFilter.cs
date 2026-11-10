namespace EventEase.Models
{
    /// <summary>
    /// The search and filter options the booking specialists can use
    /// on the booking listing pages. Bound from the query string.
    /// </summary>
    public class BookingFilter
    {
        public int? VenueId { get; set; }

        public string? SearchTerm { get; set; }

        public int? EventTypeId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool AvailableOnly { get; set; }

        // True when the specialist has chosen anything other than the defaults
        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(SearchTerm) ||
            EventTypeId.HasValue ||
            StartDate.HasValue ||
            EndDate.HasValue ||
            AvailableOnly;
    }
}
