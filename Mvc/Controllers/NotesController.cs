using NoteMarketPlaceHtml.DbModel;
using NoteMarketPlaceHtml.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NoteMarketPlaceHtml.Controllers
{
    public class NotesController : Controller
    {
        // GET: Notes
        NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (User.Identity.IsAuthenticated)
            {
                using (var db_1 = new NoteMarketPlaceEntities())
                {
                   
                    var img = (from Details in db_1.AddAdmins
                               join Users in db_1.Users on Details.UserId equals Users.Id
                               where Users.EmailId == requestContext.HttpContext.User.Identity.Name
                               select Details.ProfilePicture).FirstOrDefault();
                   

                    if (img != null)
                    {
                       
                        ViewBag.UserProfile = img;
                      
                    }
                   
                    else
                    {
                        var defaultImg = db_1.SystemConfigurations.FirstOrDefault(m => m.KeyData == "DefaultMemberDisplayPicture").ValueData;
                        ViewBag.UserProfile = defaultImg;


                    }


                }
            }

        }
        public ActionResult SearchNotes(int? Type, int? Category, string University, string Course, int? Country, int? Rating, string search)
        {

            var type = db.NoteTypes.ToList();
           
            var category = db.NoteCategories.ToList();
           
            var university = db.SellerNotes.Where(m => m.UniversityName != null).Select(x => x.UniversityName).Distinct().ToList();
            
            var course = db.SellerNotes.Where(m => m.Course != null).Select(x => x.Course).Distinct().ToList();
           
            var country = db.Countries.ToList();
            var notes = (from Notes in db.SellerNotes
                         join Status in db.ReferenceDatas on Notes.Status equals Status.Id
                         where Status.Values == "Published" && Notes.IsActive == true
                         let avgRatings = (from Review in db.SellerNotesReviews
                                           where Review.NoteId == Notes.Id
                                           group Review by Review.NoteId into grp
                                           select new AvgRatings
                                           {
                                               Rating = (double)Math.Round(grp.Average(m => m.Ratings)),
                                               Total = grp.Count()
                                           })
                         let spamNote = (from Spam in db.SellerNotesReportedIssues
                                         where Spam.NoteId == Notes.Id
                                         group Spam by Spam.NoteId into grp
                                         select new SpamNote
                                         {
                                             Total = grp.Count()
                                         })
                         select new SearchModelViewModel
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
            else
            {
                return View(filternotes);
            }


            return View(filternotes);



        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult NotesUnderReview(int ? SellerId)
        {
            using (var db = new NoteMarketPlaceEntities())
            {
             
                var seller = (from Notes in db.SellerNotes
                              join User in db.Users on Notes.SellerId equals User.Id
                              where Notes.Status == 6 
                              group new { Notes, User } by Notes.SellerId into grp
                              select new SellerDetailsModel
                              {
                                  SellerId = grp.Select(x => x.User.Id).FirstOrDefault(),
                                  Name = grp.Select(x => x.User.FirstName).FirstOrDefault() + " " + grp.Select(x => x.User.LastName).FirstOrDefault()
                              }).ToList();

                ViewBag.SellerList = seller;
                var model = (from Notes in db.SellerNotes
                             join Status in db.ReferenceDatas on Notes.Status equals Status.Id
                             join attachment in db.SellerNotesAttachements on Notes.Id equals attachment.NoteId
                             join Category in db.NoteCategories on Notes.Category equals Category.Id
                             join User in db.Users on Notes.SellerId equals User.Id
                             where Notes.Status == 6
                             select new NoteUnderReviewViewModel
                             {
                                 NoteId = Notes.Id,
                                 Title = Notes.Title,
                                 Category = Category.Name,
                                 SellerId = User.Id,
                                 Seller = User.FirstName + " " + User.LastName,
                                 status = Status.Values,
                                 NotePath=attachment.FilePath,
                                 DateAdded = (DateTime)Notes.CreatedDate
                             }).OrderByDescending(x => x.DateAdded).ToList();

                var filternotes = model;
                if (!SellerId.Equals(null))
                {
                    
                    filternotes = filternotes.Where(m => m.SellerId == SellerId).ToList();
                }
                return View(filternotes);


            }
        }
        [HttpPost]
       
        [Authorize(Roles = "SuperAdmin,Admin")]
        public void NoteStatusUpdate(int noteid, string status)
        {
            using (var db = new NoteMarketPlaceEntities())
            {
                var currentAdmin = db.Users.Single(m => m.EmailId == User.Identity.Name).Id;

                var note = db.SellerNotes.Single(m => m.Id == noteid);

                switch (status)
                {
                    case "InReview":
                        note.Status = 8;
                        break;
                    case "Approve":
                        note.Status = 9;
                        break;
                }

                note.ActionBy = currentAdmin;
                note.PublishedDate = DateTime.Now;
                note.ModifiedBy = currentAdmin;
                note.ModifiedDate = DateTime.Now;

                db.SaveChanges();
            }

        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult PublishedNotes(int? sellerId)
        {
            using (var db = new NoteMarketPlaceEntities())
            {
                // seller names
                var seller = (from Notes in db.SellerNotes
                              join User in db.Users on Notes.SellerId equals User.Id
                              where Notes.Status == 9
                              group new { Notes, User } by Notes.SellerId into grp
                              select new SellerDetailsModel
                              {
                                  SellerId = grp.Select(x => x.User.Id).FirstOrDefault(),
                                  Name = grp.Select(x => x.User.FirstName).FirstOrDefault() + " " + grp.Select(x => x.User.LastName).FirstOrDefault()
                              }).ToList();

                ViewBag.SellerList = seller;


                // set model value
                var model = (from Notes in db.SellerNotes
                             join User in db.Users on Notes.SellerId equals User.Id
                             join Admin in db.Users on Notes.ActionBy equals Admin.Id
                             join Category in db.NoteCategories on Notes.Category equals Category.Id
                             join attachment in db.SellerNotesAttachements on Notes.Id equals attachment.NoteId
                             where Notes.Status == 9
                             let total = (db.Downloads.Where(m => m.NoteID == Notes.Id && m.IsSellerHasAllowedDownload == true).Count())
                             select new PublishNoteViewModel
                             {
                                 NoteId = Notes.Id,
                                 Title = Notes.Title,
                                 Category = Category.Name,
                                 Price = (decimal)Notes.SellingPrice,
                                 SellerId = Notes.SellerId,
                                 Seller = User.FirstName + " " + User.LastName,
                                 ActionBy = Admin.FirstName + " " + Admin.LastName,
                                 PublishDate = (DateTime)Notes.PublishedDate,
                                 NotePath=attachment.FilePath,
                                 TotalDownloads = total
                             }).OrderByDescending(x => x.PublishDate).ToList();

                if (sellerId == null)
                {
                    return View(model);
                }
                else
                {
                    var filterdata = model.Where(m => m.SellerId == sellerId).ToList();
                    return View(filterdata);
                }


            }


        }


        public ActionResult NoteDetails(int? id, bool? ReadOnly)
        {
           
            using (var db = new NoteMarketPlaceEntities())
            {
                
                var defaultuserImg = db.SystemConfigurations.FirstOrDefault(m => m.KeyData == "DefaultMemberDisplayPicture ").ValueData;
                // get note details
                var notes = (from Notes in db.SellerNotes
                             join Category in db.NoteCategories on Notes.Category equals Category.Id
                             let Country = db.Countries.FirstOrDefault(m => m.Id == Notes.Country)
                             join Users in db.Users on Notes.SellerId equals Users.Id
                             where Notes.Id == id && (Notes.Status == 6 || Notes.Status == 7 || Notes.Status == 8|| Notes.Status == 9)
                             select new NoteDetailsViewModel
                             {
                                 Id = Notes.Id,
                                 Title = Notes.Title,
                                 Category = Category.Name,
                                 Description = Notes.Description,
                                 Image = Notes.DisplayPicture,
                                 Price = (decimal)Notes.SellingPrice,
                                 Institute = Notes.UniversityName == null ? "--" : Notes.UniversityName,
                                 Country = Country.Name == null ? "--" : Country.Name,
                                 CourseName = Notes.Course == null ? "--" : Notes.Course,
                                 CourseCode = Notes.CourseCode == null ? "--" : Notes.CourseCode,
                                 Professor = Notes.Professor == null ? "--" : Notes.Professor,
                                 Pages = (decimal)(Notes.NumberofPages == null ? 0 : Notes.NumberofPages),
                                 ApprovedDate = Notes.PublishedDate,
                                 NotePreview = Notes.NotesPreview,
                                 Seller = Users.FirstName + " " + Users.LastName,
                                 Status = Notes.Status
                             }).ToList();

                for (int i = 0; i < notes.Count; i++)
                {
                    notes[i].ApproveDate = notes[i].ApprovedDate.HasValue ? notes[i].ApprovedDate.GetValueOrDefault().ToString("MMMM dd yyyy") : "N/A";
                }


             
                var avg = db.SellerNotesReviews.Where(m => m.NoteId == id).ToList();
                if (avg.Count() > 0)
                {
                    var avgReview = Math.Round(Double.Parse(avg.Average(m => m.Ratings).ToString()));
                    var count = avg.Count();
                    ViewBag.TotalReview = count;
                    ViewBag.AverageReview = avgReview;
                }
                else
                {
                    ViewBag.TotalReview = 0;
                    ViewBag.AverageReview = 0;
                }

                var spam = db.SellerNotesReportedIssues.Where(m => m.NoteId == id).Count();
                ViewBag.Spam = spam;

             
                var reviews = (from Review in db.SellerNotesReviews
                               join User in db.Users on Review.AgainstDownloadsId equals User.Id
                               join UserDetail in db.UserProfiles on User.Id equals UserDetail.UserI
                               where Review.NoteId == id
                               select new CustomerReview
                               {
                                   First_Name = User.FirstName,
                                   Last_Name = User.LastName,
                                   Image = UserDetail.ProfilePicture == null ? defaultuserImg : UserDetail.ProfilePicture,
                                   Ratings = (int)Review.Ratings,
                                   Review = Review.Comments
                               }).OrderByDescending(m => m.Ratings).ToList();

                ViewBag.Reviews = reviews;
                if (!User.Identity.IsAuthenticated)
                {
                    TempData["ReadOnly"] = "true";

                }

                if (ReadOnly != null && ReadOnly == true)
                {
                    ViewBag.NoteDetails = notes;

                    TempData["ReadOnly"] = "true";
                    return View();
                }
               
                else
                {
                    ViewBag.NoteDetails = notes.Where(m => m.Status == 9).ToList();
                    return View();
                }


            }

        }

        [Authorize(Roles = "Member")]
        public ActionResult BuyNote(string noteId)
        {
            int noteid = int.Parse(noteId);

            using (var db = new NoteMarketPlaceEntities())
            {

                var user = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name);
                var note = db.SellerNotes.FirstOrDefault(m => m.Id == noteid);

                var catagory = db.NoteCategories.FirstOrDefault(m => m.Id == note.Category);
                var attachment = db.SellerNotesAttachements.FirstOrDefault(m => m.NoteId == noteid);
                string filePath = Server.MapPath(attachment.FilePath);
                byte[] filebyte = GetFile(filePath);

                if (note != null && !user.Equals(null))
                {
                    Download create = new Download();

                    if (note.SellingPrice == 0)
                    {

                        create.Downloader = user.Id;
                        create.NoteID = noteid;
                        create.Seller = note.SellerId;
                        create.PurchasedPrice = note.SellingPrice;
                        create.AttachmentDownloadedDate = DateTime.Now;
                        create.IsSellerHasAllowedDownload = true;
                        create.IsAttachmentDownloaded = true;
                        create.AttachmentPath = attachment.FilePath;
                        create.IsPaid = note.IsPaid;
                        create.NoteTitle = attachment.FileName;
                        create.NoteCategory = catagory.Name;
                        create.CreatedDate = DateTime.Now;

                        db.Downloads.Add(create);
                        db.SaveChanges();

                     

                        return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, attachment.FileName);
                    }
                    else
                    {
                        

                        create.Downloader = user.Id;
                        create.NoteID = noteid;
                        create.Seller = note.SellerId;
                        create.PurchasedPrice = note.SellingPrice;

                        create.IsSellerHasAllowedDownload = false;
                        create.IsAttachmentDownloaded = false;
                        create.AttachmentPath = attachment.FilePath;
                        create.IsPaid = note.IsPaid;
                        create.NoteTitle = attachment.FileName;
                        create.NoteCategory = catagory.Name;
                        create.CreatedDate = DateTime.Now;
                        db.Downloads.Add(create);
                        
                        db.SaveChanges();

                       
                        var seller = db.Users.FirstOrDefault(m => m.Id == note.SellerId);
                        var details = db.UserProfiles.FirstOrDefault(m => m.UserI == user.Id);
                       
                        using (MailMessage mm = new MailMessage("email", seller.EmailId))
                        {
                            mm.Subject = user.FirstName + " wants to purchase your notes";

                            string body = string.Empty;
                            using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/BuyerRequest.html")))
                            {
                                body = reader.ReadToEnd();
                            }
                            string FirstName = user.FirstName;
                            string sellerName = seller.FirstName;
                            string gender = details.Gender == 2 ? "her" : "him";
                            body = body.Replace("{FirstName}", FirstName);
                            body = body.Replace("{Seller}", sellerName);
                            body = body.Replace("{gender}", gender);
                            mm.Body = body;
                            mm.IsBodyHtml = true;
                            SmtpClient smtp = new SmtpClient();
                            smtp.Host = "smtp.gmail.com";
                            smtp.EnableSsl = true;
                            NetworkCredential NetworkCred = new NetworkCredential("email", "pass");
                            smtp.UseDefaultCredentials = true;
                            smtp.Credentials = NetworkCred;
                            smtp.Port = 587;
                            smtp.Send(mm);

                        }
                       
                      

                        TempData["UserName"] = user.FirstName;

                      
                        TempData["ShowModal"] = 1;
                        return RedirectToAction("NoteDetails", new { id = noteId });
                    }

                }

                else
                {
                    return RedirectToAction("SearchNotes","Notes");
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

        public ActionResult DownloadedNote(int? noteId, int? sellerId, int? buyerId)
        {
            
                // seller names
                var seller = (from Notes in db.SellerNotes
                              join User in db.Users on Notes.SellerId equals User.Id
                              where Notes.Status == 9
                              group new { Notes, User } by Notes.SellerId into grp
                              select new SellerDetailsModel
                              {
                                  SellerId = grp.Select(x => x.User.Id).FirstOrDefault(),
                                  Name = grp.Select(x => x.User.FirstName).FirstOrDefault() + " " + grp.Select(x => x.User.LastName).FirstOrDefault()
                              }).ToList();

                ViewBag.SellerList = seller;

                var buyer = (from Purchase in db.Downloads
                             join User in db.Users on Purchase.Downloader equals User.Id
                             where Purchase.IsSellerHasAllowedDownload == true && Purchase.IsAttachmentDownloaded == true
                             group new { Purchase, User } by Purchase.Downloader into grp
                             select new BuyerListModel
                             {
                                 BuyerId = grp.Select(x => x.User.Id).FirstOrDefault(),
                                 Name = grp.Select(x => x.User.FirstName).FirstOrDefault() + " " + grp.Select(x => x.User.LastName).FirstOrDefault()
                             }).ToList();

                ViewBag.BuyerList = buyer;

              
                var note = (from Purchase in db.Downloads
                            join Note in db.SellerNotes on Purchase.NoteID equals Note.Id
                            where Purchase.IsSellerHasAllowedDownload == true && Purchase.IsAttachmentDownloaded == true
                            group new { Note, Purchase } by Note.Id into grp
                            select new NoteListModel
                            {
                                NoteId = grp.Select(x => x.Purchase.NoteID).FirstOrDefault(),
                                Title = grp.Select(x => x.Note.Title).FirstOrDefault()
                            }).ToList();

                ViewBag.NoteList = note;

               
                var model = (from Purchase in db.Downloads
                             join Note in db.SellerNotes on Purchase.NoteID equals Note.Id
                             join attachment in db.SellerNotesAttachements on Note.Id equals attachment.NoteId
                             join Downloader in db.Users on Purchase.Downloader equals Downloader.Id
                             join Seller in db.Users on Purchase.Seller equals Seller.Id
                             join Category in db.NoteCategories on Note.Category equals Category.Id
                             where Purchase.IsSellerHasAllowedDownload == true && Purchase.IsAttachmentDownloaded == true
                             select new DownloadNotesViewModel
                             {
                                 NoteId = Purchase.NoteID,
                                 Title = Note.Title,
                                 Category = Category.Name,
                                 Price = (decimal)Purchase.PurchasedPrice,
                                 SellerId = Seller.Id,
                                 BuyerId = Downloader.Id,
                                 NotePath=attachment.FilePath,
                                 SellerName = Seller.FirstName + " " + Seller.LastName,
                                 BuyerName = Downloader.FirstName + " " + Downloader.LastName,
                                 DownloadedDate = (DateTime)Purchase.AttachmentDownloadedDate
                             }).OrderByDescending(x => x.DownloadedDate).ToList();


                var filterdata = model;

                if (!noteId.Equals(null))
                {
                    filterdata = filterdata.Where(m => m.NoteId == noteId).ToList();
                 }
            else
            {
                return View(filterdata);
            }

            if (!sellerId.Equals(null))
                {
                    filterdata = filterdata.Where(m => m.SellerId == sellerId).ToList();
                }
                else
                {
                return View(filterdata);
            }
                if (!buyerId.Equals(null))
                    {
                        filterdata = filterdata.Where(m => m.BuyerId == buyerId).ToList();
                    }
                    else
                    {
                return View(filterdata);
            }

            return View(filterdata);

            
        }
        public ActionResult RejectNote(int noteId, string Reject)
        {
           
                var currentAdmin = db.Users.Single(m => m.EmailId == User.Identity.Name).Id;

                var note = db.SellerNotes.Single(m => m.Id == noteId);
                note.Status = 10;
                note.ActionBy = currentAdmin;
                note.AdminRemarks = Reject;
                note.ModifiedDate = DateTime.Now;
                note.ActionBy = currentAdmin;

                db.SaveChanges();

                return RedirectToAction("NotesUnderReview");
            
        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult RejectedNotes(int? sellerId)
        {
           
                var seller = (from Notes in db.SellerNotes
                              join User in db.Users on Notes.SellerId equals User.Id
                              where Notes.Status == 10
                              group new { Notes, User } by Notes.SellerId into grp
                              select new SellerDetailsModel
                              {
                                  SellerId = grp.Select(x => x.User.Id).FirstOrDefault(),
                                  Name = grp.Select(x => x.User.FirstName).FirstOrDefault() + " " + grp.Select(x => x.User.LastName).FirstOrDefault()
                              }).ToList();

                ViewBag.SellerList = seller;


                var notes = (from Note in db.SellerNotes
                             join Category in db.NoteCategories on Note.Category equals Category.Id
                             join Seller in db.Users on Note.SellerId equals Seller.Id
                             join attachment in db.SellerNotesAttachements on Note.Id equals attachment.NoteId
                             join Admin in db.Users on Note.ActionBy equals Admin.Id
                             where Note.Status == 10
                             select new RejectedNoteViewModel
                             {
                                 NoteId = Note.Id,
                                 Title = Note.Title,
                                 Category = Category.Name,
                                 SellerId = Note.SellerId,
                                 Name = Seller.FirstName + " " + Seller.LastName,
                                 RejectedBy = Admin.FirstName + " " + Admin.LastName,
                                 Remarks = Note.AdminRemarks,
                                 ModifiedDate = (DateTime)Note.ModifiedDate,
                                 NotePath=attachment.FilePath
                             }).ToList();


                if (!sellerId.Equals(null))
                {
                    notes = notes.Where(x => x.SellerId == sellerId).ToList();
                }


                return View(notes);
            

        }
        [HttpPost]
        public ActionResult UnPublishNote(int noteid, string Remarks)
        {
            
                int currentAdmin = db.Users.Single(m => m.EmailId == User.Identity.Name).Id;

                var note = db.SellerNotes.Single(m => m.Id == noteid);

                var seller = db.Users.Single(m => m.Id == note.SellerId);

                note.Status = 11;
                note.AdminRemarks = Remarks;
                note.ActionBy = currentAdmin;
                note.ModifiedDate = DateTime.Now;

                db.SaveChanges();


            using (MailMessage mm = new MailMessage("email", seller.EmailId))
            {
                mm.Subject = seller.FirstName + " Sorry! We need to remove your notes from our portal";

                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/RemoveNote.html")))
                {
                    body = reader.ReadToEnd();
                }
                string Seller = seller.FirstName +" "+seller.LastName;
                
                string Remark = note.AdminRemarks;
                string Title = note.Title;
                body = body.Replace("{Seller}", Seller);
              
                body = body.Replace("{Remark}", Remark);
                body = body.Replace("{Title}", Title);
                mm.Body = body;
                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("email", "pass");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);

            }


            return RedirectToAction("PublishedNotes");
            
        }
        


    }

}