using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class BattleNetLogSource
{
    private string m_sourceName;

    public BattleNetLogSource(string sourceName)
    {
        this.m_sourceName = sourceName;
    }

    private string FormatStacktrace(StackFrame sf, bool fullPath = false)
    {
        if (sf != null)
        {
            string str = !fullPath ? System.IO.Path.GetFileName(sf.GetFileName()) : sf.GetFileName();
            return string.Format(" ({2} at {0}:{1})", str, sf.GetFileLineNumber(), sf.GetMethod());
        }
        return string.Empty;
    }

    public void LogDebug(string message)
    {
        this.LogMessage(LogLevel.Debug, message, true);
    }

    public void LogDebug(string format, params object[] args)
    {
        string message = string.Format(format, args);
        this.LogMessage(LogLevel.Debug, message, true);
    }

    public void LogDebugStackTrace(string message, int maxFrames, int skipFrames = 0)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(message + "\n");
        for (int i = 1 + skipFrames; i < maxFrames; i++)
        {
            StackTrace trace = new StackTrace(new StackFrame(i, true));
            if (trace == null)
            {
                break;
            }
            StackFrame frame = trace.GetFrame(0);
            if ((frame == null) || ((frame.GetMethod() == null) || frame.GetMethod().ToString().StartsWith("<")))
            {
                break;
            }
            builder.Append(string.Format("File \"{0}\", line {1} -- {2}\n", System.IO.Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber(), frame.GetMethod()));
        }
        this.LogMessage(LogLevel.Debug, builder.ToString().TrimEnd(new char[0]), false);
    }

    public void LogError(string message)
    {
        this.LogMessage(LogLevel.Error, message, true);
    }

    public void LogError(string format, params object[] args)
    {
        string message = string.Format(format, args);
        this.LogMessage(LogLevel.Error, message, true);
    }

    public void LogInfo(string message)
    {
        this.LogMessage(LogLevel.Info, message, true);
    }

    public void LogInfo(string format, params object[] args)
    {
        string message = string.Format(format, args);
        this.LogMessage(LogLevel.Info, message, true);
    }

    private void LogMessage(LogLevel logLevel, string message, bool includeFilename = true)
    {
        StackTrace trace = new StackTrace(new StackFrame(2, true));
        StringBuilder builder = new StringBuilder();
        builder.Append("[");
        builder.Append(this.m_sourceName);
        builder.Append("] ");
        builder.Append(message);
        if ((trace != null) && includeFilename)
        {
            StackFrame sf = trace.GetFrame(0);
            builder.Append(this.FormatStacktrace(sf, false));
        }
        Log.BattleNet.Print(logLevel, builder.ToString(), new object[0]);
        if (logLevel == LogLevel.Error)
        {
            UnityEngine.Debug.LogError(string.Format("BattleNet Error: {0}", builder.ToString()));
        }
    }

    public void LogWarning(string message)
    {
        this.LogMessage(LogLevel.Warning, message, true);
    }

    public void LogWarning(string format, params object[] args)
    {
        string message = string.Format(format, args);
        this.LogMessage(LogLevel.Warning, message, true);
    }
}

