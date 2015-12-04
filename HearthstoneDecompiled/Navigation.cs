using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Navigation
{
    private static Stack<NavigateBackHandler> history = new Stack<NavigateBackHandler>();

    public static bool BackStackContainsHandler(NavigateBackHandler handler)
    {
        return history.Contains(handler);
    }

    public static bool BlockBackingOut()
    {
        return false;
    }

    private static bool CanNavigate()
    {
        if (GameUtils.IsAnyTransitionActive())
        {
            return false;
        }
        switch (GameMgr.Get().GetFindGameState())
        {
            case FindGameState.CLIENT_STARTED:
            case FindGameState.CLIENT_CANCELED:
            case FindGameState.CLIENT_ERROR:
            case FindGameState.BNET_QUEUE_CANCELED:
            case FindGameState.BNET_ERROR:
            case FindGameState.SERVER_GAME_CONNECTING:
            case FindGameState.SERVER_GAME_STARTED:
            case FindGameState.SERVER_GAME_CANCELED:
                return false;
        }
        return true;
    }

    public static void Clear()
    {
        history.Clear();
        if (NAVIGATION_DEBUG)
        {
            DumpStack();
        }
    }

    public static void DumpStack()
    {
        Debug.Log(string.Format("Navigation Stack Dump (count: {0})\n", history.Count));
        int num = 0;
        foreach (NavigateBackHandler handler in history)
        {
            Debug.Log(string.Format("{0}: {1}\n", num, StackEntryToString(handler)));
            num++;
        }
    }

    public static bool GoBack()
    {
        if ((history.Count == 0) || !CanNavigate())
        {
            return false;
        }
        NavigateBackHandler handler = history.Peek();
        if (!handler())
        {
            return false;
        }
        if ((history.Count > 0) && (handler == history.Peek()))
        {
            history.Pop();
        }
        if (NAVIGATION_DEBUG)
        {
            DumpStack();
        }
        return true;
    }

    public static void Pop()
    {
        if ((history.Count != 0) && CanNavigate())
        {
            history.Pop();
            if (NAVIGATION_DEBUG)
            {
                DumpStack();
            }
        }
    }

    public static void PopUnique(NavigateBackHandler handler)
    {
        <PopUnique>c__AnonStorey303 storey = new <PopUnique>c__AnonStorey303 {
            handler = handler
        };
        if (history.Count != 0)
        {
            if (history.Contains(storey.handler))
            {
                history = new Stack<NavigateBackHandler>(Enumerable.Where<NavigateBackHandler>(history, new Func<NavigateBackHandler, bool>(storey.<>m__11C)).Reverse<NavigateBackHandler>());
            }
            if (NAVIGATION_DEBUG)
            {
                DumpStack();
            }
        }
    }

    public static void Push(NavigateBackHandler handler)
    {
        if (handler != null)
        {
            history.Push(handler);
            if (NAVIGATION_DEBUG)
            {
                DumpStack();
            }
        }
    }

    public static void PushUnique(NavigateBackHandler handler)
    {
        if (handler != null)
        {
            foreach (NavigateBackHandler handler2 in history)
            {
                if (handler2 == handler)
                {
                    return;
                }
            }
            history.Push(handler);
            if (NAVIGATION_DEBUG)
            {
                DumpStack();
            }
        }
    }

    private static string StackEntryToString(NavigateBackHandler entry)
    {
        return string.Format("{0}.{1} Target={2}", entry.Method.DeclaringType, entry.Method.Name, ((entry != null) && (entry.Target != null)) ? entry.Target.ToString() : (!entry.Method.IsStatic ? "null" : "<static>"));
    }

    public static bool NAVIGATION_DEBUG
    {
        get
        {
            return Vars.Key("Application.Navigation.Debug").GetBool(false);
        }
        set
        {
            VarsInternal.Get().Set("Application.Navigation.Debug", value.ToString());
        }
    }

    [CompilerGenerated]
    private sealed class <PopUnique>c__AnonStorey303
    {
        internal Navigation.NavigateBackHandler handler;

        internal bool <>m__11C(Navigation.NavigateBackHandler h)
        {
            return (h != this.handler);
        }
    }

    public delegate bool NavigateBackHandler();
}

