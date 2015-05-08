using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace WeixinServer
{
    [DataContract]
    public class AccessToken
    {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public string expires_in { get; set; }

        public AccessToken()
        {
        }

        private static AccessToken RefreshAccessToken()
        {
            string appid = @"wxd7ab93b2f82f930b";
            string secret = @"df018c6c5aa377d48da2e3ed98c6a615";
            string strUrl = string.Format(@"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appid, secret);
            AccessToken mode = new AccessToken();

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            req.Method = @"GET";
            using (WebResponse wr = req.GetResponse())
            {
                HttpWebResponse myRes = (HttpWebResponse)req.GetResponse();
                StreamReader reader = new StreamReader(myRes.GetResponseStream(), Encoding.UTF8);

                string content = reader.ReadToEnd();
                mode = JsonHelper.ParseJson<AccessToken>(content);
            }
            return mode;
        }

        public static string GetAccessToken()
        {
            string token = string.Empty;

            using (var dbContext = new WeixinDBContext())
            {
                Token tk = dbContext.Tokens.FirstOrDefault(p => p.AccessToken != string.Empty);
                if (tk == null)
                {
                    tk = new Token();
                    DateTime newExpire = DateTime.Now;
                    AccessToken mode = RefreshAccessToken();
                    token = mode.access_token;
                    tk.AccessToken = mode.access_token;
                    tk.ExpireDate = newExpire.AddSeconds(int.Parse(mode.expires_in)).ToShortDateString();
                    dbContext.Tokens.Add(tk);
                    dbContext.SaveChanges();
                }
                else if (DateTime.Now >= Convert.ToDateTime(tk.ExpireDate))
                {
                    DateTime newExpire = DateTime.Now;
                    AccessToken mode = RefreshAccessToken();
                    token = mode.access_token;
                    tk.AccessToken = mode.access_token;
                    tk.ExpireDate = newExpire.AddSeconds(int.Parse(mode.expires_in)).ToString();
                    dbContext.SaveChanges();
                }
                else
                {
                    token = tk.AccessToken;
                }
            }
            return token;
        }
    }

    public class JsonHelper
    {
        public static T ParseJson<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                return (T)serializer.ReadObject(ms);
            }
        }
    }
}