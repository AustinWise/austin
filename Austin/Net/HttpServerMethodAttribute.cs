using System;
using System.Collections.Generic;
using System.Text;

namespace Austin.Net
{
	/// <summary>
	/// Adding this attribute to a method within an <see cref="Austin.Net.HttpServer"/> makes the method callable from remote Web clients. This class cannot be inherited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class HttpServerMethodAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Net.HttpServerMethodAttribute"/> class.
		/// </summary>
		public HttpServerMethodAttribute()
			: base()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Austin.Net.HttpServerMethodAttribute"/> class that uses no template
        /// and has a custom content type.
        /// </summary>
        /// <param name="customContentType">The MIME type of the response.</param>
        public HttpServerMethodAttribute(string customContentType)
            : this()
        {
            this.m_customContentType = customContentType;
            this.m_createPageTemplate = false;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Net.HttpServerMethodAttribute"/> class.
		/// </summary>
		/// <param name="createPageTemplate">A <see cref="System.Boolean"/> indicating whether a basic HTML document
		/// should be created.</param>
		public HttpServerMethodAttribute(bool createPageTemplate)
			: this()
		{
			this.m_createPageTemplate = createPageTemplate;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Net.HttpServerMethodAttribute"/> class.
		/// </summary>
		/// <param name="createPageTemplate">A <see cref="System.Boolean"/> indicating whether a basic HTML document
		/// should be created.</param>
		/// <param name="showInIndex">A <see cref="System.Boolean"/> indicating whether or not you
		/// want this method displaed in the method index.</param>
		public HttpServerMethodAttribute(bool createPageTemplate, bool showInIndex)
			: this()
		{
			m_showInIndex = showInIndex;
			this.m_createPageTemplate = createPageTemplate;
		}

		private bool m_showInIndex = true;
		/// <summary>
		/// A <see cref="System.Boolean"/> indicating whether or not this method should be displaed in the method index.
		/// </summary>
		public bool ShowInIndex
		{
			get
			{
				return m_showInIndex;
			}
		}

		private bool m_createPageTemplate = true;
        /// <summary>
        /// A <see cref="System.Boolean"/> indicating whether or not the server should create a HTML page around the output of this method.
        /// </summary>
		public bool CreatePageTemplate
		{
			get
			{
				return this.m_createPageTemplate;
			}
		}

        private string m_customContentType = null;
        /// <summary>
        /// The MIME type of the response.
        /// </summary>
        public string CustomContentType
        {
            get
            {
                return this.m_customContentType;
            }
        }
	}
}
