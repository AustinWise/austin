using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Austin.Net
{
    /// <summary>
    /// An implementation of <see cref="IDownloadManager"/> that
    /// uses <see cref="HttpWebRequest"/> to download data.
    /// </summary>
    public class DownloadManager : IDownloadManager
    {
        private object sync = new object();
        private CookieContainer cookies = new CookieContainer();

        #region Special Cases and Filters
        private Dictionary<string, UrlTransformation> m_spcialCases = new Dictionary<string, UrlTransformation>();

        /// <summary>
        /// Adds a special case to be used by <see cref="Austin.Net.DownloadManager.TransformUrl"/>.
        /// </summary>
        /// <param name="domain">The domain in which the <paramref name="specialCase"/> will apply.</param>
        /// <param name="specialCase">The special <see cref="Austin.Net.UrlTransformation"/> to be called.</param>
        public void AddSpecialCase(string domain, UrlTransformation specialCase)
        {
            m_spcialCases.Add(domain, specialCase);
        }

        /// <summary>
        /// If there is a special case for the domain of the <paramref name="url"/>, the a transformed version of the <paramref name="url"/>
        /// is return.  Otherwise, the <paramref name="url"/> is return.
        /// </summary>
        /// <param name="url">The <see cref="System.Uri"/> to be transformed.</param>
        /// <returns>A <see cref="System.Uri"/> that may have been transformed.</returns>
        private Uri TransformUrl(Uri url)
        {
            string domain = url.Host;
            while (periodCount(domain) > 1)
            {
                domain = domain.Substring(domain.IndexOf(".") + 1);
            }
            if (m_spcialCases.ContainsKey(domain))
                return m_spcialCases[domain](url);
            else
                return url;
        }

        private int periodCount(string str)
        {
            return str.Length - str.Replace(".", string.Empty).Length;
        }

        private List<IDownloadRequestFilter> filters = new List<IDownloadRequestFilter>();
        /// <summary>
        /// Adds a filter to be run after special cases.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        public void AddFilter(IDownloadRequestFilter filter)
        {
            filters.Add(filter);
        }
        #endregion


        #region DownloadStream
        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadStream"]/*'/>
        public virtual Stream DownloadStream(DownloadRequest request)
        {
            return DownloadStreamAsync(request).Result;
        }

        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadStream"]/*'/>
        public virtual async Task<Stream> DownloadStreamAsync(DownloadRequest request)
        {
            if (request.m_IsImplicit)
                request = CreateRequest(request.Address);

            try
            {
                HttpWebRequest req = CreateRequest(request);
                var tup = await GetData(request, req);
                return tup.Item1;
            }
            finally
            {
                request.m_RequestCanceler.Dispose();
            }
        }
        #endregion

        #region DownloadData
        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadData"]/*'/>
        public virtual byte[] DownloadData(DownloadRequest request)
        {
            return DownloadDataAsync(request).Result;
        }

        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadData"]/*'/>
        public virtual async Task<byte[]> DownloadDataAsync(DownloadRequest request)
        {
            if (request.m_IsImplicit)
                request = CreateRequest(request.Address);

            string enc;
            long length;
            Stream s;
            try
            {
                HttpWebRequest req = CreateRequest(request);
                var tup = await GetData(request, req);
                s = tup.Item1;
                enc = tup.Item2;
                length = tup.Item3;
            }
            finally
            {
                request.m_RequestCanceler.Dispose();
            }

            var ms = new MemoryStream((int)length);

            var buffer = new byte[1024];
            int read;
            while ((read = s.Read(buffer, 0, 1024)) > 0)
            {
                ms.Write(buffer, 0, read);
            }

            s.Close();

            if (ms.Length == length)
                return ms.GetBuffer();
            else
                return ms.ToArray();
        }
        #endregion

        #region DownloadString
        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadString"]/*'/>
        public virtual string DownloadString(DownloadRequest request)
        {
            return DownloadStringAsync(request).Result;
        }

        /// <include file='Doc.xml' path='/Doc/Method[@name="DownloadString"]/*'/>
        public virtual async Task<string> DownloadStringAsync(DownloadRequest request)
        {
            if (request.m_IsImplicit)
                request = CreateRequest(request.Address);

            //download the data
            string enc;
            long contentLength;
            Stream bytes;
            try
            {
                HttpWebRequest req = CreateRequest(request);
                var tup = await GetData(request, req);
                bytes = tup.Item1;
                enc = tup.Item2;
                contentLength = tup.Item3;
            }
            finally
            {
                request.m_RequestCanceler.Dispose();
            }

            Encoding encoding;
            if (string.IsNullOrEmpty(enc))
                encoding = null;
            else
                try { encoding = Encoding.GetEncoding(enc); }
                catch (ArgumentException) { encoding = null; }

            StreamReader sr;
            if (encoding == null)
                sr = new StreamReader(bytes);
            else
                sr = new StreamReader(bytes, encoding);

            string res = sr.ReadToEnd();

            sr.Close();

            return res;
        }
        #endregion

        #region Helper
        private HttpWebRequest CreateRequest(DownloadRequest request)
        {
            // transform the URL.
            Uri transformedUrl = this.TransformUrl(request.Address);

            foreach (var filter in filters)
            {
                filter.Process(request);
            }

            //exit if it is in the blacklist
            if (isInBlackList(transformedUrl))
                throw new ArgumentException("The address in in the blacklist.");

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(transformedUrl);
            req.Timeout = request.Timeout;

            request.m_RequestCanceler = request.m_CancellationToken.Register(() => req.Abort());
            request.m_CancellationToken.ThrowIfCancellationRequested();

            //add headers
            if (!string.IsNullOrEmpty(request.Referer))
                req.Referer = request.Referer;
            if (!(request.Credentials == null))
                req.Credentials = request.Credentials;
            req.UserAgent = request.UserAgent;
            req.CookieContainer = cookies;
            req.AllowAutoRedirect = request.AllowAutoRedirect;
            foreach (var kvp in request.m_headers)
            {
                switch (kvp.Key)
                {
                    case "Accept":
                        req.Accept = kvp.Key;
                        break;
                    default:
                        req.Headers.Add(kvp.Key, kvp.Value);
                        break;
                }
            }

            //use a proxy if given
            if (request.Proxy != null)
            {
                req.Proxy = request.Proxy;
            }

            //if there is data to send to the server, send it!
            if (request.PostValues.Count != 0)
            {
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                List<string> values = new List<string>(request.PostValues.Count);
                foreach (KeyValuePair<string, string> value in request.PostValues)
                {
                    values.Add(string.Format("{0}={1}", System.Web.HttpUtility.UrlEncode(value.Key), System.Web.HttpUtility.UrlEncode(value.Value)));
                }
                string stringToSend = string.Join("&", values.ToArray());
                byte[] bytesToSend = Encoding.ASCII.GetBytes(stringToSend);
                req.ContentLength = bytesToSend.Length;
                Stream stream = req.GetRequestStream();
                stream.Write(bytesToSend, 0, bytesToSend.Length);
                stream.Close();
            }

            return req;
        }

        private Stream GetData(DownloadRequest down, HttpWebResponse res, out string encoding, out long contentLength)
        {
            if (down.SaveCookiesReturnedByServer)
            {
                foreach (Cookie cook in res.Cookies)
                {
                    cookies.Add(cook);
                }
            }

            Stream s = res.GetResponseStream();

            //try to get content type
            string type = res.ContentType; //Content-Type: text/html; charset=UTF-8
            type = type.ToLowerInvariant();
            int charsetIndex = type.IndexOf("charset=");
            if (charsetIndex != -1)
            {
                encoding = type.Substring(charsetIndex + "charset=".Length);
                charsetIndex = encoding.IndexOf(';');
                if (charsetIndex != -1)
                    encoding = encoding.Remove(charsetIndex);
                encoding = encoding.Trim();
            }
            else
                encoding = string.Empty;

            //try to get content length
            contentLength = res.ContentLength;

            return s;
        }

        private async Task<Tuple<Stream, string, long>> GetData(DownloadRequest down, HttpWebRequest req)
        {
            var webRes = await req.GetResponseAsync();
            string encoding;
            long contentLength;
            var stream = GetData(down, (HttpWebResponse)webRes, out encoding, out contentLength);
            return new Tuple<Stream, string, long>(stream, encoding, contentLength);
        }
        #endregion

        #region Blacklist
        private List<Uri> m_blacklist = new List<Uri>();

        private bool isInBlackList(Uri url)
        {
            lock (sync)
            {
                return m_blacklist.Contains(url);
            }
        }

        /// <summary>
        /// Adds a URL to the blacklist that so that it won't be downloaded again.
        /// </summary>
        /// <param name="url">The URL to be blacklist.</param>
        public void AddToBlacklist(Uri url)
        {
            if (isInBlackList(url))
                return;

            Monitor.Enter(sync);
            try
            {
                m_blacklist.Add(url);
            }
            finally { Monitor.Exit(sync); }
        }
        #endregion

        /// <summary>
        /// Creates a new instace of <see cref="DownloadRequest"/>.
        /// </summary>
        /// <param name="address">The address to send the request to.</param>
        /// <returns></returns>
        public virtual DownloadRequest CreateRequest(Uri address)
        {
            return new DownloadRequest(address);
        }

        /// <summary>
        /// Creates a new instace of <see cref="DownloadRequest"/>.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public virtual DownloadRequest CreateRequest(Uri address, CancellationToken ct)
        {
            return new DownloadRequest(address, ct);
        }
    }
}