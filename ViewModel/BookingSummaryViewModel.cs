using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wanderlust.Models;

namespace Wanderlust.ViewModel
{
    public class BookingSummaryViewModel
    {
        public BOOKING Booking { get; set; }
        public PAYMENT Payments { get; set; }
        public PACKAGE PackageInfo { get; set; }
        public DESTINATION DestinationInfo { get; set; }
        public ITINERARY ItineraryInfo { get; set; }
    }

}