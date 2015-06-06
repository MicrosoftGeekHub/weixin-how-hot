using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WeixinServer.Models;
using System.Web.Http;
using WeixinServer.Helpers;
namespace WeixinServer
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static OxfordFaceApiClient OxfordFaceApiClient = new OxfordFaceApiClient("2e595821a4f84c7895800984c4d5da1b");
        private static WeixinDBContext dbContext = new WeixinDBContext();
        private static Dictionary<string, List<Tuple<string, string>>> cate2ListMap = new Dictionary<string, List<Tuple<string, string>>>();
        private static Dictionary<string, string> cateMap = null;
        private static Dictionary<string, string> cate2CommentMap = null;

        public static Dictionary<int, List<Tuple<int, int, int, int, string>>> Age2FaceListMap = new Dictionary<int, List<Tuple<int, int, int, int, string>>>();

        public static ImageSearchClient ImageSearchClient;
        
        public void InitializeImageSearchClient()
        {
            ImageSearchClient = new ImageSearchClient("https://www.bing.com/api/v3/images/search?appid=8CAC7991E5BF99536524FA8020425BE86ECE21D8&amp;mkt=zh-CN");
        }
        public static Dictionary<string, List<Tuple<string, string>>> GetCateMap()
        {
            return cate2ListMap;
        }

        public static void InitFacesMap()
        {
            var ret = dbContext.Faces.Select(p => new { p.age, p.gender, p.smile, p.wearingGlasses, p.headPose, p.url }).AsEnumerable();
            foreach (var line in ret)
            {
                var key = 5 * (line.age / 5);
                var val = new Tuple<int, int, int, int, string>(line.gender, line.smile, line.wearingGlasses, line.headPose, line.url);
                if (!Age2FaceListMap.ContainsKey(key))
                {
                    Age2FaceListMap[key] = new List<Tuple<int, int, int, int, string>>();
                }
                Age2FaceListMap[key].Add(val);
            }
            //cateMap = dbContext.Story.ToDictionary(p => p.category,p => p.text);
            //cate2CommentMap = dbContext.Story.ToDictionary(p => p.category, p => p.text_comment);
            ////dbContext.Dispose();
            
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
            //dbContext.Dispose();
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //GlobalConfiguration.Configure(WebApiConfig.Register);//.RegisterGlobalFilters(GlobalFilters.Filters);
            ////GlobalConfiguration.Configure(HomeApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            InitializeImageSearchClient();
            InitCateMap();
            InitFacesMap();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //var uri = Request.Url.AbsolutePath.ToLower().Replace("://","");
            if (Request.Url.AbsolutePath.EndsWith("/") && !Request.HttpMethod.Equals("POST"))
            {
                Response.ContentType = "text/html";
                Server.Transfer(Request.Url.AbsolutePath + "howhot.html");
            }  
        }
    }
}
