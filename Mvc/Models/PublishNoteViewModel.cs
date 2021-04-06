using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class PublishNoteViewModel
    {
        public int NoteId { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public decimal Price { get; set; }

        public int SellerId { get; set; }

        public string Seller { get; set; }

        public string ActionBy { get; set; }

        public DateTime PublishDate { get; set; }

        public int? TotalDownloads { get; set; }

        public int UserId { get; set; }

        public string Filename { get; set; }

        public string NotePath { get; set; }
    }
}