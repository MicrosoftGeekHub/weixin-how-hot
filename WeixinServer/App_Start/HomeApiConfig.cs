using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace WeixinServer
{
    public static class HomeApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "secApi",
                routeTemplate: "{controller}/{action}",
                defaults: new { controller = "HomeApi", action = "Analyze",}
            );
        }
    }
}
