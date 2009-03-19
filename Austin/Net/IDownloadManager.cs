using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Austin.Net
{
    public interface IDownloadManager
    {
        Stream DownloadStream(string address);
        Stream DownloadStream(Uri address);
        Stream DownloadStream(DownloadRequest request);

        byte[] DownloadData(string address);
        byte[] DownloadData(Uri address);
        byte[] DownloadData(DownloadRequest request);

        string DownloadString(string address);
        string DownloadString(Uri address);
        string DownloadString(DownloadRequest request);

        void AddToBlacklist(Uri url);
        void AddSpecialCase(string domain, UrlTransformation specialCase);
    }
}
