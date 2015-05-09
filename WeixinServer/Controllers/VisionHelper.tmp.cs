/********************************************************
*                                                        *
*   Copyright (c) Microsoft. All rights reserved.        *
*                                                        *
*********************************************************/

namespace WeixinServer.Controllers
{
    using Microsoft.ProjectOxford.Vision;
    using Microsoft.ProjectOxford.Vision.Contract;
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Text;
    using ImageProcessor;
    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Imaging;
    using ImageProcessor.Imaging.Formats;
    using ImageProcessor.Processors;
    using System.Net;
    using System.Threading.Tasks;
    /// <summary>
    /// The class is used to access vision APIs.
    /// </summary>
    public class VisionHelper
    {
        /// <summary>
        /// The vision service client.
        /// </summary>
        private readonly IVisionServiceClient visionClient;
        private Dictionary<string, string> cateMap = null;
        private string frameImageUri;
        private string originalImageUrl;
        private byte[] photoBytes;
        private ImageLayer GetFrameImageLayer(FaceRectangle detect)
        {
            var resizeLayer = new ResizeLayer(size: new Size(detect.Width, detect.Height), resizeMode: ResizeMode.Stretch);
            var frameSteam = new MemoryStream();
            var frameFactory = new ImageFactory(preserveExifData: false);
            
            //var frameRequest = (HttpWebRequest)WebRequest.Create(this.frameImageUri);
            //var frameResponse = (HttpWebResponse)frameRequest.GetResponse();
            //var stream = frameResponse.GetResponseStream();
            frameFactory.Load(this.frameImageUri).Resize(resizeLayer).Save(frameSteam);

            var frameImage = Image.FromStream(frameSteam);
            return new ImageLayer
            {
                Image = frameImage,
                Position = new Point(detect.Left, detect.Top),
            };
        }

