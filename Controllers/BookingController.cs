using System.Web.Mvc;
using System;
using System.Linq;
using Wanderlust.Models;
using Wanderlust.ViewModel;
using System.Collections.Generic;


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

                // Print model values to debug console
                System.Diagnostics.Debug.WriteLine("Creating booking with values:");
                System.Diagnostics.Debug.WriteLine($"Type: {model.bk_type}");
                System.Diagnostics.Debug.WriteLine($"Trip ID: {model.trip_id}");
                System.Diagnostics.Debug.WriteLine($"Start Date: {model.travel_start_date}");
                System.Diagnostics.Debug.WriteLine($"End Date: {model.travel_end_date}");
                System.Diagnostics.Debug.WriteLine($"Travelers: {model.numtravelers}");
                System.Diagnostics.Debug.WriteLine($"Cost: {model.bk_cost}");
                System.Diagnostics.Debug.WriteLine($"User ID: {model.user_id}");
                System.Diagnostics.Debug.WriteLine($"Booking Date: {model.bk_date}");

                // IMPORTANT: Make sure booking_id is 0 so Entity Framework knows it's a new entry
                // This is likely the cause of your problem
                model.booking_id = 0;
                System.Diagnostics.Debug.WriteLine($"Booking Id: {model.booking_id}");
                System.Diagnostics.Debug.WriteLine($"Status: {model.status}");

                // Create a new booking object instead of using the model directly
                // This helps avoid any tracking issues with Entity Framework
                var booking = new BOOKING
                {
                    bk_type = model.bk_type,
                    trip_id = model.trip_id,
                    travel_start_date = model.travel_start_date,
                    travel_end_date = model.travel_end_date,
                    numtravelers = model.numtravelers,
                    bk_cost = model.bk_cost,
                    user_id = model.user_id,
                    bk_date = model.bk_date,
                    status = model.status
                    // Notice we're not setting booking_id - let the database auto-increment it
                };

                // Add the booking to the database
                db.BOOKINGs.Add(booking);

                try
                {
                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"Successfully created booking with ID: {booking.booking_id}");

                    // Store the booking ID for confirmation and redirect to payment page
                    TempData["BookingId"] = booking.booking_id;
                    TempData["SuccessMessage"] = "Your booking has been created successfully!";
                    return RedirectToAction("Payment", new { id = booking.booking_id });
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    // Handle validation errors
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }

                    TempData["ErrorMessage"] = "Validation error: " + dbEx.Message;
                    throw;
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException dbUpdateEx)
                {
                    // This will capture the REAL error message from SQL Server
                    var innerException = dbUpdateEx.InnerException;
                    while (innerException.InnerException != null)
                    {
                        innerException = innerException.InnerException;
                    }

                    System.Diagnostics.Debug.WriteLine($"ACTUAL DATABASE ERROR: {innerException.Message}");
                    TempData["ErrorMessage"] = "Database error: " + innerException.Message;
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Get the innermost exception (this will have the actual SQL error)
                var innerEx = ex;
                while (innerEx.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }

                System.Diagnostics.Debug.WriteLine($"Exception in CreateBooking: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"INNERMOST EXCEPTION: {innerEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = "An error occurred: " + innerEx.Message;

                // Redirect based on booking type
                switch (model.bk_type?.ToLower() ?? "")
                {
                    case "package":
                        return RedirectToAction("Details", "Package", new { id = model.trip_id });
                    case "destination":
                        return RedirectToAction("Details", "Destination", new { id = model.trip_id });
                    case "itinerary":
                        return RedirectToAction("Details", "Itinerary", new { id = model.trip_id });
                    default:
                        return RedirectToAction("Index", "Home");
                }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int id, PaymentViewModel model)
        {
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

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation failed. Please check the input.";
                return RedirectToAction("Payment", new { id = id });
            }

            var payment = new PAYMENT
            {
                booking_id = model.Payment.booking_id,
                payMethod = model.Payment.payMethod,
                amount = model.Payment.amount,
                payment_date = DateTime.Now
            };

            db.PAYMENTs.Add(payment);
            db.SaveChanges();

            try
            {
                booking.status = "Confirmed";
                db.SaveChanges();

                TempData["SuccessMessage"] = "Payment successful. Your booking is now confirmed.";
                return RedirectToAction("BookingSummary", new { id = booking.booking_id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your payment.";
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

            var payment = db.PAYMENTs.FirstOrDefault(p => p.booking_id == booking.booking_id);
            var viewModel = new BookingSummaryViewModel
            {
                Booking = booking,
                Payments = payment
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