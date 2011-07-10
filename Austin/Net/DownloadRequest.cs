using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Globalization;

namespace Austin.Net
{
    /// <summary>
    /// Provide instructions on how to download the specified resource.
    /// </summary>
    public class DownloadRequest
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DownloadRequest"></see> class that
        /// will be used to download the specified <see cref="System.Uri"/>.
        /// </summary>
        /// <param name="address">The <see cref="System.Uri"/> of the resource to be downloaded.</param>
        /// <param name="ct">A token to cancel this request.</param>
        public DownloadRequest(Uri address, CancellationToken ct)
        {
            this.Address = address;
            this.Timeout = 7500;
            this.PostValues = new Dictionary<string, string>();
            this.AllowAutoRedirect = true;
            this.m_CancellationToken = ct;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DownloadRequest"></see> class that
        /// will be used to download the specified <see cref="System.Uri"/>.
        /// </summary>
        /// <param name="address">The <see cref="System.Uri"/> of the resource to be downloaded.</param>
        public DownloadRequest(Uri address)
            : this(address, CancellationToken.None)
        {
        }

        /// <summary>
        /// The user agent that is sent to the server.
        /// </summary>
        public virtual string UserAgent
        {
            get
            {
                string userAgent = "DownloadManager/" + this.GetType().Assembly.GetName().Version.ToString() + " (compatible; {0}; .NET CLR " + Environment.Version.ToString() + ")";
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        userAgent = string.Format(CultureInfo.InvariantCulture, userAgent, "Windows; Windows NT " + Environment.OSVersion.Version.ToString());
                        break;
                    case PlatformID.Win32Windows:
                        userAgent = string.Format(CultureInfo.InvariantCulture, userAgent, "Windows; Windows 9X");
                        break;
                    default:
                        userAgent = string.Format(CultureInfo.InvariantCulture, userAgent, Environment.OSVersion.Platform.ToString());
                        break;
                }
                return userAgent;
            }
        }

        internal CancellationToken m_CancellationToken;

        /// <summary>
        /// The <see cref="System.Uri"/> of the resource to be downloaded.
        /// </summary>
        public Uri Address { get; private set; }

        /// <summary>
        /// The URL to tell the server from where the request came from when downloading the resource represented by this
        /// <see cref="Austin.Net.DownloadRequest"/> object.
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// The <see cref="System.Net.ICredentials"/> to use when downloading the resource represented by this
        /// <see cref="Austin.Net.DownloadRequest"/> object.
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// The amount of time to wait be fore a request is aborted.
        /// </summary>
        /// <value>The length of the timeout in miliseconds</value>
        public int Timeout { get; set; }

        /// <summary>
        /// Determines whether or not the result of this request will be cached in memory.
        /// </summary>
        public bool ShouldCache { get; set; }

        /// <summary>
        /// Determines whether cookies from the server will be reused in other requests.
        /// </summary>
        public bool SaveCookiesReturnedByServer { get; set; }

        /// <summary>
        /// Data to send to the server as application/x-www-form-urlencoded.
        /// </summary>
        public Dictionary<string, string> PostValues { get; private set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the request should follow redirection responses.
        /// </summary>
        public bool AllowAutoRedirect { get; set; }

        /// <summary>
        /// A web proxy to use.
        /// </summary>
        public WebProxy Proxy { get; set; }

        internal Dictionary<string, string> m_headers = new Dictionary<string, string>();

        /// <summary>
        /// Adds a header to be sent as part of the request.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void AddHeader(string key, string value)
        {
            this.m_headers.Add(key, value);
        }
    }
}
