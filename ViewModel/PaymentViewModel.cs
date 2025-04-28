using Wanderlust.Models;

namespace Wanderlust.ViewModel
{
    public class PaymentViewModel
    {
        public BOOKING Booking { get; set; }
        public decimal AmountToPay => Booking.bk_cost;
    }
}


