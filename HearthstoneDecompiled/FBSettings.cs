using System;
using UnityEngine;

public class FBSettings : ScriptableObject
{
    [SerializeField]
    private string[] appIds = new string[] { "0" };
    [SerializeField]
    private string[] appLabels = new string[] { "App Name" };
    [SerializeField]
    private bool cookie = true;
    private const string facebookSettingsAssetExtension = ".asset";
    private const string facebookSettingsAssetName = "FacebookSettings";
    private const string facebookSettingsPath = "MobileAdTracking/Facebook/Resources";
    [SerializeField]
    private bool frictionlessRequests = true;
    private static FBSettings instance;
    [SerializeField]
    private string iosURLSuffix = string.Empty;
    [SerializeField]
    private bool logging = true;
    [SerializeField]
    private int selectedAppIndex;
    [SerializeField]
    private bool status = true;
    [SerializeField]
    private bool xfbml;

    private static void DirtyEditor()
    {
    }

    public void SetAppId(int index, string value)
    {
        if (this.appIds[index] != value)
        {
            this.appIds[index] = value;
            DirtyEditor();
        }
    }

    public void SetAppIndex(int index)
    {
        if (this.selectedAppIndex != index)
        {
            this.selectedAppIndex = index;
            DirtyEditor();
        }
    }

    public void SetAppLabel(int index, string value)
    {
        if (this.appLabels[index] != value)
        {
            this.AppLabels[index] = value;
            DirtyEditor();
        }
    }

    public static string[] AllAppIds
    {
        get
        {
            return Instance.AppIds;
        }
    }

    public static string AppId
    {
        get
        {
            return Instance.AppIds[Instance.SelectedAppIndex];
        }
    }

    public string[] AppIds
    {
        get
        {
            return this.appIds;
        }
        set
        {
            if (this.appIds != value)
            {
                this.appIds = value;
                DirtyEditor();
            }
        }
    }

    public string[] AppLabels
    {
        get
        {
            return this.appLabels;
        }
        set
        {
            if (this.appLabels != value)
            {
                this.appLabels = value;
                DirtyEditor();
            }
        }
    }

    public static string ChannelUrl
    {
        get
        {
            return "/channel.html";
        }
    }

    public static bool Cookie
    {
        get
        {
            return Instance.cookie;
        }
        set
        {
            if (Instance.cookie != value)
            {
                Instance.cookie = value;
                DirtyEditor();
            }
        }
    }

    public static bool FrictionlessRequests
    {
        get
        {
            return Instance.frictionlessRequests;
        }
        set
        {
            if (Instance.frictionlessRequests != value)
            {
                Instance.frictionlessRequests = value;
                DirtyEditor();
            }
        }
    }

    private static FBSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load("FacebookSettings") as FBSettings;
                if (instance == null)
                {
                    instance = ScriptableObject.CreateInstance<FBSettings>();
                }
            }
            return instance;
        }
    }

    public static string IosURLSuffix
    {
        get
        {
            return Instance.iosURLSuffix;
        }
        set
        {
            if (Instance.iosURLSuffix != value)
            {
                Instance.iosURLSuffix = value;
                DirtyEditor();
            }
        }
    }

    public static bool IsValidAppId
    {
        get
        {
            return (((AppId != null) && (AppId.Length > 0)) && !AppId.Equals("0"));
        }
    }

    public static bool Logging
    {
        get
        {
            return Instance.logging;
        }
        set
        {
            if (Instance.logging != value)
            {
                Instance.logging = value;
                DirtyEditor();
            }
        }
    }

    public int SelectedAppIndex
    {
        get
        {
            return this.selectedAppIndex;
        }
    }

    public static bool Status
    {
        get
        {
            return Instance.status;
        }
        set
        {
            if (Instance.status != value)
            {
                Instance.status = value;
                DirtyEditor();
            }
        }
    }

    public static bool Xfbml
    {
        get
        {
            return Instance.xfbml;
        }
        set
        {
            if (Instance.xfbml != value)
            {
                Instance.xfbml = value;
                DirtyEditor();
            }
        }
    }
}

