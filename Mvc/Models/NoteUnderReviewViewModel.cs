using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class NoteUnderReviewViewModel
    {
        public int NoteId { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public int SellerId { get; set; }

        public string Seller { get; set; }

        public string status { get; set; }

        public string NotePath { get; set; }

        public DateTime DateAdded { get; set; }

    }
    public class SellerDetailsModel
    {
        public int SellerId { get; set; }

        public string Name { get; set; }
    }

}