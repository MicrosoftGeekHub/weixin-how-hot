using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WeixinServer.Models;
namespace WeixinServer.Helpers
{
    public partial class VisionHelper
    {
        private DateTime startTime;
        private StringBuilder timeLogger = new StringBuilder();
        private StringBuilder errLogger = new StringBuilder();
        private string returnImageUrl = "";
        private string curUserName = "";
        private string noAdsTxtResult;
        private void InitializePropertiesForText(string subscriptionKey)
        {
            this.visionClient = new VisionServiceClient(subscriptionKey);

            this.categoryNameMapping = new Dictionary<string, string>() {
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
                { "food_", "食物" },
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
                { "indoor_venue", "室内场所" },
                { "dark_light", "光" },
                { "others_", "奇怪的东东" },
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
                { "people_show", "秀场" },
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

        private void DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            photoBytes = e.Result;
            //Console.WriteLine(photoBytes.Length + " bytes received");
        }


        /// <summary>
        /// Analyze the given image.
        /// </summary>
        /// <param name="imagePathOrUrl">The image path or url.</param>
        public async Task<RichResult> AnalyzeImage(string imagePathOrUrl, string curUserName)
        {
            this.curUserName = curUserName;
            return await AnalyzeImage(imagePathOrUrl);
        }

        private int minNumPixs = 50;
        private int maxNumPixs = 800;


        /// <summary>
        /// Analyze the given image.
        /// </summary>
        /// <param name="imagePathOrUrl">The image path or url.</param>
        public async Task<RichResult> AnalyzeImage(Stream imageStream)
        {
            timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage\n", DateTime.Now));
            
            this.ShowInfo("Analyzing");
            AnalysisResult analysisResult = null;
            //Task<Byte[]> taskb = null;
            Task<Microsoft.ProjectOxford.Face.Contract.Face[]> taskGetFaces = null;
            string resultStr = string.Empty;
            using (WebClient client = new WebClient())
            {
                try
                {
                        var visualFeatures = new string[] { "Categories", "Adult", "Color" };  //no ImageType , "Categories"
                        timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync url begin\n", DateTime.Now - this.startTime));

                        using (var ms = new MemoryStream())
                        {

                            imageStream.CopyTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            photoBytes = ms.ToArray();
                            ms.Seek(0, SeekOrigin.Begin);
                            timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync imageStream  Seek begin {1}\n", DateTime.Now - this.startTime, imageStream.ToString()));
                            Image image = Image.FromStream(ms);
                            timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync stream begin\n", DateTime.Now - this.startTime));
                            
                            MemoryStream ms4face = new MemoryStream();
                            image.Save(ms4face, image.RawFormat);
                            timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync stream begin\n", DateTime.Now - this.startTime));
                            if (image.Height > maxNumPixs)
                            {
                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync Resize begin\n", DateTime.Now - this.startTime));
                                int height = maxNumPixs;
                                int width = (int)((float)height * image.Width / image.Height);
                                if (width < minNumPixs)
                                {
                                    width = minNumPixs;
                                    height = (int)((float)width * image.Height / image.Width);
                                }
                                Image resizedImg = Resize(image, new Size(width, height));
                                var resizedMemoryStream = new MemoryStream();
                                var resizedMemoryStream2 = new MemoryStream();
                                resizedImg.Save(resizedMemoryStream, image.RawFormat);
                                resizedImg.Save(resizedMemoryStream2, image.RawFormat);

                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync Resize end\n", DateTime.Now - this.startTime));

                                faceAgent fa = new faceAgent();
                                resizedMemoryStream.Seek(0, SeekOrigin.Begin);
                                taskGetFaces = fa.UploadStreamAndDetectFaces(resizedMemoryStream);
                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage taskGetFaces begin\n", DateTime.Now - this.startTime));



                                resizedMemoryStream2.Seek(0, SeekOrigin.Begin);
                                var task = this.visionClient.AnalyzeImageAsync(resizedMemoryStream2, visualFeatures);
                                analysisResult = await task;
                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync stream end\n", DateTime.Now - this.startTime));
                            }
                            else
                            {
                                faceAgent fa = new faceAgent();
                                ms4face.Seek(0, SeekOrigin.Begin);
                                ms.Seek(0, SeekOrigin.Begin);
                                taskGetFaces = fa.UploadStreamAndDetectFaces(ms);
                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage taskGetFaces begin\n", DateTime.Now - this.startTime));


                                ms4face.Seek(0, SeekOrigin.Begin);
                                ms.Seek(0, SeekOrigin.Begin);
                                analysisResult = await this.visionClient.AnalyzeImageAsync(ms4face, visualFeatures); ;
                               

                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync stream end\n", DateTime.Now - this.startTime));
                            }
                            //analysisResult = await taskAnalyzeUrl;

                            // timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync url end\n", DateTime.Now - this.startTime));
                        }
                    
                
                //return this.ShowRichAnalysisResult(analysisResult);
                //Microsoft.ProjectOxford.Face.Contract.Face[] faces = await taskGetFaces;
                analysisResult.RichFaces = await taskGetFaces;
                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage taskGetFaces end {1}\n analysisResult.RichFaces.Length\n", DateTime.Now - this.startTime, analysisResult.RichFaces.Length));
                var resTxt = this.ShowRichAnalysisResult(analysisResult);
               // return new RichResult(timeLogger.ToString(), resTxt.Item2, errLogger.ToString(), "http://geeekstore.blob.core.windows.net/cdn/weiTestCase/book1.jpg");

                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage ShowRichAnalysisResult end\n", DateTime.Now - this.startTime));
                var txtRichResult = new RichResult(timeLogger.ToString(), resTxt.Item2, errLogger.ToString());
                if (string.IsNullOrEmpty(resTxt.Item1)) return txtRichResult;
                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage txtRichResult end\n", DateTime.Now - this.startTime));
                //photoBytes = await taskb;
                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage client.DownloadDataTaskAsync end\n", DateTime.Now - this.startTime));

                var resImg = this.RenderAnalysisResultAsImage(analysisResult, resTxt.Item1, resTxt.Item2);
                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage\t RenderAnalysisResultAsImage end\n", DateTime.Now - this.startTime));
                if (string.IsNullOrEmpty(resImg))
                {
                    //return new RichResult(timeLogger.ToString(), resTxt, errLogger.ToString(), this.returnImageUrl, photoBytes);
                    return new RichResult(timeLogger.ToString(), resTxt.Item2, errLogger.ToString(), this.returnImageUrl);
                }

                return new RichResult(timeLogger.ToString(), resImg, errLogger.ToString(), this.returnImageUrl);
                }
                catch (Microsoft.ProjectOxford.Vision.ClientException e)
                {
                    if (e.Error != null)
                    {
                        var errMsg = string.Format("ClientException e.Error.Message:{0}", e.Error.Message);
                        errLogger.Append(errMsg);
                        return new RichResult(timeLogger.ToString(), errMsg, errLogger.ToString());
                    }
                    else
                    {
                        var errMsg = string.Format("ClientException e.Message:{0}", e.Message);
                        errLogger.Append(errMsg);
                        return new RichResult(timeLogger.ToString(), errMsg, errLogger.ToString());
                    }


                }
                catch (Exception e)
                {
                    timeLogger.Append(string.Format("Exception Time: {0}\n ", DateTime.Now));
                    var errMsg = string.Format("Exception e.Message:{0}", e.Message);
                    errLogger.Append(errMsg);
                    return new RichResult(timeLogger.ToString(), errMsg, errLogger.ToString());

                }
                finally
                {
                }
            }
        }

        
     /// <summary>
        /// Analyze the given image.
        /// </summary>
        /// <param name="imagePathOrUrl">The image path or url.</param>
        
        public async Task<RichResult> AnalyzeImage(string imagePathOrUrl)
        {
            timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage {1}\n", DateTime.Now, imagePathOrUrl));
            this.originalImageUrl = imagePathOrUrl;
            this.ShowInfo("Analyzing");
            AnalysisResult analysisResult = null;
            Task<Byte[]> taskb = null;
            Task<AnalysisResult> taskAnalyzeImageAsync = null;
            Task<Microsoft.ProjectOxford.Face.Contract.Face[]> taskGetFaces = null;
            string resultStr = string.Empty;
            using (WebClient client = new WebClient())
            {
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

                        var visualFeatures = new string[] { "Categories", "Adult", "Color" };  //no ImageType , "Categories"
                        client.DownloadDataCompleted += DownloadDataCompleted;
                        taskb = client.DownloadDataTaskAsync(new Uri(imagePathOrUrl));
                        //timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage client.DownloadDataTaskAsync begin\n url: {1}\n", DateTime.Now - this.startTime, imagePathOrUrl));
                        timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync url begin\n", DateTime.Now - this.startTime));
                        var request = System.Net.WebRequest.Create(new Uri(imagePathOrUrl));
                        request.Timeout = int.MaxValue;
                        var response = request.GetResponse();
                        var streamToUpload = response.GetResponseStream();

                        //call FaceApi

                        //string testImg = @"C:\Users\yimlin\Pictures\supgk\91girl.jpg";
                        // Do any async anything you need here without worry




                        //var taskAnalyzeUrl = this.visionClient.AnalyzeImageAsync(imagePathOrUrl, visualFeatures);

                        

                        using (var ms = new MemoryStream())
                        {
                            streamToUpload.CopyTo(ms);
                            Image image = Image.FromStream(ms);
                            MemoryStream ms4face = new MemoryStream();
                            image.Save(ms4face, image.RawFormat);
                            timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync stream begin\n", DateTime.Now - this.startTime));
                            if (image.Height > maxNumPixs)
                            {
                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync Resize begin\n", DateTime.Now - this.startTime));
                                int height = maxNumPixs;
                                int width = (int)((float)height * image.Width / image.Height);
                                if (width < minNumPixs)
                                {
                                    width = minNumPixs;
                                    height = (int)((float)width * image.Height / image.Width);
                                }
                                Image resizedImg = Resize(image, new Size(width, height));
                                var resizedMemoryStream = new MemoryStream();
                                var resizedMemoryStream2 = new MemoryStream();
                                resizedImg.Save(resizedMemoryStream, image.RawFormat);
                                resizedImg.Save(resizedMemoryStream2, image.RawFormat);
                                
                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync Resize end\n", DateTime.Now - this.startTime));
                           //     resizedMemoryStream.Seek(0, SeekOrigin.Begin);
                          //      photoBytes = resizedMemoryStream.ToArray();
                          //      timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync read Bytes end\n", DateTime.Now - this.startTime));
                                
                                faceAgent fa = new faceAgent();
                                resizedMemoryStream.Seek(0, SeekOrigin.Begin);
                                taskGetFaces = fa.UploadStreamAndDetectFaces(resizedMemoryStream);
                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage taskGetFaces begin\n", DateTime.Now - this.startTime));


                                resizedMemoryStream2.Seek(0, SeekOrigin.Begin);
                                var task = this.visionClient.AnalyzeImageAsync(resizedMemoryStream2, visualFeatures);
                                analysisResult = await task;
                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync stream end\n", DateTime.Now - this.startTime));
                            }
                            else
                            {
                                faceAgent fa = new faceAgent();
                                ms4face.Seek(0, SeekOrigin.Begin);
                                ms.Seek(0, SeekOrigin.Begin);
                                taskGetFaces = fa.UploadStreamAndDetectFaces(ms);
                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage taskGetFaces begin\n", DateTime.Now - this.startTime));
                                
                                
                                ms4face.Seek(0, SeekOrigin.Begin);
                                ms.Seek(0, SeekOrigin.Begin);
                                analysisResult = await this.visionClient.AnalyzeImageAsync(ms4face, visualFeatures); ;
                                ms.Seek(0, SeekOrigin.Begin);
                              //  photoBytes = ms.ToArray();

                                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync stream end\n", DateTime.Now - this.startTime));
                            }
                            //analysisResult = await taskAnalyzeUrl;

                           // timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage AnalyzeImageAsync url end\n", DateTime.Now - this.startTime));
                        }



                        //response.Close();


                        //}).Wait();

                    }
                    else
                    {
                        var errMsg = string.Format("Invalid image path or Url:{0}\n" + imagePathOrUrl);
                        errLogger.Append(errMsg);
                        return new RichResult(timeLogger.ToString(), errMsg, errLogger.ToString());
                    }
                }
                catch (Microsoft.ProjectOxford.Vision.ClientException e)
                {
                    if (e.Error != null)
                    {
                        var errMsg = string.Format("ClientException e.Error.Message:{0}", e.Error.Message);
                        errLogger.Append(errMsg);
                        return new RichResult(timeLogger.ToString(), errMsg, errLogger.ToString());
                    }
                    else
                    {
                        var errMsg = string.Format("ClientException e.Message:{0}", e.Message);
                        errLogger.Append(errMsg);
                        return new RichResult(timeLogger.ToString(), errMsg, errLogger.ToString());
                    }


                }
                catch (Exception e)
                {
                    timeLogger.Append(string.Format("Exception Time: {0}\n ", DateTime.Now));
                    var errMsg = string.Format("Exception e.Message:{0}", e.Message);
                    errLogger.Append(errMsg);
                    return new RichResult(timeLogger.ToString(), errMsg, errLogger.ToString());

                }
                finally
                {
                }
                //return this.ShowRichAnalysisResult(analysisResult);
                //Microsoft.ProjectOxford.Face.Contract.Face[] faces = await taskGetFaces;
                analysisResult.RichFaces = await taskGetFaces;
                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage taskGetFaces end {1}\n analysisResult.RichFaces.Length\n", DateTime.Now - this.startTime, analysisResult.RichFaces.Length));
                var resTxt = this.ShowRichAnalysisResult(analysisResult);
                var txtRichResult = new RichResult(timeLogger.ToString(), resTxt.Item2, errLogger.ToString());
                if (string.IsNullOrEmpty(resTxt.Item1)) return txtRichResult;

                photoBytes = await taskb;
                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage client.DownloadDataTaskAsync end\n", DateTime.Now - this.startTime));

                var resImg = this.RenderAnalysisResultAsImage(analysisResult, resTxt.Item1, resTxt.Item2);
                timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage\t RenderAnalysisResultAsImage end\n", DateTime.Now - this.startTime));
                if (string.IsNullOrEmpty(resImg))
                {
                    //return new RichResult(timeLogger.ToString(), resTxt, errLogger.ToString(), this.returnImageUrl, photoBytes);
                    return new RichResult(timeLogger.ToString(), resTxt.Item2, errLogger.ToString(), this.returnImageUrl);
                }

                return new RichResult(timeLogger.ToString(), resImg, errLogger.ToString(), this.returnImageUrl);
            }
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
            catch (Microsoft.ProjectOxford.Vision.ClientException e)
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
            catch (Microsoft.ProjectOxford.Vision.ClientException e)
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


