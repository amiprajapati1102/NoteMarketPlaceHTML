using NoteMarketPlaceHtml.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace NoteMarketPlaceHtml.Models
{
    public class AddNoteModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Category { get; set; }

        public string DisplayPicture { get; set; }

        public string UploadNotes { get; set; }

        public int NoteType { get; set; }

        public Nullable<int> NumberofPages { get; set; }

        public Nullable<int> Country { get; set; }

        public string UniversityName { get; set; }

        public string Course { get; set; }

        public string CourseCode { get; set; }

        public string Professor { get; set; }

        public string Description { get; set; }

        public string SellType { get; set; }

        public decimal SellingPrice { get; set; }

        public string NotePreview { get; set; }

        public void MaptoModel(SellerNote note, SellerNotesAttachement attachment)
        {
            note.Title = Title;
            note.Category = Category;
            note.DisplayPicture = DisplayPicture;
            note.NoteType = NoteType;
            note.NumberofPages = NumberofPages;
            note.Description = Description;
            note.UniversityName = UniversityName;
            note.Country = Country;
            note.Course = Course;
            note.CourseCode = CourseCode;
            note.Professor = Professor;
            note.SellingPrice = SellingPrice;
            note.NotesPreview = NotePreview;
            attachment.FileName = UploadNotes;
        }
    }
}