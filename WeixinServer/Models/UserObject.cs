using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace WeixinServer
{
    [DataContract]
    public class UserObject
    {
        [DataMember]
        public int subscribe { get; set; }
        [DataMember]
        public string openid { get; set; }
        [DataMember]
        public string nickname { get; set; }
        [DataMember]
        public int sex { get; set; }
        [DataMember]
        public string city { get; set; }
        [DataMember]
        public string country { get; set; }
        [DataMember]
        public string province { get; set; }
        [DataMember]
        public string language { get; set; }
        [DataMember]
        public string headimgurl { get; set; }
        [DataMember]
        public long subscribe_time { get; set; }
        [DataMember]
        public string unionid { get; set; }

        public UserObject()
        { }

        public static UserObject RequestUserMsg(string openId, string access_token)
        {
            UserObject uo = new UserObject();
            string requeststring = string.Format(@"https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", access_token, openId);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(requeststring);
            req.Method = @"GET";
            using (WebResponse wr = req.GetResponse())
            {
                HttpWebResponse myRes = (HttpWebResponse)req.GetResponse();
                StreamReader reader = new StreamReader(myRes.GetResponseStream(), Encoding.UTF8);

                string content = reader.ReadToEnd();
                uo = JsonHelper.ParseJson<UserObject>(content);
            }

            return uo;
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