using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class SearchModelViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string University { get; set; }
        public Nullable<decimal> Pages { get; set; }
        public DateTime? PublishDate { get; set; }

        public int Type_Id { get; set; }

        public int Category { get; set; }

        public string Course { get; set; }

        public Nullable<int> Country { get; set; }

        public Nullable<double> Rating { get; set; }

        public Nullable<int> TotalRating { get; set; }

        public Nullable<int> TotalSpams { get; set; }


    }

    public class AvgRatings
    {
        public int NoteId { get; set; }
        public double Rating { get; set; }
        public int Total { get; set; }

    }

    public class SpamNote
    {
        public int NoteId { get; set; }
        public int Total { get; set; }
    }



}