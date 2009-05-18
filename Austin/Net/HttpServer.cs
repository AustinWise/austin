using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Xml;
using System.Globalization;

namespace Austin.Net
{
    /// <summary>
    /// Provides a simple, ASMX-like HTTP protocol listener.
    /// </summary>
    public abstract partial class HttpServer : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Austin.Net.HttpServer"></see> class.
        /// </summary>
        protected HttpServer()
        {
            bool isServer = false;
            foreach (Attribute a in this.GetType().GetCustomAttributes(true))
            {
                if (a.GetType() == typeof(HttpServerAttribute))
                {
                    HttpServerAttribute ha = (HttpServerAttribute)a;
                    this.m_server.Prefixes.Add(ha.Prefix);
                    this.m_serverName = ha.Name;
                    this.m_pathPrefix = new Uri(ha.Prefix.Replace("+", "localhost").Replace("*", "localhost")).AbsolutePath;

                    isServer = true;
                    break;
                }
            }
            if (!isServer)
                throw new HttpServerException("There is no HttpServerAttribute on this class.");

            foreach (MethodInfo m in this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                //check to see if it is marked as a HttpServerMethod
                if (m.GetCustomAttributes(typeof(HttpServerMethodAttribute), true).Length == 1)
                {
                    HttpServerMethod hm = new HttpServerMethod(m);
                    this.m_functions.Add(hm.Name, hm);
                }
            }
        }

