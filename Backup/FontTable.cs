using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FontTable : MonoBehaviour
{
    private Map<string, FontDef> m_defs = new Map<string, FontDef>();
    public List<FontTableEntry> m_Entries;
    private int m_initialDefsLoading;
    private bool m_initialized;
    private List<InitializedListener> m_initializedListeners = new List<InitializedListener>();
    private static FontTable s_instance;

    public void AddInitializedCallback(InitializedCallback callback)
    {
        this.AddInitializedCallback(callback, null);
    }

    public void AddInitializedCallback(InitializedCallback callback, object userData)
    {
        InitializedListener item = new InitializedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_initializedListeners.Contains(item))
        {
            this.m_initializedListeners.Add(item);
        }
    }

    private void Awake()
    {
        s_instance = this;
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
    }

    private void FinishInitialization()
    {
        this.m_initialized = true;
        this.FireInitializedCallbacks();
    }

    private void FireInitializedCallbacks()
    {
        InitializedListener[] listenerArray = this.m_initializedListeners.ToArray();
        this.m_initializedListeners.Clear();
        foreach (InitializedListener listener in listenerArray)
        {
            listener.Fire();
        }
    }

    public static FontTable Get()
    {
        return s_instance;
    }

    public FontDef GetFontDef(string name)
    {
        FontDef def = null;
        this.m_defs.TryGetValue(name, out def);
        return def;
    }

    public FontDef GetFontDef(Font enUSFont)
    {
        string fontDefName = this.GetFontDefName(enUSFont);
        return this.GetFontDef(fontDefName);
    }

    private string GetFontDefName(string fontName)
    {
        foreach (FontTableEntry entry in this.m_Entries)
        {
            if (entry.m_FontName == fontName)
            {
                return entry.m_FontDefName;
            }
        }
        return null;
    }

    private string GetFontDefName(Font font)
    {
        if (font == null)
        {
            return null;
        }
        return this.GetFontDefName(font.name);
    }

    public void Initialize()
    {
        this.m_initialized = false;
        this.m_initialDefsLoading = this.m_Entries.Count;
        if (this.m_initialDefsLoading == 0)
        {
            this.FinishInitialization();
        }
        else
        {
            foreach (FontTableEntry entry in this.m_Entries)
            {
                AssetLoader.Get().LoadFontDef(entry.m_FontDefName, new AssetLoader.GameObjectCallback(this.OnFontDefLoaded), null, false);
            }
        }
    }

    public bool IsInitialized()
    {
        return this.m_initialized;
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        s_instance = null;
    }

    private void OnFontDefLoaded(string name, GameObject go, object userData)
    {
        if (go == null)
        {
            this.OnInitialDefLoaded();
        }
        else
        {
            FontDef component = go.GetComponent<FontDef>();
            if (component == null)
            {
                object[] args = new object[] { name };
                string message = GameStrings.Format("GLOBAL_ERROR_ASSET_INCORRECT_DATA", args);
                Error.AddFatal(message);
                Debug.LogError(string.Format("FontTable.OnFontDefLoaded() - name={0} message={1}", name, message));
                this.OnInitialDefLoaded();
            }
            else
            {
                component.transform.parent = base.transform;
                this.m_defs.Add(name, component);
                this.OnInitialDefLoaded();
            }
        }
    }

    private void OnInitialDefLoaded()
    {
        this.m_initialDefsLoading--;
        if (this.m_initialDefsLoading <= 0)
        {
            this.FinishInitialization();
        }
    }

    public bool RemoveInitializedCallback(InitializedCallback callback)
    {
        return this.RemoveInitializedCallback(callback, null);
    }

    public bool RemoveInitializedCallback(InitializedCallback callback, object userData)
    {
        InitializedListener item = new InitializedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_initializedListeners.Remove(item);
    }

    private void WillReset()
    {
        this.m_defs.Clear();
    }

    public delegate void InitializedCallback(object userData);

    private class InitializedListener : EventListener<FontTable.InitializedCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }
}

