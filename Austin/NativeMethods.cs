using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Austin
{
    internal static class NativeMethods
    {
        #region Functions
        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetGetCookie(
          string url, string name, StringBuilder data, ref int dataSize);

        [DllImport("netapi32.dll", EntryPoint = "NetServerEnum")]
        public static extern int NetServerEnum([MarshalAs(UnmanagedType.LPWStr)]string servername,
           int level,
           out IntPtr bufptr,
           int prefmaxlen,
           ref int entriesread,
           ref int totalentries,
           Austin.Net.SV_101_TYPES servertype,
           [MarshalAs(UnmanagedType.LPWStr)]string domain,
           int resume_handle);

        /// <summary>Free the buffer (NT)</summary>
        [DllImport("netapi32.dll", EntryPoint = "NetApiBufferFree")]
        public static extern int
            NetApiBufferFree(IntPtr buffer);

        [DllImport("Netapi32", CharSet = CharSet.Unicode)]
        public static extern int NetMessageBufferSend(
            string servername,
            string msgname,
            string fromname,
            string buf,
            int buflen);

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int NetShareEnum(
             StringBuilder ServerName,
             int level,
             ref IntPtr bufPtr,
             uint prefmaxlen,
             ref int entriesread,
             ref int totalentries,
             ref int resume_handle
             );

        /// <summary>Get a UNC name</summary>
        [DllImport("mpr", CharSet = CharSet.Auto)]
        public static extern int WNetGetUniversalName(string lpLocalPath,
            int dwInfoLevel, ref UNIVERSAL_NAME_INFO lpBuffer, ref int lpBufferSize);

        /// <summary>Get a UNC name</summary>
        [DllImport("mpr", CharSet = CharSet.Auto)]
        public static extern int WNetGetUniversalName(string lpLocalPath,
            int dwInfoLevel, IntPtr lpBuffer, ref int lpBufferSize);

        /// <summary>Enumerate shares (NT)</summary>
        [DllImport("netapi32", CharSet = CharSet.Unicode)]
        public static extern int NetShareEnum(string lpServerName, int dwLevel,
            out IntPtr lpBuffer, int dwPrefMaxLen, out int entriesRead,
            out int totalEntries, ref int hResume);

        /// <summary>Enumerate shares (9x)</summary>
        [DllImport("svrapi", CharSet = CharSet.Ansi)]
        public static extern int NetShareEnum(
            [MarshalAs(UnmanagedType.LPTStr)] string lpServerName, int dwLevel,
            IntPtr lpBuffer, ushort cbBuffer, out ushort entriesRead,
            out ushort totalEntries);
        #endregion

        #region Structures
        /// <summary>Unc name</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct UNIVERSAL_NAME_INFO
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpUniversalName;
        }

        /// <summary>Share information, NT, level 2</summary>
        /// <remarks>
        /// Requires admin rights to work. 
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHARE_INFO_2
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NetName;
            public Austin.Net.ShareType ShareType;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Remark;
            public int Permissions;
            public int MaxUsers;
            public int CurrentUsers;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Path;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Password;
        }

        /// <summary>Share information, NT, level 1</summary>
        /// <remarks>
        /// Fallback when no admin rights.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHARE_INFO_1
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NetName;
            public Austin.Net.ShareType ShareType;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Remark;
        }

        /// <summary>Share information, Win9x</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct SHARE_INFO_50
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
            public string NetName;

            public byte bShareType;
            public ushort Flags;

            [MarshalAs(UnmanagedType.LPTStr)]
            public string Remark;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string Path;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
            public string PasswordRW;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
            public string PasswordRO;

            public Austin.Net.ShareType ShareType
            {
                get { return (Austin.Net.ShareType)((int)bShareType & 0x7F); }
            }
        }

        /// <summary>Share information level 1, Win9x</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct SHARE_INFO_1_9x
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
            public string NetName;
            public byte Padding;

            public ushort bShareType;

            [MarshalAs(UnmanagedType.LPTStr)]
            public string Remark;

            public Austin.Net.ShareType ShareType
            {
                get { return (Austin.Net.ShareType)((int)bShareType & 0x7FFF); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SERVER_INFO_101
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 sv101_platform_id;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string sv101_name;

            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 sv101_version_major;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 sv101_version_minor;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 sv101_type;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string sv101_comment;
        }
        #endregion
    }
}
