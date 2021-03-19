
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using NoteMarketPlace.DbModel;
using NoteMarketPlace.Models;
namespace NoteMarketPlace.Controllers
{
    public class HomeController : Controller
    {
        NoteMarketPlaceHtmlEntities db = new NoteMarketPlaceHtmlEntities();

      // GET: Home
      public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "admin")]
        public ActionResult Contact()
        {

            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.Name;
                User currentUser = db.Users.FirstOrDefault(x => x.EmailID == userId);
                ContactModel model = new ContactModel();
                model.Name = currentUser.FirstName;
                model.EmailID = currentUser.EmailID;
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
            if (db.Users.Any(x => x.EmailID == model.EmailID))
            {

                using (MailMessage mm = new MailMessage("email@gmail.com", model.EmailID))
                {
                    mm.Subject = model.Name+" "+model.Comments;

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/ContactUs.html")))
                    {
                        body = reader.ReadToEnd();
                    }
                   
                    body = body.Replace("{Comments}", model.Comments);
                    body = body.Replace("{Name}", model.Name);
                    mm.Body = body;
                    mm.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    NetworkCredential NetworkCred = new NetworkCredential("email@gmail.com", "password");
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