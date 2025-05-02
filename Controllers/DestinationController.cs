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
    public class DestinationController : Controller
    {
        private WanderlustEntities db = new WanderlustEntities();

        // GET: Destination
        public ActionResult Index()
        {
            return View(db.DESTINATIONs.ToList());
        }

        // GET: Destination/Details/5
        public ActionResult Details(int id)
        {
            var destination = db.DESTINATIONs.Find(id);

            if (destination == null)
            {
                return HttpNotFound();
            }

            var reviews = db.REVIEWs
                .Where(r => r.dest_id == id)
                .OrderByDescending(r => r.review_date)
                .ToList();

            var viewModel = new DestinationDetailsViewModel
            {
                Destination = destination,
                Reviews = reviews
            };

            return View(viewModel);
        }


    }
}