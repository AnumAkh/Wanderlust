using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wanderlust.Models;
using Wanderlust.ViewModel;

namespace Wanderlust.Controllers
{
    public class PackageController : Controller
    {
        private WanderlustEntities db = new WanderlustEntities();

        // GET: Package
        public ActionResult Index()
        {
            return View(db.PACKAGEs.ToList());
        }

        // GET: Package/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Retrieve the package based on the id
            PACKAGE package = db.PACKAGEs.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }

            // Retrieve the associated destination IDs from the PACKAGE_DESTINATION table
            var destinationIds = db.PACKAGE_DESTINATION
                                    .Where(pd => pd.pkg_id == id)
                                    .Select(pd => pd.dest_id)
                                    .ToList();

            // Retrieve the destination details based on the destination IDs
            var destinations = db.DESTINATIONs
                                 .Where(d => destinationIds.Contains(d.dest_id))
                                 .ToList();

            // Create a ViewModel to pass both the package and destinations to the view
            var viewModel = new PackageDetailsViewModel
            {
                Package = package,
                Destinations = destinations
            };

            return View(viewModel);
        }


        // GET: Package/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // POST: Package/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "pkg_id,name,image,price,description")] PACKAGE pACKAGE)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Packages.Add(pACKAGE);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(pACKAGE);
        //}

        // GET: Package/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    PACKAGE package = db.Packages.Find(id);
        //    if (package == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(package);
        //}

        // POST: Package/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "pkg_id,name,image,price,description")] PACKAGE package)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(package).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(package);
        //}

        // GET: Package/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    PACKAGE package = db.Packages.Find(id);
        //    if (package == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(package);
        //}

        // POST: Package/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    PACKAGE package = db.Packages.Find(id);
        //    db.Packages.Remove(package);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
