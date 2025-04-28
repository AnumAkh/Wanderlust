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
                    // Set other properties as needed
                };

                _context.USERs.Add(user);
                _context.SaveChanges();

                // Auto login after registration
                FormsAuthentication.SetAuthCookie(user.email, false);

                Session["UserId"] = user.userID;
                Session["UserEmail"] = user.email;
                Session["UserName"] = user.firstName + " " + user.lastName;

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
