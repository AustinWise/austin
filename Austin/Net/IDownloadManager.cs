using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Austin.Net
{
    /// <summary>
    /// Represents a way to download data.
    /// </summary>
    public interface IDownloadManager
    {
        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="address">A <see cref="System.String"/> containing the URI to download.</param>
        /// <returns>A <see cref="System.IO.Stream"/> containing the downloaded resource.</returns>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        Stream DownloadStream(string address);

        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="address">A <see cref="System.Uri"/> object containing the URI to download.</param>
        /// <returns>A <see cref="System.IO.Stream"/> containing the downloaded resource.</returns>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        Stream DownloadStream(Uri address);

        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="request">A <see cref="Austin.Net.DownloadRequest"/> containing the URI to download.</param>
        /// <returns>A <see cref="System.IO.Stream"/> containing the downloaded resource.</returns>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        Stream DownloadStream(DownloadRequest request);

        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.Byte"/> array.
        /// </summary>
        /// <returns>A <see cref="System.Byte"/> array containing the downloaded resource.</returns>
        /// <param name="address">A <see cref="System.String"/> containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        byte[] DownloadData(string address);

        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.Byte"/> array.
        /// </summary>
        /// <returns>>A <see cref="System.Byte"/> array containing the downloaded resource.</returns>
        /// <param name="address">A <see cref="System.Uri"/> object containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        byte[] DownloadData(Uri address);

        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.Byte"/> array.
        /// </summary>
        /// <returns>>A <see cref="System.Byte"/> array containing the downloaded resource.</returns>
        /// <param name="request">A <see cref="Austin.Net.DownloadRequest"/> containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        byte[] DownloadData(DownloadRequest request);

        /// <summary>
        /// Downloads the specified resource as a <see cref="System.String"/>.
        /// </summary>
        /// <returns>A String containing the specified resource.</returns>
        /// <param name="address">A <see cref="System.String"/> containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        string DownloadString(string address);

        /// <summary>
        /// Downloads the specified resource as a <see cref="System.String"/>.
        /// </summary>
        /// <returns>A String containing the specified resource.</returns>
        /// <param name="address">A <see cref="System.Uri"/> object containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        string DownloadString(Uri address);

        /// <summary>
        /// Downloads the specified resource as a <see cref="System.String"/>.
        /// </summary>
        /// <returns>A String containing the specified resource.</returns>
        /// <param name="request">A <see cref="Austin.Net.DownloadRequest"/> containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        string DownloadString(DownloadRequest request);

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
    }
}
