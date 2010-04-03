using System;
using System.Collections.Generic;
using System.Text;

namespace Austin.Net
{
    /// <summary>
    /// Changes requests before they are sent sent.
    /// </summary>
    public interface IDownloadRequestFilter
    {
        /// <summary>
        /// Changes the request after special cases have been applied.
        /// </summary>
        /// <param name="req">The request to modify.</param>
        void Process(DownloadRequest req);
    }
}
