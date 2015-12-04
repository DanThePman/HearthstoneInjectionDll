using System;

public abstract class Version
{
    public const string androidTextureCompression = "";
    private static string bobNetAddress_;
    public const int clientChangelist = 0x217c2;
    private static int clientChangelist_;
    public const string cosmeticVersion = "4.1";
    private static string report_;
    private static string serverChangelist_;
    public const int version = 0x2acc;

    protected Version()
    {
    }

    private static void createReport()
    {
        object[] args = new object[] { 0x2acc, 0x217c2, serverChangelist, bobNetAddress };
        report_ = string.Format("Version {0} (client {1}{2}{3})", args);
    }

    public static void Reset()
    {
        report_ = string.Empty;
    }

    public static string bobNetAddress
    {
        get
        {
            if (bobNetAddress_ == null)
            {
            }
            return string.Empty;
        }
        set
        {
            bobNetAddress_ = string.Format(", Battle.net {0}", value);
            Reset();
        }
    }

    public static string FullReport
    {
        get
        {
            if (string.IsNullOrEmpty(report_))
            {
                createReport();
            }
            return report_;
        }
    }

    public static string serverChangelist
    {
        get
        {
            if (serverChangelist_ == null)
            {
            }
            return string.Empty;
        }
        set
        {
            serverChangelist_ = string.Format(", server {0}", value);
            Reset();
        }
    }
}

