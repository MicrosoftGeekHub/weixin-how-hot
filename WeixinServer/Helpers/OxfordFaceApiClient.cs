using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WeixinServer.Helpers
{
    public class OxfordFaceApiClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisionHelper"/> class.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        /// <param name="frameImageUri">The frame image URI.</param>
        public OxfordFaceApiClient(string subscriptionKey)
        {
            this.faceServiceClient = new FaceServiceClient(subscriptionKey);
        }

        private readonly IFaceServiceClient faceServiceClient;


        public async Task<float> CalculateSimilarity(Guid faceId1, Guid faceId2)
        { 
            var verifyResult = await faceServiceClient.VerifyAsync(faceId1, faceId2);
            return (float)verifyResult.Confidence;
        }

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
                var faces = await faceServiceClient.DetectAsync(streamToUpload, true, true, true, false);
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
                var faces = await faceServiceClient.DetectAsync(stream, true, true, true, false);
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

}