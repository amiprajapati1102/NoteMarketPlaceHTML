using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class SpamReportViewModel
    {
        public int ID { get; set; }

        public int NoteId { get; set; }

        public string ReportedBy { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string Remarks { get; set; }

        public DateTime CreatedDate { get; set; }

        public string NotePath { get; set; }

    }
}