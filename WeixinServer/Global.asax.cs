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
        private static Dictionary<string, List<Tuple<string, string>>> cate2ListMap = new Dictionary<string, List<Tuple<string, string>>>();
        private static Dictionary<string, string> cateMap = null;
        private static Dictionary<string, string> cate2CommentMap = null;
        public static Dictionary<string, List<Tuple<string, string>>> GetCateMap()
        {
            return cate2ListMap;
        }
        public static void InitCateMap()
        {
        //    var ret =
        //    from jokes in dbContext.Story
        //    select new {jokes.category, jokes.text_comment, jokes.text};
            var ret = dbContext.Story.Select(p => new { p.category, p.text, p.text_comment }).AsEnumerable();
            foreach (var line in ret)
            {
                var key = line.category;
                var val = new Tuple<string, string>(line.text, line.text_comment);
                if (! cate2ListMap.ContainsKey(key))
                {
                    cate2ListMap[key] = new List<Tuple<string, string>>();
                }
                cate2ListMap[key].Add(val);
            }
            //cateMap = dbContext.Story.ToDictionary(p => p.category,p => p.text);
            //cate2CommentMap = dbContext.Story.ToDictionary(p => p.category, p => p.text_comment);
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
