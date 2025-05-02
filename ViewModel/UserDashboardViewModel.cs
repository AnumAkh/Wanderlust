using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wanderlust.Models;

namespace Wanderlust.ViewModel
{
    public class UserDashboardViewModel
    {
        public USER User { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public List<BOOKING> UpcomingTrips { get; set; }
        public List<BOOKING> RecentBookings { get; set; }
        public List<REVIEW> RecentReviews { get; set; }
    }
}