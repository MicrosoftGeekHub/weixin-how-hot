using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeixinServer.Models
{
    public class RichResult
    {
        public RichResult(string timeLogs, string analyzeImageResult, string errorLogs)
        {
            this.timeLogs = timeLogs;
            this.analyzeImageResult = analyzeImageResult;
            this.errorLogs = errorLogs;
        }

        public RichResult(string timeLogs, string analyzeImageResult, string errorLogs, string uploadedUrl, byte[] rawImage)
        {
            this.timeLogs = timeLogs;
            this.analyzeImageResult = analyzeImageResult;
            this.errorLogs = errorLogs;
            this.uploadedUrl = uploadedUrl;
            this.rawImage = rawImage;
        }

        /// <summary>
        /// Gets or sets the timeLogs.
        /// </summary>
        /// <value>
        /// timestamp Logs.
        /// </value>
        public string timeLogs { get; set; }

        /// <summary>
        /// Gets or sets the errorLogs.
        /// </summary>
        /// <value>
        /// The errorLogs.
        /// </value>
        public string errorLogs { get; set; }

        /// <summary>
        /// Gets or sets the analyzeImageResult.
        /// </summary>
        /// <value>
        /// The Analized Result.
        /// </value>
        public string analyzeImageResult { get; set; }

        /// <summary>
        /// Gets or sets the uploaded Url.
        /// </summary>
        /// <value>
        /// The uploaded Url.
        /// </value>
        public string uploadedUrl { get; set; }

        /// <summary>
        /// Gets or sets the content processedImage.
        /// </summary>
        /// <value>
        /// The processed Image.
        /// </value>
        public byte[] processedImage { get; set; }

        /// <summary>
        /// Gets or sets the content processedImage.
        /// </summary>
        /// <value>
        /// The processed Image.
        /// </value>
        public byte[] rawImage { get; set; }
    }
}