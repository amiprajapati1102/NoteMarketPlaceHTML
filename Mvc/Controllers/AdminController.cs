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
    public class AdminController : Controller
    {
        // GET: Admin

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


        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Country()
        {
            CountryViewModel model = new CountryViewModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Country(CountryViewModel data)
        {
            if (ModelState.IsValid)
            {
                if (!db.Countries.Any(model => model.Name == data.Name && model.CountryCode==data.CountryCode))
                {
                    if (User.IsInRole("SuperAdmin"))
                    {
                        var email = User.Identity.Name;

                        User user = db.Users.Where(x => x.EmailId == email).SingleOrDefault();

                        var id = user.Id;

                        Country model = new Country();
                        model.Name = data.Name;
                        model.CountryCode = data.CountryCode;
                        model.IsActive = true;
                        model.CreatedDate = DateTime.Now;
                        model.CreatedBy = id;
                        model.ModifiedDate = DateTime.Now;
                        model.ModifiedBy = id;
                        db.Countries.Add(model);
                        db.SaveChanges();

                        return RedirectToAction("ManageCountry", "Admin");
                    }
                }
                else
                {
                    ModelState.AddModelError("Error","Country Already exists!");
                    return View();
                }

                return View();
            }
            else
            {
                return View();
            }
        }
        public ActionResult ManageCountry()
        {
            Country model = new Country();
            var data = db.Countries.ToList();
            return View(data);
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
        public ActionResult Type()
        {
            TypeViewModel model = new TypeViewModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Type(TypeViewModel data)
        {
            if (ModelState.IsValid)
            {
                if (!db.NoteTypes.Any(model => model.Name == data.Name && model.Description == data.Description))
                {
                    if (User.IsInRole("SuperAdmin"))
                    {
                        var email = User.Identity.Name;

                        User user = db.Users.Where(x => x.EmailId == email).SingleOrDefault();

                        var id = user.Id;

                        NoteType model = new NoteType();
                        model.Name = data.Name;
                        model.Description = data.Description;
                        model.IsActive = true;
                        model.CreatedDate = DateTime.Now;
                        model.CreatedBy = id;
                        model.ModifiedDate = DateTime.Now;
                        model.ModifiedBy = id;
                        db.NoteTypes.Add(model);
                        db.SaveChanges();

                        return RedirectToAction("ManageCountry", "Admin");
                    }
                }
                else
                {
                    ModelState.AddModelError("Error", "Type Already exists!");
                    return View();
                }

                return View();
            }
            else
            {
                return View();
            }
        }
        public ActionResult Category()
        {
            CatagoryViewModel model = new CatagoryViewModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Category(CatagoryViewModel data)
        {
            if (ModelState.IsValid)
            {
                if (!db.NoteCategories.Any(model => model.Name == data.Name && model.Description == data.Description))
                {
                    if (User.IsInRole("SuperAdmin"))
                    {
                        var email = User.Identity.Name;

                        User user = db.Users.Where(x => x.EmailId == email).SingleOrDefault();

                        var id = user.Id;

                        NoteCategory model = new NoteCategory();
                        model.Name = data.Name;
                        model.Description = data.Description;
                        model.IsActive = true;
                        model.CreatedDate = DateTime.Now;
                        model.CreatedBy = id;
                        model.ModifiedDate = DateTime.Now;
                        model.ModifiedBy = id;
                        db.NoteCategories.Add(model);
                        db.SaveChanges();

                        return RedirectToAction("ManageCountry", "Admin");
                    }
                }
                else
                {
                    ModelState.AddModelError("Error", "Type Already exists!");
                    return View();
                }

                return View();
            }
            else
            {
                return View();
            }

        }
        public ActionResult AddAdmin()
        {
            var currentuser = db.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name);

            // get user details
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
            else
            {

                model.CountryCode = db.Countries.
                                               Select(a => new SelectListItem
                                               {
                                                   Text = a.CountryCode, // name to show in html dropdown
                                               Value = a.CountryCode // value of html dropdown
                                           }).ToList();

                return View(model);
            }
        }
        [HttpPost]
        public ActionResult AddAdmin(AdminProfileViewModel dropdownViewModel, HttpPostedFileBase path)
        {

            using (var context = new NoteMarketPlaceEntities())
            {
                //create SelectListItem again
                dropdownViewModel.CountryCode = context.Countries.
                                           Select(a => new SelectListItem
                                           {
                                               Text = a.CountryCode, // name to show in html dropdown
                                               Value = a.CountryCode // value of html dropdown
                                           }).ToList();

            }

            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.Name;
                User currentUser = db.Users.FirstOrDefault(x => x.EmailId == userId);
                if (currentUser.EmailId != dropdownViewModel.EmailID)
                {
                    if (ModelState.IsValid)
                    {
                        User user = new User();
                        user.FirstName = dropdownViewModel.FirstName;
                        user.LastName = dropdownViewModel.LastName;
                        user.EmailId = dropdownViewModel.EmailID;
                        user.RoleId = 2;
                        user.IsVerified = true;
                        user.Code = Guid.NewGuid();
                        user.CreatedDate = DateTime.Now;
                        user.CreatedBy = 1;
                        user.ModifiedDate = DateTime.Now;
                        user.ModifiedBy = 1;
                        user.IsVerified = true;
                        user.IsActive = true;
                        user.Password = GeneratePassword().ToString();
                        db.Users.Add(user);
                        db.SaveChanges();
                        AddAdmin model = new AddAdmin();
                        model.CountryCode = dropdownViewModel.SelectedCode;
                        model.PhoneNumber = dropdownViewModel.PhoneNumber;
                        model.secondaryEmailAddress = dropdownViewModel.SecondaryEmailId;
                        string folder = Server.MapPath(string.Format("~/Members/{0}", user.Id));
                        Directory.CreateDirectory(folder);
                        string extention = Path.GetExtension(path.FileName);
                        string pathname = "DP" + extention;
                        dropdownViewModel.path = "~/Members/" + user.Id + "/" + pathname;
                        pathname = Path.Combine(Server.MapPath(string.Format("~/Members/{0}", user.Id)), pathname);
                        path.SaveAs(pathname);
                        model.ProfilePicture = dropdownViewModel.path;
                        model.UserId = user.Id;
                        db.AddAdmins.Add(model);
                        db.SaveChanges();



                        return View(dropdownViewModel);
                    } 
                }
                else
                {

                    currentUser.FirstName = dropdownViewModel.FirstName;
                    currentUser.LastName = dropdownViewModel.LastName;
                   
                    AddAdmin add = db.AddAdmins.FirstOrDefault(x => x.UserId == currentUser.Id);
                    add.CountryCode = dropdownViewModel.SelectedCode;
                    add.secondaryEmailAddress = dropdownViewModel.SecondaryEmailId;
                    add.PhoneNumber = dropdownViewModel.PhoneNumber;


                    if (path == null)
                    {

                        db.SaveChanges();

                        return View(dropdownViewModel);
                    }
                    else
                    {
                        string old = Server.MapPath((string)TempData["oldprofile"]);
                        FileInfo fileInfo = new FileInfo(old);
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();

                        }
                        string extention = Path.GetExtension(path.FileName);
                        string imgname = "DP" + extention;
                        dropdownViewModel.path = "~/Members/" + currentUser.Id + "/" + imgname;
                        string pathname = Path.Combine(Server.MapPath(dropdownViewModel.path));
                        path.SaveAs(pathname);
                        add.ProfilePicture = dropdownViewModel.path;
                        db.SaveChanges();
                    }

                }

                return View(dropdownViewModel);

            }


            return View(dropdownViewModel);



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

    }
    }