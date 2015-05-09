using System;
using System.Xml;

namespace WeixinServer
{
    public class MsgObject
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public int CreateTime { get; set; }
        public string MsgType { get; set; }
        public string Content { get; set; }
        public Int64 MsgId { get; set; }
        public string PicUrl { get; set; }
        public MsgObject(string xml)
        {
            try
            {   
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);
                XmlElement root = document.DocumentElement;
                var msgtype = root.SelectSingleNode("MsgType");
                var cdataType = (XmlCDataSection)msgtype.FirstChild;

                MsgType = cdataType.InnerText;
                if (cdataType.InnerText == @"text")
                {
                    ToUserName = ((XmlCDataSection)root.SelectSingleNode("ToUserName").FirstChild).InnerText;
                    FromUserName = ((XmlCDataSection)root.SelectSingleNode("FromUserName").FirstChild).InnerText;
                    CreateTime = int.Parse((root.SelectSingleNode("CreateTime")).InnerText);
                    Content = ((XmlCDataSection)root.SelectSingleNode("Content").FirstChild).InnerText;
                    MsgId = Int64.Parse((root.SelectSingleNode("MsgId")).InnerText);
                }

                if (cdataType.InnerText == @"image")
                {
                    ToUserName = ((XmlCDataSection)root.SelectSingleNode("ToUserName").FirstChild).InnerText;
                    FromUserName = ((XmlCDataSection)root.SelectSingleNode("FromUserName").FirstChild).InnerText;
                    CreateTime = int.Parse((root.SelectSingleNode("CreateTime")).InnerText);                    
                    PicUrl = ((XmlCDataSection)root.SelectSingleNode("PicUrl").FirstChild).InnerText;
                    MsgId = Int64.Parse((root.SelectSingleNode("MsgId")).InnerText);
                }
            }
            catch(Exception)
            {
            }
        }
    }
}