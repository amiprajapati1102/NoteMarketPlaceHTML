using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using NoteMarketPlace.DbModel;
using NoteMarketPlace.Models;

namespace WebApplication1_NoteMarketPlace.Controllers
{
    public class AccountController : Controller
    {
        NoteMarketPlaceHtmlEntities db = new NoteMarketPlaceHtmlEntities();

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Signup()
        {
            UserModel objUserModel = new UserModel();
            return View(objUserModel);
        }

        [HttpPost]
        public ActionResult Signup(UserModel user)
        {
            if (ModelState.IsValid)
            {
                if (!db.Users.Any(model => model.EmailID == user.EmailID))
                {
                    User use = new User();
                    use.FirstName = user.FirstName;
                    use.EmailID = user.EmailID;
                    use.LastName = user.LastName;
                    use.Password = user.Password;
                    use.RoleID = 3;
                    use.Code = Guid.NewGuid();
                    use.CreatedDate = DateTime.Now;
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

        private void SendActivationEmail(User objUserModel)
        {
            using (MailMessage mm = new MailMessage("your email@gmail.com", objUserModel.EmailID))
            {
                mm.Subject = "Note MarketPlace Email Verification";

                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/AccountConfirmation.html")))
                {
                    body = reader.ReadToEnd();
                }

            
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = objUserModel.EmailID, pass = objUserModel.Password , activationCode =objUserModel.Code}, protocol: Request.Url.Scheme);

                body = body.Replace("{Username}", objUserModel.FirstName);
                body = body.Replace("{ConfirmationLink}", confirmationLink);

                mm.Body = body;
                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("youremail@gmail.com", "password");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);
            }
        }

        [Route("Account/ConfirmEmail")]
        public ActionResult ConfirmEmail(string userId, string pass, string activationCode)
        {
            var check = db.Users.Where(model => model.EmailID == userId && model.Code == new Guid(activationCode)).FirstOrDefault();

            if (check != null)
            {
                if (check.Password.Equals(pass))
                {
                    check.IsEmailVerified = true;

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
            LoginModel objLoginModel = new LoginModel();
            return View(objLoginModel);
        }

        [HttpPost]
        public ActionResult Login(LoginModel objLoginModel)
        {
            if (ModelState.IsValid)
            {
                var result = db.Users.Where(model => model.EmailID == objLoginModel.EmailID && model.Password == objLoginModel.Password).FirstOrDefault();

                if (result == null)
                {
                    ModelState.AddModelError("Error", "Email or password  is Incorrect");
                    return View();
                }
                else if (result.IsEmailVerified == true)
                {

                    FormsAuthentication.SetAuthCookie(objLoginModel.EmailID, objLoginModel.RememberMe);
                    Session["EmailID"] = objLoginModel.EmailID;
                    return RedirectToAction("Index", "Home");
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
            return RedirectToAction("Index", "Home");
        }
    }
}