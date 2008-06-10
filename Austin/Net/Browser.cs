using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Runtime.InteropServices;

namespace Austin.Net
{
    /// <summary>
    /// Lets your query the Windows Browser Service.
    /// </summary>
    public static class Browser
    {
        /// <summary>
        /// Lists the computers know to the browser.
        /// </summary>
        /// <returns>The computers found by the browser server.</returns>
        public static ReadOnlyCollection<Computer> ListComputers()
        {
            List<SERVER_INFO_101> rawComputers = GetServerList(SV_101_TYPES.SV_TYPE_ALL);
            List<Computer> computers = new List<Computer>(rawComputers.Count);
            foreach (SERVER_INFO_101 s in rawComputers)
                computers.Add(new Computer(s.sv101_name, s.sv101_comment));

            return new ReadOnlyCollection<Computer>(computers);
        }

        #region Interop Crap
        // constants
        private const uint ERROR_SUCCESS = 0;
        private const uint ERROR_MORE_DATA = 234;

        //private static int NetMessageSend(string serverName, string messageName, string fromName, string strMsgBuffer, int iMsgBufferLen)
        //{
        //    return NativeMethods.NetMessageBufferSend(serverName, messageName, fromName, strMsgBuffer, iMsgBufferLen * 2);
        //}

        private static List<SERVER_INFO_101> GetServerList(SV_101_TYPES ServerType)
        {
            int entriesread = 0, totalentries = 0;
            List<SERVER_INFO_101> alServers = new List<SERVER_INFO_101>();

            do
            {
                // Buffer to store the available servers
                // Filled by the NetServerEnum function
                IntPtr buf = new IntPtr();

                SERVER_INFO_101 server;
                int ret = NativeMethods.NetServerEnum(null, 101, out buf, -1,
                    ref entriesread, ref totalentries,
                    ServerType, null, 0);

                // if the function returned any data, fill the tree view
                if (ret == ERROR_SUCCESS ||
                    ret == ERROR_MORE_DATA ||
                    entriesread > 0)
                {
                    Int32 ptr = buf.ToInt32();

                    for (int i = 0; i < entriesread; i++)
                    {
                        // cast pointer to a SERVER_INFO_101 structure
                        server = (SERVER_INFO_101)Marshal.PtrToStructure(new IntPtr(ptr), typeof(SERVER_INFO_101));

                        ptr += Marshal.SizeOf(server);

                        // add the machine name and comment to the arrayList. 
                        //You could return the entire structure here if desired
                        alServers.Add(server);
                    }
                }

                // free the buffer 
                NativeMethods.NetApiBufferFree(buf);

            }
            while
                (
                entriesread < totalentries &&
                entriesread != 0
                );

            return alServers;
        }
        #endregion
    }
}