        #region Properties
        /// <summary>
        /// Gets a value that indicates whether <see cref="Austin.Net.HttpServer"/> can be used with the current operating system.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                return HttpListener.IsSupported;
            }
        }

        private string m_pathPrefix;
        private string PathPrefix
        {
            get
            {
                return m_pathPrefix;
            }
        }

        private string m_serverName;
        /// <summary>
        /// The name of the server.
        /// </summary>
        public string ServerName
        {
            get
            {
                return this.m_serverName;
            }
        }

        private static string Styles
        {
            get
            {
                return Properties.Resources.Styles;
            }
        }

        private static Version ServerVersion
        {
            get
            {
                return typeof(HttpServer).Assembly.GetName().Version;
            }
        }

        private static string ServerType
        {
            get
            {
                return "Austin HTTP Server";
            }
        }

        private static Uri ProductUrl
        {
            get
            {
                return new Uri("http://github.com/AustinWise/austin/");
            }
        }

        private static string Copyright
        {
            get
            {
                return "2005 - 2009";
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            try
            {
                m_server.Start();
                m_isRunning = true;
            }
            catch (Exception ex)
            {
                throw new HttpServerException("Failed to start server.", ex);
            }
            m_thread = new Thread(pump);
            m_thread.Start();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            m_isRunning = false;
            m_server.Stop();
            m_thread.Abort();
            m_thread.Join();
        }

        /// <summary>
        /// Shuts down the <see cref="Austin.Net.HttpServer"/> object immediately, discarding all currently queued requests.
        /// </summary>
        public void Abort()
        {
            m_server.Abort();
        }

        /// <summary>
        /// Shuts down the <see cref="Austin.Net.HttpServer"/> after processing all currently queued requests.
        /// </summary>
        public void Close()
        {
            m_server.Close();
        }
        #endregion

        #region Listener
        private HttpListener m_server = new HttpListener();
        private static Encoding encoding = Encoding.UTF8;
        private Dictionary<string, HttpServerMethod> m_functions = new Dictionary<string, HttpServerMethod>();
        private bool m_isRunning;
        private Thread m_thread;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void pump()
        {
            while (m_isRunning)
            {
                HttpListenerContext con;
                try { con = m_server.GetContext(); }
                catch
                {
                    return;
                }
                HttpListenerRequest req = con.Request;
                HttpListenerResponse res = con.Response;

                //setup the response
                res.ContentEncoding = encoding;

                if (!(req.Url.AbsolutePath.StartsWith(this.PathPrefix)))
                {
                    res.ContentType = MediaTypeNames.Text.Html;
                    XmlWriter wr = XmlWriter.Create(res.OutputStream, CreateSettings(ConformanceLevel.Document));
                    WritePageStart(wr, ServerType);
                    WriteError(wr, "No server confirgured at this address.");
                    WritePageEnd(wr);
                    wr.Close();
                }
                else
                {

                    //get the information passes to us
                    string function = req.Url.AbsolutePath.Substring(req.Url.AbsolutePath.LastIndexOf("/") + 1);
                    NameValueCollection parameters = req.QueryString;

                    if (string.IsNullOrEmpty(function))
                        function = "__Index";

                    if (function == "__Styles")
                    {
                        res.ContentType = "text/css";
                        byte[] styleBytes = encoding.GetBytes(Styles);
                        res.OutputStream.Write(styleBytes, 0, styleBytes.Length);
                    }
                    else
                    {
                        res.ContentType = MediaTypeNames.Text.Html;
                        XmlWriter wr = XmlWriter.Create(res.OutputStream, CreateSettings(ConformanceLevel.Document));
                        writePage(function, parameters, wr);
                        wr.Close();
                    }
                }

                res.OutputStream.Close();
                res.Close();
            }
        }
        #endregion

        #region Built-In Methods
        /// <summary>
        /// Shows all methods.
        /// </summary>
        /// <param name="writer">A <see cref="System.Xml.XmlWriter"/>.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), HttpServerMethod(true, false), CLSCompliant(false)]
        public void __Index(XmlWriter writer)
        {
            writer.WriteStartElement("ul");
            foreach (HttpServerMethod m in this.m_functions.Values)
            {
                if (m.ShowInIndex)
                {
                    writer.WriteStartElement("li");
                    writer.WriteStartElement("a");
                    writer.WriteAttributeString("href", "__View?operation=" + m.Name);
                    writer.WriteString(m.Name);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Displays infomation about a method.
        /// </summary>
        /// <param name="writer">A <see cref="System.Xml.XmlWriter"/>.</param>
        /// <param name="operation">The name of the method</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), HttpServerMethod(true, false), CLSCompliant(false)]
        public void __View(XmlWriter writer, string operation)
        {
            if (!this.m_functions.ContainsKey(operation))
            {
                WriteError(writer, "No such method exists.");
                return;
            }

            HttpServerMethod m = this.m_functions[operation];

            writer.WriteElementString("h3", m.Name);

            writer.WriteStartElement("form");
            writer.WriteAttributeString("method", "get");
            writer.WriteAttributeString("action", m.Name);

            if (!(m.Parameters.Count == 0))
            {
                writer.WriteStartElement("table");

                writer.WriteStartElement("tr");
                writer.WriteElementString("th", "Name");
                writer.WriteElementString("th", "Type");
                writer.WriteElementString("th", "Value");
                writer.WriteEndElement();
                foreach (HttpServerMethodParameter p in m.Parameters)
                {
                    writer.WriteStartElement("tr");

                    writer.WriteElementString("td", p.Name);
                    writer.WriteElementString("td", p.Type.ToString());

                    writer.WriteStartElement("td");
                    writer.WriteStartElement("input");
                    writer.WriteAttributeString("type", "text");
                    writer.WriteAttributeString("id", p.Name);
                    writer.WriteAttributeString("name", p.Name);
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            writer.WriteStartElement("input");
            writer.WriteAttributeString("type", "submit");
            writer.WriteEndElement();

            writer.WriteEndElement();

        }

        private static XmlWriterSettings CreateSettings(ConformanceLevel conformanceLevel)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CloseOutput = false;
            settings.ConformanceLevel = conformanceLevel;
            settings.Encoding = encoding;
            settings.Indent = true;
            return settings;
        }
        #endregion

        #region Document Writing
        private void writePage(string methodName, NameValueCollection query, XmlWriter writer)
        {
            if (!this.m_functions.ContainsKey(methodName))
            {
                WritePageStart(writer, this.ServerName);
                WriteHeader(writer, this.ServerName, methodName, PathPrefix);
                writer.WriteStartElement("div");
                writer.WriteAttributeString("id", "content");
                WriteError(writer, "Invaild function name.");
                writer.WriteEndElement();
                WriteFooter(writer);
                WritePageEnd(writer);
                return;
            }
            HttpServerMethod method = m_functions[methodName];
            if (method.CreatePageTemplate)
            {
                WritePageStart(writer, this.ServerName);
                WriteHeader(writer, this.ServerName, methodName, PathPrefix);
                writer.WriteStartElement("div");
                writer.WriteAttributeString("id", "content");
                writeContent(method, query, writer);
                writer.WriteEndElement();
                WriteFooter(writer);
                WritePageEnd(writer);
            }
            else
            {
                writeContent(method, query, writer);
            }
        }

        /// <summary>
        /// Writes the start of the html document and the head section.
        /// </summary>
        /// <param name="writer">A <see cref="System.Xml.XmlWriter"/>.</param>
        /// <param name="title">The title of the page.</param>
        public static void WritePageStart(XmlWriter writer, string title)
        {
            writer.WriteStartDocument();
            writer.WriteDocType("html", "-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", null);
            writer.WriteStartElement("html", "http://www.w3.org/1999/xhtml");

            writer.WriteStartElement("head");
            writer.WriteElementString("title", title);
            writer.WriteStartElement("link");
            writer.WriteAttributeString("rel", "stylesheet");
            writer.WriteAttributeString("type", "text/css");
            writer.WriteAttributeString("href", "__Styles");
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement("body");
        }

        /// <summary>
        /// Closes the body and html tags.
        /// </summary>
        /// <param name="writer">A <see cref="System.Xml.XmlWriter"/>.</param>
        public static void WritePageEnd(XmlWriter writer)
        {
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void writeContent(HttpServerMethod method, NameValueCollection query, XmlWriter writer)
        {
            try
            {
                StringBuilder content = new StringBuilder();
                XmlWriter wrc = XmlWriter.Create(content, CreateSettings(ConformanceLevel.Fragment));

                List<object> parameters = new List<object>();
                parameters.Add(wrc);
                foreach (HttpServerMethodParameter p in method.Parameters)
                {
                    string value = query[p.Name];
                    if (value == null)
                        throw new HttpServerException("Parameter missing.");

                    switch (p.Type)
                    {
                        case HttpServerMethodParameterType.Int32:
                            parameters.Add(Convert.ToInt32(value, CultureInfo.InvariantCulture));
                            break;
                        case HttpServerMethodParameterType.String:
                            parameters.Add(value);
                            break;
                    }
                }

                method.Invoke(this, parameters);

                wrc.Close();
                writer.WriteRaw(content.ToString());
            }
            catch (Exception ex)
            {
                writer.WriteStartElement("pre");
                WriteError(writer, ex.ToString());
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes a header contain the name of the server and the name of the method.
        /// </summary>
        /// <param name="writer">A <see cref="System.Xml.XmlWriter"/>.</param>
        /// <param name="title">The title of the page.</param>
        /// <param name="method">The name of the method.</param>
        /// <param name="indexPagePath">The page to the index page.</param>
        public static void WriteHeader(XmlWriter writer, string title, string method, string indexPagePath)
        {
            writer.WriteStartElement("div");
            writer.WriteAttributeString("id", "Header");

            writer.WriteStartElement("h1");
            writer.WriteStartElement("a");
            writer.WriteAttributeString("href", indexPagePath);
            writer.WriteString(title);
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteElementString("h2", method.Replace("__", string.Empty));

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a red error message.
        /// </summary>
        /// <param name="writer">A <see cref="System.Xml.XmlWriter"/>.</param>
        /// <param name="errorMessage">The error message to write.</param>
        public static void WriteError(XmlWriter writer, string errorMessage)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            //write error message
            writer.WriteStartElement("p");
            writer.WriteAttributeString("class", "Error");
            writer.WriteString(errorMessage);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a footer contain the server version and copyright.
        /// </summary>
        /// <param name="writer">A <see cref="System.Xml.XmlWriter"/>.</param>
        public static void WriteFooter(XmlWriter writer)
        {
            writer.WriteStartElement("div");
            writer.WriteAttributeString("id", "Bottom");
            writer.WriteElementString("hr", string.Empty);
            writer.WriteString(string.Format(CultureInfo.InvariantCulture, "{0} V{1}", ServerType, ServerVersion));
            writer.WriteElementString("br", string.Empty);
            writeCopyright(writer);
            writer.WriteEndElement();
        }

        private static void writeCopyright(XmlWriter writer)
        {
            writer.WriteString(new string(new char[] { (char)169 }));
            writer.WriteString(" ");
            writer.WriteString(Copyright);
        }
        #endregion

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)this.m_server).Dispose();
        }
    }
}
