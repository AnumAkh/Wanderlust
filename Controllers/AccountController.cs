using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Wanderlust.Models;
using Wanderlust.ViewModel;

namespace Wanderlust.Controllers
{
    public class AccountController : Controller
    {
        private readonly WanderlustEntities _context;

        public AccountController()
        {
            _context = new WanderlustEntities();
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var b_user = _context.AUTHORs.FirstOrDefault(a => a.email == model.Email);
                if (b_user != null)
                {
                    Session["AuthorId"] = b_user.author_id;
                    Session["AuthorEmail"] = b_user.email;
                    Session["AuthorName"] = b_user.firstName + " " + b_user.lastName;
                    Session["Role"] = "Author";

                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Blog");
                }
                else
                {
                    var user = _context.USERs.FirstOrDefault(u => u.email == model.Email);

                    // Check if user exists and password matches (no hashing)
                    if (user != null && user.password == model.Password)
                    {
                        // Create auth cookie
                        FormsAuthentication.SetAuthCookie(user.email, model.RememberMe);

                        // Store user info in session
                        Session["UserId"] = user.userID;
                        Session["UserEmail"] = user.email;
                        Session["UserName"] = user.firstName + " " + user.lastName;
                        Session["Role"] = "User";


                        // Redirect to returnUrl or default page
                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError("", "Invalid email or password");
                }
            }

            return View(model);
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (_context.USERs.Any(u => u.email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use");
                    return View(model);
                }

                // Create new user (save password as plain text)
                var user = new USER
                {
                    email = model.Email,
                    password = model.Password,
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    ph_num = model.phNum,
                    addr = model.address
                };

                _context.USERs.Add(user);
                _context.SaveChanges();

                // Auto login after registration
                FormsAuthentication.SetAuthCookie(user.email, false);

                Session["UserId"] = user.userID;
                Session["UserEmail"] = user.email;
                Session["UserName"] = user.firstName + " " + user.lastName;
                Session["Role"] = "User";

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Dashboard()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get user information
            var user = _context.USERs.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Get user's bookings
            var bookings = _context.BOOKINGs
                .Where(b => b.user_id == userId)
                .OrderByDescending(b => b.bk_date)
                .ToList();

            // Count bookings by status
            var pendingCount = bookings.Count(b => b.status == "Pending");
            var confirmedCount = bookings.Count(b => b.status == "Confirmed");
            var cancelledCount = bookings.Count(b => b.status == "Cancelled");

            // Get upcoming trips (confirmed bookings with future travel dates)
            var upcomingTrips = bookings
                .Where(b => b.status == "Confirmed" && b.travel_start_date > DateTime.Now)
                .OrderBy(b => b.travel_start_date)
                .Take(3)
                .ToList();

            // Get recent bookings
            var recentBookings = bookings
                .OrderByDescending(b => b.bk_date)
                .Take(3)
                .ToList();

            // Get user's reviews
            var reviews = _context.REVIEWs
                .Where(r => r.user_id == userId)
                .OrderByDescending(r => r.review_date)
                .Take(3)
                .ToList();

            // Create view model
            var viewModel = new UserDashboardViewModel
            {
                User = user,
                TotalBookings = bookings.Count,
                PendingBookings = pendingCount,
                ConfirmedBookings = confirmedCount,
                CancelledBookings = cancelledCount,
                UpcomingTrips = upcomingTrips,
                RecentBookings = recentBookings,
                RecentReviews = reviews
            };

            return View(viewModel);
        }

        // GET: UserProfile/Edit
        public ActionResult Edit()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            var user = _context.USERs.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            var viewModel = new UserProfileEditViewModel
            {
                UserID = user.userID,
                FirstName = user.firstName,
                LastName = user.lastName,
                Email = user.email,
                Phone = user.ph_num,
                Address = user.addr,
                
            };

            return View(viewModel);
        }

        // POST: Account/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserProfileEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.USERs.Find(model.UserID);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // Update user information
                user.firstName = model.FirstName;
                user.lastName = model.LastName;
                user.ph_num = model.Phone;
                user.addr = model.Address;

                _context.SaveChanges();

                // Update session information
                Session["UserName"] = user.firstName + " " + user.lastName;

                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction("Dashboard");
            }

            return View(model);
        }



        public ActionResult ChangePassword()
        {
            // Check if user is logged in
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            // Check if user is logged in
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var user = _context.USERs.Find(userId);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Verify current password
                if (user.password != model.CurrentPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                    return View(model);
                }

                // Verify that new password and confirmation match
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "New password and confirmation do not match");
                    return View(model);
                }

                // Update password
                user.password = model.NewPassword;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Your password has been changed successfully!";
                return RedirectToAction("Dashboard");
            }

            return View(model);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
