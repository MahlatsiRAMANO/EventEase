using EventEase.Events;
using EventEase.Models;
using EventEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(BookingFilter filter)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.EventType)
                .ApplyFilters(filter)
                .OrderBy(b => b.EventDate)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            ViewBag.Filter = filter;
            await LoadEventTypes();

            return View(bookings);
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create(int? venueId)
        {
            if (venueId == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == venueId);

            if (venue == null)
            {
                return NotFound();
            }

            if (!venue.IsAvailable)
            {
                TempData["ErrorMessage"] =
                    $"{venue.Name} is currently unavailable for bookings.";

                return RedirectToAction("Index", "Venues");
            }

            ViewBag.Venue = venue;
            await LoadEventTypes();

            var booking = new Booking
            {
                VenueId = venue.Id,
                EventDate = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(12, 0, 0)
            };

            return View(booking);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == booking.VenueId);

            if (venue == null)
            {
                return NotFound();
            }

            if (!venue.IsAvailable)
            {
                ModelState.AddModelError(
                    "",
                    "This venue is currently unavailable for bookings.");
            }

            if (booking.EventTypeId == null)
            {
                ModelState.AddModelError(
                    "EventTypeId",
                    "Please select an event type.");
            }
            else if (!await _context.EventTypes.AnyAsync(et => et.Id == booking.EventTypeId))
            {
                ModelState.AddModelError(
                    "EventTypeId",
                    "Please select a valid event type.");
            }

            if (booking.NumberOfGuests > venue.Capacity)
            {
                ModelState.AddModelError(
                    "NumberOfGuests",
                    $"This venue can only accommodate {venue.Capacity} guests.");
            }

            if (booking.EventDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "EventDate",
                    "The event date cannot be in the past.");
            }

            if (booking.EndTime <= booking.StartTime)
            {
                ModelState.AddModelError(
                    "EndTime",
                    "End time must be after start time.");
            }

            var alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.VenueId == booking.VenueId &&
                b.EventDate.Date == booking.EventDate.Date &&
                booking.StartTime < b.EndTime &&
                booking.EndTime > b.StartTime
            );

            if (alreadyBooked)
            {
                ModelState.AddModelError(
                    "",
                    "This venue is already booked for the selected date and time.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Venue = venue;
                await LoadEventTypes();
                return View(booking);
            }

            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Confirmation),
                new { id = booking.Id });
        }

        // GET: Bookings/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.EventType)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Venue = booking.Venue;

            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == booking.VenueId);

            if (venue == null)
            {
                return NotFound();
            }

            if (booking.NumberOfGuests > venue.Capacity)
            {
                ModelState.AddModelError(
                    "NumberOfGuests",
                    $"This venue can only accommodate {venue.Capacity} guests.");
            }

            if (booking.EventDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "EventDate",
                    "The event date cannot be in the past.");
            }

            if (booking.EndTime <= booking.StartTime)
            {
                ModelState.AddModelError(
                    "EndTime",
                    "End time must be after start time.");
            }

            var alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.Id != booking.Id &&
                b.VenueId == booking.VenueId &&
                b.EventDate.Date == booking.EventDate.Date &&
                booking.StartTime < b.EndTime &&
                booking.EndTime > b.StartTime
            );

            if (alreadyBooked)
            {
                ModelState.AddModelError(
                    "",
                    "This venue is already booked for the selected date and time.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Venue = venue;
                return View(booking);
            }

            try
            {
                _context.Update(booking);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings
                .FindAsync(id);

            if (booking != null)
            {
                _context.Bookings.Remove(booking);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(b => b.Id == id);
        }

        private async Task LoadEventTypes()
        {
            ViewBag.EventTypes = await _context.EventTypes
                .OrderBy(et => et.Name)
                .ToListAsync();
        }
    }
}
