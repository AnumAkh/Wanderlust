using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Wanderlust.Models;

namespace Wanderlust.ViewModel
{
    public class PackageDetailsViewModel
    {

        public string bk_type { get; set; }  // "package" as a hidden value
        public int trip_id { get; set; }  // Package ID (Foreign Key)
        public decimal price_per_person { get; set; }  // Price per person

        // Booking Fields
        [Required]
        [DataType(DataType.Date)]
        public DateTime travel_start_date { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime travel_end_date { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Number of travelers must be between 1 and 10.")]
        public int numtravelers { get; set; }

        [Required]
        public decimal bk_cost { get; set; }  // Total cost (calculated)

        // Navigation Property (to Package for more details)

        public PACKAGE Package { get; set; }
        public List<DESTINATION> Destinations { get; set; }
    }

}