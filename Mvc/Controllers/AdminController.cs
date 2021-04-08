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
using System.Net.Mail;
using System.Net;

namespace NoteMarketPlaceHtml.Controllers
{
   
    public class AdminController : Controller
    {
        
      
        readonly NoteMarketPlaceEntities db = new NoteMarketPlaceEntities();
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

        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult Country(int ?edit)
        {
            ViewBag.Edit = false;

            if (edit != null)
            {
               
                    var data = db.Countries.Where(m => m.Id == edit)
                        .Select(x => new CountryViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            CountryCode = x.CountryCode
                        }).Single();

                    ViewBag.Edit = true;

                    return View(data);
                
            }
            CountryViewModel model = new CountryViewModel();
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult Country(CountryViewModel data ,int ?id)
        {
            if (ModelState.IsValid)
            {
                var email = User.Identity.Name;

                User user = db.Users.Where(x => x.EmailId == email).SingleOrDefault();

                var userId = user.Id;
                if (!id.Equals(null))
                {
                  
                    var update = db.Countries.FirstOrDefault(m => m.Id == id);
                    update.Name = data.Name;
                    update.CountryCode = data.CountryCode;
                    update.IsActive = true;
                    update.ModifiedBy = userId;
                    update.ModifiedDate = DateTime.Now;

                    db.SaveChanges();
                    return RedirectToAction("ManageCountry");
                }
                else
                {
                    if (!db.Countries.Any(model => model.Name == data.Name && model.CountryCode == data.CountryCode))
                    {
                        Country model = new Country
                        {
                            Name = data.Name,
                            CountryCode = data.CountryCode,

                              
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            CreatedBy = userId,
                           
                        };
                        db.Countries.Add(model);
                        db.SaveChanges();

                        return RedirectToAction("ManageCountry", "Admin");

                    }

                    else
                    {
                        ModelState.AddModelError("Error", "Country Already exists!");
                        return View();
                    }
                }

                
            }

            return View();

        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult ManageCountry()
        {
            var data = (from Country in db.Countries
                       join User in db.Users on Country.CreatedBy equals User.Id
                       select new ManageCountryViewModel
                       {
                           Id = Country.Id,
                           Name = Country.Name,
                           CountryCode = Country.CountryCode,
                           CreatedDate = (DateTime)Country.CreatedDate,
                           CreatedBy = User.FirstName + " " + User.LastName,
                           IsActive = Country.IsActive == true ? "Yes" : "No"
                       }).OrderByDescending(x => x.CreatedDate).ToList();

            return View(data);
        }
        public ActionResult DeleteCountry(int id)
        {
            var currentAdmin = db.Users.Single(m => m.EmailId == User.Identity.Name).Id;
            var country = db.Countries.Single(m => m.Id == id);
            country.IsActive = false;
            country.ModifiedBy = currentAdmin;
            country.ModifiedDate = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("ManageCountry");
        }
        /** public ActionResult ManageSystemConfiguration()
         {
             return View();
         }
         [HttpPost]
         public ActionResult ManageSystemConfiguration(SystemConfigurationViewModel model)
         {
            
             
             return View();
         }**/
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult Type(int ?edit)
        {
            ViewBag.Edit = false;

            if (edit != null)
            {

                var data = db.NoteTypes.Where(m => m.Id == edit)
                    .Select(x => new TypeViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description
                    }).Single();

                ViewBag.Edit = true;

                return View(data);

            }
            TypeViewModel model = new TypeViewModel();
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult Type(TypeViewModel data,int ?id)
        {
            if (ModelState.IsValid)
            {
                var email = User.Identity.Name;

                User user = db.Users.Where(x => x.EmailId == email).SingleOrDefault();

                var UserId = user.Id;
                if (!id.Equals(null))
                {
                    // add new category
                    var update = db.NoteTypes.FirstOrDefault(m => m.Id == id);
                    update.Name = data.Name;
                    update.Description = data.Description;
                    update.IsActive = true;
                    update.ModifiedBy = UserId;
                    update.ModifiedDate = DateTime.Now;

                    db.SaveChanges();
                    return RedirectToAction("ManageType");
                }
                else
                {
                    if (!db.NoteTypes.Any(model => model.Name == data.Name && model.Description == data.Description))
                    {




                        NoteType model = new NoteType
                        {
                            Name = data.Name,
                            Description = data.Description,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            CreatedBy = UserId,
                            
                        };
                        db.NoteTypes.Add(model);
                        db.SaveChanges();

                        return RedirectToAction("ManageType");

                    }
                    else
                    {
                        ModelState.AddModelError("Error", "Type Already exists!");
                        return View();
                    }
                }
            }
            return View();
           
        }
        public ActionResult ManageType()
        {
            var data = (from Type in db.NoteTypes
                        join User in db.Users on Type.CreatedBy equals User.Id
                        select new ManageTypeViewModel
                        {
                            Id = Type.Id,
                            Name = Type.Name,
                            Description = Type.Description,
                            CreatedDate = (DateTime)Type.CreatedDate,
                            CreatedBy = User.FirstName + " " + User.LastName,
                            IsActive = Type.IsActive == true ? "Yes" : "No"
                        }).OrderByDescending(x => x.CreatedDate).ToList();

            return View(data);
        }
        public ActionResult DeleteType(int id)
        {
            var currentAdmin = db.Users.Single(m => m.EmailId == User.Identity.Name).Id;
            var Type = db.NoteTypes.Single(m => m.Id == id);
            Type.IsActive = false;
            Type.ModifiedBy = currentAdmin;
            Type.ModifiedDate = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("ManageCountry");
        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult Category(int ?edit)
        {
            ViewBag.Edit = false;

            if (edit != null)
            {

                var data = db.NoteCategories.Where(m => m.Id == edit)
                    .Select(x => new CatagoryViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description
                    }).Single();

                ViewBag.Edit = true;

                return View(data);

            }
            CatagoryViewModel model = new CatagoryViewModel();
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult Category(CatagoryViewModel data,int?id)
        {

            if (ModelState.IsValid)
            {
                var email = User.Identity.Name;

                User user = db.Users.Where(x => x.EmailId == email).SingleOrDefault();

                var UserId = user.Id;
                if (!id.Equals(null))
                {
                    // add new category
                    var update = db.NoteCategories.FirstOrDefault(m => m.Id == id);
                    update.Name = data.Name;
                    update.Description = data.Description;
                    update.IsActive = true;
                    update.ModifiedBy = UserId;
                    update.ModifiedDate = DateTime.Now;

                    db.SaveChanges();
                    return RedirectToAction("ManageCategory");
                }
                else
                {
                    if (!db.NoteCategories.Any(model => model.Name == data.Name && model.Description == data.Description))
                    {

                      


                        NoteCategory model = new NoteCategory
                        {
                            Name = data.Name,
                            Description = data.Description,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            CreatedBy = UserId,
                          
                        };
                        db.NoteCategories.Add(model);
                        db.SaveChanges();

                        return RedirectToAction("ManageCatagory");

                    }
                    else
                    {
                        ModelState.AddModelError("Error", "Catagory Already exists!");
                        return View();
                    }
                }
               
            }
            else
            {
                return View();
            }

        }
        [Authorize(Roles = "SuperAdmin,Admin")]
       
        public ActionResult ManageCategory()
        {
            var data = (from model in db.NoteCategories
                        join User in db.Users on model.CreatedBy equals User.Id
                        select new ManageCatagoryViewModel
                        {
                            Id = model.Id,
                            Name = model.Name,
                            Description = model.Description,
                            CreatedDate = (DateTime)model.CreatedDate,
                            CreatedBy = User.FirstName + " " + User.LastName,
                            IsActive = model.IsActive == true ? "Yes" : "No"
                        }).OrderByDescending(x => x.CreatedDate).ToList();

            return View(data);
        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult DeleteCategory(int id)
        {
            var currentAdmin = db.Users.Single(m => m.EmailId == User.Identity.Name).Id;
            var Type = db.NoteCategories.Single(m => m.Id == id);
            Type.IsActive = false;
            Type.ModifiedBy = currentAdmin;
            Type.ModifiedDate = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("ManageCategory");
        }

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult ManageAdmin()
        {
           
                var model = (from User in db.Users
                             where User.RoleId == 2
                             join Detail in db.AddAdmins on User.Id equals Detail.UserId
                             select new ManageAdminViewModel
                             {
                                 Id = User.Id,
                                 FirstName = User.FirstName,
                                 LastName = User.LastName,
                                 Email = User.EmailId,
                                 Phone =Detail.PhoneNumber,
                                 IsActive = User.IsActive == true ? "Yes" : "No",
                                 CreatedDate = (DateTime)User.CreatedDate
                             }).OrderByDescending(x => x.CreatedDate).ToList();

                return View(model);
            
        }


        [Authorize(Roles = "SuperAdmin")]
        public ActionResult AddAdmin(int ?edit)
        {
            var isDetailsAvailable = db.AddAdmins.FirstOrDefault(m => m.UserId == edit);
            var model = new AdminProfileViewModel();
            if (edit != null)
            {


                model = (from User in db.Users
                         where User.Id == edit && User.IsActive == true
                         join Detail in db.AddAdmins on User.Id equals Detail.UserId
                         select new AdminProfileViewModel
                         {
                             Id = User.Id,
                             FirstName = User.FirstName,
                             LastName = User.LastName,
                             EmailID = User.EmailId,
                            
                             SelectedCode = Detail.CountryCode,
                             PhoneNumber = Detail.PhoneNumber,
                             

                         }).FirstOrDefault<AdminProfileViewModel>();
                model.CountryCode = db.Countries.
                                              Select(a => new SelectListItem
                                              {
                                                  Text = a.CountryCode, // name to show in html dropdown
                                                  Value = a.CountryCode // value of html dropdown
                                              }).ToList();
                ViewBag.Readonly = true;//false
                ViewBag.Edit = true;

                return View(model);

            }
            ViewBag.Edit = false;

            model.CountryCode = db.Countries.
                                             Select(a => new SelectListItem
                                             {
                                                 Text = a.CountryCode, // name to show in html dropdown
                                                  Value = a.CountryCode // value of html dropdown
                                              }).ToList();
            return View(model);



           

        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult AddAdmin(AdminProfileViewModel admin,int?id)
        {


            admin.CountryCode = db.Countries.
                                       Select(a => new SelectListItem
                                       {
                                           Text = a.CountryCode, // name to show in html dropdown
                                               Value = a.CountryCode // value of html dropdown
                                           }).ToList();



            if (ModelState.IsValid)
            {
                string userId = User.Identity.Name;
                User currentUser = db.Users.FirstOrDefault(x => x.EmailId == userId);
                if (!id.Equals(null))
                {
                    if (currentUser.EmailId != admin.EmailID)
                    {

                        var userdata = db.Users.FirstOrDefault(m => m.Id == id);
                        var update = db.AddAdmins.FirstOrDefault(m => m.UserId == id);
                        userdata.FirstName = admin.FirstName;
                        userdata.LastName = admin.LastName;
                        update.CountryCode = admin.SelectedCode;
                        update.PhoneNumber = admin.PhoneNumber;

                        db.SaveChanges();
                        return RedirectToAction("ManageAdmin");
                    }
                }
                else {
                    if (currentUser.EmailId != admin.EmailID)
                    {
                        if (!db.Users.Any(model => model.EmailId == admin.EmailID))
                        {

                        User user = new User();
                        user.FirstName = admin.FirstName;
                        user.LastName = admin.LastName;
                        user.EmailId = admin.EmailID;
                        user.RoleId = 2;
                        user.IsVerified = true;
                        user.Code = Guid.NewGuid();
                        user.CreatedDate = DateTime.Now;
                        user.CreatedBy = currentUser.Id;

                        user.IsVerified = true;
                        user.IsActive = true;
                        user.Password = GeneratePassword().ToString();
                        db.Users.Add(user);
                        db.SaveChanges();
                        AddAdmin model = new AddAdmin();
                        model.CountryCode = admin.SelectedCode;
                        model.PhoneNumber = admin.PhoneNumber;

                        model.UserId = user.Id;
                        model.IsActive = true;
                        db.AddAdmins.Add(model);
                        db.SaveChanges();

                        var defaultmail = db.SystemConfigurations.FirstOrDefault(m => m.KeyData == "EmailAddresssesForNotify").ValueData;
                        using (MailMessage mm = new MailMessage(defaultmail, admin.EmailID))
                        {
                            mm.Subject = "New Temporary Password has been created for you";

                            string body = string.Empty;
                            using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/Password.html")))
                            {
                                body = reader.ReadToEnd();
                            }
                           
                            body = body.Replace("{SystemPassword}", user.Password);
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


                        return RedirectToAction("ManageAdmin");

                        }
                        else
                        {
                            ModelState.AddModelError("Error", "Email is Already exists!");
                            return View(admin);
                        }
                    }


                }



            }


            return View();



        }

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult DeleteAdmin(int id)
        {


            var currentAdmin = db.Users.Single(m => m.EmailId == User.Identity.Name).Id;

            var user = db.Users.Single(m => m.Id == id);
            
            user.IsActive = false;
            user.ModifiedBy = currentAdmin;
            user.ModifiedDate = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("ManageAdmin");
        }







        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult AdminProfile()
        {
            var currentuser = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name);

          
            var isDetailsAvailable = db.AddAdmins.FirstOrDefault(m => m.UserId == currentuser.Id);
            var model = new AdminProfileViewModel();
            if (isDetailsAvailable != null)
            {

                model = (from Detail in db.AddAdmins
                         join User in db.Users on Detail.UserId equals User.Id
                         join Country in db.Countries on Detail.CountryCode equals Country.CountryCode
                         where Detail.UserId == currentuser.Id
                         select new AdminProfileViewModel
                         {
                             FirstName = User.FirstName,
                             LastName = User.LastName,
                             EmailID = User.EmailId,
                             SecondaryEmailId =Detail.secondaryEmailAddress,
                             SelectedCode = Detail.CountryCode,
                             PhoneNumber = Detail.PhoneNumber,
                             path = Detail.ProfilePicture

                         }).FirstOrDefault<AdminProfileViewModel>();
                model.CountryCode = db.Countries.
                                              Select(a => new SelectListItem
                                              {
                                                  Text = a.CountryCode, // name to show in html dropdown
                                                   Value = a.CountryCode // value of html dropdown
                                               }).ToList();
                ViewBag.Readonly = true;//false

                return View(model);

            }
            return View();
           
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult AdminProfile(AdminProfileViewModel profile, HttpPostedFileBase path)
        {

           
                profile.CountryCode = db.Countries.
                                           Select(a => new SelectListItem
                                           {
                                               Text = a.CountryCode, // name to show in html dropdown
                                               Value = a.CountryCode // value of html dropdown
                                           }).ToList();

            

            if (ModelState.IsValid)
            {
                string userId = User.Identity.Name;
                User currentUser = db.Users.FirstOrDefault(x => x.EmailId == userId);
              

                    currentUser.FirstName = profile.FirstName;
                    currentUser.LastName = profile.LastName;
                   
                    var updateDetails = db.AddAdmins.FirstOrDefault(m => m.UserId == currentUser.Id);

                    updateDetails.CountryCode = profile.SelectedCode;
                    updateDetails.secondaryEmailAddress = profile.SecondaryEmailId;
                    updateDetails.PhoneNumber = profile.PhoneNumber;

                    updateDetails.ModifiedDate = DateTime.Now;
                    updateDetails.ModifiedBy = currentUser.Id;
                  
                   
                   

                    if (path == null)
                    {

                        db.SaveChanges();

                        return View(profile);
                    }
                    else
                    {
                    string folder1 = Server.MapPath(string.Format("../Members/{0}/",currentUser.Id));
                    Directory.CreateDirectory(folder1);
                    string old = Server.MapPath((string)TempData["oldprofile"]);
                        FileInfo fileInfo = new FileInfo(old);
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();

                        }
                        string extention = Path.GetExtension(path.FileName);
                        string imgname = "DP" + extention;
                        profile.path = "../Members/" + currentUser.Id + "/" + imgname;
                        string pathname = Path.Combine(Server.MapPath(profile.path));
                        path.SaveAs(pathname);
                        updateDetails.ProfilePicture = profile.path;
                       
                        db.SaveChanges();
                    }

                

                return View(profile);

            }


            return View();



        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult Dashboard(int? month)
        {
            
               
                var inReviewNote = db.SellerNotes.Where(m =>  m.Status == 8 && m.IsActive == true).Count();
             
                var condition = DateTime.Now.Date.AddDays(-7);
                var downloads = db.Downloads.Where(m => m.IsAttachmentDownloaded == true && m.AttachmentDownloadedDate >= condition).Count();
               
                var registration = db.Users.Where(m => m.CreatedDate >= condition&&m.RoleId==3).Count();

                ViewBag.InReview = inReviewNote;
                ViewBag.Downloads = downloads;
                ViewBag.Registration = registration;


              
                var monthList = new List<MonthModel>();
                for (int i = 0; i <= 8; i--)
                {
                    monthList.Add(new MonthModel
                    {
                        Id = DateTime.Today.AddMonths(+i).Month,
                        Month = DateTime.Today.AddMonths(+i).ToString("MMMM")
                    });
                }
                ViewBag.MonthList = monthList.OrderBy(x=>x.Id);


                var note = (from Note in db.SellerNotes
                            join Attachment in db.SellerNotesAttachements on Note.Id equals Attachment.NoteId
                            join Category in db.NoteCategories on Note.Category equals Category.Id
                            join User in db.Users on Note.SellerId equals User.Id
                            where Note.Status == 9
                            let total = (db.Downloads.Where(m => m.NoteID == Note.Id).Count())
                            select new AdminDashBoardViewModel
                            {
                                Id = Note.Id,
                                Title = Note.Title,
                                Category = Category.Name,
                                Price = (decimal)Note.SellingPrice,
                                Publisher = User.FirstName + " " + User.LastName,
                                PublishDate = (DateTime)Note.PublishedDate,
                                publishMonth = Note.PublishedDate.Value.Month,
                                TotalDownloads = total,
                                UserId = Note.SellerId,
                                FileName = Attachment.FileName,
                                FilePath=Attachment.FilePath
                            }).OrderByDescending(x => x.TotalDownloads).ToList();

              
                foreach (var data in note)
                {
                    data.AttachmentSize = GetSize(data.FilePath);
                }

                if (month == null)
                {
                    var filternote = note.Where(m => m.publishMonth == DateTime.Now.Month).ToList();
                    return View(filternote);
                }
                else
                {
                    var filternote = note.Where(m => m.publishMonth == month).ToList();
                    return View(filternote);
                }

            

        }
        public float GetSize(string Filepath)
        {
            string filePath = Server.MapPath(Filepath);
            System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
            return (fs.Length / 1000);
        }

        public string GeneratePassword()
        {
            string PasswordLength = "6";
            string NewPassword = "";

            string allowedChars = "";
            allowedChars = "1,2,3,4,5,6,7,8,9,0";
            //allowedChars += "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,";
            //allowedChars += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,";

            char[] sep = { ',' };
            string[] arr = allowedChars.Split(sep);
            string IDString = "";
            string temp = "";
            Random rand = new Random();
            for (int i = 0; i < Convert.ToInt32(PasswordLength); i++)
            {
                temp = arr[rand.Next(0, arr.Length)];
                IDString += temp;
                NewPassword = IDString;
            }
            return NewPassword;
        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult Members()
        {
           

                var model = (from User in db.Users
                             where User.RoleId == 3 && User.IsActive == true
                             let underReview = (from Notes in db.SellerNotes
                                                where Notes.SellerId == User.Id && (Notes.Status == 7 || Notes.Status == 8)
                                                select Notes).Count()
                             let published = (from Notes in db.SellerNotes
                                              where Notes.SellerId == User.Id && Notes.Status == 9
                                              select Notes).Count()
                             let downloaded = (from Purchase in db.Downloads
                                               where Purchase.Downloader == User.Id && Purchase.IsAttachmentDownloaded == true
                                               select Purchase)
                             let sell = (from Purchase in db.Downloads
                                         where Purchase.Seller == User.Id && Purchase.IsSellerHasAllowedDownload == true
                                         select Purchase)
                             select new MemberViewModel
                             {
                                 Id = User.Id,
                                 FirstName = User.FirstName,
                                 LastName = User.LastName,
                                 Email = User.EmailId,
                                 JoinDate = (DateTime)User.CreatedDate,
                                 UnderReviewNotes = underReview,
                                 PublishedNotes = published,
                                 DownloadedNotes = downloaded.Count(),
                                 TotalExpense = downloaded.Sum(x => x.PurchasedPrice),
                                 TotalEarning = sell.Sum(x => x.PurchasedPrice)
                             }).OrderByDescending(x => x.JoinDate).ToList();

                return View(model);
            

        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public ActionResult MemberDetails(int sellerId)
        {
            
                
              
                
                var details = (from User in db.Users
                               where User.Id == sellerId
                               
                               join Details in db.UserProfiles on User.Id equals Details.UserI
                              
                               join Country in db.Countries on Details.Country equals Country.Name
                               select new MemberDetailsViewModel
                               {
                                   FirstName = User.FirstName,
                                   LastName = User.LastName,
                                   ProfileImage = Details.ProfilePicture == null ? DefaultImg: Details.ProfilePicture,
                                   Email = User.EmailId,
                                   DOB = Details.DOB,
                                   Phone = Details.PhoneNumber,
                                   Collage = Details.College,
                                   University=Details.University,
                                   Address1 = Details.AddressLine1,
                                   Address2 = Details.AddressLine2,
                                   City = Details.City,
                                   State = Details.State,
                                   Country = Country.Name,
                                   Zipcode = Details.ZipCode
                                   
                                   
                               }).SingleOrDefault();

                ViewBag.Details = details;


               
                var notes = (from Note in db.SellerNotes
                             where Note.SellerId == sellerId
                             join Status in db.ReferenceDatas on Note.Status equals Status.Id 
                             join attachment in db.SellerNotesAttachements on Note.Id equals attachment.NoteId
                             join Category in db.NoteCategories on Note.Category equals Category.Id
                             let downloadedNotes = (db.Downloads.Where(m => m.Downloader == sellerId && m.IsAttachmentDownloaded == true &&m.NoteID==Note.Id).Count())
                             let earning = (db.Downloads.Where(m => m.Seller == sellerId && m.NoteID==Note.Id).Sum(x => x.PurchasedPrice))
                             where Note.Status!=6
                         
                             select new MemberNoteViewModel
                             {
                                 NoteId = Note.Id,
                                 Title = Note.Title,
                                 Category = Category.Name,
                                 Status = Status.Values,
                                 DownloadedNote = downloadedNotes,
                                 Earning = earning,
                                 DateAdded = (DateTime)Note.CreatedDate,
                                 PublishedDate = Note.PublishedDate,
                                 NotePath = attachment.FilePath
                             }).ToList();


                return View(notes);
            

        }
        [Authorize(Roles = "SuperAdmin,Admin")]
        public void DeactivateMember(int memberId)
        {
            var currentAdmin = db.Users.Single(m => m.EmailId == User.Identity.Name).Id;

            var user = db.Users.Single(m => m.Id == memberId);
     
            user.IsActive = false;
            user.ModifiedBy = currentAdmin;
            user.ModifiedDate = DateTime.Now;
           
            db.SaveChanges();

            var notes = db.SellerNotes.Where(m => m.SellerId == memberId).ToList();

            for (int i = 0; i < notes.Count; i++)
            {
                var note = notes[i];

                var Attachment = db.SellerNotesAttachements.Single(m => m.NoteId == note.Id);
                Attachment.IsActive = false;
                Attachment.ModifiedBy = currentAdmin;
                Attachment.ModifiedDate = DateTime.Now;

                notes[i].IsActive = false;
                notes[i].Status = 8;
                notes[i].ActionBy = currentAdmin;
                notes[i].ModifiedDate = DateTime.Now;

                db.SaveChanges();
            }

        }


        public ActionResult SpamReports()
        {
           
                var data = (from Spam in db.SellerNotesReportedIssues
                            join Note in db.SellerNotes on Spam.NoteId equals Note.Id
                            join Attachment in db.SellerNotesAttachements on Note.Id equals Attachment.NoteId
                            join User in db.Users on Spam.AgainstDownloadId equals User.Id
                          
                            join Category in db.NoteCategories on Note.Category equals Category.Id
                            select new SpamReportViewModel
                            {
                                ID = Spam.Id,
                                NoteId = Spam.NoteId,
                                Title = Note.Title,
                                ReportedBy = User.FirstName + " " + User.LastName,
                                Remarks = Spam.Remarks,
                                Category = Category.Name,
                                CreatedDate = (DateTime)Spam.CreatedDate,
                                NotePath=Attachment.FilePath
                            }).OrderByDescending(x => x.CreatedDate).ToList();

                return View(data);
            

        }
        [HttpPost]
        [Route("DeleteSpamReport")]
        public void DeleteSpamReport(int Id)
        {
           
                var report = db.SellerNotesReportedIssues.Single(m => m.Id == Id);
                db.SellerNotesReportedIssues.Remove(report);
                db.SaveChanges();
            
        }


    }

}