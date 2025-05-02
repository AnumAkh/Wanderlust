using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wanderlust.Models;

namespace Wanderlust.Controllers
{
    public class WishlistController : Controller
    {
        private WanderlustEntities db = new WanderlustEntities();


        public ActionResult Index()
        {

            if (Session["UserId"] == null)
            {
                TempData["ErrorMessage"] = "Please login to view your wishlist.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);


            var userWishlist = db.WISHLISTs
                .Include(w => w.DESTINATION)
                .Where(w => w.user_id == userId)
                .ToList();

            return View(userWishlist);
        }

        // GET: Wishlist/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WISHLIST wISHLIST = db.WISHLISTs.Find(id);
            if (wISHLIST == null)
            {
                return HttpNotFound();
            }
            return View(wISHLIST);
        }

        // GET: Wishlist/Create
        // GET: Wishlist/AddToWishlist/5
        public ActionResult AddToWishlist(int id)
        {
            if (Session["UserId"] == null)
            {
                TempData["ErrorMessage"] = "Please login to add items to your wishlist.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            // Check if this destination is already in the user's wishlist
            var existingWishlist = db.WISHLISTs.FirstOrDefault(w => w.dest_id == id && w.user_id == userId);

            if (existingWishlist == null)
            {
                // Create new wishlist item
                var wishlist = new WISHLIST
                {
                    dest_id = id,
                    user_id = userId
                };

                db.WISHLISTs.Add(wishlist);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Added to Wishlist!";
            }
            else
            {
                TempData["InfoMessage"] = "This item is already in your wishlist.";
            }

            // Return to the previous page
            return Redirect(Request.UrlReferrer.ToString());
        }



        // GET: Wishlist/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WISHLIST wISHLIST = db.WISHLISTs.Find(id);
            if (wISHLIST == null)
            {
                return HttpNotFound();
            }
            return View(wISHLIST);
        }

        // POST: Wishlist/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            WISHLIST wISHLIST = db.WISHLISTs.Find(id);
            db.WISHLISTs.Remove(wISHLIST);
            db.SaveChanges();
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