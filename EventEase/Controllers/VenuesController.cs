using EventEase.Events;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Venues
        public async Task<IActionResult> Index(bool availableOnly = false)
        {
            var query = _context.Venues.AsQueryable();

            // Optionally show only the venues that are open for bookings
            if (availableOnly)
            {
                query = query.Where(v => v.IsAvailable);
            }

            var venues = await query
                .OrderBy(v => v.Name)
                .ToListAsync();

            ViewBag.AvailableOnly = availableOnly;

            return View(venues);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(ToViewModel(venue));
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VenueEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.ImageFileName = venue.ImageFileName;
                return View(model);
            }

            venue.Name = model.Name.Trim();
            venue.Location = model.Location.Trim();
            venue.Description = model.Description?.Trim() ?? "";
            venue.Capacity = model.Capacity;
            venue.IsAvailable = model.IsAvailable;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{venue.Name} was updated and is now marked as " +
                (venue.IsAvailable ? "available." : "unavailable.");

            return RedirectToAction(nameof(Index));
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null)
            {
                TempData["ErrorMessage"] =
                    "The venue could not be found.";

                return RedirectToAction(nameof(Index));
            }

            // Check if the venue has any bookings
            var hasBookings = await _context.Bookings
                .AnyAsync(b => b.VenueId == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] =
                    "This venue cannot be deleted because it has an existing booking.";

                return RedirectToAction(nameof(Index));
            }

            _context.Venues.Remove(venue);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Venue deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private static VenueEditViewModel ToViewModel(Venue venue)
        {
            return new VenueEditViewModel
            {
                Id = venue.Id,
                Name = venue.Name,
                Location = venue.Location,
                Description = venue.Description,
                Capacity = venue.Capacity,
                IsAvailable = venue.IsAvailable,
                ImageFileName = venue.ImageFileName
            };
        }
    }
}

