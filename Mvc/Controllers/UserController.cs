using NoteMarketPlaceHtml.DbModel;
using NoteMarketPlaceHtml.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.IO;
using System.Web.Routing;

namespace NoteMarketPlaceHtml.Controllers
{
    public class UserController : Controller
    {
        NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();
        public UserController()
        {
            using (var _Context = new NoteMarketPlaceEntities())
            {
                // set social urls
                var socialUrl = _Context.SystemConfigurations.Where(m => m.KeyData == "Facebook" || m.KeyData == "Twitter" || m.KeyData == "Linkedin").ToList();
                ViewBag.URLs = socialUrl;
            }

        }
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (User.Identity.IsAuthenticated)
            {
                using (var db_1 = new NoteMarketPlaceEntities())
                {
                    //current user profile img
                    // set default image
                    var img = (from Details in db_1.AddAdmins
                               join Users in db_1.Users on Details.UserId equals Users.Id
                               where Users.EmailId == requestContext.HttpContext.User.Identity.Name
                               select Details.ProfilePicture).FirstOrDefault();

                    if (img == null)
                    {
                        // set default image
                        var defaultImg = db_1.SystemConfigurations.FirstOrDefault(m => m.KeyData == "DefaultMemberDisplayPicture").ValueData;
                        ViewBag.UserProfile = defaultImg;
                    }
                    else
                    {
                        ViewBag.UserProfile = img;
                        TempData["oldprofile"] = img;
                    }


                }
            }

        }
        public ActionResult SearchNotes(int? Type, int? Category, string University, string Course, int? Country, int? Rating, string search)
        {
            using (var _Context = new NoteMarketPlaceEntities())
            {
                // get all types
                var type = _Context.NoteTypes.ToList();
                // get all category
                var category = _Context.NoteCategories.ToList();
                // get distinct university
                var university = _Context.SellerNotes.Where(m => m.UniversityName != null).Select(x => x.UniversityName).Distinct().ToList();
                // get distinct courses
                var course = _Context.SellerNotes.Where(m => m.Course != null).Select(x => x.Course).Distinct().ToList();
                // get all countries
                var country = _Context.Countries.ToList();

                // get all book details
                var notes = (from Notes in _Context.SellerNotes
                             join Status in _Context.ReferenceDatas on Notes.Status equals Status.Id
                             where Status.Values == "Published" && Notes.IsActive == true
                             let avgRatings = (from Review in _Context.SellerNotesReviews
                                               where Review.NoteId == Notes.Id
                                               group Review by Review.NoteId into grp
                                               select new AvgRatings
                                               {
                                                   Rating = (double)Math.Round(grp.Average(m => m.Ratings)),
                                                   Total = grp.Count()
                                               })
                             let spamNote = (from Spam in _Context.SellerNotesReportedIssues
                                             where Spam.NoteId == Notes.Id
                                             group Spam by Spam.NoteId into grp
                                             select new SpamNote
                                             {
                                                 Total = grp.Count()
                                             })
                             select new Search_Notes_Model
                             {
                                 Id = Notes.Id,
                                 Title = Notes.Title,
                                 University = Notes.UniversityName,
                                 Pages = Notes.NumberofPages == null ? 0 : Notes.NumberofPages,
                                 Image = Notes.DisplayPicture,
                                 PublishDate = Notes.PublishedDate,
                                 Type_Id = (int)Notes.NoteType,
                                 Category = Notes.Category,
                                 Country = Notes.Country,
                                 Course = Notes.Course,
                                 Rating = avgRatings.Select(a => a.Rating).FirstOrDefault(),
                                 TotalRating = avgRatings.Select(a => a.Total).FirstOrDefault(),
                                 TotalSpams = spamNote.Select(a => a.Total).FirstOrDefault()
                             }).ToList();

                ViewBag.TypeList = type;
                ViewBag.CategoryList = category;
                ViewBag.University = university;
                ViewBag.Course = course;
                ViewBag.Country = country;


                var filternotes = notes;

                // if filter value is available
                if (!Type.Equals(null))
                {
                    filternotes = filternotes.Where(m => m.Type_Id == Type).ToList();
                }
                if (!Category.Equals(null))
                {
                    filternotes = filternotes.Where(m => m.Category == Category).ToList();
                }
                if (University != null)
                {
                    filternotes = filternotes.Where(m => m.University == University).ToList();
                }
                if (Course != null)
                {
                    filternotes = filternotes.Where(m => m.Course == Course).ToList();
                }
                if (!Country.Equals(null))
                {
                    filternotes = filternotes.Where(m => m.Country == Country).ToList();
                }
                if (!Rating.Equals(null))
                {
                    filternotes = filternotes.Where(m => m.Rating >= Rating).ToList();
                }
                if (search != null)
                {
                    filternotes = filternotes.Where(m => m.Title.ToLower().Contains(search.ToLower())).ToList();
                }


                return View(filternotes);
            }


        }

