using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Wanderlust.Models;
using Wanderlust.ViewModel;

public class HomeController : Controller
{
    private readonly WanderlustEntities _context;

    public HomeController()
    {
        _context = new WanderlustEntities(); // default initialization
    }

    public HomeController(WanderlustEntities context)
    {
        _context = context;
    }

    public ActionResult Index()
    {
        var viewModel = new HomeViewModel
        {
            FeaturedDestinations = _context.DESTINATIONs.Take(4).ToList(),
            FeaturedPackages = _context.Packages.Take(4).ToList()
        };
        return View(viewModel);
    }
}
