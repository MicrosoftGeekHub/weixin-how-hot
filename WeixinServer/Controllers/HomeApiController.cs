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
using System.Web.Http;
using System.Web.Security;
using WeixinServer.Helpers;
using WeixinServer.Models;
using System.Drawing;
using System.Net.Http.Headers;
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
//using UpYunLibrary;
namespace WeixinServer.Controllers
{
    public class HomeApiController : ApiController
    {

        //public void Index()
        //{

        //    if (Request.Url.AbsolutePath.EndsWith("/"))
        //    {
        //        Server.Transfer(Request.Url.AbsolutePath + "index.aspx");
        //    }
        //}

        private DateTime startTime = DateTime.Now;

        
        private byte[] mDummyBytes = Encoding.ASCII.GetBytes("[object Object]");
        [System.Web.Mvc.HttpPost]
        public HttpResponseMessage ImageSearch([NakedBody] byte[] queryBytes)
        {
            String query = System.Text.Encoding.UTF8.GetString(queryBytes);
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
        private string[] keyPool = new string[4] { "cc9e33682fcd4eeab114f9a63dc16021", "d536bbe7125b42a9b948338a54b4ebb7", "8b2854891a9f436e8ecd60127ca62fd9", "b03be7fdc0fd476db7e35fda40494090" };
        private string GetVisionAPIkey()
        {
            var random = new Random();
            var getrandomIdx = random.Next(0, 3);
            return keyPool[getrandomIdx];
        }
        private string fontPath = System.Web.HttpContext.Current.Server.MapPath(@"~\App_Data\xujl-font.ttf");
        private string md5;
        [System.Web.Mvc.HttpPost]
        public async Task<HttpResponseMessage> Analyze(string faceUrl = "", string photoName = "")
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
                if (string.Equals(Request.Content.GetType(), "application/octet-stream"))
                {
                    // contentLength = Request.Content.;
                    var stream = new MemoryStream();
                    await Request.Content.CopyToAsync(stream);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    //var img = System.Drawing.Image.FromStream(Request.InputStream);
                    //return Json(JsonConvert.SerializeObject(img.Width), "application/json");
                    res = await vision.AnalyzeImage(stream);

                }
                else if (!string.IsNullOrEmpty(faceUrl) && faceUrl != "undefined")
                {
                    res = await vision.AnalyzeImage(faceUrl);
                }


                //Trace.WriteLine(string.Format("Completed Analyze Request: RequestId: {0};", requestId));
                //return Json(JsonConvert.SerializeObject(res),(Newtonsoft.Json.JsonSerializerSettings) "application/json", System.Text.Encoding.UTF8);


                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                //return Json(JsonConvert.SerializeObject(Json(JsonConvert.SerializeObject(res)).Content));
                result.Content = new StringContent(JsonConvert.SerializeObject(Json(JsonConvert.SerializeObject(res)).Content));//new StringContent(JsonConvert.SerializeObject(res));
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return result;

                //Error	20	Argument 2: cannot convert from 'string' to 'Newtonsoft.Json.JsonSerializerSettings'

            }
            catch (Exception e)
            {
                return null;//new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}