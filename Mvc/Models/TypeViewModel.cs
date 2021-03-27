using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class TypeViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Type Name is Required.")]
        [DisplayName("Name")]
        public string Name { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Description is Required.")]
       
        [DisplayName("Description")]
        
        public string Description { get; set; }
    }
}