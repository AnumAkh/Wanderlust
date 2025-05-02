using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wanderlust.ViewModel
{
    public class UserProfileEditViewModel
    {
        public int UserID { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Country { get; set; }
    }
}