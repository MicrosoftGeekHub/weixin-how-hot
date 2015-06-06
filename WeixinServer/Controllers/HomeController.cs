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
using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.ProjectOxford.Face.Contract;

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
                //return View("howhot");
            }

            return View("howhot");
            //return View();
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
                    var returnString = string.Format("{0} 想知道贵图有多火辣么? 请看归图:{1}", ret.analyzeImageResult, ret.uploadedUrl);
                    Response.Write(string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>12345678</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{2}]]></Content></xml>",
                    msg.FromUserName, msg.ToUserName, returnString));
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



        private async Task<AnalyzeResultsModel> GetAnalyzeResultsModelFromUrl(string faceUrl = null, string faceName = null, bool isTest = false)
        {
            string requestId = Guid.NewGuid().ToString();
            int? contentLength = null;
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                //Trace.WriteLine(string.Format("Start Analyze Request: RequestId: {0};", requestId));
                //ImageSubmissionMethod method;
                var imageResult = new List<FaceModel>();
                if (Request.ContentType == null)
                {
                    return null;
                }
                if (string.Equals(Request.ContentType, "application/octet-stream"))
                {
                    contentLength = Request.ContentLength;
                    Microsoft.ProjectOxford.Face.Contract.Face[] retfaces = await MvcApplication.OxfordFaceApiClient.UploadStreamAndDetectFaces(Request.InputStream);

                    for (int i = 0; i < retfaces.Length; i++)
                    {
                        var r = retfaces[i].FaceRectangle;
                        var faceModel = new FaceModel
                        {
                            FaceRectangle = new WeixinServer.Models.Rectangle
                            {
                                Left = r.Left,
                                Top = r.Top,
                                Width = r.Width,
                                Height = r.Height,
                            }
                        };
                        faceModel.FaceId = retfaces[i].FaceId.ToString();
                        faceModel.Attributes = new FaceAttributes();
                        imageResult.Add(faceModel);
                    }
                    ////imageResult = MvcApplication.FaceClientDirect.AnalyzeByImageData(Request.InputStream);

                }
                else if (!string.IsNullOrEmpty(faceUrl) && faceUrl != "undefined")
                {

                    Microsoft.ProjectOxford.Face.Contract.Face[] retfaces = await MvcApplication.OxfordFaceApiClient.UploadStreamAndDetectFaces(faceUrl);
                    for (int i = 0; i < retfaces.Length; i++)
                    {
                        var r = retfaces[i].FaceRectangle;
                        var faceModel = new FaceModel
                        {
                            FaceRectangle = new WeixinServer.Models.Rectangle
                            {
                                Left = r.Left,
                                Top = r.Top,
                                Width = r.Width,
                                Height = r.Height,
                            }
                        };
                        faceModel.FaceId = retfaces[i].FaceId.ToString();
                        faceModel.Attributes = new FaceAttributes();
                        imageResult.Add(faceModel);
                    }
                }
                else
                {
                    //Telemetry.TrackWarning(
                    //    string.Format(
                    //        "Bad Request while Analyzing Image. faceUrl: {0}; faceName: {1}; isTest: {2}", faceUrl,
                    //        faceName, isTest), requestId);
                    return null;
                }
                AnalyzeResultsModel model = new AnalyzeResultsModel();
                model.Faces = imageResult;
                long elapsed = stopwatch.ElapsedMilliseconds;
                
                return model;
            }
            catch (Exception e)
            {
                //Telemetry.TrackError(
                //    string.Format(
                //        "Error Analyzing Image. Error: {0}; faceUrl: {1}; faceName: {2}; isTest: {3}",
                //        e.ToString(), faceUrl, faceName, isTest), requestId);
                return null;
            }
        }

        public void TagModel(AnalyzeResultsModel leftmodel, string belongTo)
        {
            foreach (var face in leftmodel.Faces)
            {
                face.BelongTo = belongTo;
            }
            if (leftmodel.Faces.Count == 0)
            {
                leftmodel.Category = "nofaces";
            }
            else if (leftmodel.Faces.Count > 1)
            {
                leftmodel.Category = "faceselect";
            }
            else
            {
                leftmodel.Category = "success";
            }

        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> Analyze(string leftFaceUrl = null, string leftFaceName = null, string rightFaceUrl = null, string rightFaceName = null, bool isTest = false)
        {
            if (Request.ContentType == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "BadRequest");

            }
            AnalyzeResultsModel[] models = new AnalyzeResultsModel[2];
            models[0] = new AnalyzeResultsModel();
            models[1] = new AnalyzeResultsModel();
            models[0] = await GetAnalyzeResultsModelFromUrl(leftFaceUrl, leftFaceName);
            models[1] = await GetAnalyzeResultsModelFromUrl(rightFaceUrl, rightFaceName);
            if (models[0] == null || models[1] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error");
            }

            TagModel(models[0], "left");
            TagModel(models[1], "right");

            JObject jresponse = new JObject();
            jresponse["models"] = JsonConvert.SerializeObject(models);
            if (models[0].Category.Equals(models[1].Category) && models[0].Category.Equals("success"))
            {
                var similarity = 0.0f;//await MvcApplication.OxfordFaceApiClient.CalculateSimilarity(Guid.Parse(models[0].Faces[0].FaceId), Guid.Parse(models[1].Faces[0].FaceId));
                string description = string.Format("{0:F2}% with FaceId {1}", similarity * 100, models[0].Faces[0].ToString());
                jresponse["similarity"] = similarity;
                jresponse["description"] = description;
            }
            return Json(JsonConvert.SerializeObject(jresponse), "application/json");
            //return Content(JsonConvert.SerializeObject(models), "application/json");
        }


        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> RelationShipAnalyze(string faceUrl = null, string faceName = null, bool isTest = false)
        {
            if (Request.ContentType == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "BadRequest");

            }
            AnalyzeResultsModel[] models = new AnalyzeResultsModel[1];
            models[0] = new AnalyzeResultsModel();
            models[0] = await GetAnalyzeResultsModelFromUrl(faceUrl, faceName);
            if (models[0] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error");
            }

            JObject jresponse = new JObject();
            foreach (var face in models[0].Faces)
            { 
                jresponse["models"] = JsonConvert.SerializeObject(models);
                var similarity = await MvcApplication.OxfordFaceApiClient.CalculateSimilarity(Guid.Parse(models[0].Faces[0].FaceId), Guid.Parse(models[1].Faces[0].FaceId));
                string description = string.Format("{0:F2}% with FaceId {1}", similarity * 100, models[0].Faces[0].FaceId.ToString());
                jresponse["similarity"] = similarity;
                jresponse["description"] = description;
            }
            return Json(JsonConvert.SerializeObject(jresponse), "application/json");
            //return Content(JsonConvert.SerializeObject(models), "application/json");
        }



        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> HowSimilar(string leftFaceID = null, string rightFaceID = null)
        {
            var similarity = 0.0f;

            similarity = await MvcApplication.OxfordFaceApiClient.CalculateSimilarity(Guid.Parse(leftFaceID), Guid.Parse(rightFaceID));

            string description = string.Format("{0:F2}%", similarity * 100);

            JObject jresponse = new JObject();
            jresponse["similarity"] = similarity;
            jresponse["description"] = description;
            string responseStr = JsonConvert.SerializeObject(jresponse);
            return Json(JsonConvert.SerializeObject(responseStr), "application/json");
        }


        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> AnalyzeHome(string faceUrl = "", string photoName = "")
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

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> AnalyzeOneImage(string faceUrl = "", string photoName = "")
        {
            string requestId = Guid.NewGuid().ToString();
            //int? contentLength = null;
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
                //if (Request.Content.GetType() == null)
                //{
                //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "BadRequest");

                //}

                if (!string.IsNullOrEmpty(faceUrl) && faceUrl != "undefined")
                {
                    res = await vision.AnalyzeImage(faceUrl);
                }
                else
                {
                    //if (string.Equals(Request.Headers.GetType(), "application/octet-stream"))
                    //{
                    // contentLength = Request.Content.;
                    //contentLength = Request.ContentLength;      
                    //var stream = new MemoryStream();
                    //await Request.Content.CopyToAsync(stream);
                    //stream.Seek(0, System.IO.SeekOrigin.Begin);
                    res = await vision.AnalyzeImage(Request.InputStream);

                }

                if (res.analysisResult.RichFaces.Length > 0)
                {
                    res.stepSize = 5;
                    res.minAge = res.stepSize * (int)((int)res.analysisResult.RichFaces[0].Attributes.Age / (float)res.stepSize);
                    res.maxAge = 90;

                    var urls = new List<string>();
                    for (var age = res.minAge; age <= res.maxAge; age += res.stepSize)
                    {
                        var url = GetImageUrlByAge(age, res.analysisResult.RichFaces[0]);
                        urls.Add(!string.IsNullOrEmpty(url) ? url : "");
                    }
                    res.agingImgUrls = urls.ToArray();
                }
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                return Json(JsonConvert.SerializeObject(res), "application/json");
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.Message);
            }
        }


        private byte[] mDummyBytes = Encoding.ASCII.GetBytes("[object Object]");
        [System.Web.Mvc.HttpPost]
        public HttpResponseMessage ImageSearch([NakedBody] byte[] queryBytes)
        {
            String query = "";//System.Text.Encoding.UTF8.GetString(queryBytes);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://how-old.net/Home/BingImageSearch?query=" + query);
            request.Method = "POST";
            request.ContentType = "text/plain;charset=UTF-8";

            request.ContentLength = mDummyBytes.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(mDummyBytes, 0, mDummyBytes.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            String responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(responseString);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return result;
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> BingImageSearch(string query)
        {
            string requestId = Guid.NewGuid().ToString();
            try
            {
                //Trace.WriteLine(string.Format("Start Search Request: RequestId: {0};", requestId));
                var results = await MvcApplication.ImageSearchClient.SearchImages(query);
                if (results == null || results.Length == 0)
                {
                    return HttpNotFound();
                }
                //Trace.WriteLine(string.Format("Completed Search Request: RequestId: {0};", requestId));
                return Json(JsonConvert.SerializeObject(results), "application/json");
            }
            catch (Exception e)
            {
                //Telemetry.TrackError(string.Format("Error While Searching: {0}; Error:{1}", query, e.ToString()), requestId);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error");
            }
        }

        public string GetImageUrlByAge(int age, Face face)
        {
            //get images with the given age
            try
            {
                var list = MvcApplication.Age2FaceListMap[age];
                string url = "";
                var urlList = new List<string>();
                var random = new Random();
                int idx = 0;
                foreach (var line in list)
                {
                    var gender = line.Item1;
                    int faceGender = 1; //male
                    if (! face.Attributes.Gender.Equals("male"))
                        faceGender = 2;
                    if (gender != faceGender) continue;
                    urlList.Add(line.Item5);
                    idx = random.Next(0, urlList.Count);
                    if (idx > 10 - urlList.Count)
                    {
                        return urlList[idx];
                    }

                }
                var getrandomIdx = random.Next(0, urlList.Count - 1);
                return urlList[getrandomIdx];
            }
            catch (Exception e)
            {
                return null;//new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}