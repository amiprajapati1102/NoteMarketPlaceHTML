using NoteMarketPlaceHtml.DbModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class AddUserProfileViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> DOB { get; set; }

        public Nullable<int> Gender { get; set; }

        [Required]
        public string CountryCode { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string ProfilePicture { get; set; }

        public HttpPostedFileBase Image { get; set; }

        [Required]
        public string Address1 { get; set; }

        [Required]
        public string Address2 { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Zipcode { get; set; }

        public string Country { get; set; }

        [Required]
        public string University { get; set; }

        [Required]
        public string College { get; set; }


        public List<GenderModel> genderModel { get; set; }

        public List<CountryModel> countryModel { get; set; }

        public List<CountryModel> CountryCodeModel { get; set; }

    }
    public class GenderModel
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class CountryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
    }
   
}