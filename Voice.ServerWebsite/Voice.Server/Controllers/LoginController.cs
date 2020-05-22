using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using Voice.Shared.Core;
using Voice.Shared.Core.dto;
using Voice.Shared.Core.Interfaces;

namespace Voice.Server.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
     public class LoginController : ApiController
    {
        string _controller_key;
        static IServiceFactory _ServiceFactory;
       
        [HttpPost]
        [ActionName("Login")]
        [ResponseType(typeof(dto_voice_userC))]
        public IHttpActionResult LoginUser(dto_login dto)
        {
            var _loginService = _ServiceFactory.GetService<ILoginService>();
            _loginService.controller_key = _controller_key;
            var _user = _loginService.Login(dto).Result;
            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState);
            }
            return Ok(_user);

        }
    }
}
