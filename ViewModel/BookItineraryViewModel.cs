using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Wanderlust.Models;

namespace Wanderlust.ViewModel
{
    public class BookItineraryViewModel
    {
        public int ItineraryId { get; set; }

        [Display(Name = "Itinerary Name")]
        public string ItineraryName { get; set; }

        [Display(Name = "Total Itinerary Price")]
        [DisplayFormat(DataFormatString = "${0:N2}")]
        public decimal ItineraryPrice { get; set; }

        public List<DESTINATION> Destinations { get; set; }

        [Required]
        [Display(Name = "Departure Date")]
        [DataType(DataType.Date)]
        public DateTime TravelStartDate { get; set; }

        [Required]
        [Display(Name = "Return Date")]
        [DataType(DataType.Date)]
        public DateTime TravelEndDate { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Number of travelers must be between 1 and 10")]
        [Display(Name = "Number of Travelers")]
        public int NumTravelers { get; set; }

        [Display(Name = "Total Cost")]
        [DisplayFormat(DataFormatString = "${0:N2}")]
        public decimal TotalCost
        {
            get
            {
                return ItineraryPrice * NumTravelers;
            }
        }

        [Display(Name = "Special Requests/Notes")]
        [DataType(DataType.MultilineText)]
        [StringLength(500, ErrorMessage = "Special requests cannot exceed 500 characters")]
        public string SpecialRequests { get; set; }
    }
}