using EventEase.Events;
using EventEase.Models;
using EventEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AzureBlobService _blobService;

        public EventsController(
            ApplicationDbContext context,
            AzureBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }


        // GET: Events
        public async Task<IActionResult> Index(int? venueId)
        {
            var query = _context.Bookings
                .Include(b => b.Venue)
                .AsQueryable();

            if (venueId.HasValue)
            {
                query = query.Where(b => b.VenueId == venueId.Value);
            }

            var bookings = await query
                .OrderBy(b => b.EventDate)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            // Create secure URLs for private Azure Blob images
            foreach (var booking in bookings)
            {
                if (!string.IsNullOrEmpty(booking.ImageUrl))
                {
                    booking.ImageUrl = _blobService.GetImageUrl(booking.ImageUrl);
                }
            }

            ViewBag.VenueId = venueId;

            return View(bookings);
        }


        // GET: Events/Create
        public async Task<IActionResult> Create()
        {
            await LoadVenues();

            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Booking booking,
            IFormFile? imageFile)
        {
            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == booking.VenueId);

            if (venue == null)
            {
                ModelState.AddModelError(
                    "VenueId",
                    "Please select a valid venue.");
            }
            else
            {
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

                var venueAlreadyBooked = await _context.Bookings.AnyAsync(b =>
                    b.VenueId == booking.VenueId &&
                    b.EventDate.Date == booking.EventDate.Date &&
                    booking.StartTime < b.EndTime &&
                    booking.EndTime > b.StartTime
                );

                if (venueAlreadyBooked)
                {
                    ModelState.AddModelError(
                        "",
                        "This venue is already booked for the selected date and time.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadVenues();
                return View(booking);
            }

            // Upload event image to Azure Blob Storage
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName =
                    $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

                booking.ImageUrl = await _blobService.UploadImageAsync(
                    imageFile,
                    fileName);
            }

            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event created successfully!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Edit/5
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

            ViewBag.Venues = await _context.Venues
                .OrderBy(v => v.Name)
                .ToListAsync();

            return View(booking);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Booking booking,
            IFormFile? imageFile)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBooking == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == booking.VenueId);

            if (venue == null)
            {
                ModelState.AddModelError(
                    "VenueId",
                    "Please select a valid venue.");
            }
            else
            {
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

                var venueAlreadyBooked = await _context.Bookings.AnyAsync(b =>
                    b.Id != booking.Id &&
                    b.VenueId == booking.VenueId &&
                    b.EventDate.Date == booking.EventDate.Date &&
                    booking.StartTime < b.EndTime &&
                    booking.EndTime > b.StartTime
                );

                if (venueAlreadyBooked)
                {
                    ModelState.AddModelError(
                        "",
                        "This venue is already booked for the selected date and time.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Venues = await _context.Venues
                    .OrderBy(v => v.Name)
                    .ToListAsync();

                return View(booking);
            }

            existingBooking.VenueId = booking.VenueId;
            existingBooking.EventName = booking.EventName;
            existingBooking.CustomerName = booking.CustomerName;
            existingBooking.CustomerEmail = booking.CustomerEmail;
            existingBooking.EventDate = booking.EventDate;
            existingBooking.StartTime = booking.StartTime;
            existingBooking.EndTime = booking.EndTime;
            existingBooking.NumberOfGuests = booking.NumberOfGuests;

            // Upload a new image if one was selected
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName =
                    $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

                existingBooking.ImageUrl =
                    await _blobService.UploadImageAsync(
                        imageFile,
                        fileName);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Delete/5
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

        // POST: Events/Delete/5
        [HttpPost]
        [ActionName("Delete")]
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

        private async Task LoadVenues()
        {
            ViewBag.Venues = await _context.Venues
                .OrderBy(v => v.Name)
                .ToListAsync();
        }
    }
}
