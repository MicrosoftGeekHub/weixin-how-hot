using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WeixinServer.Models;

namespace WeixinServer
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static WeixinDBContext dbContext = new WeixinDBContext();
        private static Dictionary<string, string> cateMap = null;
        public static Dictionary<string, string> GetCateMap()
        {
            return  cateMap;
        }
        public static void InitCateMap()
        {
            cateMap = dbContext.Story.ToDictionary(p => p.category, p => p.text);
            dbContext.Dispose();
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            InitCateMap();
        }
    }
}
