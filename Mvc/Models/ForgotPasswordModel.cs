using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email Id is required.")]
        public string EmailID { get; set; }
    }
}