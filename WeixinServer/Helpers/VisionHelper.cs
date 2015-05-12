using Microsoft.ProjectOxford.Vision;
using System;
using System.Collections.Generic;
using System.Net;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="VisionHelper"/> class.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        /// <param name="frameImageUri">The frame image URI.</param>
        public VisionHelper(string subscriptionKey, string frontImageUri)
        {
            timeLogger.Append(string.Format("{0} VisionHelper::InitializePropertiesForText\n", DateTime.Now));
            InitializePropertiesForAzure();
            this.InitializePropertiesForText(subscriptionKey);
            this.InitializePropertiesForImage(frontImageUri);
        }
    }
}
