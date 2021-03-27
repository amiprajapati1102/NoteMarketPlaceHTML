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
        
        public ActionResult SearchNotes()
        {
            return View();
        }

        [Authorize]
        [Route("User/Dashboard")]
        public ActionResult Dashboard()
        {
            using (var _Context = new NoteMarketPlaceEntities())
            {
             
                var currentUser = _Context.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name);

               
                var earning = (from download in _Context.Downloads
                               where download.Seller == currentUser.Id && download.IsSellerHasAllowedDownload == true
                               group download by download.Seller into total
                               select total.Sum(model => model.PurchasedPrice)).ToList();
                ViewBag.TotalEarning = earning.Count() == 0 ? 0 : earning[0];

                var soldNotes = (from download in _Context.Downloads
                                 where download.Seller == currentUser.Id
                                 group download by download.Seller into total
                                 select total.Count()).ToList();
                ViewBag.TotalSoldNotes = soldNotes.Count() == 0 ? 0 : soldNotes[0];

                var downloadNotes = (from download in _Context.Downloads
                                     where download.Downloader == currentUser.Id
                                     group download by download.Downloader into total
                                     select total.Count()).ToList();
                ViewBag.TotalDownloadNotes = downloadNotes.Count() == 0 ? 0 : downloadNotes[0];

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
               
                var type = _Context.NoteTypes.ToList();

           
                var category = _Context.NoteCategories.ToList();

                var country = _Context.Countries.ToList();

                ViewBag.CategoryList = category;
                ViewBag.NoteTypeList = type;
                ViewBag.CountryList = country;
                ViewBag.Edit = false;

              
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
                                    UploadNotes = Attachment.FileName,
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

        
       
        [Route("User/Save")]
        public ActionResult Save(int? Id, AddNoteModel note, HttpPostedFileBase UploadNotes, HttpPostedFileBase DisplayPicture,HttpPostedFileBase NotePreview)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddNote",note);
            }

            using (var _Context = new NoteMarketPlaceEntities())
            {
               
                var currentuserId = _Context.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name).Id;

                if (!Id.Equals(null))
                {
                    var noteDetails = _Context.SellerNotes.SingleOrDefault(model => model.Id == Id && model.Status == 6);
                    var attachment = _Context.SellerNotesAttachements.SingleOrDefault(model => model.NoteId == Id);

                    note.MaptoModel(noteDetails, attachment);
                    noteDetails.ModifiedDate = DateTime.Now;
                    attachment.ModifiedDate = DateTime.Now;

                    _Context.SaveChanges();

                    return RedirectToAction("Dashboard");
                }
               
                else
                {

                    if (!_Context.SellerNotes.Any(model => model.Title == note.Title))
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
                      

                        _Context.SaveChanges();
                        return RedirectToAction("Dashboard", "User");

                    }
                    else
                    {
                        return RedirectToAction("AddNote", "User");
                    }
                   
                }
            }
                
        }


       

       
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
               
                var currentuserId = _Context.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name).Id;

                
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
    }
}