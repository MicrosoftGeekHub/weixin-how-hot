using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using Microsoft.ProjectOxford.Vision.Contract;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace WeixinServer.Helpers
{
    public partial class VisionHelper
    {
        private void InitializePropertiesForImage(string frameImageUri)
        {
            this.frameImageUri = frameImageUri;
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
                            break;//only one
                        }

                        // Save
                        imageFactory
                            .Watermark(this.GetTextLayer(captionText, result.Metadata.Width, result.Metadata.Height, result.Color))
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
