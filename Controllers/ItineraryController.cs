using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using System.Linq;
using Wanderlust.Models;
using System;

public class ItineraryController : Controller
{
    private WanderlustEntities db = new WanderlustEntities();

    // GET: Itinerary
    public ActionResult Index()
    {
        // Check if there's a temporary itinerary in session
        var sessionItinerary = Session["Itinerary"] as List<int> ?? new List<int>();
        var sessionDestinations = db.DESTINATIONs.Where(d => sessionItinerary.Contains(d.dest_id)).ToList();
        ViewBag.SessionDestinations = sessionDestinations;

        // Also get the user's saved itineraries from database
        var savedItineraries = new List<ITINERARY>();

        if (Session["UserId"] != null)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            savedItineraries = db.ITINERARies
                .Where(i => i.user_id == userId)
                .OrderByDescending(i => i.itinerary_id)
                .ToList();
        }

        return View(savedItineraries);
    }

    // POST: Itinerary/AddToItinerary
    [HttpPost]
    public ActionResult AddToItinerary(int id, int? itineraryId, string newItineraryName)
    {
        // If user is not logged in, store in session
        if (Session["UserId"] == null)
        {
            var itinerary = Session["Itinerary"] as List<int> ?? new List<int>();
            if (!itinerary.Contains(id))
                itinerary.Add(id);
            Session["Itinerary"] = itinerary;

            // Redirect to itinerary index or destination details
            return RedirectToAction("Index", "Itinerary");
        }
        else
        {
            // User is logged in, add to selected itinerary or create new one
            int userId = Convert.ToInt32(Session["UserId"]);

            if (itineraryId.HasValue && itineraryId.Value > 0)
            {
                // Add to existing itinerary
                var existingItinerary = db.ITINERARies.Find(itineraryId.Value);
                if (existingItinerary != null && existingItinerary.user_id == userId)
                {
                    // Check if destination is already in this itinerary
                    var exists = db.ITINERARY_DESTINATION.Any(i =>i.itinerary_id == itineraryId.Value && i.dest_id == id);

                    if (!exists)
                    {
                        db.ITINERARY_DESTINATION.Add(new ITINERARY_DESTINATION
                        {
                            itinerary_id = itineraryId.Value,
                            dest_id = id
                        });

                        // Update total price
                        var destPrice = db.DESTINATIONs.Find(id).price;
                        existingItinerary.price += destPrice;

                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Details", new { id = itineraryId.Value });
            }
            else if (!string.IsNullOrWhiteSpace(newItineraryName))
            {
                // Create new itinerary with the destination
                var destPrice = db.DESTINATIONs.Find(id).price;
                var newItinerary = new ITINERARY
                {
                    user_id = userId,
                    itinerary_name = newItineraryName,
                    price = destPrice
                };

                db.ITINERARies.Add(newItinerary);
                db.SaveChanges();

                db.ITINERARY_DESTINATION.Add(new ITINERARY_DESTINATION
                {
                    itinerary_id = newItinerary.itinerary_id,
                    dest_id = id
                });

                db.SaveChanges();
                return RedirectToAction("Details", new { id = newItinerary.itinerary_id });
            }
            else
            {
                // Default: add to session itinerary
                var itinerary = Session["Itinerary"] as List<int> ?? new List<int>();
                if (!itinerary.Contains(id))
                    itinerary.Add(id);
                Session["Itinerary"] = itinerary;
                return RedirectToAction("Index");
            }
        }
    }

    // GET: Itinerary/GetUserItineraries
    public JsonResult GetUserItineraries()
    {
        if (Session["UserId"] == null)
            return Json(new List<object>(), JsonRequestBehavior.AllowGet);

        int userId = Convert.ToInt32(Session["UserId"]);
        var itineraries = db.ITINERARies
            .Where(i => i.user_id == userId)
            .Select(i => new
            {
                itinerary_id = i.itinerary_id,
                itinerary_name = i.itinerary_name
            })
            .ToList();

        return Json(itineraries, JsonRequestBehavior.AllowGet);
    }

    // POST: Itinerary/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Book(string itineraryName)
    {
        if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
        var itinerary = Session["Itinerary"] as List<int>;
        if (itinerary == null || !itinerary.Any()) return RedirectToAction("Index");
        var newItinerary = new ITINERARY
        {
            user_id = Convert.ToInt32(Session["UserId"]),
            itinerary_name = itineraryName,
            price = db.DESTINATIONs.Where(d => itinerary.Contains(d.dest_id)).Sum(d => d.price)
        };
        db.ITINERARies.Add(newItinerary);
        db.SaveChanges();
        foreach (var destId in itinerary)
        {
            db.ITINERARY_DESTINATION.Add(new ITINERARY_DESTINATION
            {
                itinerary_id = newItinerary.itinerary_id,
                dest_id = destId
            });
        }
        db.SaveChanges();
        Session["Itinerary"] = null;
        return RedirectToAction("Details", new { id = newItinerary.itinerary_id });
    }

    // GET: Itinerary/Details/5
    public ActionResult Details(int? id)
    {
        if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        ITINERARY itinerary = db.ITINERARies.Find(id);
        if (itinerary == null) return HttpNotFound();
        var destinations = db.ITINERARY_DESTINATION
                             .Where(i => i.itinerary_id == id)
                             .Select(i => i.DESTINATION)
                             .ToList();
        ViewBag.Destinations = destinations;
        return View(itinerary);
    }
}