using System;

public class BugReporter
{
    private static BugReporter s_instance;

    public static BugReporter Get()
    {
        if (s_instance == null)
        {
            s_instance = new BugReporter();
        }
        return s_instance;
    }

    public void OnGUI()
    {
    }
}

