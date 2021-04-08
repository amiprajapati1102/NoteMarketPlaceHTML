using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class AdminDashBoardViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public float AttachmentSize { get; set; }

        public decimal Price { get; set; }

        public string Publisher { get; set; }

        public DateTime PublishDate { get; set; }

        public int publishMonth { get; set; }

        public int? TotalDownloads { get; set; }

        public int UserId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }
    }
    public class MonthModel
    {
        public int Id { get; set; }
        public string Month { get; set; }
    }
}