using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Austin.Net
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SERVER_INFO_101
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
}
