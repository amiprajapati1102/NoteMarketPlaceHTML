using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace NoteMarketPlace.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email Id is required.")]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}