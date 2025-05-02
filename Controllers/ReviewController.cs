using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wanderlust.Models;
using Wanderlust.ViewModel;

namespace Wanderlust.Controllers
{
    public class ReviewController : Controller
    {
        private WanderlustEntities db = new WanderlustEntities();

        // GET: Review
        public ActionResult Index()
        {
            var reviews = db.REVIEWs.Include(r => r.DESTINATION).Include(r => r.USER);
            return View(reviews.ToList());
        }

        // GET: Review/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            REVIEW review = db.REVIEWs.Find(id);
            if (review == null)
                return HttpNotFound();

            return View(review);
        }

        // GET: Review/Create
        public ActionResult Create()
        {
            ViewBag.dest_id = new SelectList(db.DESTINATIONs, "dest_id", "destName");
            ViewBag.user_id = new SelectList(db.USERs, "userID", "email");
            return View();
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "dest_id,rating,content")] REVIEW review)
        {
            if (ModelState.IsValid)
            {
                // Set user_id from session
                if (Session["UserId"] != null)
                {
                    review.user_id = Convert.ToInt32(Session["UserId"]);
                }
                else
                {
                    // Redirect to login if not logged in
                    return RedirectToAction("Login", "Account");
                }

                // Set current date
                review.review_date = DateTime.Now;

                db.REVIEWs.Add(review);
                db.SaveChanges();

                // Redirect to destination details page
                return RedirectToAction("Details", "Destination", new { id = review.dest_id });
            }

            ViewBag.dest_id = new SelectList(db.DESTINATIONs, "dest_id", "destName", review.dest_id);
            return View(review);
        }

        // GET: Review/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            REVIEW review = db.REVIEWs.Find(id);
            if (review == null)
                return HttpNotFound();

            if (Session["UserId"] == null || review.user_id != Convert.ToInt32(Session["UserId"]))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            // Create view model for editing reviews
            var viewModel = new ReviewEditViewModel
            {
                Review = review,
                DestinationName = review.DESTINATION.destName,
                DestinationImage = review.DESTINATION.image
            };

            return View(viewModel);
        }

        // POST: Review/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ReviewEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                REVIEW review = db.REVIEWs.Find(viewModel.Review.review_id);
                if (review == null)
                    return HttpNotFound();

                if (Session["UserId"] == null || review.user_id != Convert.ToInt32(Session["UserId"]))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

                // Update only rating and content
                review.content = viewModel.Review.content;
                review.rating = viewModel.Review.rating;
                review.review_date = DateTime.Now; // Update review date

                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Details", "Destination", new { id = review.dest_id });
            }

            // If validation fails, get destination info again
            var existingReview = db.REVIEWs.Find(viewModel.Review.review_id);
            if (existingReview != null)
            {
                viewModel.DestinationName = existingReview.DESTINATION.destName;
                viewModel.DestinationImage = existingReview.DESTINATION.image;
            }

            return View(viewModel);
        }

        // POST: Review/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(FormCollection form)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    // Redirect to login if not logged in
                    return RedirectToAction("Login", "Account");
                }

                // Parse form data
                int destId = Convert.ToInt32(form["dest_id"]);
                int rating = Convert.ToInt32(form["rating"]);
                string content = form["content"];

                // Basic validation
                if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(content))
                {
                    TempData["ErrorMessage"] = "Please provide a valid rating and review content.";
                    return RedirectToAction("Details", "Destination", new { id = destId });
                }

                // Create and save review
                var review = new REVIEW
                {
                    dest_id = destId,
                    rating = rating,
                    content = content,
                    user_id = Convert.ToInt32(Session["UserId"]),
                    review_date = DateTime.Now
                };

                db.REVIEWs.Add(review);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Your review has been successfully added.";
                return RedirectToAction("Details", "Destination", new { id = destId });
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = "An error occurred while adding your review.";
                return RedirectToAction("Details", "Destination", new { id = form["dest_id"] });
            }
        }

        // POST: Review/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            REVIEW review = db.REVIEWs.Find(id);
            if (review == null)
                return HttpNotFound();

            if (Session["UserId"] == null || review.user_id != Convert.ToInt32(Session["UserId"]))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            int destId = review.dest_id; // Store for redirection

            db.REVIEWs.Remove(review);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Your review has been successfully deleted.";
            return RedirectToAction("Details", "Destination", new { id = destId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}