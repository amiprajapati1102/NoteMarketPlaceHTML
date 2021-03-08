using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models
{
    public class UserModel
    {
        public int ID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage ="First Name is Required.")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name is Required.")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID is Required.")]
        public string EmailID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is Required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Confirm-Password is Required.")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Password and Confirm password should be same.")]
        public string ConfirmPassword { get; set; }

        public string SuccessMessage { get; set; }
        
    }
}