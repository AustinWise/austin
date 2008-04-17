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
	public sealed class DownloadRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Security.Cryptography.Hash"></see> class from the specified <see cref="System.Byte"/> array.
		/// </summary>
		/// <param name="address">The <see cref="System.Uri"/> of the resource to be downloaded.</param>
		public DownloadRequest(Uri address)
		{
			m_address = DownloadManager.TransformUrl(address);
		}

		internal string UserAgent
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

		private Uri m_address;
		/// <summary>
		/// The <see cref="System.Uri"/> of the resource to be downloaded.
		/// </summary>
		public Uri Address
		{
			get
			{
				return m_address;
			}
		}

		private string m_referer = string.Empty;
		/// <summary>
		/// The URL to tell the server from where the request came from when downloading the resource represented by this
		/// <see cref="Austin.Net.DownloadRequest"/> object.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		public string Referer
		{
			get
			{
				return m_referer;
			}
			set
			{
				m_referer = value;
			}
		}

		private ICredentials m_credentials;
		/// <summary>
		/// The <see cref="System.Net.ICredentials"/> to use when downloading the resource represented by this
		/// <see cref="Austin.Net.DownloadRequest"/> object.
		/// </summary>
		public ICredentials Credentials
		{
			get
			{
				return m_credentials;
			}
			set
			{
				m_credentials = value;
			}
		}

		private int m_timeout = 7500;
		/// <summary>
		/// The amount of time to wait be fore a request is aborted.
		/// </summary>
		/// <value>The length of the timeout in miliseconds</value>
		public int Timeout
		{
			get
			{
				return m_timeout;
			}
			set
			{
				m_timeout = value;
			}
		}

        private bool m_shouldCache;
        /// <summary>
        /// Determines whether or not the result of this request will be cached in memory.
        /// </summary>
        public bool ShouldCache
        {
            get
            {
                return this.m_shouldCache;
            }
            set
            {
                this.m_shouldCache = value;
            }
        }

        private bool m_saveCookies = false;
        /// <summary>
        /// Determines whether cookies from the server will be reused in other requests.
        /// </summary>
        public bool SaveCookiesReturnedByServer
        {
            get { return m_saveCookies; }
            set { m_saveCookies = value; }
        }

        private Dictionary<string, string> m_postValues = new Dictionary<string, string>();
        /// <summary>
        /// Data to send to the server as application/x-www-form-urlencoded.
        /// </summary>
        public Dictionary<string,string> PostValues
        {
            get { return m_postValues; }
            set { m_postValues = value; }
        }

        private bool m_allowAutoRedirect = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the request should follow redirection responses.
        /// </summary>
        public bool AllowAutoRedirect
        {
            get { return m_allowAutoRedirect; }
            set { m_allowAutoRedirect = value; }
        }

        private WebProxy m_proxy;
        /// <summary>
        /// A web proxy to use.
        /// </summary>
        public WebProxy Proxy
        {
            get { return m_proxy; }
            set { m_proxy = value; }
        }

	
	}
}
