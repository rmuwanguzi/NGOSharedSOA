using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;

namespace Voice.Server
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Add(new Handlers.PreflightRequestsHandler());
            config.MessageHandlers.Add(new Handlers.XAccessTokenHandler());
       
            config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }

             );
           // var cors = new EnableCorsAttribute("www.example.com", "*", "*");
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
         

            if (datam.DATA_CONTROLLER_MODEL_STATE == null)
            {
                datam.DATA_CONTROLLER_MODEL_STATE = new SortedList<string, System.Web.Http.ModelBinding.ModelStateDictionary>();
            }
            fn.Injection();
        }
    }
}
