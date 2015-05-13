using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
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

        private DateTime startTime = DateTime.Now;
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
        private bool ProcessMsg(string xml)
        {
            MsgObject msg = new MsgObject(xml);

            if (msg.MsgType != "image")
            {
                string resString = "请点+号输入一张人物风景照片试试";
                Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content></xml>",
                    msg.FromUserName, msg.ToUserName, resString));
                Response.End();
                return false;
            }

            bool isDebug = false;
            if (isDebug)
            {
                string debugString = msg.PicUrl;
                Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content></xml>",
                    msg.FromUserName, msg.ToUserName, msg.PicUrl));
                Response.End();
                return false;
            }

            //check data from db
            using (var dbContext = new WeixinDBContext())
            {
                ImageStorage image = dbContext.ImageStorages.FirstOrDefault(p => p.OpenId == msg.FromUserName && p.PicUrl == msg.PicUrl && p.CreateTime == msg.CreateTime);
                //ImageStorage image = dbContext.ImageStorages.FirstOrDefault(p => p.PicUrl == msg.PicUrl);
                if (image != null)
                {
                    Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content><DebugInfo><![CDATA[{3}]]></DebugInfo><ErrorInfo><![CDATA[{4}]]></ErrorInfo></xml>",
                        msg.FromUserName, msg.ToUserName, image.ParsedDescription, image.TimeLog, string.Empty));
                    Response.End();
                    return true;
                }
            }

            //string imagePathorUrl = msg.PicUrl;
            //string imagePathorUrl = msg.PicUrl.Replace("https://", "").Replace("http://", "");
            //var ret = vision.AnalyzeImage(msg.PicUrl);
            RichResult ret = null;
            //ret = vision.AnalyzeImage(msg.PicUrl); 
            VisionHelper vision = new VisionHelper("cc9e33682fcd4eeab114f9a63dc16021", System.Web.HttpContext.Current.Server.MapPath(@"~\App_Data\xujl-font.ttf"), this.startTime);
           
            Task.Run(async () =>
            {
                ret = await vision.AnalyzeImage(msg.PicUrl, msg.FromUserName);

            }).Wait();

            //save db first without parsedContent
            int id = -1;
            using (var dbContext = new WeixinDBContext())
            {
                ImageStorage image = new ImageStorage();
                image.OpenId = msg.FromUserName;
                image.CreateTime = msg.CreateTime;
                image.PicUrl = msg.PicUrl;
                image.PicContent = ret.rawImage;
                image.ParsedUrl = ret.uploadedUrl;
                image.ParsedContent = null;
                image.ParsedDescription = ret.analyzeImageResult;
                image.TimeLog = ret.timeLogs;
                dbContext.ImageStorages.Add(image);
                dbContext.SaveChanges();
                id = image.Id;
            }

            // Debug mode
            Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content><DebugInfo><![CDATA[{3}]]></DebugInfo><ErrorInfo><![CDATA[{4}]]></ErrorInfo></xml>",
                msg.FromUserName, msg.ToUserName, ret.analyzeImageResult, ret.timeLogs, ret.errorLogs));

            // Production mode
            //Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content></xml>", msg.FromUserName, msg.ToUserName, ret.analyzeImageResult));

            Response.End();

            //write to DB
            var webClient = new WebClient();
            var processedImageBytes = webClient.DownloadData(ret.uploadedUrl);

            using (var dbContext = new WeixinDBContext())
            {
                ImageStorage image = dbContext.ImageStorages.FirstOrDefault(p => p.Id == id);
                if (image != null)
                {
                    image.ParsedContent = processedImageBytes;
                    dbContext.SaveChanges();   
                }
            }

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