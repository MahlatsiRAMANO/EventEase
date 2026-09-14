using EventEase.Events;
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
        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venues
                .OrderBy(v => v.Name)
                .ToListAsync();

            return View(venues);
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
    }
}

