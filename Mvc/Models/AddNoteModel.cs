using ExpressiveAnnotations.Attributes;
using NoteMarketPlaceHtml.DbModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace NoteMarketPlaceHtml.Models
{
    public class AddNoteModel
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Title Name is Required.")]
        [DisplayName("Title")]
        public string Title { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Category is Required.")]
        [DisplayName("Category")]
        public int Category { get; set; }
      
        [DisplayName("Display Picture")]
        public string DisplayPicture { get; set; }

        
        [DisplayName("Upload Note")]
        public string UploadNotes { get; set; }

       
        [DisplayName("Note Type")]
        public Nullable<int> NoteType { get; set; }
        [DisplayName("No of Pages")]
        public Nullable<int> NumberofPages { get; set; }
        [DisplayName("Country Name")]
        public Nullable<int> Country { get; set; }

        [DisplayName("University Name")]
        public string UniversityName { get; set; }

        [DisplayName("Course Name")]
        public string Course { get; set; }
        [DisplayName("Course Code")]
        public string CourseCode { get; set; }

        [DisplayName("Professor Name")]
        public string Professor { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Description is Required.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Sell Typeis Required.")]
        [DisplayName("Sell For")]
        public string SellType { get; set; }

        public decimal SellingPrice { get; set; }

        public string NotePreview { get; set; }

       
    }
}