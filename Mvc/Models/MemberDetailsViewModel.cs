using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class MemberDetailsViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfileImage { get; set; }

        public string Email { get; set; }
        [DisplayFormat(DataFormatString ="{0:yyyy/MM/dd}")]
        public DateTime? DOB { get; set; }

        public string Phone { get; set; }

        public string University { get; set; }
        public string Collage { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

     

        public string Zipcode { get; set; }

    }
    public class MemberNoteViewModel
    {
        public int Id { get; set; }

        public int NoteId { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string Status { get; set; }

        public int DownloadedNote { get; set; }

        public decimal? Earning { get; set; }

        public DateTime DateAdded { get; set; }

        public DateTime? PublishedDate { get; set; }
        public string NotePath { get; set; }



    }
}
