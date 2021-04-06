using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class MyDownoadViewModel
    {
        public int NoteId { get; set; }

        public int DowloadId { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string Buyer { get; set; }

        public string SellType { get; set; }

        public decimal Price { get; set; }

        public DateTime? DownloadDate { get; set; }
    }
}