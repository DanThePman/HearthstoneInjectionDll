using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class BnetEventMgr
{
    private List<ChangeListener> m_changeListeners = new List<ChangeListener>();
    private static BnetEventMgr s_instance;

    public bool AddChangeListener(ChangeCallback callback)
    {
        return this.AddChangeListener(callback, null);
    }

    public bool AddChangeListener(ChangeCallback callback, object userData)
    {
        ChangeListener item = new ChangeListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_changeListeners.Contains(item))
        {
            return false;
        }
        this.m_changeListeners.Add(item);
        return true;
    }

    private void FireChangeEvent(BattleNet.BnetEvent stateChange)
    {
        foreach (ChangeListener listener in this.m_changeListeners.ToArray())
        {
            listener.Fire(stateChange);
        }
    }

    public static BnetEventMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new BnetEventMgr();
            s_instance.Initialize();
        }
        return s_instance;
    }

    public void Initialize()
    {
        Network.Get().SetBnetStateHandler(new Network.BnetEventHandler(this.OnBnetEventsOccurred));
    }

    private void OnBnetEventsOccurred(BattleNet.BnetEvent[] bnetEvents)
    {
        foreach (BattleNet.BnetEvent event2 in bnetEvents)
        {
            this.FireChangeEvent(event2);
        }
    }

    public bool RemoveChangeListener(ChangeCallback callback)
    {
        return this.RemoveChangeListener(callback, null);
    }

    public bool RemoveChangeListener(ChangeCallback callback, object userData)
    {
        ChangeListener item = new ChangeListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_changeListeners.Remove(item);
    }

    public void Shutdown()
    {
    }

    public delegate void ChangeCallback(BattleNet.BnetEvent stateChange, object userData);

    private class ChangeListener : EventListener<BnetEventMgr.ChangeCallback>
    {
        public void Fire(BattleNet.BnetEvent stateChange)
        {
            base.m_callback(stateChange, base.m_userData);
        }
    }
}

