using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models
{ 
    public class ContactModel
    {
        [Required(ErrorMessage = "FullName is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email ID is required.")]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Questions is required.")]
        public string Comments { get; set; }


    }
}