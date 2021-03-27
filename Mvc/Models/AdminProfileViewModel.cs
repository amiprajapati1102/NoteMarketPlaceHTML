using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NoteMarketPlaceHtml.Models
{
    public class AdminProfileViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is Required.")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name is Required.")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID is Required.")]
        [DisplayName("Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailID { get; set; }

        [DisplayName(" Secondary Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string SecondaryEmailId { get; set; }
       
        public string PhoneNumber { get; set; }

       
      
        
      
        [DisplayName("Profile Picture")]
        public string path { get; set; }



        public IEnumerable<SelectListItem> CountryCode { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Country Code is Required.")]
        public string SelectedCode { get; set; }
       

    }
}