        private Tuple<string, string> ShowRichAnalysisResult(AnalysisResult result)
        {
            timeLogger.Append(string.Format("{0} VisionHelper::ShowRichAnalysisResult begin\n", DateTime.Now - this.startTime));
            //Console.ForegroundColor = ConsoleColor.Green;
            string res = "result";
            string des = "这幅图片";
            StringWriter desStringWriter = new StringWriter();
            StringWriter commentStringWriter = new StringWriter();
            //Dictionary<string, string> map = new Dictionary<string, string>();
            if (result == null)
            {
                //res = "NULL";
                //des += "看不出任何东西";
                commentStringWriter.Write("看不出任何东西");
                return new Tuple<string, string>(desStringWriter.ToString(), commentStringWriter.ToString());
            }

            if (result.Metadata != null)
            {
                //res += "Image Format : " + result.Metadata.Format;
                //res += "Image Dimensions : " + result.Metadata.Width + " x " + result.Metadata.Height;
                ////des += string.Format("格式是：{0}\n", result.Metadata.Format);
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

            double ascr = 0.0f, rscr = 0.0f, saoBility = 0.0f;
            if (result.Adult != null)
            {
                //res += "Is Adult Content : " + result.Adult.IsAdultContent;
                //map.Add("isadult", result.Adult.IsAdultContent.ToString());
                //res += "Adult Score : " + result.Adult.AdultScore;
                //map.Add("adultscore", result.Adult.AdultScore.ToString());
                //res += "Is Racy Content : " + result.Adult.IsRacyContent;
                //map.Add("isRacy", result.Adult.IsRacyContent.ToString());
                //res += "Racy Score : " + result.Adult.RacyScore;
                //map.Add("RacyScore", result.Adult.RacyScore.ToString());

                ascr = (int)(result.Adult.AdultScore * 2500);
                rscr = (int)(result.Adult.RacyScore * 5000);
                saoBility = ascr + rscr;
            }
            if (result.Adult.IsAdultContent)
            {
                desStringWriter.Write("手哥：黄图, 滚粗~！\n");
                return new Tuple<string, string>(desStringWriter.ToString(), commentStringWriter.ToString());
            }
            desStringWriter.Write(string.Format("辣火比：{0:F2}%\n", saoBility / 10.0));//TODO 少量 or More by Score
           // desStringWriter.Write(string.Format("手哥评分: {0:F0}\n", rscr));//TODO 少量 or More by Score
            var cat2StoryMap = MvcApplication.GetCateMap();
            var storyList = new List<Tuple<string, string>>();
            //desStringWriter.Write(string.Format(": {0:F2}%\n", ascr));//TODO 少量 or More by Score
            if (result.Categories != null && result.Categories.Length > 0)
            {
                //res += "Categories : ";
                desStringWriter.Write(string.Format("画面里有"));
                //var sb = new StringBuilder();
                string preFix = "";
                string postFix = "";
                //MvcApplication.InitCateMap();
                
                
                foreach (var category in result.Categories)
                {
                    //res += "   Name : " + category.Name;
                    //res += "; Score : " + category.Score;
                    if (cat2StoryMap.ContainsKey(category.Name))
                    {
                        storyList.AddRange(cat2StoryMap[category.Name]);
                    }
                    //if (categoryNameMapping.ContainsKey(category.Name) && ! category.Name.EndsWith("_"))
                    if (category.Name.Equals("others_"))
                    {
                        postFix = string.Format("{0}", categoryNameMapping[category.Name]);
                        continue;
                    }
                    if (categoryNameMapping.ContainsKey(category.Name))
                        preFix += string.Format("{0}、", categoryNameMapping[category.Name]);
                }

                
               
                if (result.Categories.Length == 1)
                {
                    desStringWriter.Write(string.Format("{0}{1}", preFix.TrimEnd('、'), postFix));
                }
                else if (preFix.Length > 0 && postFix.Length > 0)
                {
                    desStringWriter.Write(string.Format("{0}和{1}", preFix.TrimEnd('、'), postFix));
                }
                else
                {
                    desStringWriter.Write(string.Format("{0}{1}", preFix.TrimEnd('、'), postFix));
                }

                if (result.Categories.Length > 1 && preFix.Length > 1)
                    desStringWriter.Write(string.Format("等内容"));
                desStringWriter.Write(string.Format("。\n"));
            }

            if (result.RichFaces != null && result.RichFaces.Length > 0)
            {
               // var shenPrice = (result.Adult.AdultScore + 2 * result.Adult.RacyScore) * result.RichFaces.Length * 2500;
               // desStringWriter.Write(string.Format("集体肾价：${0:F0}万，打八折只要998！\n", shenPrice));//TODO 少量 or More by Score
                res += "Faces : ";
                int numFemale = 0, numMale = 0;
                float avgAge = 0.0f, mAvgAge = 0.01f, fAvgAge = 0.01f;
                var maleAgeMap = new Dictionary<string, int>();
                var femaleAgeMap = new Dictionary<string, int>();
                foreach (var face in result.RichFaces)
                {
                    res += "   Age : " + face.Attributes.Age;
                    avgAge += (float)face.Attributes.Age;
                    res += " Gender : " + face.Attributes.Gender;
                    if (face.Attributes.Gender.ToLower().Equals("male"))
                    {
                        ++numMale;
                        mAvgAge += (float)face.Attributes.Age;
                        var title = MaleTitleAsPerAge(face.Attributes.Age);
                        if (maleAgeMap.ContainsKey(title))
                            maleAgeMap[title] += 1;
                        else maleAgeMap.Add(title, 1);
                    }
                    else
                    {
                        ++numFemale;
                        fAvgAge += (float)face.Attributes.Age;
                        var title = FemaleTitleAsPerAge(face.Attributes.Age);
                        if (femaleAgeMap.ContainsKey(title))
                            femaleAgeMap[title] += 1;
                        else femaleAgeMap.Add(title, 1);
                    }

                    //read FaceLandmarks

                    //big eyes
                    //face.FaceLandmarks.
                }





                //这个大叔很幸福
                if (numFemale >= numMale && numMale > 0)
                {
                    commentStringWriter.Write(string.Format("画说，这"));
                    foreach (var key in maleAgeMap.Keys)
                    {
                        commentStringWriter.Write(string.Format("{0}个{1}，",
                            NumberToChineseChar(maleAgeMap[key]), key));
                    }
                    commentStringWriter.Write(string.Format("看起来很幸福 :) ，因为身边有"));
                    foreach (var key in femaleAgeMap.Keys)
                    {
                        commentStringWriter.Write(string.Format("{0}个{1}，",
                            NumberToChineseChar(femaleAgeMap[key]), key));
                    }
                }
                else if (numFemale < numMale && numFemale > 0)
                {

                    foreach (var key in femaleAgeMap.Keys)
                    {
                        commentStringWriter.Write(string.Format("这{0}个{1}，",
                            NumberToChineseChar(femaleAgeMap[key]), key));
                    }
                    commentStringWriter.Write(string.Format("看起来很满足:) \n因为身边有"));
                    foreach (var key in maleAgeMap.Keys)
                    {
                        commentStringWriter.Write(string.Format("{0}个{1}，",
                            NumberToChineseChar(maleAgeMap[key]), key));
                    }
                }
                else if (numFemale == 0 && numMale == 1)
                {
                    commentStringWriter.Write(string.Format("画说，"));
                    foreach (var key in maleAgeMap.Keys)
                    {
                        commentStringWriter.Write(string.Format("这{0}个{1}，",
                            NumberToChineseChar(maleAgeMap[key]), key));
                    }
                    commentStringWriter.Write(string.Format("看起来很孤独 :)"));

                }
                else if (numMale == 0 && numFemale == 1)
                {
                    commentStringWriter.Write(string.Format("画说，"));
                    foreach (var key in femaleAgeMap.Keys)
                    {
                        commentStringWriter.Write(string.Format("这{0}个{1}，",
                            NumberToChineseChar(femaleAgeMap[key]), key));
                    }
                    commentStringWriter.Write(string.Format("看起来很无辜 :)"));
                }
                else if (numMale > 1 || numFemale > 1)
                {
                    desStringWriter.Write(string.Format("画说，"));
                    foreach (var key in maleAgeMap.Keys)
                    {
                        commentStringWriter.Write(string.Format("这{0}个{1}，",
                            NumberToChineseChar(maleAgeMap[key]), key));
                    }
                    foreach (var key in femaleAgeMap.Keys)
                    {
                        commentStringWriter.Write(string.Format("这{0}个{1}，",
                            NumberToChineseChar(femaleAgeMap[key]), key));
                    }
                    commentStringWriter.Write(string.Format("在一起很开心:)"));
                }
                else { }

                //老驴啃嫩草
                //float ratio = mAvgAge / fAvgAge;
                //if (ratio > 1.2 && numFemale > 0) desStringWriter.Write(string.Format("\n因为僧多粥少，所以{0}头老驴啃{1}棵嫩草", NumberToChineseChar(numMale), NumberToChineseChar(numFemale)));
                //else if (ratio < 0.8 && numMale > 0) desStringWriter.Write(string.Format("\n因为粥多僧少，所以{0}棵老草啃{1}头嫩驴", NumberToChineseChar(numFemale), NumberToChineseChar(numMale)));
                //else if (numFemale > 0 && numMale > 0)
                //{
                //    desStringWriter.Write(string.Format("{0}男{1}女，年轻的朋友们，今天来相会，荡起小船儿暖风轻轻吹", NumberToChineseChar(numMale), NumberToChineseChar(numFemale)));
                //}
                //else
                //{
                //}

                if (storyList.Count == 0)
                {
                    storyList.AddRange(cat2StoryMap["people_"]);
                }
                var random = new Random();
                var getrandomIdx = random.Next(0, storyList.Count - 1);

                var storyTuple = storyList.ToArray()[getrandomIdx];
                var story = String.Format("--嗯，谈画我联想到一个笑话: {0}\n作为一个高冷机器人，我只能说，{1}", storyTuple.Item1, storyTuple.Item2);
                if (!string.IsNullOrEmpty(story))
                {
                    //story += String.Format("\n\n我是谈画机器人--画说，{0}", cat2CommentMap[category.Name]);
                    //story = Encoding.ASCII.GetString(Encoding.Unicode.GetBytes(story));
                    //char[] charArr = { '，', '。' };
                    //foreach (var line in story.Split(charArr))
                    //    commentStringWriter.WriteLine(line.Trim());
                    commentStringWriter.WriteLine(story);
                }
               
                
            }

            if (result.Color != null)
            {
                //res += "AccentColor : " + result.Color.AccentColor;
                //res += "Dominant Color Background : " + result.Color.DominantColorBackground;
                //res += "Dominant Color Foreground : " + result.Color.DominantColorForeground;

                //if (result.Color.DominantColors != null && result.Color.DominantColors.Length > 0)
                //{
                //    //res += "Dominant Colors : ";
                //    foreach (var color in result.Color.DominantColors)
                //    {
                //        res += "color ";
                //    }
                //}
            }
            timeLogger.Append(string.Format("{0} VisionHelper::ShowRichAnalysisResult end\n", DateTime.Now - this.startTime));
            //Console.ResetColor();
            noAdsTxtResult = desStringWriter.ToString();
            desStringWriter.Write("关注geekplus-ms，点+号发图片，我就跟您谈画\n");


            return new Tuple<string, string>(desStringWriter.ToString(), commentStringWriter.ToString());
        }

        private static string FemaleTitleAsPerAge(double age)
        {
            string title;

            if (age <= 4) title = "灵童";
            else if (age <= 12) title = "萝莉";
            else if (age <= 15) title = "少女";
            else if (age <= 16) title = "花季少女";
            else if (age <= 17) title = "雨季少女";
            else if (age <= 24) title = "软妹子";
            else if (age <= 30) title = "圣女";
            else if (age <= 42) title = "大圣女";
            else if (age <= 55) title = "大姐";
            else if (age <= 75) title = "大妈";
            else title = "奶奶";
            
            return title;
        }

        private static string MaleTitleAsPerAge(double age)
        {
            string title;

            if (age <= 4) title = "神童";
            else if (age <= 12) title = "正太";
            else if (age <= 15) title = "少年";
            else if (age <= 16) title = "花季少年";
            else if (age <= 17) title = "雨季少年";
            else if (age <= 24) title = "小鲜肉";
            else if (age <= 30) title = "文青";
            else if (age <= 35) title = "男神";
            else if (age <= 40) title = "大哥";
            else if (age <= 55) title = "大叔";
            else if (age <= 70) title = "大爷";
            else title = "爷爷";
            
            return title;
        }

        private static string NumberToChineseChar(int number)
        {
            var chineseCharStr = "零一两三四五六七八九".ToCharArray();
            return number < chineseCharStr.Length ? chineseCharStr[number].ToString() : number.ToString();
        }
    }
}
