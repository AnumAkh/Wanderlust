using System;
using System.ComponentModel.DataAnnotations;
using Wanderlust.Models;

namespace Wanderlust.ViewModel
{
    public class ReviewEditViewModel
    {
        // The review being edited
        public REVIEW Review { get; set; }

        // Destination information for context
        public string DestinationName { get; set; }
        public string DestinationImage { get; set; }
    }
}