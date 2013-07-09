using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Austin.Net
{
    /// <summary>
    /// Represents a way to download data.
    /// </summary>
    public interface IDownloadManager
    {
        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadStream"]/*'/>
        Stream DownloadStream(DownloadRequest request);

        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadData"]/*'/>
        byte[] DownloadData(DownloadRequest request);

        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadString"]/*'/>
        string DownloadString(DownloadRequest request);

        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadStream"]/*'/>
        Task<Stream> DownloadStreamAsync(DownloadRequest request);

        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadData"]/*'/>
        Task<byte[]> DownloadDataAsync(DownloadRequest request);

        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadString"]/*'/>
        Task<string> DownloadStringAsync(DownloadRequest request);

        /// <summary>
        /// Adds a URL to the blacklist that so that it won't be downloaded again.
        /// </summary>
        /// <param name="url">The URL to be blacklist.</param>
        void AddToBlacklist(Uri url);

        /// <summary>
        /// Adds a filter to be run after special cases.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        void AddFilter(IDownloadRequestFilter filter);

        /// <summary>
        /// Adds a special case to be used by <see cref="Austin.Net.DownloadManager.TransformUrl"/>.
        /// </summary>
        /// <param name="domain">The domain in which the <paramref name="specialCase"/> will apply.</param>
        /// <param name="specialCase">The special <see cref="Austin.Net.UrlTransformation"/> to be called.</param>
        void AddSpecialCase(string domain, UrlTransformation specialCase);

        /// <summary>
        /// Creates a new instace of <see cref="DownloadRequest"/>.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        DownloadRequest CreateRequest(Uri address);

        /// <summary>
        /// Creates a new instace of <see cref="DownloadRequest"/>.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        DownloadRequest CreateRequest(Uri address, CancellationToken ct);
    }
}
