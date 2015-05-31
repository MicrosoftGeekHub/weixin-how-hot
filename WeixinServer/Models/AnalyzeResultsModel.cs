
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Vision.Contract;

namespace WeixinServer.Models
{
    public class FaceModel
    {
        [JsonProperty("faceId")]
        public string FaceId { get; set; }

        [JsonProperty("faceRectangle")]
        public Rectangle FaceRectangle { get; set; }

        [JsonProperty("attributes")]
        public FaceAttributes Attributes { get; set; }

        //For example
        public string BelongTo { get; set; } //”left” or “right”
    }

    public class AnalyzeResultsModel
    {
        public string AnalyticsEvent { get; set; }

        public List<FaceModel> Faces { get; set; }

        public string Category { get; set; } //”success”, “nofaces”, “faceselect”
        
    }

    public class FaceAttributes
    {
        [JsonProperty("gender")]
        public string Gender { get; set; }


        [JsonProperty("age")]
        public double Age { get; set; }
    }

    public class Rectangle
    {
        [JsonProperty("top")]
        public int Top { get; set; }

        [JsonProperty("left")]
        public int Left { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}