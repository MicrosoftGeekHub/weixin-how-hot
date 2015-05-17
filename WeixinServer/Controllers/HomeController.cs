using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using WeixinServer.Helpers;
using WeixinServer.Models;
using System.Drawing;

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
                Valid();
                return View("HowOld");
            }

            return View();
        }

        public ActionResult Image()
        {
            if (Request.HttpMethod.ToUpper() == "POST")
            {
                //send response
                ProcessPost(true);
            }
            else
            {
                if (!Valid())
                {
                    return View("Index");
                }
            }

            return View("Index");
        }

        public void ProcessPost(bool returnImage = false)
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

                bool isProcessed = false;
                Task.Run(async () =>
            {
                isProcessed = await ProcessMsg(postString, returnImage);
            }).Wait();


                if (isProcessed)
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
        private static string GetMd5(string str)
        {
            var m = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] s = m.ComputeHash(UnicodeEncoding.UTF8.GetBytes(str));
            string resule = BitConverter.ToString(s);
            resule = resule.Replace("-", "");
            return resule.ToLower();
        }

        private async Task<RichResult> QuickReturn(VisionHelper vision, MsgObject msg)
        {
            var ret = await vision.AnalyzeImage(msg.PicUrl, msg.FromUserName);
            if (ret != null)
            {
                //Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content><DebugInfo><![CDATA[{3}]]></DebugInfo><ErrorInfo><![CDATA[{4}]]></ErrorInfo></xml>",
                //    msg.FromUserName, msg.ToUserName, ret.analyzeImageResult, ret.timeLogs, ret.errorLogs));
                if (ret.errorLogs.Equals(""))
                {
                    // Production mode
                    Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content></xml>",
                    msg.FromUserName, msg.ToUserName, ret.analyzeImageResult));
                    Response.End();
                    return ret;
                }
                else 
                {
                    //Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content></xml>",
                    //msg.FromUserName, msg.ToUserName, "机器人很忙，请稍后再试"));
                    //Response.End();
                    return ret;
                }
                
            }
            return null;
        }

        private string[] keyPool = new string[4] { "cc9e33682fcd4eeab114f9a63dc16021", "d536bbe7125b42a9b948338a54b4ebb7", "8b2854891a9f436e8ecd60127ca62fd9", "b03be7fdc0fd476db7e35fda40494090" };
        private string GetVisionAPIkey()
        {
            var random = new Random();
            var getrandomIdx = random.Next(0, 3);
            return keyPool[getrandomIdx];
        }
        private string fontPath = System.Web.HttpContext.Current.Server.MapPath(@"~\App_Data\xujl-font.ttf");
        private string md5;
        private async Task<bool> ProcessMsg(string xml, bool returnImage)
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



            //string imagePathorUrl = msg.PicUrl;
            //string imagePathorUrl = msg.PicUrl.Replace("https://", "").Replace("http://", "");
            //var ret = vision.AnalyzeImage(msg.PicUrl);
            RichResult ret = null;
            //ret = vision.AnalyzeImage(msg.PicUrl); 

            VisionHelper vision = new VisionHelper(GetVisionAPIkey(), fontPath, DateTime.Now, fontPath);


            var task = QuickReturn(vision, msg);
            //md5 = GetMd5(msg.PicUrl);
            md5 = GetMd5(msg.PicUrl + msg.CreateTime.ToString());
            //check data from db
            using (var dbContext = new WeixinDBContext())
            {
                //ImageStorage image = dbContext.ImageStorages.FirstOrDefault(p => p.OpenId == msg.FromUserName && p.PicUrl == msg.PicUrl && p.CreateTime == msg.CreateTime);
                //ImageStorage image = dbContext.ImageStorages.FirstOrDefault(p => p.PicUrl == msg.PicUrl);
                //ImageStorage image = dbContext.ImageStorages.FirstOrDefault(p => p.PicUrl == msg.PicUrl && p.CreateTime == msg.CreateTime);

                //ImageStorage image = dbContext.ImageStorages.FirstOrDefault(p => p.Md5 == md5 && p.CreateTime == msg.CreateTime);
                ImageStorage image = dbContext.ImageStorages.FirstOrDefault(p => p.Md5 == md5);    
                if (image != null)
                {
                    Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content><DebugInfo><![CDATA[{3}]]></DebugInfo><ErrorInfo><![CDATA[{4}]]></ErrorInfo></xml>",
                        msg.FromUserName, msg.ToUserName, image.ParsedDescription, image.TimeLog, md5));
                    Response.End();
                    return true;
                }
                dbContext.Dispose();
            }


            
            ret = await task;

            //when not results in DB and got error
            if (!ret.errorLogs.Equals(""))
            {
                //Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content></xml>",
                //        msg.FromUserName, msg.ToUserName, "机器人很忙，请稍后再试"));
                Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content><DebugInfo><![CDATA[{3}]]></DebugInfo><ErrorInfo><![CDATA[{4}]]></ErrorInfo></xml>",
                msg.FromUserName, msg.ToUserName, ret.analyzeImageResult, ret.timeLogs, ret.errorLogs));
                Response.End();
            }
            //// Debug mode
            //Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content><DebugInfo><![CDATA[{3}]]></DebugInfo><ErrorInfo><![CDATA[{4}]]></ErrorInfo></xml>",
            //    msg.FromUserName, msg.ToUserName, ret.analyzeImageResult, ret.timeLogs, ret.errorLogs));

            // Production mode
            //Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content></xml>", msg.FromUserName, msg.ToUserName, ret.analyzeImageResult));

            //Response.End();


            //save db first without parsedContent
            int id = -1;
            using (var dbContext = new WeixinDBContext())
            {
                ImageStorage image = new ImageStorage();
                image.OpenId = msg.FromUserName;
                image.CreateTime = msg.CreateTime;
                image.PicUrl = msg.PicUrl;
                image.Md5 = GetMd5(msg.PicUrl + msg.CreateTime.ToString() + ret.errorLogs);
                //image.PicContent = ret.rawImage;
                image.ParsedUrl = ret.uploadedUrl;
                image.ParsedDescription = ret.analyzeImageResult + ret.errorLogs;
                image.TimeLog = ret.timeLogs;
                dbContext.ImageStorages.Add(image);
                dbContext.SaveChanges();
                id = image.Id;
                dbContext.Dispose();
            }

            //write to DB
            //var webClient = new WebClient();
            //var processedImageBytes = webClient.DownloadData(ret.uploadedUrl);

            //using (var dbContext = new WeixinDBContext())
            //{
            //    ImageStorage image = dbContext.ImageStorages.FirstOrDefault(p => p.Id == id);
            //    if (image != null)
            //    {
            //        image.ParsedContent = processedImageBytes;
            //        dbContext.SaveChanges();   
            //    }
            //}

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


        [HttpPost]
        public async Task<ActionResult> Analyze(string faceUrl = "")
        {
            string requestId = Guid.NewGuid().ToString();
            int? contentLength = null;
            VisionHelper vision = new VisionHelper(GetVisionAPIkey(), fontPath, DateTime.Now, fontPath);
            RichResult res = null;
            try
            {

                string postString = string.Empty;

                //using (Stream stream = Request.InputStream)
                //{
                //    byte[] postBytes = new byte[stream.Length];
                //    stream.Read(postBytes, 0, (int)stream.Length);
                //    postString = Encoding.Unicode.GetString(postBytes);
                //    return Json(JsonConvert.SerializeObject(postString), "application/json");
                //}

                Stopwatch stopwatch = Stopwatch.StartNew();
                //Trace.WriteLine(string.Format("Start Analyze Request: RequestId: {0};", requestId));
                if (Request.ContentType == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "BadRequest");

                }
                if (string.Equals(Request.ContentType, "application/octet-stream"))
                {
                    contentLength = Request.ContentLength;
                    Request.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                    //var img = System.Drawing.Image.FromStream(Request.InputStream);

                    //return Json(JsonConvert.SerializeObject(img.Width), "application/json");
                    res = await vision.AnalyzeImage(Request.InputStream);

                }
                else if (!string.IsNullOrEmpty(faceUrl) && faceUrl != "undefined")
                {
                    res = await vision.AnalyzeImage(faceUrl);
                }
                

                //Trace.WriteLine(string.Format("Completed Analyze Request: RequestId: {0};", requestId));
                return Json(JsonConvert.SerializeObject(res), "application/json");
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        //[HttpPost]
        //public async Task<HttpResponseMessage> ImageSearch(string query)
        //{
        //    string requestId = Guid.NewGuid().ToString();
        //    try
        //    {
        //        //Trace.WriteLine(string.Format("Start Search Request: RequestId: {0};", requestId));
        //        var results = await ImageSearchClient.SearchImages(query);
        //        if (results == null || results.Length == 0)
        //        {
        //            return HttpNotFound();
        //        }
        //        //Trace.WriteLine(string.Format("Completed Search Request: RequestId: {0};", requestId));
        //        return Json(JsonConvert.SerializeObject(results), "application/json");
        //    }
        //    catch (Exception e)
        //    {
        //        Telemetry.TrackError(string.Format("Error While Searching: {0}; Error:{1}", query, e.ToString()), requestId);
        //        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error");
        //    }
        //}
    }
}