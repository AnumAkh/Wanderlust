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
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DESTINATION dESTINATION = db.DESTINATIONs.Find(id);
            if (dESTINATION == null)
            {
                return HttpNotFound();

            }
            var viewModel = new DestinationDetailsViewModel
            {

                Destination = dESTINATION
            };
            return View(viewModel);
        }

    }
}