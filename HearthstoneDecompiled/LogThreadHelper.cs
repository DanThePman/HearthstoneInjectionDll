using System;
using System.Collections.Generic;

public class LogThreadHelper
{
    private BattleNetLogSource m_logSource;
    private List<LogEntry> m_queuedLogs = new List<LogEntry>();

    public LogThreadHelper(string name)
    {
        this.m_logSource = new BattleNetLogSource(name);
    }

    public void LogDebug(string message)
    {
        this.LogMessage(message, LogLevel.Debug);
    }

    public void LogDebug(string format, params object[] args)
    {
        string message = string.Format(format, args);
        this.LogMessage(message, LogLevel.Debug);
    }

    public void LogError(string message)
    {
        this.LogMessage(message, LogLevel.Error);
    }

    public void LogError(string format, params object[] args)
    {
        string message = string.Format(format, args);
        this.LogMessage(message, LogLevel.Error);
    }

    public void LogInfo(string message)
    {
        this.LogMessage(message, LogLevel.Info);
    }

    public void LogInfo(string format, params object[] args)
    {
        string message = string.Format(format, args);
        this.LogMessage(message, LogLevel.Info);
    }

    private void LogMessage(string message, LogLevel level)
    {
        List<LogEntry> queuedLogs = this.m_queuedLogs;
        lock (queuedLogs)
        {
            LogEntry item = new LogEntry {
                Message = message,
                Level = level
            };
            this.m_queuedLogs.Add(item);
        }
    }

    public void LogWarning(string message)
    {
        this.LogMessage(message, LogLevel.Warning);
    }

    public void LogWarning(string format, params object[] args)
    {
        string message = string.Format(format, args);
        this.LogMessage(message, LogLevel.Warning);
    }

    public void Process()
    {
        List<LogEntry> queuedLogs = this.m_queuedLogs;
        lock (queuedLogs)
        {
            if (this.m_queuedLogs.Count > 0)
            {
                foreach (LogEntry entry in this.m_queuedLogs)
                {
                    switch (entry.Level)
                    {
                        case LogLevel.Info:
                        {
                            this.m_logSource.LogInfo(entry.Message);
                            continue;
                        }
                        case LogLevel.Warning:
                        {
                            this.m_logSource.LogWarning(entry.Message);
                            continue;
                        }
                        case LogLevel.Error:
                        {
                            this.m_logSource.LogError(entry.Message);
                            continue;
                        }
                    }
                    this.m_logSource.LogDebug(entry.Message);
                }
                this.m_queuedLogs.Clear();
            }
        }
    }

    private class LogEntry
    {
        public LogLevel Level;
        public string Message;
    }
}

