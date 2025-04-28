using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wanderlust.Models;

namespace Wanderlust.ViewModel
{
    public class BookingDetailsViewModel
    {
        public BOOKING Booking { get; set; }
        public PACKAGE PackageInfo { get; set; }
        public DESTINATION DestinationInfo { get; set; }
        public ITINERARY ItineraryInfo { get; set; }
        public List<PAYMENT> Payments { get; set; }
        public USER UserInfo { get; set; }
    }
}