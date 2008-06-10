using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace Austin.Net
{
    internal class HttpServerMethodParameter
    {
        private static List<Type> goodTypes = new List<Type>();
        static HttpServerMethodParameter()
        {
            goodTypes.Add(typeof(string));
            goodTypes.Add(typeof(int));
        }

        public HttpServerMethodParameter(ParameterInfo param)
            : base()
        {
            if (!goodTypes.Contains(param.ParameterType))
                throw new HttpServerException(String.Format(CultureInfo.InvariantCulture, "{0} is an invalid parameter type.", param.Name));
            m_name = param.Name;
            Type t = param.ParameterType;
            if (t == typeof(string))
                this.m_type = HttpServerMethodParameterType.String;
            if (t == typeof(int))
                this.m_type = HttpServerMethodParameterType.Int32;
        }

        private string m_name;
        private HttpServerMethodParameterType m_type;

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public HttpServerMethodParameterType Type
        {
            get
            {
                return m_type;
            }
        }
    }
}
