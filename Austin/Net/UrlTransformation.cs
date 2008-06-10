using System;

namespace Austin.Net
{
	/// <summary>
	/// Represents a method that transforms a URL.
	/// </summary>
	/// <param name="url">The URL to be transformed.</param>
	/// <returns>The transformed URL.</returns>
	public delegate Uri UrlTransformation(Uri url);
}