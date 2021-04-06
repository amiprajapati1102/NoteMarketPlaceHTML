using NoteMarketPlaceHtml.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NoteMarketPlaceHtml.Controllers
{
    public class HomeController : Controller
    {
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

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Faq()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}