        static TextLayer GetTextLayer(string text, int width, int height)
        {
            var x = (int)(width * 0.05);
            var y = (int)(height * 0.85);

            var fontSize = width < 1000 ? 24 : 36;

            return new TextLayer
            {
                DropShadow = true,
                FontColor = System.Drawing.Color.DeepPink,
                FontSize = fontSize,
                FontFamily = new FontFamily(GenericFontFamilies.SansSerif),
                Text = text,
                Style = FontStyle.Bold,
                Position = new Point(x, y),
                Opacity = 85,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisionHelper"/> class.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        public VisionHelper(string subscriptionKey, string frameImageUri)
        {
            this.visionClient = new VisionServiceClient(subscriptionKey);
            this.frameImageUri = frameImageUri;

            //this.cateMap = new Dictionary<string, string>();
            //var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            //    "86.cate.scp.split.ts.scp");
            //using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(@"~\App_Data\86.cate.scp.split.ts.scp")))
            //{
            //    var line = reader.ReadLine();
            //    string[] sp = line.Split('\t');
            //    this.cateMap.Add(sp[0], sp[1]);
            //}
            this.cateMap = new Dictionary<string, string>() {
                { "abstract_", "抽象" },
                { "abstract_net", "带有网格的抽象" },
                { "abstract_nonphoto", "不是照片的抽象东东" },
                { "abstract_rect", "矩形的抽象" },
                { "abstract_shape", "有形的抽象" },
                { "abstract_texture", "带有纹理的抽象" },
                { "animal_", "动物" },
                { "animal_bird", "鸟" },
                { "animal_cat", "猫" },
                { "animal_dog", "狗" },
                { "animal_horse", "马" },
                { "animal_panda", "熊猫" },
                { "building_", "建筑" },
                { "building_arch", "拱" },
                { "building_brickwall", "砖墙" },
                { "building_church", "教堂" },
                { "building_corner", "墙角" },
                { "building_doorwindows", "门窗" },
                { "building_pillar", "柱子" },
                { "building_stair", "楼梯" },
                { "building_street", "街道" },
                { "dark_", "黑暗" },
                { "drink_", "饮料" },
                { "drink_can", "罐装饮料" },
                { "dark_fire", "火" },
                { "dark_fireworks", "烟花" },
                { "sky_object", "天空" },
                { "food_", "食品" },
                { "food_bread", "面包" },
                { "food_fastfood", "快餐" },
                { "food_grilled", "烤肉" },
                { "food_pizza", "比萨饼" },
                { "indoor_", "室内" },
                { "indoor_churchwindow", "教堂窗户" },
                { "indoor_court", "球场" },
                { "indoor_doorwindows", "室内门窗" },
                { "indoor_marketstore", "市场店" },
                { "indoor_room", "房间" },
                { "indoor_venue", "体育场" },
                { "dark_light", "光" },
                { "others_", "其他" },
                { "outdoor_", "户外" },
                { "outdoor_city", "城市" },
                { "outdoor_field", "农田" },
                { "outdoor_grass", "草坪" },
                { "outdoor_house", "房子" },
                { "outdoor_mountain", "山" },
                { "outdoor_oceanbeach", "海滩" },
                { "outdoor_playground", "操场" },
                { "outdoor_railway", "铁路" },
                { "outdoor_road", "马路" },
                { "outdoor_sportsfield", "运动场" },
                { "outdoor_stonerock", "岩石" },
                { "outdoor_street", "街道" },
                { "outdoor_water", "水" },
                { "outdoor_waterside", "湖滨" },
                { "people_", "人" },
                { "people_baby", "宝宝" },
                { "people_crowd", "群众" },
                { "people_group", "人群" },
                { "people_hand", "手" },
                { "people_many", "许多人" },
                { "people_portrait", "肖像" },
                { "people_show", "达人秀" },
                { "people_tattoo", "纹身" },
                { "people_young", "年轻人" },
                { "plant_", "植物" },
                { "plant_branch", "树枝" },
                { "plant_flower", "花" },
                { "plant_leaves", "叶子" },
                { "plant_tree", "树" },
                { "object_screen", "屏幕" },
                { "object_sculpture", "雕塑" },
                { "sky_cloud", "云" },
                { "sky_sun", "太阳" },
                { "people_swimming", "游泳者" },
                { "outdoor_pool", "游泳池" },
                { "text_", "文本" },
                { "text_mag", "杂志" },
                { "text_map", "地图" },
                { "text_menu", "菜单" },
                { "text_sign", "符号" },
                { "trans_bicycle", "自行车" },
                { "trans_bus", "公交" },
                { "trans_car", "汽车" },
                { "trans_trainstation", "交通工具" },
		    };


        }

        private void DownloadDataCompleted(object sender,
            DownloadDataCompletedEventArgs e)
        {
            photoBytes  = e.Result;
            //Console.WriteLine(photoBytes.Length + " bytes received");
        }

        /// <summary>
        /// Analyze the given image.
        /// </summary>
        /// <param name="imagePathOrUrl">The image path or url.</param>
        public async Task<string> AnalyzeImage(string imagePathOrUrl)
        {
            this.originalImageUrl = imagePathOrUrl;
            this.ShowInfo("Analyzing");
            AnalysisResult analysisResult = null;

            string resultStr = string.Empty;
            try
            {
                if (File.Exists(imagePathOrUrl))
                {
                    using (FileStream stream = File.Open(imagePathOrUrl, FileMode.Open))
                    {
                        analysisResult = this.visionClient.AnalyzeImageAsync(stream).Result;
                    }
                }
                else if (Uri.IsWellFormedUriString(imagePathOrUrl, UriKind.Absolute))
                {
                    //var visualFeatures = new string[]{"faceId", "age", "gender", "faceRectangle", "faceLandmarks", "attributes"};
                    //analysisResult = this.visionClient.AnalyzeImageAsync(imagePathOrUrl, visualFeatures).Result;

                    //Task.Run(async () =>
                    //{
                        var ret = this.visionClient.AnalyzeImageAsync(imagePathOrUrl);
                        analysisResult = await ret;
                    //}).Wait();
                        WebClient client = new WebClient();
                        client.DownloadDataCompleted += DownloadDataCompleted;
                        client.DownloadDataAsync(new Uri(imagePathOrUrl));

                    
                }
                else
                {
                    this.ShowError("Invalid image path or Url");
                    return "Invalid image path or Url" + imagePathOrUrl;
                }
            }
            catch (ClientException e)
            {
                if (e.Error != null)
                {
                    this.ShowError(e.Error.Message);
                    return string.Format("ClientException e.Error.Message:{0}", e.Error.Message);
                }
                else
                {
                    this.ShowError(e.Message);
                    return string.Format("ClientException e.Error.Message:{0}", e.Message);
                }

                
            }
            catch (Exception e)
            {
                this.ShowError("Some error happened.");

                return string.Format("Exception :{0}", e.Message);

            }
            //return this.ShowRichAnalysisResult(analysisResult);
            var resTxt = this.ShowRichAnalysisResult(analysisResult);
            if (string.IsNullOrEmpty(resTxt)) return resTxt;

            var resImg = this.RenderAnalysisResultAsImage(analysisResult, resTxt);
            if (string.IsNullOrEmpty(resImg)) return resTxt;
            return resImg;
        }

        /// <summary>
        /// Recognize text from given image.
        /// </summary>
        /// <param name="imagePathOrUrl">The image path or url.</param>
        /// <param name="detectOrientation">if set to <c>true</c> [detect orientation].</param>
        /// <param name="languageCode">The language code.</param>
        public void RecognizeText(string imagePathOrUrl, bool detectOrientation = true, string languageCode = LanguageCodes.AutoDetect)
        {
            this.ShowInfo("Recognizing");
            OcrResults ocrResult = null;
            string resultStr = string.Empty;

            try
            {
                if (File.Exists(imagePathOrUrl))
                {
                    using (FileStream stream = File.Open(imagePathOrUrl, FileMode.Open))
                    {
                        ocrResult = this.visionClient.RecognizeTextAsync(stream, languageCode, detectOrientation).Result;
                    }
                }
                else if (Uri.IsWellFormedUriString(imagePathOrUrl, UriKind.Absolute))
                {
                    ocrResult = this.visionClient.RecognizeTextAsync(imagePathOrUrl, languageCode, detectOrientation).Result;
                }
                else
                {
                    this.ShowError("Invalid image path or Url");
                }
            }
            catch (ClientException e)
            {
                if (e.Error != null)
                {
                    this.ShowError(e.Error.Message);
                }
                else
                {
                    this.ShowError(e.Message);
                }

                return;
            }
            catch (Exception)
            {
                this.ShowError("Some error happened.");
                return;
            }

            this.ShowRetrieveText(ocrResult);
        }

        /// <summary>
        /// Gets thumbnail for given image.
        /// </summary>
        /// <param name="imagePathOrUrl">The image path or url.</param>
        /// <param name="width">Width of the thumbnail. It must be between 1 and 1024.</param>
        /// <param name="height">Height of the thumbnail. It must be between 1 and 1024.</param>
        /// <param name="smartCropping">Whether enable smart cropping.</param>
        /// <param name="resultFolder">result Folder.</param>
        public void GetThumbnail(string imagePathOrUrl, int width, int height, bool smartCropping, string resultFolder)
        {
            this.ShowInfo("Get Thumbnail");
            byte[] thumbnailResult = null;
            string resultStr = string.Empty;

            try
            {
                if (File.Exists(imagePathOrUrl))
                {
                    using (FileStream stream = File.Open(imagePathOrUrl, FileMode.Open))
                    {
                        thumbnailResult = this.visionClient.GetThumbnailAsync(stream, width, height, smartCropping).Result;
                    }
                }
                else if (Uri.IsWellFormedUriString(imagePathOrUrl, UriKind.Absolute))
                {
                    thumbnailResult = this.visionClient.GetThumbnailAsync(imagePathOrUrl, width, height, smartCropping).Result;
                }
                else
                {
                    this.ShowError("Invalid image path or Url");
                }
            }
            catch (ClientException e)
            {
                if (e.Error != null)
                {
                    this.ShowError(e.Error.Message);
                }
                else
                {
                    this.ShowError(e.Message);
                }

                return;
            }
            catch (Exception)
            {
                this.ShowError("Some error happened.");
                return;
            }

            // Write the result to local file
            string filePath = string.Format("{0}\\thumbnailResult_{1}.jpg", resultFolder, DateTime.UtcNow.Ticks.ToString());

            using (BinaryWriter binaryWrite = new BinaryWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write)))
            {
                binaryWrite.Write(thumbnailResult);
            }

            this.ShowResult(string.Format("The result file has been saved to {0}", Path.GetFullPath(filePath)));
        }

