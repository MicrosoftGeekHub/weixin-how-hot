using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
namespace WeixinServer.Helpers
{
    /// <summary>
    /// The class is used to access vision APIs.
    /// </summary>
    public partial class VisionHelper
    {
        private IVisionServiceClient visionClient;

        private Dictionary<string, string> categoryNameMapping = null;
        
        private string frontImageUri;
        
        private string originalImageUrl;
        
        private byte[] photoBytes;


        class faceAgent
        {

            private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("8c00452ccad0465baa695f7779fbf4a9");
            public async Task<Microsoft.ProjectOxford.Face.Contract.FaceRectangle[]> UploadAndDetectFaces(string imageFilePath)
            {

                try
                {

                    using (Stream imageFileStream = File.OpenRead(imageFilePath))
                    {

                        var faces = await faceServiceClient.DetectAsync(imageFileStream);



                        var faceRects = faces.Select(face => face.FaceRectangle);



                        return faceRects.ToArray();

                    }

                }

                catch (Exception)
                {

                    return new Microsoft.ProjectOxford.Face.Contract.FaceRectangle[0];

                }



            }

            public async Task<Microsoft.ProjectOxford.Face.Contract.Face[]> UploadStreamAndDetectFaces(string url)
            {

                try
                {

                    var request = System.Net.WebRequest.Create(new Uri(url));
                    request.Timeout = int.MaxValue;
                    var response = request.GetResponse();
                    var streamToUpload = response.GetResponseStream();
                    var faces = await faceServiceClient.DetectAsync(streamToUpload);
                    return faces.ToArray();
                }

                catch (Exception)
                {

                    return new Microsoft.ProjectOxford.Face.Contract.Face[0];

                }



            }

            public async Task<Microsoft.ProjectOxford.Face.Contract.Face[]> UploadStreamAndDetectFaces(Stream stream)
            {

                try
                {
                    //if (stream == null) return null;
                    //stream.Seek(0, SeekOrigin.Begin);
                    var faces = await faceServiceClient.DetectAsync(stream, false, true, true, false);
                    return faces.ToArray();
                }

                catch (Exception)
                {

                    return new Microsoft.ProjectOxford.Face.Contract.Face[0];

                }



            }

            public async Task<Microsoft.ProjectOxford.Face.Contract.Face[]> UploadAndReturnFaces(string imageFilePath)
            {

                try
                {

                    using (Stream imageFileStream = File.OpenRead(imageFilePath))
                    {

                        var faces = await faceServiceClient.DetectAsync(imageFileStream, true, true, true, true);

                        return faces.ToArray();

                    }

                }

                catch (Exception)
                {

                    return new Microsoft.ProjectOxford.Face.Contract.Face[0];

                }


            }

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisionHelper"/> class.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        /// <param name="frameImageUri">The frame image URI.</param>
        public VisionHelper(string subscriptionKey, string frontImageUri, DateTime startTime)
        {
            this.startTime = startTime;
            timeLogger.Append(string.Format("{0} VisionHelper::InitializePropertiesForText\n", DateTime.Now));
            InitializePropertiesForAzure();
            this.InitializePropertiesForText(subscriptionKey);
            this.InitializePropertiesForImage(frontImageUri);
        }
    }
}
