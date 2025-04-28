using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wanderlust.Models;

namespace Wanderlust.ViewModel
{
    public class HomeViewModel
    {
        public List<DESTINATION> FeaturedDestinations { get; set; }
        public List<PACKAGE> FeaturedPackages { get; set; }
    }
}