        /// <summary>
        /// Retrieve text from the given OCR results object.
        /// </summary>
        /// <param name="results">The OCR results.</param>
        /// <returns>Return the text.</returns>
        private void ShowRetrieveText(OcrResults results)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (results != null && results.Regions != null)
            {
                stringBuilder.Append("Text: ");
                stringBuilder.AppendLine();
                foreach (var item in results.Regions)
                {
                    foreach (var line in item.Lines)
                    {
                        foreach (var word in line.Words)
                        {
                            stringBuilder.Append(word.Text);
                            stringBuilder.Append(" ");
                        }

                        stringBuilder.AppendLine();
                    }

                    stringBuilder.AppendLine();
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(stringBuilder.ToString());
            Console.ResetColor();
        }

        /// <summary>
        /// Show the working item.
        /// </summary>
        /// <param name="workStr">The work item's string.</param>
        private void ShowInfo(string workStr)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(string.Format("{0}......", workStr));
            Console.ResetColor();
        }

        /// <summary>
        /// Show error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private void ShowError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
        }

        /// <summary>
        /// Show result message.
        /// </summary>
        /// <param name="resultMessage">The result message.</param>
        private void ShowResult(string resultMessage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(resultMessage);
            Console.ResetColor();
        }

        /// <summary>
        /// Show Analysis Result
        /// </summary>
        /// <param name="result">Analysis Result</param>
        private void ShowAnalysisResult(AnalysisResult result)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (result == null)
            {
                Console.WriteLine("null");
                return;
            }

