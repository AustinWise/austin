using System;
using System.Collections.Generic;
using System.Text;

namespace Austin.Net
{
    /// <summary>
    /// Represents as computer on the network.
    /// </summary>
    public class Computer
    {
        internal Computer(string name, string comment)
        {
            this.m_name = name;
            this.m_comment = comment;
        }

        private string m_name;
        /// <summary>
        /// The name of the computer.
        /// </summary>
        public string Name
        {
            get
            {
                return this.m_name;
            }
        }

        private string m_comment;
        /// <summary>
        /// A note about the computer.
        /// </summary>
        public string Comment
        {
            get
            {
                return this.m_comment;
            }
        }
    }
}
