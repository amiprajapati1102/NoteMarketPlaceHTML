using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class ManageCountryViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CountryCode { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string IsActive { get; set; }
    }
}