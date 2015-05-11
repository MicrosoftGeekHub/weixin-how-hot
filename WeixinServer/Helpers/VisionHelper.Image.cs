﻿using ImageProcessor;
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

        private void InitializePropertiesForAzure() 
        {
            storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=geeekstore;AccountKey=gwn9gAn+Uo6YqjdRNBF/mrM0Hbb54Ns61Rq9Q+ahhSyqrq64jrLMTn834cKmMKbqSFv9BTW8NtCFbUte49EzcA==";
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("cdn");
        }

        private void InitializePropertiesForImage(string frameImageUri)
        {
            this.frameImageUri = frameImageUri;
        }
        
        
        static MemoryStream DrawRects(MemoryStream inStream, Face[] faceDetections)
        {
            Image image = Image.FromStream(inStream);
            using (Graphics g = Graphics.FromImage(image))
            {
                var rectangles = new List<System.Drawing.Rectangle>();
                var femelRectangles = new List<System.Drawing.Rectangle>();
                foreach (var faceDetect in faceDetections)
                {
                    if(faceDetect.Gender.Equals("Male")) rectangles.Add(new System.Drawing.Rectangle(faceDetect.FaceRectangle.Left,
                        faceDetect.FaceRectangle.Top, faceDetect.FaceRectangle.Width, faceDetect.FaceRectangle.Height));
                    else femelRectangles.Add(new System.Drawing.Rectangle(faceDetect.FaceRectangle.Left,
                        faceDetect.FaceRectangle.Top, faceDetect.FaceRectangle.Width, faceDetect.FaceRectangle.Height));
                }
                // Modify the image using g here... 
                // Create a brush with an alpha value and use the g.FillRectangle function
                var customColor = System.Drawing.Color.FromArgb(255, System.Drawing.Color.Gray);
                TextureBrush shadowBrush = new TextureBrush(image);
                if (femelRectangles.Count > 0)
                {
                    Pen pen = new Pen(System.Drawing.Color.Magenta, 2);
                    g.DrawRectangles(pen, femelRectangles.ToArray());
                }
                if (rectangles.Count > 0)
                {
                    Pen pen = new Pen(System.Drawing.Color.Lime, 2);
                    g.DrawRectangles(pen, rectangles.ToArray());
                }
                
                //image.Save(@"d:\tmp\0510save.jpg");
            }

            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms;
        }

        private ImageLayer GetFrameImageLayer(FaceRectangle detect)
        {
            var resizeLayer = new ResizeLayer(size: new Size(detect.Width, detect.Height), resizeMode: ResizeMode.Stretch);
            var frameSteam = new MemoryStream();
            var frameFactory = new ImageFactory(preserveExifData: false);
            frameFactory.Load(this.frameImageUri).Resize(resizeLayer).Save(frameSteam);

            var frameImage = Image.FromStream(frameSteam);
            return new ImageLayer
            {
                Image = frameImage,
                Position = new Point(detect.Left, detect.Top),
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
        private static string random_string(int length = 12)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

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
                        var midStream = DrawRects(inStream, result.Faces);
                        timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage imageFactory.Load midStream generated\n", DateTime.Now - this.startTime));
                        imageFactory.Load(midStream);
                        timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage imageFactory.Load end\n", DateTime.Now - this.startTime));
                        //// Add frame
                        //foreach (var detect in result.Faces)
                        //{
                        //    imageFactory.Overlay(this.GetFrameImageLayer(detect.FaceRectangle));
                        //    //break;//only one
                        //}
                        
                        // Save
                        timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage Watermark begin\n", DateTime.Now - this.startTime));
                        imageFactory
                            .Watermark(this.GetTextLayer(captionText, result.Metadata.Width, result.Metadata.Height, result.Color))
                            .Format(new JpegFormat())
                            .Save(outStream);
                        timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage Watermark end\n", DateTime.Now - this.startTime));
                    }
                    timeLogger.Append(string.Format("{0} VisionHelper::AnalyzeImage::RenderAnalysisResultAsImage Upload to image CDN begin\n", DateTime.Now - this.startTime));
                    // Upload to image CDN
                    
                 //   return string.Format("酷评:\n{0}\n归图:\n{1}", captionText, resultUrl);
                    

                    // Retrieve reference to a blob named "myblob".
                    //string blobName = string.Format("{0}_{1}.jpg", this.curUserName, random_string(12));
                    string blobName = string.Format("{0}_{1}.jpg", this.curUserName.Substring(this.curUserName.LastIndexOf('_') - 1), random_string(12));
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                    // Create or overwrite the "myblob" blob with contents from a local file.
                    blockBlob.UploadFromStream(outStream);
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
