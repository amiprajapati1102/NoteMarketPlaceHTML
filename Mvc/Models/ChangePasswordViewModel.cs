using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class ChangePasswordViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Old Password is Required.")]
        [DataType(DataType.Password)]
        [DisplayName("Old Password")]
        public string OldPassword { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is Required.")]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string NewPassword { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Confirm-Password is Required.")]
        [DataType(DataType.Password)]
        [DisplayName(" Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Password and Confirm password should be same.")]
        public string ConfirmPassword { get; set; }
    }
}