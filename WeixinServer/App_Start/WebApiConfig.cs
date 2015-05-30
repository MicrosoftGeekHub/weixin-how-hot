using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using System.Web.Routing;

namespace WeixinServer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { controller = "Home", action = "AnalyzeHome"}
                //constraints: new { httpMethod = new HttpMethodConstraint("GET") }

            );

            config.Routes.MapHttpRoute(
                name: "firstHomeApi",
                routeTemplate: "{controller}/{action}",
                defaults: new { controller = "HomeApi", action = "Analyze"}
                //constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );

            //config.Routes.MapHttpRoute(
            //    name: "secondHomeApi",
            //    routeTemplate: "{controller}/{action}",
            //    defaults: new { controller = "Home", action = "Analyze", }
            //    //constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            //);


            config.Routes.MapHttpRoute(
                name: "DefaultMainPage",
                routeTemplate: "{controller}/{action}",
                defaults: new { controller = "HomeApi", action = "ImageSearch"}
                //constraints: new { httpMethod = new HttpMethodConstraint("GET") }

            );
        }
    }
}
