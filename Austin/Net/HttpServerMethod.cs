using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace Austin.Net
{
	internal class HttpServerMethod
	{

		public HttpServerMethod(MethodInfo m)
			: base()
		{
			HttpServerMethodAttribute a = (HttpServerMethodAttribute)m.GetCustomAttributes(typeof(HttpServerMethodAttribute), true)[0];

			if ((!(m.ReturnType.FullName == "System.Void")) || (m.GetParameters().Length == 0))
				throw new HttpServerException(string.Format(CultureInfo.InvariantCulture, "{0} is an invalid method.", m.Name));

			int i = 0;
			foreach (ParameterInfo p in m.GetParameters())
			{
				if (i == 0)
				{
					if (!(p.ParameterType == typeof(System.Xml.XmlWriter)))
						throw new HttpServerException(string.Format(CultureInfo.InvariantCulture, "{0} is an invalid method.  The first parameter of the method must be a XmlTextWriter.", m.Name));
				}
				else
				{
					this.m_parameters.Add(new HttpServerMethodParameter(p));
				}
				i++;
			}
			this.m_method = m;
			this.m_name = m.Name;
			this.m_showInIndex = a.ShowInIndex;
			this.m_createPageTemplate = a.CreatePageTemplate;
		}

		private bool m_showInIndex;
		private MethodInfo m_method;
		private string m_name;
		private bool m_createPageTemplate;
		private List<HttpServerMethodParameter> m_parameters = new List<HttpServerMethodParameter>();


		public object Invoke(object obj, object[] parameters)
		{
			return this.m_method.Invoke(obj, parameters);
		}

		public object Invoke(object obj, List<object> parameters)
		{
			return this.Invoke(obj, parameters.ToArray());
		}

		public bool ShowInIndex
		{
			get
			{
				return this.m_showInIndex;
			}
		}

		public string Name
		{
			get
			{
				return m_name;
			}
		}

		public List<HttpServerMethodParameter> Parameters
		{
			get
			{
				return m_parameters;
			}
		}

		public bool CreatePageTemplate
		{
			get
			{
				return this.m_createPageTemplate;
			}
		}
	}
}
