using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Wanderlust.Models;

namespace Wanderlust.ViewModel
{
    public class DestinationDetailsViewModel
    {
        // Existing properties
        public DESTINATION Destination { get; set; }

        [Required]
        [Display(Name = "Travel Start Date")]
        [DataType(DataType.Date)]
        public DateTime travel_start_date { get; set; }

        [Required]
        [Display(Name = "Travel End Date")]
        [DataType(DataType.Date)]
        public DateTime travel_end_date { get; set; }

        [Required]
        [Display(Name = "Number of Travelers")]
        [Range(1, 10)]
        public int numtravelers { get; set; }

        [Required]
        [Display(Name = "Total Cost")]
        [DataType(DataType.Currency)]
        public decimal bk_cost { get; set; }

        public REVIEW NewReview { get; set; } = new REVIEW();

        // New property for reviews
        public List<REVIEW> Reviews { get; set; }
    }
}