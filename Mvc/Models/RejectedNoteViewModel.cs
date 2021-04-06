using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class RejectedNoteViewModel
    {
        public int NoteId { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public int SellerId { get; set; }

        public string Name { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string RejectedBy { get; set; }

        public string NotePath { get; set; }
        public string Remarks { get; set; }
    }
}