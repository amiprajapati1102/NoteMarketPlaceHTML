using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace NoteMarketPlaceHtml.Models
{
    public class SystemConfigurationViewModel
    {
       
        public string SupportEmailAddress { get; set; }
        public string SupportContactNumber { get; set; }
        public string EmailAddresssesForNotify { get; set; }
        public string DefaultNoteDisplayPicture { get; set; }
        public string DefaultMemberDisplayPicture { get; set; }
        public string FBICON { get; set; }
        public string TWITTERICON { get; set; }
        public string LNICON { get; set; }

       


    }
}