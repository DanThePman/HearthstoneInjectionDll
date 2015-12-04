using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

internal class LogArchive
{
    [CompilerGenerated]
    private static Comparison<FileInfo> <>f__am$cache5;
    private string m_logPath;
    private int m_maxFileSizeKB = 0x1388;
    private ulong m_numLinesWritten;
    private bool m_stopLogging;
    private static LogArchive s_instance;

    public LogArchive()
    {
        string logFolderPath = FileUtils.PersistentDataPath + "/Logs";
        this.MakeLogPath(logFolderPath);
        try
        {
            Directory.CreateDirectory(logFolderPath);
            this.CleanOldLogs(logFolderPath);
            Application.logMessageReceived += new Application.LogCallback(this.HandleLog);
            Log.Cameron.Print("Logging Unity output to: " + this.m_logPath, new object[0]);
        }
        catch (IOException exception)
        {
            Debug.LogWarning("Failed to write archive logs to: \"" + this.m_logPath + "\"!");
            Debug.LogWarning(exception);
        }
    }

    private void CleanOldLogs(string logFolderPath)
    {
        int num = 5;
        FileInfo[] files = new DirectoryInfo(logFolderPath).GetFiles();
        if (<>f__am$cache5 == null)
        {
            <>f__am$cache5 = (a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime);
        }
        Array.Sort<FileInfo>(files, <>f__am$cache5);
        int num2 = files.Length - (num - 1);
        for (int i = 0; (i < num2) && (i < files.Length); i++)
        {
            files[i].Delete();
        }
    }

    public static LogArchive Get()
    {
        if (s_instance == null)
        {
            s_instance = new LogArchive();
        }
        return s_instance;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (!this.m_stopLogging)
        {
            try
            {
                if ((this.m_numLinesWritten % ((ulong) 100L)) == 0)
                {
                    FileInfo info = new FileInfo(this.m_logPath);
                    if (info.Exists && (info.Length > (this.m_maxFileSizeKB * 0x400)))
                    {
                        this.m_stopLogging = true;
                        using (StreamWriter writer = new StreamWriter(this.m_logPath, true))
                        {
                            Blizzard.Log.SayToFile(writer, string.Empty, new object[0]);
                            Blizzard.Log.SayToFile(writer, string.Empty, new object[0]);
                            Blizzard.Log.SayToFile(writer, "==================================================================", new object[0]);
                            string format = string.Format("Truncating log, which has reached the size limit of {0}KB", this.m_maxFileSizeKB);
                            Blizzard.Log.SayToFile(writer, format, new object[0]);
                            Blizzard.Log.SayToFile(writer, "==================================================================\n\n", new object[0]);
                        }
                        return;
                    }
                }
                using (StreamWriter writer2 = new StreamWriter(this.m_logPath, true))
                {
                    if ((type == LogType.Error) || (type == LogType.Exception))
                    {
                        object[] args = new object[] { logString, stackTrace };
                        Blizzard.Log.SayToFile(writer2, "{0}\n{1}", args);
                    }
                    else
                    {
                        object[] objArray2 = new object[] { logString, stackTrace };
                        Blizzard.Log.SayToFile(writer2, "{0}", objArray2);
                    }
                    this.m_numLinesWritten += (ulong) 1L;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("LogArchive.HandleLog() - Failed to write \"{0}\". Exception={1}", logString, exception.Message));
            }
        }
    }

    public static void Init()
    {
        Get();
    }

    private void MakeLogPath(string logFolderPath)
    {
        if (string.IsNullOrEmpty(this.m_logPath))
        {
            string str = Blizzard.Time.Stamp();
            str = str.Replace("-", "_").Replace(" ", "_").Replace(":", "_").Remove(str.Length - 4);
            string str2 = "hearthstone_" + str + ".log";
            this.m_logPath = logFolderPath + "/" + str2;
        }
    }
}

