
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
using System.Web.Security;

namespace NoteMarketPlaceHtml.Controllers
{
    public class AccountController : Controller
    {

        // GET: Account
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
                    var imgMember = (from Details in db_1.UserProfiles
                                     join Users in db_1.Users on Details.UserI equals Users.Id
                                     where Users.EmailId == requestContext.HttpContext.User.Identity.Name
                                     select Details.ProfilePicture).FirstOrDefault();

                    if (img != null)
                    {
                        // set default image
                        ViewBag.UserProfile = img;
                       
                    }
                    else if (imgMember != null)
                    {
                        // set default image
                        ViewBag.UserProfile = imgMember;
                    }
                    else
                    {
                        var defaultImg = db_1.SystemConfigurations.FirstOrDefault(m => m.KeyData == "DefaultMemberDisplayPicture").ValueData;
                        ViewBag.UserProfile = defaultImg;


                    }


                }
            }

        }
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult RegisterUser()
        {
            RegisterUserViewModel model = new RegisterUserViewModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult RegisterUser(RegisterUserViewModel user)
        {

            if (ModelState.IsValid)
            {
                if (!db.Users.Any(model => model.EmailId == user.EmailID))
                {
                    User use = new User
                    {
                        FirstName = user.FirstName,
                        EmailId = user.EmailID,
                        LastName = user.LastName,
                        Password = user.Password,
                        RoleId = 3,
                        Code = Guid.NewGuid(),
                        CreatedDate = DateTime.Now
                    };
                    db.Users.Add(use);
                    db.SaveChanges();
                    SendActivationEmail(use);
                    ViewBag.Success = "Your account is Created  Verify Email.";
                    return View(user);
                }
                else
                {
                    ModelState.AddModelError("Error", "Email is Already exists!");
                    return View();
                }
            }
            return View();
        }

        private void SendActivationEmail(User model)
        {
            using (MailMessage mm = new MailMessage("email", model.EmailId))
            {
                mm.Subject = "Note MarketPlace Email Verification";

                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/AccountConfirmation.html")))
                {
                    body = reader.ReadToEnd();
                }


                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = model.EmailId, pass = model.Password, activationCode = model.Code }, protocol: Request.Url.Scheme);

                body = body.Replace("{Username}", model.FirstName);
                body = body.Replace("{ConfirmationLink}", confirmationLink);

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
        }

        [Route("Account/ConfirmEmail")]
        public ActionResult ConfirmEmail(string userId, string pass, string activationCode)
        {
            var check = db.Users.Where(model => model.EmailId == userId && model.Code == new Guid(activationCode)).FirstOrDefault();

            if (check != null)
            {
                if (check.Password.Equals(pass))
                {
                    check.IsVerified = true;

                    db.SaveChanges();


                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    return Content(" Credentials are Invalid");
                }
            }

            return Content(" Credentials are Invalid");
        }
        public ActionResult Login()
        {
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel objLoginModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = db.Users.Where(model => model.EmailId == objLoginModel.EmailID && model.Password == objLoginModel.Password).FirstOrDefault();
                


                if (result == null)
                {

                    ModelState.AddModelError("Error", "Email or password  is Incorrect");
                    return View();
                }
             
                else if (result.IsVerified == true && result.RoleId==3&&result.IsActive==true)
                {

                    FormsAuthentication.SetAuthCookie(objLoginModel.EmailID, false);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    Session["EmailID"] = objLoginModel.EmailID;
                    
                    return RedirectToAction("SearchNotes", "Notes");
                }
                else if (result.IsVerified == true && (result.RoleId == 1  ||result.RoleId==2)&&result.IsActive==true)
                {
                    Session["EmailID"] = objLoginModel.EmailID;
                    FormsAuthentication.SetAuthCookie(objLoginModel.EmailID, false);
                    return RedirectToAction("Index", "Admin");
                   
                }
                else
                {
                    @ViewBag.Email = "Please verify your account.";
                }


            }
            return View();
        }


        public ActionResult Logout()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult ForgotPassword()
        {
            ForgotPasswordModel model = new ForgotPasswordModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordModel pass)
        {
            if (db.Users.Any(model => model.EmailId == pass.EmailID))
            {

                SendForgotPasswordEmail(pass);
                ViewBag.Success = "Check Your Email for Temporary Password .";
                return View();
            }
            else
            {
                ModelState.AddModelError("Error", "Email Does Not exists!");
                return View();
            }
        }
        private void SendForgotPasswordEmail(ForgotPasswordModel model)
        {
            var check = db.Users.Where(x => x.EmailId == model.EmailID).FirstOrDefault();
            var defaultmail = db.SystemConfigurations.FirstOrDefault(m => m.KeyData == "EmailAddresssesForNotify").ValueData;
            using (MailMessage mm = new MailMessage(defaultmail, model.EmailID))
            {
                mm.Subject = "New Temporary Password has been created for you";

                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/ForgotPassword.html")))
                {
                    body = reader.ReadToEnd();
                }
                string strNewPassword = GeneratePassword().ToString();
                body = body.Replace("{SystemPassword}", strNewPassword);
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
                if (strNewPassword != null)
                {
                    var change = db.Users.Where(x => x.EmailId == model.EmailID).FirstOrDefault();
                    if (change != null)
                    {

                        change.Password = strNewPassword;

                        db.SaveChanges();


                    }



                }
            }
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
        public ActionResult ChangePassword()
        {
            ChangePasswordViewModel model = new ChangePasswordViewModel();
            return View(model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {

            if (ModelState.IsValid)
            {
                using (var _Context = new NoteMarketPlaceEntities())
                {
                    // get current user
                    var currentUser = _Context.Users.FirstOrDefault(m => m.EmailId == User.Identity.Name);

                    // old password not match
                    if (!currentUser.Password.Equals(model.OldPassword))
                    {
                       
                        return View();
                    }

                    if (currentUser.Password == model.NewPassword)
                    {

                        ModelState.AddModelError("Error", "Old Password and New Password Are Same");
                        return View();
                    }

                    // update password
                    currentUser.Password = model.ConfirmPassword;
                    currentUser.ModifiedDate = DateTime.Now;
                    currentUser.ModifiedBy = currentUser.Id;
                    _Context.SaveChanges();

                    FormsAuthentication.SignOut();

                    return RedirectToAction("Login", "Account");
                }

            }
            return View();
        }


            public ActionResult Contact()
        {

            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.Name;
                User currentUser = db.Users.FirstOrDefault(x => x.EmailId == userId);
                ContactModel model = new ContactModel();
                model.Name = currentUser.FirstName;
                model.EmailID = currentUser.EmailId;
                return View(model);
            }
            else
            {
                ContactModel model = new ContactModel();
                return View(model);
            }



        }
        [HttpPost]
        public ActionResult Contact(ContactModel model)
        {
            if (db.Users.Any(x => x.EmailId == model.EmailID))
            {
                var defaultmail = db.SystemConfigurations.FirstOrDefault(m => m.KeyData == "EmailAddresssesForNotify").ValueData;
                using (MailMessage mm = new MailMessage( model.EmailID,defaultmail))
                {
                    mm.Subject = model.Name + " " + model.Comments;

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/ContactUs.html")))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("{Comments}", model.Comments);
                    body = body.Replace("{Name}", model.EmailID);
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
            }
            return View();

        }


    }
}
