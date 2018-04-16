using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Text;
using System.IO;

namespace 浏览器
{
    public enum STATURL_QUERYFLAGS : uint
    {
        STATURL_QUERYFLAG_ISCACHED = 0x00010000,
        STATURL_QUERYFLAG_NOURL = 0x00020000,
        STATURL_QUERYFLAG_NOTITLE = 0x00040000,
        STATURL_QUERYFLAG_TOPLEVEL = 0x00080000,

    }
    public enum STATURLFLAGS : uint
    {
        STATURLFLAG_ISCACHED = 0x00000001,
        STATURLFLAG_ISTOPLEVEL = 0x00000002,
    }
    public enum ADDURL_FLAG : uint
    {
        ADDURL_ADDTOHISTORYANDCACHE = 0,
        ADDURL_ADDTOCACHE = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STATURL
    {
        public int cbSize;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwcsUrl;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwcsTitle;

        public FILETIME ftLastVisited;

        public FILETIME ftLastUpdated;

        public FILETIME ftExpires;

        public STATURLFLAGS dwFlags;


        public string URL
        {
            get { return pwcsUrl; }
        }

        public string Title
        {
            get
            {
                if (pwcsUrl.StartsWith("file:"))
                    return Win32api.CannonializeURL(pwcsUrl, Win32api.shlwapi_URL.URL_UNESCAPE).Substring(8).Replace('/', '\\');
                else
                    return pwcsTitle;
            }
        }

        public DateTime LastVisited
        {
            get
            {
                return Win32api.FileTimeToDateTime(ftLastVisited).ToLocalTime();
            }
        }

        public DateTime LastUpdated
        {
            get
            {
                return Win32api.FileTimeToDateTime(ftLastUpdated).ToLocalTime();
            }
        }

        public DateTime Expires
        {
            get
            {
                try
                {
                    return Win32api.FileTimeToDateTime(ftExpires).ToLocalTime();
                }
                catch (Exception)
                {
                    return DateTime.Now;
                }
            }
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UUID
    {
        public int Data1;
        public short Data2;
        public short Data3;
        public byte[] Data4;
    }

    //Enumerates the cached URLs
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3C374A42-BAE4-11CF-BF7D-00AA006946EE")]
    public interface IEnumSTATURL
    {
        void Next(int celt, ref STATURL rgelt, out int pceltFetched);	//Returns the next \"celt\" URLS from the cache
        void Skip(int celt);	//Skips the next \"celt\" URLS from the cache. doed not work.
        void Reset();	//Resets the enumeration
        void Clone(out IEnumSTATURL ppenum);	//Clones this object
        void SetFilter([MarshalAs(UnmanagedType.LPWStr)] string poszFilter, STATURLFLAGS dwFlags);	//Sets the enumeration filter
    }


    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3C374A41-BAE4-11CF-BF7D-00AA006946EE")]
    public interface IUrlHistoryStg
    {
        void AddUrl(string pocsUrl, string pocsTitle, ADDURL_FLAG dwFlags);	//Adds a new history entry
        void DeleteUrl(string pocsUrl, int dwFlags);	//Deletes an entry by its URL. does not work!
        void QueryUrl([MarshalAs(UnmanagedType.LPWStr)] string pocsUrl, STATURL_QUERYFLAGS dwFlags, ref STATURL lpSTATURL);	//Returns a STATURL for a given URL
        void BindToObject([In] string pocsUrl, [In] UUID riid, IntPtr ppvOut); //Binds to an object. does not work!
        object EnumUrls { [return: MarshalAs(UnmanagedType.IUnknown)] get;}	//Returns an enumerator for URLs
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("AFA0DC11-C313-11D0-831A-00C04FD5AE38")]
    public interface IUrlHistoryStg2 : IUrlHistoryStg
    {
        new void AddUrl(string pocsUrl, string pocsTitle, ADDURL_FLAG dwFlags);	//Adds a new history entry
        new void DeleteUrl(string pocsUrl, int dwFlags);	//Deletes an entry by its URL. does not work!
        new void QueryUrl([MarshalAs(UnmanagedType.LPWStr)] string pocsUrl, STATURL_QUERYFLAGS dwFlags, ref STATURL lpSTATURL);	//Returns a STATURL for a given URL
        new void BindToObject([In] string pocsUrl, [In] UUID riid, IntPtr ppvOut);	//Binds to an object. does not work!
        new object EnumUrls { [return: MarshalAs(UnmanagedType.IUnknown)] get;}	//Returns an enumerator for URLs

        void AddUrlAndNotify(string pocsUrl, string pocsTitle, int dwFlags, int fWriteHistory, object poctNotify, object punkISFolder);//does not work!
        void ClearHistory();	//Removes all history items
    }

    //UrlHistory class
    [ComImport]
    [Guid("3C374A40-BAE4-11CF-BF7D-00AA006946EE")]
    public class UrlHistoryClass
    {
    }

}
namespace 浏览器
{
    public class Win32api
    {
        [Flags]
        public enum shlwapi_URL : uint
        {
            URL_DONT_SIMPLIFY = 0x08000000,
            URL_ESCAPE_PERCENT = 0x00001000,
            URL_ESCAPE_SPACES_ONLY = 0x04000000,
            URL_ESCAPE_UNSAFE = 0x20000000,
            URL_PLUGGABLE_PROTOCOL = 0x40000000,
            URL_UNESCAPE = 0x10000000
        }

        [DllImport("shlwapi.dll")]
        public static extern int UrlCanonicalize(
            string pszUrl,
            StringBuilder pszCanonicalized,
            ref int pcchCanonicalized,
            shlwapi_URL dwFlags
            );


        public static string CannonializeURL(string pszUrl, shlwapi_URL dwFlags)
        {
            StringBuilder buff = new StringBuilder(260);
            int s = buff.Capacity;
            int c = UrlCanonicalize(pszUrl, buff, ref s, dwFlags);
            if (c == 0)
                return buff.ToString();
            else
            {
                buff.Capacity = s;
                c = UrlCanonicalize(pszUrl, buff, ref s, dwFlags);
                return buff.ToString();
            }
        }


        public struct SYSTEMTIME
        {
            public Int16 Year;
            public Int16 Month;
            public Int16 DayOfWeek;
            public Int16 Day;
            public Int16 Hour;
            public Int16 Minute;
            public Int16 Second;
            public Int16 Milliseconds;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool FileTimeToSystemTime
            (ref FILETIME FileTime, ref SYSTEMTIME SystemTime);


        public static DateTime FileTimeToDateTime(FILETIME filetime)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            FileTimeToSystemTime(ref filetime, ref st);
            return new DateTime(st.Year, st.Month, st.Day, st.Hour, st.Minute, st.Second, st.Milliseconds);

        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool SystemTimeToFileTime([In] ref SYSTEMTIME lpSystemTime,
            out FILETIME lpFileTime);


        public static FILETIME DateTimeToFileTime(DateTime datetime)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            st.Year = (short)datetime.Year;
            st.Month = (short)datetime.Month;
            st.Day = (short)datetime.Day;
            st.Hour = (short)datetime.Hour;
            st.Minute = (short)datetime.Minute;
            st.Second = (short)datetime.Second;
            st.Milliseconds = (short)datetime.Millisecond;
            FILETIME filetime;
            SystemTimeToFileTime(ref st, out filetime);
            return filetime;

        }
        [DllImport("Kernel32.dll")]
        public static extern int CompareFileTime([In] ref FILETIME lpFileTime1, [In] ref FILETIME lpFileTime2);

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        public const uint SHGFI_ATTR_SPECIFIED =
            0x20000;
        public const uint SHGFI_ATTRIBUTES = 0x800;
        public const uint SHGFI_PIDL = 0x8;
        public const uint SHGFI_DISPLAYNAME =
            0x200;
        public const uint SHGFI_USEFILEATTRIBUTES
            = 0x10;
        public const uint FILE_ATTRIBUTRE_NORMAL =
            0x4000;
        public const uint SHGFI_EXETYPE = 0x2000;
        public const uint SHGFI_SYSICONINDEX =
            0x4000;
        public const uint ILC_COLORDDB = 0x1;
        public const uint ILC_MASK = 0x0;
        public const uint ILD_TRANSPARENT = 0x1;
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;
        public const uint SHGFI_SHELLICONSIZE =
            0x4;
        public const uint SHGFI_SMALLICON = 0x1;
        public const uint SHGFI_TYPENAME = 0x400;
        public const uint SHGFI_ICONLOCATION =
            0x1000;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    public class SortFileTimeAscendingHelper : IComparer
    {
        [DllImport("Kernel32.dll")]
        static extern int CompareFileTime([In] ref FILETIME lpFileTime1, [In] ref FILETIME lpFileTime2);

        int IComparer.Compare(object a, object b)
        {
            STATURL c1 = (STATURL)a;
            STATURL c2 = (STATURL)b;
            return (CompareFileTime(ref c1.ftLastVisited, ref c2.ftLastVisited));
        }

        public static IComparer SortFileTimeAscending()
        {
            return (IComparer)new SortFileTimeAscendingHelper();
        }
    }
}
namespace 浏览器
{
    public class UrlHistoryWrapperClass
    {

        UrlHistoryClass urlHistory;
        IUrlHistoryStg2 obj;

        public UrlHistoryWrapperClass()
        {
            urlHistory = new UrlHistoryClass();
            obj = (IUrlHistoryStg2)urlHistory;
        }

        public void Dispose()
        {
            Marshal.ReleaseComObject(obj);
            urlHistory = null;
        }

        public void AddHistoryEntry(string pocsUrl, string pocsTitle, ADDURL_FLAG dwFlags)
        {
            obj.AddUrl(pocsUrl, pocsTitle, dwFlags);
        }

        public void DeleteHistoryEntry(string pocsUrl, int dwFlags)
        {

            try
            {
                obj.DeleteUrl(pocsUrl, dwFlags);
            }
            catch (Exception)
            {

            }
        }

        public STATURL QueryUrl(string pocsUrl, STATURL_QUERYFLAGS dwFlags)
        {
            STATURL lpSTATURL = new STATURL();

            try
            {
                obj.QueryUrl(pocsUrl, dwFlags, ref lpSTATURL);
                return lpSTATURL;
            }
            catch (FileNotFoundException)
            {
                return lpSTATURL;
            }
        }

        public void ClearHistory()
        {
            obj.ClearHistory();
        }

        public STATURLEnumerator GetEnumerator()
        {
            return new STATURLEnumerator((IEnumSTATURL)obj.EnumUrls);
        }

        public class STATURLEnumerator
        {
            IEnumSTATURL enumrator;
            int index;
            STATURL staturl;

            public STATURLEnumerator(IEnumSTATURL enumrator)
            {
                this.enumrator = enumrator;
            }
            public bool MoveNext()
            {
                staturl = new STATURL();
                enumrator.Next(1, ref staturl, out index);
                if (index == 0)
                    return false;
                else
                    return true;
            }
            public STATURL Current
            {
                get
                {
                    return staturl;
                }
            }

            public void Skip(int celt)
            {
                enumrator.Skip(celt);
            }
            public void Reset()
            {
                enumrator.Reset();
            }
            public STATURLEnumerator Clone()
            {
                IEnumSTATURL ppenum;
                enumrator.Clone(out ppenum);
                return new STATURLEnumerator(ppenum);

            }
            public void SetFilter(string poszFilter, STATURLFLAGS dwFlags)
            {
                enumrator.SetFilter(poszFilter, dwFlags);
            }
            public void GetUrlHistory(IList list)
            {

                while (true)
                {
                    staturl = new STATURL();
                    enumrator.Next(1, ref staturl, out index);
                    if (index == 0)
                        break;
                    list.Add(staturl);

                }
                enumrator.Reset();
            }
        }
    }
}

