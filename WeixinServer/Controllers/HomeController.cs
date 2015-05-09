using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using WeixinServer.Helpers;
using WeixinServer.Models;

//using UpYunLibrary;
namespace WeixinServer.Controllers
{
    public class HomeController : Controller
    {

        
        public ActionResult Index()
        {
            
            if (Request.HttpMethod.ToUpper() == "POST")
            {
                //send response
                ProcessPost();
            }
            else
            {
                if (!Valid())
                {
                    return View();
                }
            }

            return View();
        }

        public void ProcessPost()
        {
            string postString = string.Empty;

            using (Stream stream = Request.InputStream)
            {
                byte[] postBytes = new byte[stream.Length];
                stream.Read(postBytes, 0, (int)stream.Length);
                postString = Encoding.UTF8.GetString(postBytes);                
            }

            if (!string.IsNullOrEmpty(postString))
            {

                if (ProcessMsg(postString))
                {
                    //...
                }
                else
                {
                    //...
                }
            }
        }

        private bool Valid()
        {
            string echoStr = Request.QueryString["echoStr"];
            if (!checkSignature())
            {
                return false;
            }

            if (string.IsNullOrEmpty(echoStr))
            {
                return false;
            }

            Response.Write(echoStr);
            Response.End();
            return true;
        }

        private bool checkSignature()
        {
            try
            {
                string signature = Request.QueryString["signature"];
                string timestamp = Request.QueryString["timestamp"];
                string nonce = Request.QueryString["nonce"];
                string[] array = new string[] { "msftgeek", timestamp, nonce };
                Array.Sort(array);
                string tmpStr = string.Join("", array);
                tmpStr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
                tmpStr = tmpStr.ToLower();

                if (tmpStr == signature)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string subscriptionKey = ConfigurationManager.AppSettings["subscriptionKey"];
        private VisionHelper vision = new VisionHelper("8b2854891a9f436e8ecd60127ca62fd9", System.Web.HttpContext.Current.Server.MapPath(@"~\App_Data\frame.png"));
        private bool ProcessMsg(string xml)
        {
            MsgObject msg = new MsgObject(xml);
            if (msg.MsgType != "image")
            {
                Response.Write("目前只支持jpg/png等图片格式");
                Response.End();
                return false;
            }

            //string imagePathorUrl = msg.PicUrl;
            //string imagePathorUrl = msg.PicUrl.Replace("https://", "").Replace("http://", "");
            //var ret = vision.AnalyzeImage(msg.PicUrl);
            RichResult ret = null;
            //ret = vision.AnalyzeImage(msg.PicUrl);            
            Task.Run(async () =>
            {
                ret = await vision.AnalyzeImage(msg.PicUrl);

            }).Wait();
            
            // Debug mode
            Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content><DebugInfo><![CDATA[{3}]]></DebugInfo><ErrorInfo><![CDATA[{4}]]></ErrorInfo></xml>", msg.FromUserName, msg.ToUserName, ret.analyzeImageResult, ret.timeLogs, ret.errorLogs));

            // Production mode
            //Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content></xml>", msg.FromUserName, msg.ToUserName, ret.analyzeImageResult));
            
            Response.End();

            return true;
        }

        private bool ProcessTxtMsg(string xml)
        {
            MsgObject msg = new MsgObject(xml);
            if (msg.MsgType != "text")
            {
                return false;
            }

            string content = msg.Content;
            if (content.Length > 0)
            {
                using (var dbContext = new WeixinDBContext())
                {
                    bool filter = false;
                    var blackList = dbContext.BlackList.Select(p => p.Value).ToList();
                    foreach (var value in blackList)
                    {
                        if (content.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            filter = true;
                            break;
                        }
                    }
                    if (filter == false)
                    {
                        Danmu danmu = new Danmu();
                        danmu.UserName = string.Empty;
                        danmu.OpenId = msg.FromUserName;
                        danmu.CreateTime = msg.CreateTime;
                        danmu.Content = content;
                        dbContext.Danmus.Add(danmu);
                        dbContext.SaveChanges();
                    }
                }
            }

            return true;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}