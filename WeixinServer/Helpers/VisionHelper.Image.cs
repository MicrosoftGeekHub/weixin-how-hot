using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Linq;
using WeixinServer.Models;
namespace WeixinServer.Helpers
{
    public partial class VisionHelper
    {
        private string storageConnectionString;
        private CloudStorageAccount storageAccount;
        // Create the blob client.
        private CloudBlobClient blobClient = null;

        // Retrieve reference to a previously created container.
        private CloudBlobContainer container = null;

        private MemoryStream midStream;

        private void InitializePropertiesForAzure() 
        {
            storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=geeekstore;AccountKey=gwn9gAn+Uo6YqjdRNBF/mrM0Hbb54Ns61Rq9Q+ahhSyqrq64jrLMTn834cKmMKbqSFv9BTW8NtCFbUte49EzcA==";
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("cdn");
        }

        private void InitializePropertiesForImage(string frontImageUri)
        {
            this.frontImageUri = frontImageUri;
        }
        
        static Font FindFont(System.Drawing.Graphics g, string longString, Size Room, Font PreferedFont)
        {
            //you should perform some scale functions
            SizeF RealSize = g.MeasureString(longString, PreferedFont);
            float HeightScaleRatio = Room.Height / RealSize.Height;
            float WidthScaleRatio = Room.Width / RealSize.Width;
            float ScaleRatio = (HeightScaleRatio < WidthScaleRatio) ? ScaleRatio = HeightScaleRatio : ScaleRatio = WidthScaleRatio;
            float ScaleFontSize = PreferedFont.Size * ScaleRatio;
            return new Font(PreferedFont.FontFamily, ScaleFontSize);
        }

