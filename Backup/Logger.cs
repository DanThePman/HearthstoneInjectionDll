using System;
using System.IO;
using System.Text;
using UnityEngine;

public class Logger
{
    private StreamWriter m_fileWriter;
    private bool m_fileWriterInitialized;
    private string m_name;
    private const string OUTPUT_DIRECTORY_NAME = "Logs";
    private const string OUTPUT_FILE_EXTENSION = "log";

    public Logger(string name)
    {
        this.m_name = name;
    }

    public bool CanPrint()
    {
        LogInfo logInfo = Log.Get().GetLogInfo(this.m_name);
        if (logInfo == null)
        {
            return false;
        }
        return (logInfo.m_consolePrinting || (logInfo.m_screenPrinting || logInfo.m_filePrinting));
    }

    public bool CanPrint(LogTarget target, LogLevel level, bool verbose)
    {
        LogInfo logInfo = Log.Get().GetLogInfo(this.m_name);
        if (logInfo != null)
        {
            if (level < logInfo.m_minLevel)
            {
                return false;
            }
            if (verbose && !logInfo.m_verbose)
            {
                return false;
            }
            switch (target)
            {
                case LogTarget.CONSOLE:
                    return logInfo.m_consolePrinting;

                case LogTarget.SCREEN:
                    return logInfo.m_screenPrinting;

                case LogTarget.FILE:
                    return logInfo.m_filePrinting;
            }
        }
        return false;
    }

    public void ConsolePrint(string format, params object[] args)
    {
        LogLevel defaultLevel = this.GetDefaultLevel();
        this.ConsolePrint(defaultLevel, false, format, args);
    }

