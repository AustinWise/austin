using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;

namespace Austin.Net
{
    /// <summary>
    /// Represents a network share on a computer.
    /// </summary>
    sealed public class Share
    {
        #region Instance
        #region Private data

        private string _server;
        private string _netName;
        private string _path;
        private ShareType _shareType;
        private string _remark;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Share(string server, string netName, string path, ShareType shareType, string remark)
        {
            if (ShareType.Special == shareType && "IPC$" == netName)
            {
                shareType |= ShareType.IPC;
            }

            _server = server;
            _netName = netName;
            _path = path;
            _shareType = shareType;
            _remark = remark;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The name of the computer that this share belongs to
        /// </summary>
        public string Server
        {
            get { return _server; }
        }

        /// <summary>
        /// Share name
        /// </summary>
        public string NetName
        {
            get { return _netName; }
        }

        /// <summary>
        /// Local path
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Share type
        /// </summary>
        public ShareType ShareType
        {
            get { return _shareType; }
        }

        /// <summary>
        /// Comment
        /// </summary>
        public string Remark
        {
            get { return _remark; }
        }

        /// <summary>
        /// Returns true if this is a file system share
        /// </summary>
        public bool IsFileSystem
        {
            get
            {
                // Shared device
                if (0 != (_shareType & ShareType.Device)) return false;
                // IPC share
                if (0 != (_shareType & ShareType.IPC)) return false;
                // Shared printer
                if (0 != (_shareType & ShareType.Printer)) return false;

                // Standard disk share
                if (0 == (_shareType & ShareType.Special)) return true;

                // Special disk share (e.g. C$)
                if (ShareType.Special == _shareType && null != _netName && 0 != _netName.Length)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Get the root of a disk-based share
        /// </summary>
        public DirectoryInfo Root
        {
            get
            {
                if (IsFileSystem)
                {
                    if (null == _server || 0 == _server.Length)
                        if (null == _path || 0 == _path.Length)
                            return new DirectoryInfo(ToString());
                        else
                            return new DirectoryInfo(_path);
                    else
                        return new DirectoryInfo(ToString());
                }
                else
                    return null;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Returns the path to this share
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (null == _server || 0 == _server.Length)
            {
                return string.Format(CultureInfo.InvariantCulture, @"\\{0}\{1}", Environment.MachineName, _netName);
            }
            else
                return string.Format(CultureInfo.InvariantCulture, @"\\{0}\{1}", _server, _netName);
        }

        /// <summary>
        /// Returns true if this share matches the local path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool MatchesPath(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!IsFileSystem) return false;

            return path.ToLowerInvariant().StartsWith(_path.ToLowerInvariant());
        }
        #endregion
        #endregion

        #region Static
        #region Platform

        /// <summary>
        /// Is this an NT platform?
        /// </summary>
        private static bool IsNT
        {
            get { return (PlatformID.Win32NT == Environment.OSVersion.Platform); }
        }

        /// <summary>
        /// Returns true if this is Windows 2000 or higher
        /// </summary>
        private static bool IsW2KUp
        {
            get
            {
                OperatingSystem os = Environment.OSVersion;
                if (PlatformID.Win32NT == os.Platform && os.Version.Major >= 5)
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #region Interop

        #region Constants

        /// <summary>Maximum path length</summary>
        private const int MAX_PATH = 260;
        /// <summary>No error</summary>
        private const int NO_ERROR = 0;
        /// <summary>Access denied</summary>
        private const int ERROR_ACCESS_DENIED = 5;
        /// <summary>Access denied</summary>
        private const int ERROR_WRONG_LEVEL = 124;
        /// <summary>More data available</summary>
        private const int ERROR_MORE_DATA = 234;
        /// <summary>Not connected</summary>
        private const int ERROR_NOT_CONNECTED = 2250;
        /// <summary>Level 1</summary>
        private const int UNIVERSAL_NAME_INFO_LEVEL = 1;
        /// <summary>Max extries (9x)</summary>
        private const int MAX_SI50_ENTRIES = 20;

        #endregion

        #region Enumerate shares

        /// <summary>
        /// Enumerates the shares on Windows NT
        /// </summary>
        /// <param name="server">The server name</param>
        private static List<Share> EnumerateSharesNT(string server)
        {
            List<Share> shares = new List<Share>();

            int level = 2;
            int entriesRead, totalEntries, nRet, hResume = 0;
            IntPtr pBuffer = IntPtr.Zero;

            try
            {
                nRet = Austin.NativeMethods.NetShareEnum(server, level, out pBuffer, -1,
                    out entriesRead, out totalEntries, ref hResume);

                if (ERROR_ACCESS_DENIED == nRet)
                {
                    //Need admin for level 2, drop to level 1
                    level = 1;
                    nRet = Austin.NativeMethods.NetShareEnum(server, level, out pBuffer, -1,
                        out entriesRead, out totalEntries, ref hResume);
                }

                if (NO_ERROR == nRet && entriesRead > 0)
                {
                    Type t = (2 == level) ? typeof(Austin.NativeMethods.SHARE_INFO_2) : typeof(Austin.NativeMethods.SHARE_INFO_1);
                    int offset = Marshal.SizeOf(t);

                    for (int i = 0, lpItem = pBuffer.ToInt32(); i < entriesRead; i++, lpItem += offset)
                    {
                        IntPtr pItem = new IntPtr(lpItem);
                        if (1 == level)
                        {
                            Austin.NativeMethods.SHARE_INFO_1 si = (Austin.NativeMethods.SHARE_INFO_1)Marshal.PtrToStructure(pItem, t);
                            shares.Add(new Share(server, si.NetName, string.Empty, si.ShareType, si.Remark));
                        }
                        else
                        {
                            Austin.NativeMethods.SHARE_INFO_2 si = (Austin.NativeMethods.SHARE_INFO_2)Marshal.PtrToStructure(pItem, t);
                            shares.Add(new Share(server, si.NetName, si.Path, si.ShareType, si.Remark));
                        }
                    }
                }

            }
            finally
            {
                // Clean up buffer allocated by system
                if (IntPtr.Zero != pBuffer)
                    Austin.NativeMethods.NetApiBufferFree(pBuffer);
            }
            return shares;
        }

        /// <summary>
        /// Enumerates the shares on Windows 9x
        /// </summary>
        /// <param name="server">The server name</param>
        private static List<Share> EnumerateShares9x(string server)
        {
            List<Share> shares = new List<Share>();

            int level = 50;
            int nRet = 0;
            ushort entriesRead, totalEntries;

            Type t = typeof(Austin.NativeMethods.SHARE_INFO_50);
            int size = Marshal.SizeOf(t);
            ushort cbBuffer = (ushort)(MAX_SI50_ENTRIES * size);
            //On Win9x, must allocate buffer before calling API
            IntPtr pBuffer = Marshal.AllocHGlobal(cbBuffer);

            try
            {
                nRet = Austin.NativeMethods.NetShareEnum(server, level, pBuffer, cbBuffer,
                    out entriesRead, out totalEntries);

                if (ERROR_WRONG_LEVEL == nRet)
                {
                    level = 1;
                    t = typeof(Austin.NativeMethods.SHARE_INFO_1_9x);
                    size = Marshal.SizeOf(t);

                    nRet = Austin.NativeMethods.NetShareEnum(server, level, pBuffer, cbBuffer,
                        out entriesRead, out totalEntries);
                }

                if (NO_ERROR == nRet || ERROR_MORE_DATA == nRet)
                {
                    for (int i = 0, lpItem = pBuffer.ToInt32(); i < entriesRead; i++, lpItem += size)
                    {
                        IntPtr pItem = new IntPtr(lpItem);

                        if (1 == level)
                        {
                            Austin.NativeMethods.SHARE_INFO_1_9x si = (Austin.NativeMethods.SHARE_INFO_1_9x)Marshal.PtrToStructure(pItem, t);
                            shares.Add(new Share(server, si.NetName, string.Empty, si.ShareType, si.Remark));
                        }
                        else
                        {
                            Austin.NativeMethods.SHARE_INFO_50 si = (Austin.NativeMethods.SHARE_INFO_50)Marshal.PtrToStructure(pItem, t);
                            shares.Add(new Share(server, si.NetName, si.Path, si.ShareType, si.Remark));
                        }
                    }
                }
                else
                    Console.WriteLine(nRet);

            }
            finally
            {
                //Clean up buffer
                Marshal.FreeHGlobal(pBuffer);
            }
            return shares;
        }

        /// <summary>
        /// Enumerates the shares
        /// </summary>
        /// <param name="server">The server name</param>
        private static List<Share> EnumerateShares(string server)
        {
            List<Share> shares = new List<Share>();

            if (null != server && 0 != server.Length && !IsW2KUp)
            {
                server = server.ToUpperInvariant();

                // On NT4, 9x and Me, server has to start with "\\"
                if (!('\\' == server[0] && '\\' == server[1]))
                    server = @"\\" + server;
            }

            if (IsNT)
                shares = EnumerateSharesNT(server);
            else
                shares = EnumerateShares9x(server);

            return shares;
        }

        #endregion

        #endregion

        #region Static methods

        ///// <summary>
        ///// Returns true if fileName is a valid local file-name of the form:
        ///// X:\, where X is a drive letter from A-Z
        ///// </summary>
        ///// <param name="fileName">The filename to check</param>
        ///// <returns></returns>
        //public static bool IsValidFilePath(string fileName)
        //{
        //    if (null == fileName || 0 == fileName.Length) return false;

        //    char drive = char.ToUpper(fileName[0]);
        //    if ('A' > drive || drive > 'Z')
        //        return false;

        //    else if (System.IO.Path.VolumeSeparatorChar != fileName[1])
        //        return false;
        //    else if (System.IO.Path.DirectorySeparatorChar != fileName[2])
        //        return false;
        //    else
        //        return true;
        //}

        ///// <summary>
        ///// Returns the UNC path for a mapped drive or local share.
        ///// </summary>
        ///// <param name="fileName">The path to map</param>
        ///// <returns>The UNC path (if available)</returns>
        //public static string PathToUnc(string fileName)
        //{
        //    if (null == fileName || 0 == fileName.Length) return string.Empty;

        //    fileName = System.IO.Path.GetFullPath(fileName);
        //    if (!IsValidFilePath(fileName)) return fileName;

        //    int nRet = 0;
        //    UNIVERSAL_NAME_INFO rni = new UNIVERSAL_NAME_INFO();
        //    int bufferSize = Marshal.SizeOf(rni);

        //    nRet = WNetGetUniversalName(
        //        fileName, UNIVERSAL_NAME_INFO_LEVEL,
        //        ref rni, ref bufferSize);

        //    if (ERROR_MORE_DATA == nRet)
        //    {
        //        IntPtr pBuffer = Marshal.AllocHGlobal(bufferSize); ;
        //        try
        //        {
        //            nRet = WNetGetUniversalName(
        //                fileName, UNIVERSAL_NAME_INFO_LEVEL,
        //                pBuffer, ref bufferSize);

        //            if (NO_ERROR == nRet)
        //            {
        //                rni = (UNIVERSAL_NAME_INFO)Marshal.PtrToStructure(pBuffer,
        //                    typeof(UNIVERSAL_NAME_INFO));
        //            }
        //        }
        //        finally
        //        {
        //            Marshal.FreeHGlobal(pBuffer);
        //        }
        //    }

        //    switch (nRet)
        //    {
        //        case NO_ERROR:
        //            return rni.lpUniversalName;

        //        case ERROR_NOT_CONNECTED:
        //            //Local file-name
        //            List<Share> shi = LocalShares;
        //            if (null != shi)
        //            {
        //                Share share = shi[fileName];
        //                if (null != share)
        //                {
        //                    string path = share.Path;
        //                    if (null != path && 0 != path.Length)
        //                    {
        //                        int index = path.Length;
        //                        if (Path.DirectorySeparatorChar != path[path.Length - 1])
        //                            index++;

        //                        if (index < fileName.Length)
        //                            fileName = fileName.Substring(index);
        //                        else
        //                            fileName = string.Empty;

        //                        fileName = Path.Combine(share.ToString(), fileName);
        //                    }
        //                }
        //            }

        //            return fileName;

        //        default:
        //            Console.WriteLine("Unknown return value: {0}", nRet);
        //            return string.Empty;
        //    }
        //}

        ///// <summary>
        ///// Returns the local <see cref="Share"/> object with the best match
        ///// to the specified path.
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //public static Share PathToShare(string fileName)
        //{
        //    if (null == fileName || 0 == fileName.Length) return null;

        //    fileName = Path.GetFullPath(fileName);
        //    if (!IsValidFilePath(fileName)) return null;

        //    ShareCollection shi = LocalShares;
        //    if (null == shi)
        //        return null;
        //    else
        //        return shi[fileName];
        //}

        #endregion

        #region Local shares

        /// <summary>The local shares</summary>
        private static ReadOnlyCollection<Share> _local;

        /// <summary>
        /// Return the local shares
        /// </summary>
        public static ReadOnlyCollection<Share> LocalShares
        {
            get
            {
                if (null == _local)
                    _local = new ReadOnlyCollection<Share>(EnumerateShares(string.Empty));

                return _local;
            }
        }

        /// <summary>
        /// Return the shares for a specified machine
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static ReadOnlyCollection<Share> GetShares(string server)
        {
            return new ReadOnlyCollection<Share>(EnumerateShares(server));
        }

        #endregion
        #endregion
    }
}
