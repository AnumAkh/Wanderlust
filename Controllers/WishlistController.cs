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

        // GET: Wishlist
        public ActionResult Index()
        {
            var wISHLISTs = db.WISHLISTs.Include(w => w.DESTINATION).Include(w => w.USER);
            return View(wISHLISTs.ToList());
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
        public ActionResult Create()
        {
            ViewBag.dest_id = new SelectList(db.DESTINATIONs, "dest_id", "destName");
            ViewBag.user_id = new SelectList(db.USERs, "userID", "email");
            return View();
        }

        // POST: Wishlist/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "item_id,dest_id,user_id")] WISHLIST wISHLIST)
        {
            if (ModelState.IsValid)
            {
                db.WISHLISTs.Add(wISHLIST);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.dest_id = new SelectList(db.DESTINATIONs, "dest_id", "destName", wISHLIST.dest_id);
            ViewBag.user_id = new SelectList(db.USERs, "userID", "email", wISHLIST.user_id);
            return View(wISHLIST);
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