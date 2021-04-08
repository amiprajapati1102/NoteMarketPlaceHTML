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
using System.Net;
using System.Net.Mail;

namespace NoteMarketPlaceHtml.Controllers
{
    public class UserController : Controller
    {
        readonly NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();
       
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
                    

                    if (img != null)
                    {
                        // set default image
                        ViewBag.UserProfile = img;
                        TempData["oldprofile"] = img;
                    }
                   
                    else
                    {
                        var defaultImg = db_1.SystemConfigurations.FirstOrDefault(m => m.KeyData == "DefaultMemberDisplayPicture").ValueData;
                        ViewBag.UserProfile = defaultImg;


                    }


                }
            }

        }


        [Authorize]
        [Route("User/Dashboard")]
        public ActionResult Dashboard()
        {
           
               
                var currentUser = db.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name);

                
                var earning = (from download in db.Downloads
                               where download.Seller == currentUser.Id && download.IsSellerHasAllowedDownload == true
                               group download by download.Seller into total
                               select total.Sum(model => model.PurchasedPrice)).ToList();
                ViewBag.TotalEarning = earning.Count() == 0 ? 0 : earning[0];

              
                var soldNotes = (from download in db.Downloads
                                 where download.Seller == currentUser.Id &&download.IsSellerHasAllowedDownload == true
                                 group download by download.Seller into total
                                 select total.Count()).ToList();
                ViewBag.TotalSoldNotes = soldNotes.Count() == 0 ? 0 : soldNotes[0];

             
                var downloadNotes = (from download in db.Downloads
                                     where download.Downloader == currentUser.Id && download.IsSellerHasAllowedDownload == true
                                     group download by download.Downloader into total
                                     select total.Count()).ToList();
                ViewBag.TotalDownloadNotes = downloadNotes.Count() == 0 ? 0 : downloadNotes[0];

              
                var rejectedNotes = (from notes in db.SellerNotes
                                     join status in db.ReferenceDatas on notes.Status equals status.Id
                                     where status.RefCategory == "Notes Status" && status.Values == "Rejected" && notes.SellerId == currentUser.Id
                                     group notes by notes.SellerId into total
                                     select total.Count()).ToList();
                ViewBag.TotalRejectedNotes = rejectedNotes.Count() == 0 ? 0 : rejectedNotes[0];

              
                var buyerRequest = (from download in db.Downloads
                                    where download.IsSellerHasAllowedDownload == false && download.Seller == currentUser.Id
                                    group download by download.Seller into total
                                    select total.Count()).ToList();
                ViewBag.TotalBuyerRequest = buyerRequest.Count() == 0 ? 0 : buyerRequest[0];

               
                var progressNotes = (from notes in db.SellerNotes
                                     join category in db.NoteCategories on notes.Category equals category.Id
                                     join status in db.ReferenceDatas on notes.Status equals status.Id
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

              
                var publishedNotes = (from notes in db.SellerNotes
                                      join category in db.NoteCategories on notes.Category equals category.Id
                                      join status in db.ReferenceDatas on notes.Status equals status.Id
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

        [Authorize]
        public ActionResult AddNote(int? edit)
        {
           
                
                var type = db.NoteTypes.ToList();

              
                var category = db.NoteCategories.ToList();

               
                var country = db.Countries.ToList();

                ViewBag.CategoryList = category;
                ViewBag.NoteTypeList = type;
                ViewBag.CountryList = country;
            
               
               
                if (!edit.Equals(null))
                {
                    var note = (from Notes in db.SellerNotes
                                join Attachment in db.SellerNotesAttachements on Notes.Id equals Attachment.NoteId
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
                                    SellType = Notes.IsPaid == true ? "Paid" : "Free",
                                    SellingPrice = (decimal)Notes.SellingPrice,
                                    NotePreview = Notes.NotesPreview
                                }).FirstOrDefault<AddNoteModel>();
                 
                    return View(note);
                }
                else
                {
                    AddNoteModel model = new AddNoteModel();
                   
                    return View(model);
                }

              
            
        }


        [HttpPost]

        public ActionResult AddNote(int? Id, AddNoteModel note)
        {
            
                var type = db.NoteTypes.ToList();

                // all category
                var category = db.NoteCategories.ToList();

                // all country
                var country = db.Countries.ToList();

                ViewBag.CategoryList = category;
                ViewBag.NoteTypeList = type;
                ViewBag.CountryList = country;
                ViewBag.Edit = false;
           
            if (ModelState.IsValid)
            {
               
           
                var currentuser= db.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name);
                var DefaultNoteDisplayPicture = db.SystemConfigurations.FirstOrDefault(m => m.KeyData == "DefaultNoteDisplayPicture").ValueData;
                var DefaultNote = db.SystemConfigurations.FirstOrDefault(m => m.KeyData == "DefaultNote").ValueData;
             

                if (!Id.Equals(null))
                {
                    var noteDetails = db.SellerNotes.SingleOrDefault(model => model.Id == Id && (model.Status == 6));
                    var attachment = db.SellerNotesAttachements.SingleOrDefault(model => model.NoteId == Id);

                    if (UploadNotes == null)
                    {
                        attachment.FilePath = attachment.FilePath;
                        attachment.FileName = attachment.FileName;
                    }
                    else
                    {
                        string old = Server.MapPath(attachment.FilePath);
                        FileInfo fileInfo = new FileInfo(old);
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();

                        }
                        string atta1 = UploadNotes.FileName;
                        attachment.FilePath = "../Members/" + currentuser.Id + "/" + Id + "/" + "Attachment/" + atta1;
                        atta1 = Path.Combine(Server.MapPath(string.Format("../Members/{0}/{1}/Attachment", currentuser.Id, Id)), atta1);
                        UploadNotes.SaveAs(atta1);
                        attachment.FileName = UploadNotes.FileName;
                    }
                    if (NotePreview == null)
                    {
                        noteDetails.NotesPreview = noteDetails.NotesPreview;

                    }
                    else
                    {
                        string old = Server.MapPath(noteDetails.NotesPreview);
                        FileInfo fileInfo = new FileInfo(old);
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();

                        }
                    
                        noteDetails.NotesPreview = "../Members/" + currentuser.Id+ "/" + Id + "/" + NotesPreview.FileName;
                       string  atta1 = Path.Combine(Server.MapPath(string.Format("../Members/{0}/{1}", currentuser.Id, Id)), atta1);
                        NotePreview.SaveAs(atta1);

                    }
                    if (DisplayPicture == null)
                    {
                        noteDetails.DisplayPicture = noteDetails.DisplayPicture;

                    }
                    else
                    {
                        string old = Server.MapPath(noteDetails.DisplayPicture);
                        FileInfo fileInfo = new FileInfo(old);
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();

                        }
                        string extention_pic = Path.GetExtension(DisplayPicture.FileName);
                        string Pic = "DP_" + extention_pic;
                        noteDetails.DisplayPicture = "../Members/" + currentuser.Id + "/" + Id + "/" + Pic;

                        Pic = Path.Combine(Server.MapPath(string.Format("../Members/{0}/{1}", currentuser.Id, Id)), Pic);

                        DisplayPicture.SaveAs(Pic);

                    }

                    noteDetails.Title = note.Title;
                    noteDetails.Category = note.Category;

                    noteDetails.NoteType = note.NoteType;
                    noteDetails.NumberofPages = note.NumberofPages;
                    noteDetails.Description = note.Description;
                    noteDetails.UniversityName = note.UniversityName;
                    noteDetails.Country = note.Country;
                    noteDetails.Course = note.Course;
                    noteDetails.CourseCode = note.CourseCode;
                    noteDetails.Professor = note.Professor;
                    noteDetails.IsPaid = note.SellType == "Free" ? false : true;
                    noteDetails.SellingPrice = note.SellType == "Free" ? 0 : note.SellingPrice;


                    noteDetails.ModifiedDate = DateTime.Now;
                    attachment.ModifiedDate = DateTime.Now;
                    noteDetails.Status =  6;
                    db.SaveChanges();
                  
                  

                    return RedirectToAction("Dashboard");
                }
                else
                {
                    SellerNote noteDetails = new SellerNote
                    {
                        Title = note.Title,
                        SellerId = currentuser.Id,
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
                        Status =  6,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };
                    db.SellerNotes.Add(noteDetails);
                    db.SaveChanges();

                    var CreatedNote = db.SellerNotes.FirstOrDefault(model => model.SellerId == currentuser.Id && model.Title == note.Title);

                    // make folder
                    string folder1 = Server.MapPath(string.Format("../Members/{0}/{1}/Attachment", currentuser.Id, CreatedNote.Id));
                    Directory.CreateDirectory(folder1);

                    string atta1 = UploadNotes.FileName;
                    note.UploadNotes = "../Members/" + currentuser.Id + "/" + CreatedNote.Id + "/" + "Attachment/" + atta1;
                    atta1 = Path.Combine(Server.MapPath(string.Format("../Members/{0}/{1}/Attachment", currentuser.Id, CreatedNote.Id)), atta1);


                    UploadNotes.SaveAs(atta1);

                    var attachments = db.SellerNotesAttachements;
                    attachments.Add(new SellerNotesAttachement
                    {
                        NoteId = CreatedNote.Id,
                        FileName = UploadNotes.FileName,
                        FilePath = note.UploadNotes,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    });
                    var sell = db.SellerNotes.FirstOrDefault(model => model.SellerId == currentuser.Id && model.Title == note.Title);
                    if (DisplayPicture == null)
                    {
                        sell.DisplayPicture = DefaultNoteDisplayPicture;

                    }
                    else
                    {
                        string extention_pic = Path.GetExtension(DisplayPicture.FileName);
                        string Pic = "DP_" + extention_pic;
                        sell.DisplayPicture = "../Members/" + currentuser.Id + "/" + CreatedNote.Id + "/" + Pic;

                        Pic = Path.Combine(Server.MapPath(string.Format("../Members/{0}/{1}", currentuser.Id, CreatedNote.Id)), Pic);

                        DisplayPicture.SaveAs(Pic);


                    }


                    if (NotePreview == null)
                    {
                        sell.NotesPreview = DefaultNote;

                    }
                    else
                    {
                        string extention_pre = Path.GetExtension(NotePreview.FileName);
                        string Pre = "Preview_"+extention_pre;
                        sell.NotesPreview = "../Members/" + currentuser.Id + "/" + CreatedNote.Id + "/" + Pre;

                        Pre = Path.Combine(Server.MapPath(string.Format("../Members/{0}/{1}", currentuser.Id, CreatedNote.Id)), Pre);


                        NotePreview.SaveAs(Pre);


                    }



                    db.SaveChanges();
                   

                    return RedirectToAction("Dashboard", "User");
                }

                



            }
           
                return View();
                // return RedirectToAction("AddNote", new { edit = note.Id});
            }

           
        }

       

        [HttpPost]
        [Route("User/delete")]
        public string Delete(int Id)
        {
            
                // get current user
                var currentuserId = db.Users.FirstOrDefault(model => model.EmailId == User.Identity.Name).Id;

                var note = db.SellerNotes.SingleOrDefault(model => model.Id == Id && model.Status == 6 && model.SellerId == currentuserId);

                var attachment = db.SellerNotesAttachements.SingleOrDefault(model => model.NoteId == Id);

                db.SellerNotesAttachements.Remove(attachment);
                db.SellerNotes.Remove(note);
                db.SaveChanges();

            

            return "Dashboard";
        }


  
        public ActionResult MyProfile()
        {
            
                // get gender for dropdown
                var gender = db.ReferenceDatas.Where(m => m.RefCategory == "Gender").ToList();
                // get country
                var countryList = db.Countries.ToList();


                // get current userId
                var currentuser = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name);

                // get user details
                var isDetailsAvailable = db.UserProfiles.FirstOrDefault(m => m.UserI == currentuser.Id);

                
                var UserProfile = new AddUserProfileViewModel();

                // check user details available or not
                if (isDetailsAvailable != null)
                {
                    UserProfile = (from Detail in db.UserProfiles
                                   join User in db.Users on Detail.UserI equals User.Id
                                   join Country in db.Countries on Detail.Country equals Country.Name
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

        [HttpPost]
       
        public ActionResult MyProfile(AddUserProfileViewModel user,HttpPostedFileBase ProfilePicture)
        {
            int currentuser = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name).Id;
            var defaultImg = db.SystemConfigurations.FirstOrDefault(m => m.KeyData == "DefaultMemberDisplayPicture").ValueData;

            // get user details
            var isDetailsAvailable = db.UserProfiles.FirstOrDefault(m => m.UserI == currentuser);
            if (ModelState.IsValid)
            {
                    // check user details available or not
                    if (isDetailsAvailable != null && user != null)
                    {
                        // update details
                        var userUpdate = db.Users.FirstOrDefault(m => m.Id == currentuser);
                        var detailsUpdate = db.UserProfiles.FirstOrDefault(m => m.UserI == currentuser);
                        userUpdate.FirstName = user.FirstName;
                        userUpdate.LastName = user.LastName;
                        userUpdate.EmailId = user.Email;
                        detailsUpdate.DOB = user.DOB;
                        detailsUpdate.Gender = user.Gender;
                        detailsUpdate.CountryCode = user.CountryCode;
                        detailsUpdate.PhoneNumber = user.PhoneNumber;

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
                       
                            string old = Server.MapPath(detailsUpdate.ProfilePicture);
                            FileInfo fileInfo = new FileInfo(old);
                            if (fileInfo.Exists)
                            {
                                fileInfo.Delete();

                            }
                            string extention_pic = Path.GetExtension(ProfilePicture.FileName);
                            string Pic = "DP_" + extention_pic;
                            detailsUpdate.ProfilePicture = "../Members/" + currentuser + "/" + Pic;

                            Pic = Path.Combine(Server.MapPath(string.Format("../Members/{0}", currentuser)), Pic);

                            ProfilePicture.SaveAs(Pic);

                        
                    }
                        userUpdate.ModifiedDate = DateTime.Now;
                        detailsUpdate.ModifiedDate = DateTime.Now;

                        db.SaveChanges();
                        return RedirectToAction("Dashboard");
                    }
                    else
                    {

                    UserProfile create = new UserProfile
                    {
                        UserI = currentuser,
                        DOB = user.DOB,
                        Gender = user.Gender,
                        CountryCode = user.CountryCode,
                        PhoneNumber = user.PhoneNumber,

                        AddressLine1 = user.Address1,
                        AddressLine2 = user.Address2,
                        City = user.City,
                        State = user.State,
                        ZipCode = user.Zipcode,
                        Country = (user.Country),
                        University = user.University,
                        College = user.College,
                        CreatedDate = DateTime.Now
                    };

                    if (ProfilePicture == null)
                    {
                        create.ProfilePicture = defaultImg;
                        db.UserProfiles.Add(create);
                    }
                    else
                    {
                        string extention_pic = Path.GetExtension(ProfilePicture.FileName);
                        string Pic = "DP_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extention_pic;
                        user.ProfilePicture = "../Members/" + currentuser + "/" + Pic;

                        Pic = Path.Combine(Server.MapPath(string.Format("../Members/{0}", currentuser)), Pic);

                        ProfilePicture.SaveAs(Pic);
                        db.UserProfiles.Add(create);
                    }
                         db.SaveChanges();





                        return RedirectToAction("Dashboard");

                    }


            }
            
           
            return View(user);

        }
       
        public ActionResult BuyerRequest()
        {
            // current login user email
            string userEmail = User.Identity.Name;

           
                var result = (from download in db.Downloads
                              join Note in db.SellerNotes on download.NoteID equals Note.Id
                              join Downloader in db.Users on download.Downloader equals Downloader.Id
                              join Details in db.UserProfiles on download.Downloader equals Details.UserI
                              join Seller in db.Users on download.Seller equals Seller.Id
                              join Category in db.NoteCategories on Note.Category equals Category.Id
                              where download.IsSellerHasAllowedDownload == false && Seller.EmailId == userEmail
                              select new ByuerRequestViewModel
                              {
                                  NoteId = download.NoteID,
                                  DownloadId = download.Id,
                                  Title = Note.Title,
                                  Category = Category.Name,
                                  Buyer = Downloader.EmailId,
                                  PhoneNo=Details.PhoneNumber,
                                  Selltype = download.downloaddPrice == 0 ? "Free" : "Paid",
                                  Price = (decimal)download.PurchasedPrice,
                                  RequestDate = (DateTime)Purchase.CreatedDate
                              }).OrderByDescending(m => m.RequestDate).ToList();

                return View(result);

            


        }
        public HttpStatusCodeResult AllowDownload(int id)
        {
            if (id.Equals(null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

             
                var result = db.Downloads.FirstOrDefault(m => m.Id == id);
                var seller = db.Users.FirstOrDefault(m => m.Id == result.Seller);
                var downloader = db.Users.FirstOrDefault(m => m.Id == result.Downloader);

                // if result not available
                if (result != null)
                {
                    // set allowDownload true
                    result.IsSellerHasAllowedDownload = true;
                   
                    db.SaveChanges();

                    using (MailMessage mm = new MailMessage("email@gmail.com", seller.EmailId))
                    {
                        mm.Subject = seller.FirstName + " Allows you to download a note";

                        string body = string.Empty;
                        using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/AllowDownload.html")))
                        {
                            body = reader.ReadToEnd();
                        }
                        string downloaderName = downloader.FirstName;
                        string sellerName = seller.FirstName;
                        body = body.Replace("{downloder}", downloaderName);
                        body = body.Replace("{seller}", sellerName);
                        mm.Body = body;
                        mm.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.EnableSsl = true;
                        NetworkCredential NetworkCred = new NetworkCredential("email@gmail.com", "pass");
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = NetworkCred;
                        smtp.Port = 587;
                        smtp.Send(mm);

                    }

                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

            

        }



        public ActionResult MyDownloads()
        {
           
                // current login userId
                int currentUser = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name).Id;

                var result = (from download in db.Downloads
                              join Note in db.SellerNotes on download.NoteID equals Note.Id
                              join Downloader in db.Users on download.Downloader equals Downloader.Id
                              join Seller in db.Users on download.Seller equals Seller.Id
                              join Category in db.NoteCategories on Note.Category equals Category.Id
                              where download.IsSellerHasAllowedDownload == true && download.Downloader == currentUser
                              select new MyDownoadViewModel
                              {
                                  NoteId = download.NoteID,
                                  DowloadId = download.Id,
                                  Title = Note.Title,
                                  Category = Category.Name,
                                  Buyer = Downloader.EmailId,
                                  Price = (decimal)Purchase.PurchasedPrice,
                                  SellType = download.downloaddPrice == 0 ? "Free" : "Paid",
                                  DownloadDate = Purchase.AttachmentDownloadedDate
                              }).ToList();

                return View(result);
            
        }
        public FileResult DownloadNote(int? purchaseId, int? noteId)
        {
            
            if (!noteId.Equals(null))
            {
               //rejectedNotes
                    int currentuser = db.Users.SingleOrDefault(m => m.EmailId == User.Identity.Name).Id;
                    var note = (from Note in db.SellerNotes
                                join Attachment in db.SellerNotesAttachements on Note.Id equals Attachment.NoteId
                                where Note.Id == noteId && Note.Status == 10 && Note.SellerId == currentuser
                                select new { Attachment.FilePath ,Attachment.FileName}).SingleOrDefault();

                    string file = Server.MapPath(note.FilePath);
                    byte[] filebyte = GetFile(file);
                    return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, note.FileName);

                

            }
            
            else
            {
                
                    int currentuser = db.Users.SingleOrDefault(m => m.EmailId == User.Identity.Name).Id;
                    var note = (from download in db.Downloads
                                join Attachment in db.SellerNotesAttachements on download.NoteID equals Attachment.NoteId
                                where download.Id == downloadId &&
                                (download.Seller == currentuser || (download.Downloader == currentuser && download.IsSellerHasAllowedDownload == true))
                                select new { Attachment.NoteId, Attachment.FilePath, Attachment.FileName, download.Downloader, download.Seller }).SingleOrDefault();

                    if (note != null)
                    {

                        string file = Server.MapPath(note.FilePath);

                        if (currentuser == note.Downloader)
                        {
                            var update = db.Downloads.FirstOrDefault(m => m.Id == purchaseId && m.Downloader == currentuser);
                            update.IsAttachmentDownloaded = true;
                            update.AttachmentDownloadedDate = DateTime.Now;
                            db.SaveChanges();
                        }

                       
                        byte[] filebyte = GetFile(file);
                        return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, note.FileName);
                    }
                    else
                    {
                        return null;
                    }
                
            }
        }
        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
            {
                throw new System.IO.IOException(s);
            }
            return data;
        }


        public ActionResult MySoldNotes()
        {
            int currentUser = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name).Id;
            // current login userId
            var result = (from download in db.Downloads
                          join Note in db.SellerNotes on download.NoteID equals Note.Id
                          join Downloader in db.Users on download.Downloader equals Downloader.Id
                          join Seller in db.Users on download.Seller equals Seller.Id
                          join Category in db.NoteCategories on Note.Category equals Category.Id
                          where download.IsSellerHasAllowedDownload == true && download.Seller == currentUser
                          select new MySoldNotesViewModel
                          {
                              Id = download.Id,
                              NoteId = download.NoteID,
                              Title = Note.Title,
                              Category = Category.Name,
                              Buyer = Downloader.EmailId,
                              SellType = download.PurchasedPrice == 0 ? "Free" : "Paid",
                              Price = (decimal)download.PurchasedPrice,
                              DownloadDate = (DateTime)download.AttachmentDownloadedDate
                          }).ToList();

            return View(result);



        }


      
        [Route("User/MyRejectedNotes")]
        public ActionResult MyRejectedNotes()
        {
           
                // current login userId
                int currentUser = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name).Id;

                var result = (from Notes in db.SellerNotes
                              join Status in db.ReferenceDatas on Notes.Status equals Status.Id
                              join Category in db.NoteCategories on Notes.Category equals Category.Id
                              join attachment in db.SellerNotesAttachements on Notes.Id equals attachment.NoteId
                              where Status.RefCategory == "Notes Status" && Status.Values == "Rejected" && Notes.SellerId == currentUser
                              select new MyRejectedNoteViewModel
                              {
                                  Id = Notes.Id,
                                  Title = Notes.Title,
                                  Category = Category.Name,
                                  Remark = Notes.AdminRemarks,
                                 
                              }).ToList();

                return View(result);
            


        }
       
        [HttpPost]
        public ActionResult MemberReview(int DowloadId, int review_rating, string reviewcomment)
        {

           
                var currentuser = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name).Id;

                var purchase = db.Downloads.FirstOrDefault(m => m.Id == DowloadId && m.Downloader == currentuser);

                var review = db.SellerNotesReviews;
                review.Add(new SellerNotesReview
                {
                    NoteId = purchase.NoteID,
                    ReviewedById = currentuser,
                    AgainstDownloadsId = Id,
                    Comments = reviewcomment,
                    Ratings = review_rating,
                    CreatedDate = DateTime.Now
                });

                db.SaveChanges();

                return RedirectToAction("MyDownloads");
            

         
            
        }
        public ActionResult UserReport(int Id, string UserRemarks)
        {

            var currentuser = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name);

            var purchase = db.Downloads.FirstOrDefault(m => m.Id == Id && m.Downloader == currentuser.Id);

            var note = db.SellerNotes.First(m => m.Id == purchase.NoteID);

            var seller = db.Users.First(m => m.Id == note.SellerId);

                    SellerNotesReportedIssue review = new SellerNotesReportedIssue
                    {

                        NoteId = purchase.NoteID,
                        ReportedById = currentuser.Id,
                        AgainstDownloadId = Id,
                        Remarks = UserRemarks,
                        CreatedDate = DateTime.Now
                    };
                 db.SellerNotesReportedIssues.Add(review);

                db.SaveChanges();

               
                var Adminemail = db.SystemConfigurations.Where(m => m.KeyData == "EmailAddresssesForNotify ").First().ValueData;

               
                using (MailMessage mm = new MailMessage("email@gmail.com", Adminemail))
                {
                    mm.Subject = currentuser.FirstName + " Reported an issue for"+note.Title;

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/ReportIssue.html")))
                    {
                        body = reader.ReadToEnd();
                    }
                    string user = currentuser.FirstName;
                    string sellerName = seller.FirstName;
                    string Title = note.Title;
                    body = body.Replace("{FirstName}", user);
                    body = body.Replace("{seller}", sellerName);
                    body = body.Replace("{Title}", Title);
                    mm.Body = body;
                    mm.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    NetworkCredential NetworkCred = new NetworkCredential("email@gmail.com", "pass");
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = 587;
                    smtp.Send(mm);

                }


                return RedirectToAction("MyDownloads");
            
        }
        public ActionResult CloneNote(int noteId)
        {
           
              


                var clonenote = db.SellerNotes;
                clonenote.Add(new SellerNote
                {
                    Title = oldnote.Note.Title,
                    Category = oldnote.Note.Category,
                    DisplayPicture = oldnote.Note.DisplayPicture,
                    NoteType = oldnote.Note.NoteType,
                    NumberofPages = oldnote.Note.NumberofPages,
                    Description = oldnote.Note.Description,
                    UniversityName = oldnote.Note.UniversityName,
                    Country = oldnote.Note.Country,
                    Course = oldnote.Note.Course,
                    CourseCode = oldnote.Note.CourseCode,
                    Professor = oldnote.Note.Professor,
                    SellingPrice = oldnote.Note.SellingPrice,
                    NotesPreview = oldnote.Note.NotesPreview,
                    Status = 6,
                    CreatedDate = DateTime.Now,
                    SellerId = currentuser.Id,
                    IsActive = true
                });

                db.SaveChanges();

                // get created note Id
               
                    


                    var attachments = db.SellerNotesAttachements;
                    attachments.Add(new SellerNotesAttachement
                    {
                        NoteId = newnote.Id,
                        FileName = atta1,
                        FilePath = newfile,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    });
                   
                       
                    db.SaveChanges();
            return RedirectToAction("Dashboard");
        }




    }
}