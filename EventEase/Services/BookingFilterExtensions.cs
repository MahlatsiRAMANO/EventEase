using EventEase.Models;

namespace EventEase.Services
{
    public static class BookingFilterExtensions
    {
        /// <summary>
        /// Applies the search term, event type, date range and venue
        /// availability filters to a bookings query.
        /// </summary>
        public static IQueryable<Booking> ApplyFilters(
            this IQueryable<Booking> query,
            BookingFilter filter)
        {
            if (filter.VenueId.HasValue)
            {
                var venueId = filter.VenueId.Value;
                query = query.Where(b => b.VenueId == venueId);
            }

            // Search by Booking ID or Event Name
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim();

                if (int.TryParse(searchTerm, out int bookingId))
                {
                    query = query.Where(b =>
                        b.Id == bookingId ||
                        b.EventName.Contains(searchTerm));
                }
                else
                {
                    query = query.Where(b =>
                        b.EventName.Contains(searchTerm));
                }
            }

            // Filter by event type
            if (filter.EventTypeId.HasValue)
            {
                var eventTypeId = filter.EventTypeId.Value;
                query = query.Where(b => b.EventTypeId == eventTypeId);
            }

            // Filter by date range (both ends included)
            var start = filter.StartDate?.Date;
            var end = filter.EndDate?.Date;

            // If the dates were entered the wrong way round, swap them
            if (start.HasValue && end.HasValue && start > end)
            {
                (start, end) = (end, start);
            }

            if (start.HasValue)
            {
                var from = start.Value;
                query = query.Where(b => b.EventDate >= from);
            }

            if (end.HasValue)
            {
                var until = end.Value.AddDays(1);
                query = query.Where(b => b.EventDate < until);
            }

            // Only bookings at venues that are currently available
            if (filter.AvailableOnly)
            {
                query = query.Where(b => b.Venue != null && b.Venue.IsAvailable);
            }

            return query;
        }
    }
}
