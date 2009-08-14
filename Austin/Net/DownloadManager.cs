using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;

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

        #region Special Cases
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
        #endregion

        #region DownloadStream
        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="address">A <see cref="System.String"/> containing the URI to download.</param>
        /// <returns>A <see cref="System.IO.Stream"/> containing the downloaded resource.</returns>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        public Stream DownloadStream(string address)
        {
            return DownloadStream(new Uri(address));
        }

        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="address">A <see cref="System.Uri"/> object containing the URI to download.</param>
        /// <returns>A <see cref="System.IO.Stream"/> containing the downloaded resource.</returns>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        public Stream DownloadStream(Uri address)
        {
            return DownloadStream(CreateRequest(address));
        }

        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="request">A <see cref="Austin.Net.DownloadRequest"/> containing the URI to download.</param>
        /// <returns>A <see cref="System.IO.Stream"/> containing the downloaded resource.</returns>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        public Stream DownloadStream(DownloadRequest request)
        {
            HttpWebRequest req = GetRequest(request);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            if (request.SaveCookiesReturnedByServer)
            {
                foreach (Cookie cook in res.Cookies)
                {
                    cookies.Add(cook);
                }
            }
            return res.GetResponseStream();
        }
        #endregion

        #region DownloadData
        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.Byte"/> array.
        /// </summary>
        /// <returns>A <see cref="System.Byte"/> array containing the downloaded resource.</returns>
        /// <param name="address">A <see cref="System.String"/> containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        public byte[] DownloadData(string address)
        {
            return DownloadData(new Uri(address));
        }

        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.Byte"/> array.
        /// </summary>
        /// <returns>>A <see cref="System.Byte"/> array containing the downloaded resource.</returns>
        /// <param name="address">A <see cref="System.Uri"/> object containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        public byte[] DownloadData(Uri address)
        {
            return DownloadData(CreateRequest(address));
        }

        /// <summary>
        /// Downloads the resource with the specified URI as a <see cref="System.Byte"/> array.
        /// </summary>
        /// <returns>>A <see cref="System.Byte"/> array containing the downloaded resource.</returns>
        /// <param name="request">A <see cref="Austin.Net.DownloadRequest"/> containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        public byte[] DownloadData(DownloadRequest request)
        {
            HttpWebRequest req = GetRequest(request);
            return GetData(request, req);
        }
        #endregion

        #region DownloadString
        /// <summary>
        /// Downloads the specified resource as a <see cref="System.String"/>.
        /// </summary>
        /// <returns>A String containing the specified resource.</returns>
        /// <param name="address">A <see cref="System.String"/> containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        public string DownloadString(string address)
        {
            return DownloadString(new Uri(address));
        }

        /// <summary>
        /// Downloads the specified resource as a <see cref="System.String"/>.
        /// </summary>
        /// <returns>A String containing the specified resource.</returns>
        /// <param name="address">A <see cref="System.Uri"/> object containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        public string DownloadString(Uri address)
        {
            return DownloadString(CreateRequest(address));
        }

        /// <summary>
        /// Downloads the specified resource as a <see cref="System.String"/>.
        /// </summary>
        /// <returns>A String containing the specified resource.</returns>
        /// <param name="request">A <see cref="Austin.Net.DownloadRequest"/> containing the URI to download.</param>
        /// <exception cref="System.ArgumentException">The address is in the blacklist.</exception>
        /// <exception cref="System.Net.WebException">The time-out period for the request expired.-or- An error occurred while processing the request.</exception>
        public string DownloadString(DownloadRequest request)
        {
            //download the data
            HttpWebRequest req = GetRequest(request);
            byte[] bytes = GetData(request, req);
            string res = GuessDownloadEncoding(req).GetString(bytes);

            return res;
        }
        #endregion

        #region Helper
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private HttpWebRequest GetRequest(DownloadRequest request)
        {
            // transform the URL.
            Uri transformedUrl = this.TransformUrl(request.Address);

            //exit if it is in the blacklist
            if (isInBlackList(transformedUrl))
                throw new ArgumentException("The address in in the blacklist.");

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(transformedUrl);
            req.Timeout = request.Timeout;

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

        private byte[] GetData(DownloadRequest down, HttpWebResponse res)
        {
            if (down.SaveCookiesReturnedByServer)
            {
                foreach (Cookie cook in res.Cookies)
                {
                    cookies.Add(cook);
                }
            }

            //get the response and setup readers
            Stream s = res.GetResponseStream();
            List<byte> bytes = new List<byte>();
            BinaryReader reader = new BinaryReader(s);

            //suck all the bytes out of the stream
            byte[] buffer = reader.ReadBytes(100);
            while (buffer.Length > 0)
            {
                bytes.AddRange(buffer);
                buffer = reader.ReadBytes(100);
            }
            reader.Close();

            //return data
            return bytes.ToArray();
        }

        private byte[] GetData(DownloadRequest down, HttpWebRequest req)
        {
            return GetData(down, (HttpWebResponse)req.GetResponse());
        }

        //from System.Net.WebClient
        private Encoding GuessDownloadEncoding(HttpWebRequest request)
        {
            try
            {
                string text1;
                if ((text1 = request.ContentType) == null)
                {
                    return Encoding.Default;
                }
                text1 = text1.ToLower(System.Globalization.CultureInfo.InvariantCulture);
                string[] textArray1 = text1.Split(new char[3] { ';', '=', ' ' });
                bool flag1 = false;
                string[] textArray2 = textArray1;
                for (int num1 = 0; num1 < textArray2.Length; num1++)
                {
                    string text2 = textArray2[num1];
                    if (text2 == "charset")
                    {
                        flag1 = true;
                    }
                    else if (flag1)
                    {
                        return Encoding.GetEncoding(text2);
                    }
                }
            }
            catch (Exception exception1)
            {
                if (((exception1 is System.Threading.ThreadAbortException) || (exception1 is StackOverflowException)) || (exception1 is OutOfMemoryException))
                {
                    throw;
                }
            }
            return Encoding.Default;
        }
        #endregion

        #region Blacklist
        private List<Uri> m_blacklist = new List<Uri>();

        private bool isInBlackList(Uri url)
        {
            bool ret = false;
            Monitor.Enter(sync);
            try
            {
                ret = m_blacklist.Contains(url);
            }
            finally { Monitor.Exit(sync); }
            return ret;
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
    }
}