            if (result.Metadata != null)
            {
                Console.WriteLine("Image Format : " + result.Metadata.Format);
                Console.WriteLine("Image Dimensions : " + result.Metadata.Width + " x " + result.Metadata.Height);
            }

            if (result.ImageType != null)
            {
                string clipArtType;
                switch (result.ImageType.ClipArtType)
                {
                    case 0:
                        clipArtType = "0 Non-clipart";
                        break;
                    case 1:
                        clipArtType = "1 ambiguous";
                        break;
                    case 2:
                        clipArtType = "2 normal-clipart";
                        break;
                    case 3:
                        clipArtType = "3 good-clipart";
                        break;
                    default:
                        clipArtType = "Unknown";
                        break;
                }
                Console.WriteLine("Clip Art Type : " + clipArtType);

                string lineDrawingType;
                switch (result.ImageType.LineDrawingType)
                {
                    case 0:
                        lineDrawingType = "0 Non-LineDrawing";
                        break;
                    case 1:
                        lineDrawingType = "1 LineDrawing";
                        break;
                    default:
                        lineDrawingType = "Unknown";
                        break;
                }
                Console.WriteLine("Line Drawing Type : " + lineDrawingType);
            }


            if (result.Adult != null)
            {
                Console.WriteLine("Is Adult Content : " + result.Adult.IsAdultContent);
                Console.WriteLine("Adult Score : " + result.Adult.AdultScore);
                Console.WriteLine("Is Racy Content : " + result.Adult.IsRacyContent);
                Console.WriteLine("Racy Score : " + result.Adult.RacyScore);
            }

            if (result.Categories != null && result.Categories.Length > 0)
            {
                Console.WriteLine("Categories : ");
                foreach (var category in result.Categories)
                {
                    Console.Write("   Name : " + category.Name);
                    Console.WriteLine("; Score : " + category.Score);
                }
            }

            if (result.Faces != null && result.Faces.Length > 0)
            {
                Console.WriteLine("Faces : ");
                foreach (var face in result.Faces)
                {
                    Console.Write("   Age : " + face.Age);
                    Console.Write("; Gender : " + face.Gender);
                }
            }

