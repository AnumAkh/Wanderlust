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
        //GET: All Bookings
        // GET: Booking/UserBookings
        public ActionResult UserBookings()
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
                    viewModel.PackageInfo = db.PACKAGEs.FirstOrDefault(p => p.pkg_id == booking.trip_id);
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

        // GET: Booking/BookItinerary/5
        public ActionResult BookItinerary(int id)
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Please login to book an itinerary.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            // Get the itinerary
            var itinerary = db.ITINERARies.Find(id);
            if (itinerary == null)
            {
                return HttpNotFound();
            }

            // Verify the user owns this itinerary
            if (itinerary.user_id != userId)
            {
                TempData["ErrorMessage"] = "You can only book itineraries that belong to your account.";
                return RedirectToAction("Index", "Itinerary");
            }

            // Get destinations in this itinerary
            var destinations = db.ITINERARY_DESTINATION
                                .Where(i => i.itinerary_id == id)
                                .Select(i => i.DESTINATION)
                                .ToList();

            // Initialize booking view model
            var viewModel = new BookItineraryViewModel
            {
                ItineraryId = id,
                ItineraryName = itinerary.itinerary_name,
                ItineraryPrice = itinerary.price,
                Destinations = destinations,
                NumTravelers = 1,
                // Set default dates (start tomorrow, end a week later)
                TravelStartDate = DateTime.Now.AddDays(1),
                TravelEndDate = DateTime.Now.AddDays(8)
            };
            return View(viewModel);
        }

        // POST: Booking/BookItinerary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookItinerary(BookItineraryViewModel model)
        {
            if (Session["UserID"] == null)
            {
                TempData["ErrorMessage"] = "Please login to book an itinerary.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Get itinerary details to rebuild the view
                var itinerary = db.ITINERARies.Find(model.ItineraryId);
                var destinations = db.ITINERARY_DESTINATION
                                    .Where(i => i.itinerary_id == model.ItineraryId)
                                    .Select(i => i.DESTINATION)
                                    .ToList();

                model.ItineraryName = itinerary.itinerary_name;
                model.ItineraryPrice = itinerary.price;
                model.Destinations = destinations;

                return View(model);
            }

            try
            {
                // Create a new booking
                var booking = new BOOKING
                {
                    bk_type = "itinerary",
                    trip_id = model.ItineraryId,
                    travel_start_date = model.TravelStartDate,
                    travel_end_date = model.TravelEndDate,
                    numtravelers = model.NumTravelers,
                    bk_cost = model.ItineraryPrice * model.NumTravelers,
                    user_id = Convert.ToInt32(Session["UserID"]),
                    bk_date = DateTime.Now,
                    status = "Pending"
                };

                // Add to database
                db.BOOKINGs.Add(booking);
                db.SaveChanges();

                // Redirect to payment page
                TempData["SuccessMessage"] = "Itinerary booking created successfully!";
                return RedirectToAction("Payment", new { id = booking.booking_id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToAction("BookItinerary", new { id = model.ItineraryId });
            }
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

            // Set the user ID from the session
            model.user_id = Convert.ToInt32(Session["UserID"]);

            // Set the booking date to current date
            model.bk_date = DateTime.Now;

            // Set initial status
            model.status = "Pending";

            model.booking_id = 0;

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
                    viewModel.PackageInfo = db.PACKAGEs.FirstOrDefault(p => p.pkg_id == booking.trip_id);
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

        [HttpGet]
        public ActionResult CancelBooking(int id)
        {
            var booking = db.BOOKINGs.Find(id); // Replace with your actual DbContext
            if (booking == null)
            {
                return HttpNotFound();
            }

            if (booking.status == "Pending")
            {
                booking.status = "Cancelled";
                db.SaveChanges();
                TempData["Message"] = "Booking has been cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Only pending bookings can be cancelled.";
            }

            return RedirectToAction("Index");
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