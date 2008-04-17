using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace Austin.Net
{
	/// <summary>
	/// An exception that represents an error in the http server.
	/// </summary>
	[Serializable()]
	public class HttpServerException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Net.HttpServerException"></see> class.
		/// </summary>
		public HttpServerException()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Net.HttpServerException"></see> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public HttpServerException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Net.HttpServerException"></see> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is cause of the current exception.</param>
		public HttpServerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Net.HttpServerException"></see> class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected HttpServerException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
