using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class ByuerRequestViewModel
    {
        public int NoteId { get; set; }

        public int DownloadId { get; set; }

        public string Title { get; set; }
        public string PhoneNo { get; set; }

        public string Category { get; set; }

        public string Buyer { get; set; }

        public string CountryCode { get; set; }

       

        public string Selltype { get; set; }

        public decimal Price { get; set; }

        public DateTime RequestDate { get; set; }
    }
}