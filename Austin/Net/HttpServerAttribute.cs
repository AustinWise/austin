using System;
using System.Collections.Generic;
using System.Text;

namespace Austin.Net
{
	/// <summary>
	/// Used to add additional information to an HTTP Server, such as a string describing its functionality.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class HttpServerAttribute : Attribute
	{
		private string m_prefix;
		private string m_name;

		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Net.HttpServerAttribute"></see> class.
		/// </summary>
		/// <param name="name">The name of the HTTP server.</param>
		/// <param name="prefix">The Uniform Resource Identifier (URI) prefix handled by this <see cref="Austin.Net.HttpServer"/> object.</param>
		public HttpServerAttribute(string name, string prefix)
			: base()
		{
			m_name = name;
			m_prefix = prefix;
		}

		/// <summary>
		/// Gets the Uniform Resource Identifier (URI) prefix handled by this <see cref="Austin.Net.HttpServer"/> object.
		/// </summary>
		public string Prefix
		{
			get
			{ return this.m_prefix; }
		}

		/// <summary>
		/// The name of the server.
		/// </summary>
		public string Name
		{
			get
			{ return m_name; }
		}
	}
}
