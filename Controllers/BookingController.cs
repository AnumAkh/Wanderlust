using System.Web.Mvc;
using System;
using System.Linq;
using Wanderlust.Models;
using Wanderlust.ViewModel;


namespace Wanderlust.Controllers
{
    public class BookingController : Controller
    {
        // Database context
        private WanderlustEntities db = new WanderlustEntities();

        // GET: Booking
        public ActionResult Index()
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Please login to view your bookings.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // Get all bookings for the current user
            var bookings = db.BOOKINGs
                .Where(b => b.user_id == userId)
                .OrderByDescending(b => b.bk_date)
                .ToList();

            return View(bookings);
        }

        // GET: Booking/Details/5
        public ActionResult Details(int id)
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Please login to view booking details.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // Get the booking with specified ID that belongs to the current user
            var booking = db.BOOKINGs
                .FirstOrDefault(b => b.booking_id == id && b.user_id == userId);

            if (booking == null)
            {
                return HttpNotFound();
            }

            // Create view model with booking information
            var viewModel = new BookingDetailsViewModel
            {
                Booking = booking,
                Payments = booking.PAYMENTs.ToList()
            };

            // Get trip details based on booking type
            switch (booking.bk_type.ToLower())
            {
                case "package":
                    viewModel.PackageInfo = db.Packages.FirstOrDefault(p => p.pkg_id == booking.trip_id);
                    break;
                case "destination":
                    viewModel.DestinationInfo = db.DESTINATIONs.FirstOrDefault(d => d.dest_id == booking.trip_id);
                    break;
                case "itinerary":
                    viewModel.ItineraryInfo = db.ITINERARies.FirstOrDefault(i => i.itinerary_id == booking.trip_id);
                    break;
            }

            return View(viewModel);
        }

        // POST: Booking/CreateBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBooking(BOOKING model)
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Please login to make a booking.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Set the user ID from the session
                model.user_id = Convert.ToInt32(Session["UserID"]);

                // Set the booking date to current date
                model.bk_date = DateTime.Now;

                // Set initial status
                model.status = "Pending";
                
                

                // Set trip ID based on booking type
                //switch (model.bk_type?.ToLower() ?? "")
                //{
                //    case "package":
                //        model.trip_id = model.trip_id; // Ensure you get correct trip ID based on type
                //        break;
                //    case "destination":
                //        model.trip_id = model.trip_id;
                //        break;
                //    case "itinerary":
                //        model.trip_id = model.trip_id;
                //        break;
                //    default:
                //        TempData["ErrorMessage"] = "Invalid booking type.";
                //        return RedirectToAction("Index", "Home");
                //}
                //var booking = new BOOKING
                //{
                //    bk_type = model.bk_type,
                //    trip_id = model.trip_id,
                //    travel_start_date = model.travel_start_date,
                //    travel_end_date = model.travel_end_date,
                //    numtravelers = model.numtravelers,
                //    bk_cost = model.bk_cost,
                //    user_id = model.user_id,
                //    bk_date = DateTime.Now,
                //    status = "Pending"
                //};

                // Add the booking to the database
                db.BOOKINGs.Add(model);
                db.SaveChanges();

                // Store the booking ID for confirmation and redirect to payment page
                TempData["BookingId"] = model.booking_id;
                return RedirectToAction("Payment", new { id = model.booking_id });
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = "An error occurred while processing your booking. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Booking/Payment/5
        public ActionResult Payment(int id)
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Please login to confirm payment.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            var booking = db.BOOKINGs.FirstOrDefault(b => b.booking_id == id && b.user_id == userId);

            if (booking == null || booking.status != "Pending")
            {
                return HttpNotFound();
            }

            // Create view model for payment confirmation
            var viewModel = new PaymentViewModel
            {
                Booking = booking
            };

            return View(viewModel);
        }

        // POST: Booking/ConfirmPayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int id)
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Please login to confirm payment.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            var booking = db.BOOKINGs.FirstOrDefault(b => b.booking_id == id && b.user_id == userId);

            if (booking == null || booking.status != "Pending")
            {
                return HttpNotFound();
            }

            try
            {
                // Update status to "Confirmed"
                booking.status = "Confirmed";
                db.SaveChanges();

                TempData["SuccessMessage"] = "Payment successful. Your booking is now confirmed.";

                return RedirectToAction("BookingSummary", new { id = booking.booking_id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your payment. Please try again.";
                return RedirectToAction("Payment", new { id = id });
            }
        }

        // GET: Booking/BookingSummary/5
        public ActionResult BookingSummary(int id)
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Please login to view booking summary.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            var booking = db.BOOKINGs.FirstOrDefault(b => b.booking_id == id && b.user_id == userId);

            if (booking == null || booking.status != "Confirmed")
            {
                return HttpNotFound();
            }

            var viewModel = new BookingSummaryViewModel
            {
                Booking = booking,
                Payments = booking.PAYMENTs.ToList()
            };

            // Include additional trip details based on booking type
            switch (booking.bk_type.ToLower())
            {
                case "package":
                    viewModel.PackageInfo = db.Packages.FirstOrDefault(p => p.pkg_id == booking.trip_id);
                    break;
                case "destination":
                    viewModel.DestinationInfo = db.DESTINATIONs.FirstOrDefault(d => d.dest_id == booking.trip_id);
                    break;
                case "itinerary":
                    viewModel.ItineraryInfo = db.ITINERARies.FirstOrDefault(i => i.itinerary_id == booking.trip_id);
                    break;
            }

            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
