using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class CountryViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Country Name is Required.")]
        [DisplayName("Country Name")]
        public string Name { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Country Code is Required.")]
        [RegularExpression(@"^\+(\d{1}\-)?(\d{1,3})$", ErrorMessage = "Invalid Country Code")]
        [DisplayName("Country Code")]
        public string CountryCode { get; set; }
    }
}