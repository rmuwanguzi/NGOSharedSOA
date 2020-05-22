using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Autofac;
using System.Web.Http.Routing;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Dispatcher;

namespace Voice.Server.Handlers
{
    public class XAccessTokenHandler:DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) //Standard signature
        {
            string tokenName = "XAuth-Token";
            if (request.Headers.Contains(tokenName)) //Check if request header contains auth token or not.
            {
                 string requestToken= request.Headers.GetValues(tokenName).First(); //get the first of Auth token from request header
                try
                {
                   if (string.IsNullOrWhiteSpace(requestToken))
                    {
                        if (IsLoginController(request, "Login"))
                        {
                            return base.SendAsync(request, cancellationToken);
                        }
                        throw new Exception();
                        
                    }
                    ClaimsPrincipal claimsPrincipal = Voice.Shared.Core.ObjectBase._container.Resolve<Voice.Shared.Core.Interfaces.ITokenService>().ValidateToken(requestToken);
                    if (claimsPrincipal != null)
                    {
                        request.GetRequestContext().Principal = claimsPrincipal;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception ex) //token not found or invalid token
                {
                    HttpResponseMessage reply = request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid Token.");
                    return Task.FromResult(reply);
                }
                return base.SendAsync(request, cancellationToken);
            }
            else
            {
                if (IsLoginController(request, "Login"))
                {
                    return base.SendAsync(request, cancellationToken);
                }
            }
           
           HttpResponseMessage error_reply = request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Oooops,You Cannot Make This Request Without A Token");
             return Task.FromResult(error_reply);
            
        }
        public bool IsLoginController(HttpRequestMessage request, string controller_name)
        {
            try
            {
                var config = request.GetConfiguration();
                var routeData = config.Routes.GetRouteData(request);
                var controllerContext = new HttpControllerContext(config, routeData, request);

                request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
                controllerContext.RouteData = routeData;

                // get controller type
                var controllerDescriptor = new DefaultHttpControllerSelector(config).SelectController(request);
                var name = controllerDescriptor.ControllerName;
                if(name.ToLower()==controller_name.ToLower())
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

    }
}