    public void ConsolePrint(LogLevel level, bool verbose, string message)
    {
        if (this.CanPrint(LogTarget.CONSOLE, level, verbose))
        {
            string str = string.Format("[{0}] {1}", this.m_name, message);
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(str);
                    break;

                case LogLevel.Warning:
                    Debug.LogWarning(str);
                    break;

                case LogLevel.Error:
                    Debug.LogError(str);
                    break;
            }
        }
    }

    public void ConsolePrint(LogLevel level, string format, params object[] args)
    {
        this.ConsolePrint(level, false, format, args);
    }

    public void ConsolePrint(bool verbose, string format, params object[] args)
    {
        LogLevel defaultLevel = this.GetDefaultLevel();
        this.ConsolePrint(defaultLevel, verbose, format, args);
    }

    public void ConsolePrint(LogLevel level, bool verbose, string format, params object[] args)
    {
        string message = GeneralUtils.SafeFormat(format, args);
        this.ConsolePrint(level, verbose, message);
    }

    public void FilePrint(string format, params object[] args)
    {
        LogLevel defaultLevel = this.GetDefaultLevel();
        this.FilePrint(defaultLevel, false, format, args);
    }

    public void FilePrint(LogLevel level, bool verbose, string message)
    {
        if (this.CanPrint(LogTarget.FILE, level, verbose))
        {
            this.InitFileWriter();
            if (this.m_fileWriter != null)
            {
                StringBuilder builder = new StringBuilder();
                switch (level)
                {
                    case LogLevel.Debug:
                        builder.Append("D ");
                        break;

                    case LogLevel.Info:
                        builder.Append("I ");
                        break;

                    case LogLevel.Warning:
                        builder.Append("W ");
                        break;

                    case LogLevel.Error:
                        builder.Append("E ");
                        break;
                }
                builder.Append(DateTime.Now.TimeOfDay.ToString());
                builder.Append(" ");
                builder.Append(message);
                this.m_fileWriter.WriteLine(builder.ToString());
                this.m_fileWriter.Flush();
            }
        }
    }

    public void FilePrint(LogLevel level, string format, params object[] args)
    {
        this.FilePrint(level, false, format, args);
    }

    public void FilePrint(bool verbose, string format, params object[] args)
    {
        LogLevel defaultLevel = this.GetDefaultLevel();
        this.FilePrint(defaultLevel, verbose, format, args);
    }

    public void FilePrint(LogLevel level, bool verbose, string format, params object[] args)
    {
        string message = GeneralUtils.SafeFormat(format, args);
        this.FilePrint(level, verbose, message);
    }

    public LogLevel GetDefaultLevel()
    {
        LogInfo logInfo = Log.Get().GetLogInfo(this.m_name);
        if (logInfo == null)
        {
            return LogLevel.Debug;
        }
        return logInfo.m_defaultLevel;
    }

    private void InitFileWriter()
    {
        if (!this.m_fileWriterInitialized)
        {
            this.m_fileWriter = null;
            string path = "Logs";
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception)
                {
                }
            }
            string str2 = string.Format("{0}/{1}.{2}", path, this.m_name, "log");
            try
            {
                this.m_fileWriter = new StreamWriter(new FileStream(str2, FileMode.Create, FileAccess.ReadWrite));
            }
            catch (Exception)
            {
            }
            this.m_fileWriterInitialized = true;
        }
    }

    public bool IsVerbose()
    {
        LogInfo logInfo = Log.Get().GetLogInfo(this.m_name);
        if (logInfo == null)
        {
            return false;
        }
        return logInfo.m_verbose;
    }

    public void Print(string format, params object[] args)
    {
        LogLevel defaultLevel = this.GetDefaultLevel();
        this.Print(defaultLevel, false, format, args);
    }

    public void Print(LogLevel level, bool verbose, string message)
    {
        this.FilePrint(level, verbose, message);
        this.ConsolePrint(level, verbose, message);
        this.ScreenPrint(level, verbose, message);
    }

    public void Print(LogLevel level, string format, params object[] args)
    {
        this.Print(level, false, format, args);
    }

    public void Print(bool verbose, string format, params object[] args)
    {
        LogLevel defaultLevel = this.GetDefaultLevel();
        this.Print(defaultLevel, verbose, format, args);
    }

    public void Print(LogLevel level, bool verbose, string format, params object[] args)
    {
        string message = GeneralUtils.SafeFormat(format, args);
        this.Print(level, verbose, message);
    }

    public void PrintDebug(string format, params object[] args)
    {
        this.PrintDebug(false, format, args);
    }

    public void PrintDebug(bool verbose, string format, params object[] args)
    {
        this.Print(LogLevel.Debug, verbose, format, args);
    }

    public void PrintError(string format, params object[] args)
    {
        this.PrintError(false, format, args);
    }

    public void PrintError(bool verbose, string format, params object[] args)
    {
        this.Print(LogLevel.Error, verbose, format, args);
    }

    public void PrintInfo(string format, params object[] args)
    {
        this.PrintInfo(false, format, args);
    }

    public void PrintInfo(bool verbose, string format, params object[] args)
    {
        this.Print(LogLevel.Info, verbose, format, args);
    }

    public void PrintWarning(string format, params object[] args)
    {
        this.PrintWarning(false, format, args);
    }

    public void PrintWarning(bool verbose, string format, params object[] args)
    {
        this.Print(LogLevel.Warning, verbose, format, args);
    }

    public void ScreenPrint(string format, params object[] args)
    {
        LogLevel defaultLevel = this.GetDefaultLevel();
        this.ScreenPrint(defaultLevel, false, format, args);
    }

    public void ScreenPrint(LogLevel level, bool verbose, string message)
    {
        if (this.CanPrint(LogTarget.SCREEN, level, verbose) && (SceneDebugger.Get() != null))
        {
            string str = string.Format("[{0}] {1}", this.m_name, message);
            SceneDebugger.Get().AddMessage(str);
        }
    }

    public void ScreenPrint(LogLevel level, string format, params object[] args)
    {
        this.ScreenPrint(level, false, format, args);
    }

    public void ScreenPrint(bool verbose, string format, params object[] args)
    {
        LogLevel defaultLevel = this.GetDefaultLevel();
        this.ScreenPrint(defaultLevel, verbose, format, args);
    }

    public void ScreenPrint(LogLevel level, bool verbose, string format, params object[] args)
    {
        string message = GeneralUtils.SafeFormat(format, args);
        this.ScreenPrint(level, verbose, message);
    }
}