        private MemoryStream DrawRects(MemoryStream inStream, AnalysisResult analysisResult) 
        {
            Face[] faceDetections = analysisResult.Faces;
            int ascr = (int)(analysisResult.Adult.AdultScore * 2500);
            int rscr = (int)(analysisResult.Adult.RacyScore * 5000);
            int saoBility = ascr + rscr;
            Image image = Image.FromStream(inStream);
            //        Watermark
            var clr = new Microsoft.ProjectOxford.Vision.Contract.Color();
            clr.AccentColor = "CAA501";
            var layers = new List<TextLayer>();
            var ms = new MemoryStream();
            // Modify the image using g

            //string fontName = "YourFont.ttf";
            PrivateFontCollection pfcoll = new PrivateFontCollection();
            //put a font file under a Fonts directory within your application root
            pfcoll.AddFontFile(this.frontImageUri);
            FontFamily ff = pfcoll.Families[0];

            using (Graphics g = Graphics.FromImage(image))
            {
                var txtRectangles = new List<System.Drawing.Rectangle>();
                var femelRectangles = new List<System.Drawing.Rectangle>();
                var maleRectangles = new List<System.Drawing.Rectangle>();
                foreach (var faceDetect in faceDetections)
                {
                    string genderInfo = "";

                    //int topText = faceDetect.FaceRectangle.Top + faceDetect.FaceRectangle.Height + 5;
                    int topText = faceDetect.FaceRectangle.Top - 150;
                    topText = topText > 0 ? topText : 0;
                    int leftText = faceDetect.FaceRectangle.Left;

                    if (faceDetect.Gender.Equals("Male"))
                    {
                        genderInfo += "♂";
                        maleRectangles.Add(new System.Drawing.Rectangle(faceDetect.FaceRectangle.Left,
                            faceDetect.FaceRectangle.Top, faceDetect.FaceRectangle.Width, faceDetect.FaceRectangle.Height));
                        //maleRectangles.Add(new System.Drawing.Rectangle(leftText,
                        //    topText, faceDetect.FaceRectangle.Width, faceDetect.FaceRectangle.Top - topText));
                    }
                    else
                    {
                        genderInfo += "♀";
                        femelRectangles.Add(new System.Drawing.Rectangle(faceDetect.FaceRectangle.Left,
                            faceDetect.FaceRectangle.Top, faceDetect.FaceRectangle.Width, faceDetect.FaceRectangle.Height));
                        //femelRectangles.Add(new System.Drawing.Rectangle(leftText,
                        //    topText, faceDetect.FaceRectangle.Width, faceDetect.FaceRectangle.Top - topText));
                    }
                    //draw text 
                    //float size = faceDetect.FaceRectangle.Width / 5.0f;
                    string info = string.Format("{0}颜龄{1}\n骚值{2:F0}\n肾价{3:F2}万", genderInfo, faceDetect.Age, 
                        saoBility * faceDetect.Age, ascr / faceDetect.Age);
                    Size room = new Size(faceDetect.FaceRectangle.Width, faceDetect.FaceRectangle.Top - topText);
                    Font f = new Font(ff, 24, FontStyle.Bold, GraphicsUnit.Pixel);
                    //Font f = FindFont(g, info, room, new Font("Arial", 600, FontStyle.Regular, GraphicsUnit.Pixel));
                    g.DrawString(info, f, new SolidBrush(System.Drawing.Color.LimeGreen), new Point(leftText, topText));

                    //layers.Add(this.GetFaceTextLayer(info, leftText, topText, clr));

                }       //end of for

                // Create a brush with an alpha value and use the g.FillRectangle function
                //var customColor = System.Drawing.Color.FromArgb(255, System.Drawing.Color.Gray);
                //TextureBrush shadowBrush = new TextureBrush(image);
                if (femelRectangles.Count > 0)
                {
                    Pen pen = new Pen(System.Drawing.Color.Magenta, 2);
                    g.DrawRectangles(pen, femelRectangles.ToArray());
                }
                if (maleRectangles.Count > 0)
                {
                    Pen pen = new Pen(System.Drawing.Color.Lime, 2);
                    g.DrawRectangles(pen, maleRectangles.ToArray());
                }

                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            
            
            return ms;
            //timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage imageFactory.Load begin\n", DateTime.Now - this.startTime));
            //using (var imageFactory = new ImageFactory(preserveExifData: false))
            //{
            //    imageFactory.Load(ms);
            //    foreach(var layer in layers)
            //    {
            //        imageFactory.Watermark(layer);
                                
            //    }
            //    imageFactory.Format(new JpegFormat()).Save(outStream);
            //}
            //timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage imageFactory.Load midStream generated\n", DateTime.Now - this.startTime));
            //return outStream;
        }

        private ImageLayer GetFrameImageLayer(FaceRectangle detect)
        {
            var resizeLayer = new ResizeLayer(size: new Size(detect.Width, detect.Height), resizeMode: ResizeMode.Stretch);
            var frameSteam = new MemoryStream();
            var frameFactory = new ImageFactory(preserveExifData: false);
            //frameFactory.Load(this.frameImageUri).Resize(resizeLayer).Save(frameSteam);

            var frameImage = Image.FromStream(frameSteam);
            return new ImageLayer
            {
                Image = frameImage,
                Position = new Point(detect.Left, detect.Top),
            };
        }

        private TextLayer GetFaceTextLayer(string text, int x, int y, Microsoft.ProjectOxford.Vision.Contract.Color color)
        {
            const int RGBMAX = 255;

            System.Drawing.Color fontColor = System.Drawing.Color.DeepPink;
            if (color != null && !string.IsNullOrWhiteSpace(color.AccentColor))
            {
                var accentColor = ColorTranslator.FromHtml("#" + color.AccentColor);
                fontColor = System.Drawing.Color.FromArgb(RGBMAX - accentColor.R, RGBMAX - accentColor.G, RGBMAX - accentColor.B);
            }

            var fontSize = 30;//width < 1000 ? 24 : 36;


            return new TextLayer
            {
                DropShadow = true,
                FontColor = fontColor,
                FontSize = fontSize,
                FontFamily = new FontFamily(GenericFontFamilies.SansSerif),
                Text = text,
                Style = FontStyle.Bold,
                Position = new Point(x, y),
                Opacity = 85,
            };
        }


        private TextLayer GetTextLayer(string text, int width, int height, Microsoft.ProjectOxford.Vision.Contract.Color color)
        {
            const int RGBMAX = 255;

            System.Drawing.Color fontColor = System.Drawing.Color.DeepPink;
            if (color != null && !string.IsNullOrWhiteSpace(color.AccentColor))
            {
                var accentColor = ColorTranslator.FromHtml("#" + color.AccentColor);
                fontColor = System.Drawing.Color.FromArgb(RGBMAX - accentColor.R, RGBMAX - accentColor.G, RGBMAX - accentColor.B);
            }

            var fontSize = width < 1000 ? 24 : 36;

            var x = (int)(width * 0.05);
            var y = height - (fontSize + 5) * 5;

            return new TextLayer
            {
                DropShadow = true,
                FontColor = fontColor,
                FontSize = fontSize,
                FontFamily = new FontFamily(GenericFontFamilies.SansSerif),
                Text = text,
                Style = FontStyle.Bold,
                Position = new Point(x, y),
                Opacity = 85,
            };
        }

        private MemoryStream DrawText(string text, int width, int height, Microsoft.ProjectOxford.Vision.Contract.Color color)
        {
            const int RGBMAX = 255;
            PrivateFontCollection pfcoll = new PrivateFontCollection();
            //put a font file under a Fonts directory within your application root
            pfcoll.AddFontFile(this.frontImageUri);
            FontFamily ff = pfcoll.Families[0];
            System.Drawing.Color fontColor = System.Drawing.Color.DeepPink;
            if (color != null && !string.IsNullOrWhiteSpace(color.AccentColor))
            {
                var accentColor = ColorTranslator.FromHtml("#" + color.AccentColor);
                fontColor = System.Drawing.Color.FromArgb(RGBMAX - accentColor.R, RGBMAX - accentColor.G, RGBMAX - accentColor.B);
            }

            //var fontSize = width < 1000 ? 24 : 36;
            var fontSize = 36;

            var x = (int)(width * 0.05);
            var y = height - (fontSize + 5) * 5;

            //DropShadow = true,
            //FontColor = fontColor,
            //FontSize = fontSize,
            //FontFamily = new FontFamily(GenericFontFamilies.SansSerif),
            //Text = text,
            //Style = FontStyle.Bold,
            //Position = new Point(x, y),
            //Opacity = 85,


            var ms = new MemoryStream();
            // Modify the image using g
            midStream.Seek(0, SeekOrigin.Begin);
            Image image = Image.FromStream(midStream);
            //        Watermark
            //var clr = new Microsoft.ProjectOxford.Vision.Contract.Color();
            //clr.AccentColor = "CAA501";
            using (Graphics g = Graphics.FromImage(image))
            {
                    //draw text 
                    //Size room = new Size(faceDetect.FaceRectangle.Width, faceDetect.FaceRectangle.Top - topText);
                Font f = new Font(ff, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                    //Font f = FindFont(g, info, room, new Font("Arial", 600, FontStyle.Regular, GraphicsUnit.Pixel));
                g.DrawString(text, f, new SolidBrush(System.Drawing.Color.LimeGreen), new Point(x, y));
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            return ms;
        }

        private static string random_string(int length = 12)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

        }
        private string RenderAnalysisResultAsImage(AnalysisResult result, RichResult txtRichResult)
        {
            return RenderAnalysisResultAsImage(result, txtRichResult.analyzeImageResult);
        }
        private string RenderAnalysisResultAsImage(AnalysisResult result, string captionText)
        {
            timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage begin\n", DateTime.Now - this.startTime));
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
                        timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage imageFactory.Load begin\n", DateTime.Now - this.startTime));
                        midStream = DrawRects(inStream, result);
                        //var midStream = 
                        timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage imageFactory.Load midStream generated\n", DateTime.Now - this.startTime));

                        midStream = DrawText(captionText, result.Metadata.Width, result.Metadata.Height, result.Color);

                        //imageFactory.Load(midStream);
                        //timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage imageFactory.Load end\n", DateTime.Now - this.startTime));
                        ////// Add frame
                        ////foreach (var detect in result.Faces)
                        ////{
                        ////    imageFactory.Overlay(this.GetFrameImageLayer(detect.FaceRectangle));
                        ////    //break;//only one
                        ////}
                        
                        //// Save
                        //timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage Watermark begin\n", DateTime.Now - this.startTime));
                        //imageFactory
                        //   // .Watermark(this.GetTextLayer(captionText, result.Metadata.Width, result.Metadata.Height, result.Color))
                        //    .Format(new JpegFormat())
                        //    .Save(outStream);
                        //timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage Watermark end\n", DateTime.Now - this.startTime));
                    }
                    timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage Upload to image CDN begin\n", DateTime.Now - this.startTime));
                    // Upload to image CDN
                    
                 //   return string.Format("酷评:\n{0}\n归图:\n{1}", captionText, resultUrl);
                    

                    // Retrieve reference to a blob named "myblob".
                    //string blobName = string.Format("{0}_{1}.jpg", this.curUserName, random_string(12));
                    int idx = this.curUserName.LastIndexOf('_');
                    idx = idx > -1? idx : 0;
                    string blobName = string.Format("{0}{1}.jpg", random_string(12), this.curUserName.Substring(idx));
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                    // Create or overwrite the "myblob" blob with contents from a local file.
                    midStream.Seek(0, SeekOrigin.Begin);
                    blockBlob.UploadFromStream(midStream);
                    resultUrl = "http://geeekstore.blob.core.windows.net/cdn/" + blobName;
                    //resultUrl = upyun.UploadImageStream(outStream);

                    this.returnImageUrl = resultUrl;                    
                    timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage Upload to image CDN end\n", DateTime.Now - this.startTime));
                }
            }

            return string.Format("酷评:\n{0}\n归图:\n{1}", captionText, resultUrl);
            //return string.Format("酷评:\n{0}\n归图:\n{1}\n原图:\n{2}", captionText, resultUrl, this.originalImageUrl);
        }
    }
}
