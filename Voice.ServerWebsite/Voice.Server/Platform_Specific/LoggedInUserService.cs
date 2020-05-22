using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Voice.Shared.Core.dto;

namespace Voice.Server.Platform_Specific
{
    public class LoggedInUserService : Voice.Shared.Core.Interfaces.ILoggedInUserService
    {
        public string controller_key { get; set; }
        private dto_logged_user _l_user { get; set; }
        public dto_logged_user GetLoggedInUser()
        {
            return _l_user;
        }
        public LoggedInUserService()
        {
            _l_user = new dto_logged_user();
        }
    }
}