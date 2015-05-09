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
    }
}