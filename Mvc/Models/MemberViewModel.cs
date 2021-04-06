using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class MemberViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public DateTime JoinDate { get; set; }

        public int UnderReviewNotes { get; set; }

        public int PublishedNotes { get; set; }

        public int DownloadedNotes { get; set; }

        public decimal? TotalExpense { get; set; }

        public decimal? TotalEarning { get; set; }
    }
}