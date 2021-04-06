using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class DownloadNotesViewModel
    {
        public int NoteId { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public int BuyerId { get; set; }

        public string BuyerName { get; set; }

        public int SellerId { get; set; }

        public string SellerName { get; set; }

        public string NotePath { get; set; }

        public decimal Price { get; set; }

        public DateTime DownloadedDate { get; set; }


    }
    public class BuyerListModel
    {
        public int BuyerId { get; set; }

        public string Name { get; set; }
    }

    public class NoteListModel
    {
        public int NoteId { get; set; }

        public string Title { get; set; }
    }
}