        /**GET: User
        public ActionResult SearchNotes()
        {
            return View();
        }**/

        [Authorize]
        [Route("User/Dashboard")]
        public ActionResult Dashboard()
        {
            using (var _Context = new NoteMarketPlaceEntities())
            {
                // current user
                var currentUser = _Context.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name);

                // my total earning
                var earning = (from download in _Context.Downloads
                               where download.Seller == currentUser.Id && download.IsSellerHasAllowedDownload == true
                               group download by download.Seller into total
                               select total.Sum(model => model.PurchasedPrice)).ToList();
                ViewBag.TotalEarning = earning.Count() == 0 ? 0 : earning[0];

                // my total sold notes
                var soldNotes = (from download in _Context.Downloads
                                 where download.Seller == currentUser.Id
                                 group download by download.Seller into total
                                 select total.Count()).ToList();
                ViewBag.TotalSoldNotes = soldNotes.Count() == 0 ? 0 : soldNotes[0];

                // my download notes
                var downloadNotes = (from download in _Context.Downloads
                                     where download.Downloader == currentUser.Id
                                     group download by download.Downloader into total
                                     select total.Count()).ToList();
                ViewBag.TotalDownloadNotes = downloadNotes.Count() == 0 ? 0 : downloadNotes[0];

                // my rejected notes
                var rejectedNotes = (from notes in _Context.SellerNotes
                                     join status in _Context.ReferenceDatas on notes.Status equals status.Id
                                     where status.RefCategory == "Notes Status" && status.Values == "Rejected" && notes.SellerId == currentUser.Id
                                     group notes by notes.SellerId into total
                                     select total.Count()).ToList();
                ViewBag.TotalRejectedNotes = rejectedNotes.Count() == 0 ? 0 : rejectedNotes[0];

                // Buyer request
                var buyerRequest = (from download in _Context.Downloads
                                    where download.IsSellerHasAllowedDownload == false && download.Seller == currentUser.Id
                                    group download by download.Seller into total
                                    select total.Count()).ToList();
                ViewBag.TotalBuyerRequest = buyerRequest.Count() == 0 ? 0 : buyerRequest[0];

                // In progress notes
                var progressNotes = (from notes in _Context.SellerNotes
                                     join category in _Context.NoteCategories on notes.Category equals category.Id
                                     join status in _Context.ReferenceDatas on notes.Status equals status.Id
                                     where status.RefCategory == "Notes Status" && notes.SellerId == currentUser.Id &&
                                     (status.Values == "Draft" || status.Values == "Submitted For Review" || status.Values == "In Review")
                                     select new DashboardInProgressModel
                                     {
                                         Id = notes.Id,
                                         Title = notes.Title,
                                         Category = category.Name,
                                         Status = status.Values,
                                         CreatedDate = (DateTime)notes.CreatedDate
                                     }).OrderByDescending(model => model.CreatedDate).ToList();

                ViewBag.ProgressNotes = progressNotes;

                // published notes
                var publishedNotes = (from notes in _Context.SellerNotes
                                      join category in _Context.NoteCategories on notes.Category equals category.Id
                                      join status in _Context.ReferenceDatas on notes.Status equals status.Id
                                      where status.RefCategory == "Notes Status" && status.Values == "Published" && notes.SellerId == currentUser.Id
                                      select new DashboardInPublishedModel
                                      {
                                          Id = notes.Id,
                                          Title = notes.Title,
                                          Category = category.Name,
                                          Price = (decimal)notes.SellingPrice,
                                          SellType = (notes.SellingPrice == 0 ? "Free" : "PaId"),
                                          CreatedDate = (DateTime)notes.CreatedDate
                                      }).OrderByDescending(model => model.CreatedDate).ToList();

                ViewBag.PublishNotes = publishedNotes;

                return View();
            }
        }

        [Authorize]
        public ActionResult AddNote(int? edit)
        {
            using (var _Context = new NoteMarketPlaceEntities())
            {
                // all type
                var type = _Context.NoteTypes.ToList();

                // all category
                var category = _Context.NoteCategories.ToList();

                // all country
                var country = _Context.Countries.ToList();

                ViewBag.CategoryList = category;
                ViewBag.NoteTypeList = type;
                ViewBag.CountryList = country;
                ViewBag.Edit = false;

                // edit details
                if (!edit.Equals(null))
                {
                    var note = (from Notes in _Context.SellerNotes
                                join Attachment in _Context.SellerNotesAttachements on Notes.Id equals Attachment.NoteId
                                where Notes.Id == edit && Notes.Status == 6
                                select new AddNoteModel
                                {
                                    Id = Notes.Id,
                                    Title = Notes.Title,
                                    Category = Notes.Category,
                                    UploadNotes = Attachment.FilePath,
                                    NoteType = (int)Notes.NoteType,
                                    NumberofPages = Notes.NumberofPages,
                                    Country = Notes.Country,
                                    UniversityName = Notes.UniversityName,
                                    Course = Notes.Course,
                                    CourseCode = Notes.CourseCode,
                                    Professor = Notes.Professor,
                                    Description = Notes.Description,
                                    SellType = Notes.IsPaid == true ? "Free" : "Paid",
                                    SellingPrice = (decimal)Notes.SellingPrice,
                                    NotePreview = Notes.NotesPreview
                                }).FirstOrDefault<AddNoteModel>();

                    ViewBag.Edit = true;
                    return View(note);
                }

                return View();
            }
        }


        [HttpPost]

        public ActionResult AddNote(int? Id, AddNoteModel note, HttpPostedFileBase UploadNotes, HttpPostedFileBase DisplayPicture, HttpPostedFileBase NotePreview, string submitdata1, string submitdata2)
        {
            using (var _Context = new NoteMarketPlaceEntities())
            {
                var type = _Context.NoteTypes.ToList();

                // all category
                var category = _Context.NoteCategories.ToList();

                // all country
                var country = _Context.Countries.ToList();

                ViewBag.CategoryList = category;
                ViewBag.NoteTypeList = type;
                ViewBag.CountryList = country;
                ViewBag.Edit = false;
                if (ModelState.IsValid)
                {
                    var currentuserId = _Context.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name).Id;
                    if (submitdata1 != null)
                    {

                      
                            // get current userId
                         

                            if (!Id.Equals(null))
                            {
                                var noteDetails = _Context.SellerNotes.SingleOrDefault(model => model.Id == Id && model.Status == 6);
                                var attachment = _Context.SellerNotesAttachements.SingleOrDefault(model => model.NoteId == Id);
                            string folder1 = Server.MapPath(string.Format("~/Members/{0}/{1}/Attachment", currentuserId, Id));
                            Directory.CreateDirectory(folder1);
                            string extention_pdf = Path.GetExtension(UploadNotes.FileName);
                            string atta1 = "att1" + extention_pdf;
                            attachment.FilePath = "~/Members/" + currentuserId + "/" + Id+ "/" + "Attachment/" + atta1;
                            atta1 = Path.Combine(Server.MapPath(string.Format("~/Members/{0}/{1}/Attachment", currentuserId, Id)), atta1);


                            UploadNotes.SaveAs(atta1);
                            note.MaptoModel(noteDetails, attachment);
                                noteDetails.ModifiedDate = DateTime.Now;
                                attachment.ModifiedDate = DateTime.Now;

                                _Context.SaveChanges();

                                return RedirectToAction("Dashboard");
                            }
                            else
                            {


                                var noteDetails = _Context.SellerNotes;
                                noteDetails.Add(new SellerNote
                                {
                                    Title = note.Title,
                                    SellerId = currentuserId,
                                    Category = note.Category,
                                    DisplayPicture = note.DisplayPicture,
                                    NoteType = note.NoteType,
                                    NumberofPages = note.NumberofPages,
                                    Description = note.Description,
                                    UniversityName = note.UniversityName,
                                    Country = note.Country,
                                    Course = note.Course,
                                    CourseCode = note.CourseCode,
                                    Professor = note.Professor,
                                    SellingPrice = note.SellType == "Free" ? 0 : note.SellingPrice,
                                    IsPaid = note.SellType == "Free" ? false : true,
                                    NotesPreview = note.NotePreview,
                                    Status = 6,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true
                                });
                                _Context.SaveChanges();

                                var CreatedNote = noteDetails.FirstOrDefault(model => model.SellerId == currentuserId && model.Title == note.Title);

                                // make folder
                                string folder1 = Server.MapPath(string.Format("~/Members/{0}/{1}/Attachment", currentuserId, CreatedNote.Id));
                                Directory.CreateDirectory(folder1);
                                string extention_pdf = Path.GetExtension(UploadNotes.FileName);
                                string atta1 = "att1_2" + extention_pdf;
                                note.UploadNotes = "~/Members/" + currentuserId + "/" + CreatedNote.Id + "/" + "Attachment/" + atta1;
                                atta1 = Path.Combine(Server.MapPath(string.Format("~/Members/{0}/{1}/Attachment", currentuserId, CreatedNote.Id)), atta1);


                                UploadNotes.SaveAs(atta1);

                                var attachments = _Context.SellerNotesAttachements;
                                attachments.Add(new SellerNotesAttachement
                                {
                                    NoteId = CreatedNote.Id,
                                    FileName = UploadNotes.FileName,
                                    FilePath = note.UploadNotes,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true
                                });
                                var sell = noteDetails.FirstOrDefault(model => model.SellerId == currentuserId && model.Title == note.Title);
                                string extention_pic = Path.GetExtension(DisplayPicture.FileName);
                                string Pic = "DP" + extention_pic;
                                sell.DisplayPicture = "~/Members/" + currentuserId + "/" + CreatedNote.Id + "/" + Pic;

                                Pic = Path.Combine(Server.MapPath(string.Format("~/Members/{0}/{1}", currentuserId, CreatedNote.Id)), Pic);

                                DisplayPicture.SaveAs(Pic);
                                string extention_pre = Path.GetExtension(NotePreview.FileName);
                                string Pre = "predata" + extention_pre;
                                sell.NotesPreview = "~/Members/" + currentuserId + "/" + CreatedNote.Id + "/" + Pre;

                                Pre = Path.Combine(Server.MapPath(string.Format("~/Members/{0}/{1}", currentuserId, CreatedNote.Id)), Pre);


                                NotePreview.SaveAs(Pre);


                                _Context.SaveChanges();
                                return RedirectToAction("Dashboard", "User");



                            }
                        

                    }
                    else if (submitdata2 != null)
                    {
                     
                        if (!Id.Equals(null))
                        {
                            var noteDraft = _Context.SellerNotes.FirstOrDefault(model => model.Id == Id && model.Status == 6);
                            var draftAttachment = _Context.SellerNotesAttachements.FirstOrDefault(model => model.NoteId == Id);
                            note.MaptoModel(noteDraft, draftAttachment);

                            noteDraft.Status = 7;
                            noteDraft.ModifiedDate = DateTime.Now;
                            draftAttachment.ModifiedDate = DateTime.Now;

                            _Context.SaveChanges();

                            return RedirectToAction("Dashboard", "User");
                        }
                    }
                    else
                    {
                        var noteDetails = _Context.SellerNotes;
                        string folder = Server.MapPath(string.Format("~/Members/{0}", currentuserId));
                        Directory.CreateDirectory(folder);
                        string extention = Path.GetExtension(DisplayPicture.FileName);
                        string pathname = "DP_" + extention;
                        note.DisplayPicture = "~/Members/" + currentuserId + "/" + pathname;
                        pathname = Path.Combine(Server.MapPath(string.Format("~/Members/{0}", currentuserId)), pathname);
                        DisplayPicture.SaveAs(pathname);

                        noteDetails.Add(new SellerNote
                        {
                            Title = note.Title,
                            SellerId = currentuserId,
                            Category = note.Category,
                            DisplayPicture = note.DisplayPicture,
                            NoteType = note.NoteType,
                            NumberofPages = note.NumberofPages,
                            Description = note.Description,
                            UniversityName = note.UniversityName,
                            Country = note.Country,
                            Course = note.Course,
                            CourseCode = note.CourseCode,
                            Professor = note.Professor,
                            SellingPrice = note.SellType == "Free" ? 0 : note.SellingPrice,
                            IsPaid = note.SellType == "Free" ? false : true,
                            NotesPreview = note.NotePreview,
                            Status = 7,
                            CreatedDate = DateTime.Now,
                            IsActive = true
                        });
                        _Context.SaveChanges();

                        var createdNote = noteDetails.FirstOrDefault(model => model.SellerId == currentuserId && model.Title == note.Title).Id;

                        string path = MakeDirectory(currentuserId, createdNote);

                        var attachments = _Context.SellerNotesAttachements;
                        attachments.Add(new SellerNotesAttachement
                        {
                            NoteId = createdNote,
                            FileName = note.UploadNotes,
                            FilePath = path,
                            CreatedDate = DateTime.Now,
                            IsActive = true
                        });
                        _Context.SaveChanges();

                        return RedirectToAction("Dashboard", "User");
                    }
                    return View();

                }
                return View();
            }

               
           
        }

        public string MakeDirectory(int userId, int noteId)
        {
            string path = @"C:\Users\amipr\source\repos\NoteMarketPlaceHtml\Members" + userId + "\\" + noteId + "\\Attachment";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return path;
            }
            else
            {
                return null;
            }
        }

        // draft notes
        [HttpPost]
        [Route("User/Publish")]
        public ActionResult Publish(int? Id, AddNoteModel note, HttpPostedFileBase DisplayPicture)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddNote");
            }

            using (var _Context = new NoteMarketPlaceEntities())
            {
                // get current userId
                var currentuserId = _Context.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name).Id;

                // default book image
                // var bookDefaultImg = _Context.tblSystemConfigurations.FirstOrDefault(model => model.Key == "DefaultNoteDisplayPicture").Values;

                // pusblish as a draft notes
                if (!Id.Equals(null))
                {
                    var noteDraft = _Context.SellerNotes.FirstOrDefault(model => model.Id == Id && model.Status == 6);
                    var draftAttachment = _Context.SellerNotesAttachements.FirstOrDefault(model => model.NoteId == Id);
                    note.MaptoModel(noteDraft, draftAttachment);

                    noteDraft.Status = 7;
                    noteDraft.ModifiedDate = DateTime.Now;
                    draftAttachment.ModifiedDate = DateTime.Now;

                    _Context.SaveChanges();

                    return RedirectToAction("Dashboard", "User");
                }
                else
                {
                    var noteDetails = _Context.SellerNotes;
                    string folder = Server.MapPath(string.Format("~/Members/{0}", currentuserId));
                    Directory.CreateDirectory(folder);
                    string extention = Path.GetExtension(DisplayPicture.FileName);
                    string pathname = "DP_" + extention;
                    note.DisplayPicture = "~/Members/" + currentuserId + "/" + pathname;
                    pathname = Path.Combine(Server.MapPath(string.Format("~/Members/{0}", currentuserId)), pathname);
                    DisplayPicture.SaveAs(pathname);
                  
                    noteDetails.Add(new SellerNote
                    {
                        Title = note.Title,
                        SellerId = currentuserId,
                        Category = note.Category,
                        DisplayPicture = note.DisplayPicture,
                        NoteType = note.NoteType,
                        NumberofPages = note.NumberofPages,
                        Description = note.Description,
                        UniversityName = note.UniversityName,
                        Country = note.Country,
                        Course = note.Course,
                        CourseCode = note.CourseCode,
                        Professor = note.Professor,
                        SellingPrice = note.SellType == "Free" ? 0 : note.SellingPrice,
                        IsPaid = note.SellType == "Free" ? false : true,
                        NotesPreview = note.NotePreview,
                        Status = 7,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    });
                    _Context.SaveChanges();

                    var createdNote = noteDetails.FirstOrDefault(model => model.SellerId == currentuserId && model.Title == note.Title).Id;

                    string path = MakeDirectory(currentuserId, createdNote);

                    var attachments = _Context.SellerNotesAttachements;
                    attachments.Add(new SellerNotesAttachement
                    {
                        NoteId = createdNote,
                        FileName = note.UploadNotes,
                        FilePath = path,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    });
                    _Context.SaveChanges();

                    return RedirectToAction("Dashboard", "User");
                }


            }
        }

        // delete note
        [HttpPost]
        [Route("User/delete")]
        public string Delete(int Id)
        {
            using (var _Context = new NoteMarketPlaceEntities())
            {
                // get current user
                var currentuserId = _Context.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name).Id;

                var note = _Context.SellerNotes.SingleOrDefault(model => model.Id == Id && model.Status == 6 && model.SellerId == currentuserId);

                var attachment = _Context.SellerNotesAttachements.SingleOrDefault(model => model.NoteId == Id);

                _Context.SellerNotesAttachements.Remove(attachment);
                _Context.SellerNotes.Remove(note);
                _Context.SaveChanges();

            }

            return "Dashboard";
        }


        public string MakeDirectory(int userId)
        {
            string path = @"C:\Users\amipr\source\repos\NoteMarketPlaceHtml\Members" + userId;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return path;
            }
            else
            {
                return null;
            }
        }
        public ActionResult MyProfile()
        {
            using (var _Context = new NoteMarketPlaceEntities())
            {
                // get gender for dropdown
                var gender = _Context.ReferenceDatas.Where(m => m.RefCategory == "Gender").ToList();
                // get country
                var countryList = _Context.Countries.ToList();


                // get current userId
                var currentuser = _Context.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name);

                // get user details
                var isDetailsAvailable = db.UserProfiles.FirstOrDefault(m => m.UserI == currentuser.Id);


                var UserProfile = new AddUserProfileViewModel();

                // check user details available or not
                if (isDetailsAvailable != null)
                {
                    UserProfile = (from Detail in _Context.UserProfiles
                                   join User in _Context.Users on Detail.UserI equals User.Id
                                   join Country in _Context.Countries on Detail.Country equals Country.Name
                                   where Detail.UserI == currentuser.Id
                                   select new AddUserProfileViewModel
                                   {
                                       FirstName = User.FirstName,
                                       LastName = User.LastName,
                                       Email = User.EmailId,
                                       Gender = Detail.Gender,
                                       DOB = Detail.DOB,
                                       CountryCode = Detail.CountryCode,
                                       PhoneNumber = Detail.PhoneNumber,
                                       ProfilePicture = Detail.ProfilePicture,
                                       Address1 = Detail.AddressLine1,
                                       Address2 = Detail.AddressLine1,
                                       City = Detail.City,
                                       State = Detail.State,
                                       Zipcode = Detail.ZipCode,
                                       Country = Detail.Country,
                                       University = Detail.University,
                                       College = Detail.College
                                   }).FirstOrDefault<AddUserProfileViewModel>();

                    UserProfile.ProfilePicture = "DP.png";

                    UserProfile.genderModel = gender.Select(x => new GenderModel { Id = x.Id, Value = x.Values }).ToList();
                    UserProfile.countryModel = countryList.Select(x => new CountryModel { Id = x.Id, Name = x.Name }).ToList();
                    UserProfile.CountryCodeModel = countryList.Select(x => new CountryModel { CountryCode = x.CountryCode }).ToList();

                    return View(UserProfile);
                }
                // if user is first time login
                else
                {
                    UserProfile.FirstName = currentuser.FirstName;
                    UserProfile.LastName = currentuser.LastName;
                    UserProfile.Email = currentuser.EmailId;
                    UserProfile.genderModel = gender.Select(x => new GenderModel { Id = x.Id, Value = x.DataValue }).ToList();
                    UserProfile.countryModel = countryList.Select(x => new CountryModel { Id = x.Id, Name = x.Name }).ToList();
                    UserProfile.CountryCodeModel = countryList.Select(x => new CountryModel { CountryCode = x.CountryCode }).ToList();

                    return View(UserProfile);
                }

            }
        }

        [HttpPost]
        [Route("User/MyProfile")]
        public ActionResult MyProfile(AddUserProfileViewModel user,HttpPostedFileBase ProfilePicture)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("MyProfile");
            }

            using (var _Context = new NoteMarketPlaceEntities())
            {
                // get current userId
                int currentuser = _Context.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name).Id;

                // get user details
                var isDetailsAvailable = _Context.UserProfiles.FirstOrDefault(m => m.UserI == currentuser);

                // check user details available or not
                if (isDetailsAvailable != null && user != null)
                {
                    // update details
                    var userUpdate = _Context.Users.FirstOrDefault(m => m.Id == currentuser);
                    var detailsUpdate = _Context.UserProfiles.FirstOrDefault(m => m.UserI == currentuser);
                    userUpdate.FirstName = user.FirstName;
                    userUpdate.LastName = user.LastName;
                    userUpdate.EmailId= user.Email;
                    detailsUpdate.DOB = user.DOB;
                    detailsUpdate.Gender = user.Gender;
                    detailsUpdate.CountryCode = user.CountryCode;
                    detailsUpdate.PhoneNumber = user.PhoneNumber;
                    detailsUpdate.ProfilePicture = user. ProfilePicture;
                    detailsUpdate.AddressLine1 = user.Address1;
                    detailsUpdate.AddressLine2 = user.Address2;
                    detailsUpdate.City = user.City;
                    detailsUpdate.State = user.State;
                    detailsUpdate.ZipCode = user.Zipcode;
                    detailsUpdate.Country = user.Country;
                    detailsUpdate.College = user.College;
                    detailsUpdate.University = user.University;
                    

                    if (ProfilePicture == null)
                    {
                        detailsUpdate.ProfilePicture = detailsUpdate.ProfilePicture;
                    }
                    else
                    {
                        detailsUpdate.ProfilePicture = "../Members/" + currentuser + "/" +ProfilePicture.FileName;
                        string _path = System.IO.Path.Combine(Server.MapPath("~/Members/" + currentuser), ProfilePicture.FileName);
                        ProfilePicture.SaveAs(_path);
                    }
                    userUpdate.ModifiedDate = DateTime.Now;
                    detailsUpdate.ModifiedDate = DateTime.Now;

                    _Context.SaveChanges();

                    return RedirectToAction("Dashbord");
                }
                else
                {
                    // create new details
                    var create = _Context.UserProfiles;
                    create.Add(new UserProfile
                    {
                        UserI = currentuser,
                        DOB = user.DOB,
                        Gender = user.Gender,
                        CountryCode = user.CountryCode,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePicture = user.ProfilePicture == null ? null : "../Members/" + currentuser + "/" + user.ProfilePicture,
                        AddressLine1 = user.Address1,
                        AddressLine2 = user.Address2,
                        City = user.City,
                        State = user.State,
                        ZipCode = user.Zipcode,
                        Country = (user.Country),
                        University = user.University,
                        College = user.College,
                        CreatedDate = DateTime.Now
                    });

                    _Context.SaveChanges();

                    CreateDirectory(create.FirstOrDefault(m => m.UserI == currentuser).Id);

                    if (user.Image != null)
                    {
                        string _path = System.IO.Path.Combine(Server.MapPath("~/Members/" + currentuser), user.ProfilePicture);
                        user.Image.SaveAs(_path);
                    }

                    return RedirectToAction("MyProfile");

                }

            }

        }
        public string CreateDirectory(int userid)
        {

            string path = @"C:\Users\amipr\source\repos\NoteMarketPlaceHtml\Members\" + userid;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return path;
            }
            else
            {
                return null;
            }
        }
    }
}