            if (result.Color != null)
            {
                Console.WriteLine("AccentColor : " + result.Color.AccentColor);
                Console.WriteLine("Dominant Color Background : " + result.Color.DominantColorBackground);
                Console.WriteLine("Dominant Color Foreground : " + result.Color.DominantColorForeground);

                if (result.Color.DominantColors != null && result.Color.DominantColors.Length > 0)
                {
                    Console.Write("Dominant Colors : ");
                    foreach (var color in result.Color.DominantColors)
                    {
                        Console.Write(color + " ");
                    }
                }
            }

            Console.ResetColor();
        }


        private string ShowRichAnalysisResult(AnalysisResult result)
        {
            //Console.ForegroundColor = ConsoleColor.Green;
            string res = "result";
            string des = "这幅图片";
            StringWriter desStringWriter = new StringWriter();
            Dictionary<string, string> map = new Dictionary<string, string>();
            if (result == null)
            {
                res = "NULL";
                des += "看不出任何东西";
                desStringWriter.Write("看不出任何东西");
                return des;
            }

            if (result.Metadata != null)
            {
                res += "Image Format : " + result.Metadata.Format;
                res += "Image Dimensions : " + result.Metadata.Width + " x " + result.Metadata.Height;
                //des += string.Format("格式是：{0}\n", result.Metadata.Format);
                //des += string.Format("分辨率是：{0}X{1}\n", result.Metadata.Width, result.Metadata.Height);
            }

            if (result.ImageType != null)
            {
                string clipArtType;
                switch (result.ImageType.ClipArtType)
                {
                    case 0:
                        clipArtType = "0 Non-clipart";
                        break;
                    case 1:
                        clipArtType = "1 ambiguous";
                        break;
                    case 2:
                        clipArtType = "2 normal-clipart";
                        break;
                    case 3:
                        clipArtType = "3 good-clipart";
                        break;
                    default:
                        clipArtType = "Unknown";
                        break;
                }
                res += string.Format("Clip Art Type : {0}", clipArtType);
                string lineDrawingType;
                switch (result.ImageType.LineDrawingType)
                {
                    case 0:
                        lineDrawingType = "0 Non-LineDrawing";
                        break;
                    case 1:
                        lineDrawingType = "1 LineDrawing";
                        break;
                    default:
                        lineDrawingType = "Unknown";
                        break;
                }
                res += "Line Drawing Type : " + lineDrawingType;
            }

            double ascr = 0.0f, rscr = 0.0f;
            if (result.Adult != null)
            {
                res += "Is Adult Content : " + result.Adult.IsAdultContent;
                map.Add("isadult", result.Adult.IsAdultContent.ToString());
                res += "Adult Score : " + result.Adult.AdultScore;
                map.Add("adultscore", result.Adult.AdultScore.ToString());
                res += "Is Racy Content : " + result.Adult.IsRacyContent;
                map.Add("isRacy", result.Adult.IsRacyContent.ToString());
                res += "Racy Score : " + result.Adult.RacyScore;
                map.Add("RacyScore", result.Adult.RacyScore.ToString());

                ascr = result.Adult.AdultScore * 150.0;
                rscr = result.Adult.RacyScore * 100.0;
            }
            desStringWriter.Write(string.Format("活泼内容比例为{0:F2}%\n", rscr));//TODO 少量 or More by Score
            if (result.Categories != null && result.Categories.Length > 0)
            {
                //res += "Categories : ";
                desStringWriter.Write(string.Format("画面里有"));
                //var sb = new StringBuilder();
                string sb = "";
                
                foreach (var category in result.Categories)
                {
                    //res += "   Name : " + category.Name;
                    //res += "; Score : " + category.Score;
                    //if (cateMap.ContainsKey(category.Name) && ! category.Name.EndsWith("_"))
                    if (cateMap.ContainsKey(category.Name))
                        sb += string.Format("{0}、", cateMap[category.Name]);
                    
                }

                //if (result.Categories.Length == 1 || sb.Length < 2)
                //{
                //    sb += string.Format("{0}", cateMap[result.Categories[0].Name]);
                //}
                desStringWriter.Write(string.Format("{0}", sb.TrimEnd('、')));
                if (result.Categories.Length > 1 && sb.Length > 1)
                    desStringWriter.Write(string.Format("等内容"));
            }

            if (result.Faces != null && result.Faces.Length > 0)
            {
                res += "Faces : ";
                int numFemale = 0, numMale = 0;
                float avgAge = 0.0f, mAvgAge = 0.01f, fAvgAge = 0.01f;
                foreach (var face in result.Faces)
                {
                    res += "   Age : " + face.Age;
                    avgAge += face.Age;
                    res += " Gender : " + face.Gender;
                    if (face.Gender.ToLower().Equals("male"))
                    {
                        ++numMale;
                        mAvgAge += face.Age;
                    }
                    else
                    {
                        ++numFemale;
                        fAvgAge += face.Age;
                    }
                }

                desStringWriter.Write(string.Format(", 火爆指数为{0:F0}。\n", ascr));//TODO 少量 or More by Score

                //里面的男人很幸福
                //一群男or女屌丝
                if (numFemale > numMale && numMale > 0) desStringWriter.Write(string.Format("这{0}个男人很幸福:)", numMale));
                else if (numFemale < numMale && numFemale > 0) desStringWriter.Write(string.Format("这{0}个女人很幸福:)", numFemale));
                else if (numFemale == 0) desStringWriter.Write(string.Format("{0}个孤独的男屌丝……", numMale));
                else if (numMale == 0) desStringWriter.Write(string.Format("{0}个寂寞的女屌丝……", numFemale));
                else 
                {
                    //desStringWriter.Write(string.Format("里面有{0}男{1}女,", numMale, numFemale));//TODO 少量 or More by Score
                    //desStringWriter.Write(string.Format("平均年龄{0:F0}岁", avgAge / (numMale + numFemale)));//TODO 少量 or More by Score
                    desStringWriter.Write(string.Format(",{0}位颜龄{1:F0}岁左右的男人,和{2}位颜龄{3:F0}岁左右的女人", numMale, mAvgAge / numMale, numFemale, fAvgAge / numFemale));//TODO 少量 or More by Score
                }
                //老驴啃嫩草
                float ratio = mAvgAge / fAvgAge;
                if (ratio > 1.2 && numFemale > 0) desStringWriter.Write(string.Format("= {0}头老驴啃{1}棵嫩草", numMale, numFemale));
                else if (ratio < 0.8 && numMale > 0) desStringWriter.Write(string.Format("= {0}棵老草啃{1}头嫩驴", numFemale, numMale));
                else { }
                
            }

            if (result.Color != null)
            {
                res += "AccentColor : " + result.Color.AccentColor;
                res += "Dominant Color Background : " + result.Color.DominantColorBackground;
                res += "Dominant Color Foreground : " + result.Color.DominantColorForeground;

                if (result.Color.DominantColors != null && result.Color.DominantColors.Length > 0)
                {
                    res += "Dominant Colors : ";
                    foreach (var color in result.Color.DominantColors)
                    {
                        res += "color ";
                    }
                }
            }

            //Console.ResetColor();
            return desStringWriter.ToString(); 
                //des;
        }
        private string RenderAnalysisResultAsImage(AnalysisResult result, string captionText)
        {
            var originalUrl = this.originalImageUrl;
            //var webClient = new WebClient();
            //var photoBytes = webClient.DownloadData(originalUrl);
            string resultUrl = null;

            var upyun = new UpYun("wxhowoldtest", "work01", "vYiJVc7iYY33w58O");

            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (var imageFactory = new ImageFactory(preserveExifData: false))
                    {
                        // Load
                        imageFactory.Load(inStream);

                        // Add frame
                        foreach (var detect in result.Faces)
                        {
                            imageFactory.Overlay(this.GetFrameImageLayer(detect.FaceRectangle));
                        }

                        // Save
                        imageFactory
                            .Watermark(GetTextLayer(captionText, result.Metadata.Width, result.Metadata.Height))
                            .Format(new JpegFormat())
                            .Save(outStream);
                    }

                    // Upload to image CDN
                    resultUrl = upyun.UploadImageStream(outStream);
                }
            }

            return string.Format("酷评:\n{0}\n归图:\n{1}", captionText, resultUrl);
        }
    }
}
