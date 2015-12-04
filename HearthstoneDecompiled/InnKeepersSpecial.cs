using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class InnKeepersSpecial : MonoBehaviour
{
    public GameObject adBackground;
    public PegUIElement adButton;
    public UberText adButtonText;
    public GameObject adImage;
    public UberText adSubtitle;
    public UberText adTitle;
    public string adUrlOverride;
    public GameObject content;
    private Map<string, JSONNode> m_adMetadata;
    private JSONNode m_adToDisplay;
    private string m_gameAction;
    private bool m_hasSeenResponse;
    private Dictionary<string, string> m_headers;
    private string m_lastUrl;
    private string m_link;
    private bool m_loadedSuccessfully;
    private JSONNode m_response;
    private GeneralStoreMode m_storeMode;
    private WWW m_textureWWW;
    private string m_url;
    private static InnKeepersSpecial s_instance;

    protected void Awake()
    {
        this.Show(false);
        this.m_headers = new Dictionary<string, string>();
        this.m_headers["Accept"] = "application/json";
        string str = (BattleNet.GetCurrentRegion() != Network.BnetRegion.REGION_CN) ? "us" : "cn";
        string str2 = "https://api.battlenet.com.cn/cms/ad/list?locale=zh_cn&community=hearthstone&mediaCategory=IN_GAME_AD&apikey=4r78qhz9atqzsxkk2qhqku6gy7p9tj8c";
        string str3 = string.Format("https://us.api.battle.net/cms/ad/list?locale={0}&community=hearthstone&mediaCategory=IN_GAME_AD&apikey=4r78qhz9atqzsxkk2qhqku6gy7p9tj8c", Localization.GetLocaleName());
        if (str.Equals("cn"))
        {
            this.m_url = str2;
        }
        else
        {
            this.m_url = str3;
        }
        Log.InnKeepersSpecial.Print("Inkeeper Ad: " + this.m_url, new object[0]);
        this.m_link = null;
        this.adButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.Click));
        this.Update();
    }

    private void Click(UIEvent e)
    {
        Log.InnKeepersSpecial.Print("IKS on release! " + this.m_link, new object[0]);
        if (this.m_gameAction != null)
        {
            WelcomeQuests.OnNavigateBack();
            this.Show(false);
            string str = this.m_gameAction.ToLowerInvariant();
            if (str.StartsWith("store"))
            {
                char[] separator = new char[] { ' ' };
                string[] strArray = str.Split(separator);
                if (strArray.Length > 1)
                {
                    BoosterDbId iNVALID = BoosterDbId.INVALID;
                    AdventureDbId id2 = AdventureDbId.INVALID;
                    string str2 = strArray[1];
                    try
                    {
                        iNVALID = (BoosterDbId) ((int) Enum.Parse(typeof(BoosterDbId), str2.ToUpper()));
                    }
                    catch (ArgumentException)
                    {
                    }
                    try
                    {
                        id2 = (AdventureDbId) ((int) Enum.Parse(typeof(AdventureDbId), str2.ToUpper()));
                    }
                    catch (ArgumentException)
                    {
                    }
                    if (iNVALID != BoosterDbId.INVALID)
                    {
                        Options.Get().SetInt(Option.LAST_SELECTED_STORE_BOOSTER_ID, (int) iNVALID);
                        this.ShowStore(GeneralStoreMode.CARDS);
                    }
                    else if (id2 != AdventureDbId.INVALID)
                    {
                        Options.Get().SetInt(Option.LAST_SELECTED_STORE_ADVENTURE_ID, (int) id2);
                        this.ShowStore(GeneralStoreMode.ADVENTURE);
                    }
                    else
                    {
                        this.ShowStore(GeneralStoreMode.NONE);
                    }
                }
                else
                {
                    this.ShowStore(GeneralStoreMode.NONE);
                }
            }
        }
        else if (this.m_link != null)
        {
            Application.OpenURL(this.m_link);
        }
    }

    public static ulong DateTimeToUnixTimeStamp(DateTime time)
    {
        DateTime time2 = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan span = (TimeSpan) (time.ToUniversalTime() - time2);
        return (ulong) span.TotalSeconds;
    }

    public static InnKeepersSpecial Get()
    {
        Init();
        return s_instance;
    }

    private JSONNode GetAdToDisplay(JSONNode response)
    {
        try
        {
            JSONNode node = null;
            int num = 0;
            double num2 = 0.0;
            IEnumerator enumerator = response["ads"].AsArray.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    JSONNode current = (JSONNode) enumerator.Current;
                    int asInt = current["importance"].AsInt;
                    if (current["maxImportance"].AsBool)
                    {
                        asInt = 6;
                    }
                    double asDouble = current["publish"].AsDouble;
                    JSONNode node3 = current["metadata"];
                    Map<string, JSONNode> map = new Map<string, JSONNode>();
                    if (node3 != null)
                    {
                        IEnumerator enumerator2 = node3.AsArray.GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                JSONNode node4 = (JSONNode) enumerator2.Current;
                                map[(string) node4["key"]] = node4["value"];
                            }
                        }
                        finally
                        {
                            IDisposable disposable = enumerator2 as IDisposable;
                            if (disposable == null)
                            {
                            }
                            disposable.Dispose();
                        }
                    }
                    if ((!map.ContainsKey("clientVersion") || StringUtils.CompareIgnoreCase(map["clientVersion"].Value, "4.1")) && ((asInt > num) || ((asInt == num) && (asDouble > num2))))
                    {
                        node = current;
                        this.m_adMetadata = map;
                        num = asInt;
                        num2 = asDouble;
                    }
                }
            }
            finally
            {
                IDisposable disposable2 = enumerator as IDisposable;
                if (disposable2 == null)
                {
                }
                disposable2.Dispose();
            }
            return node;
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogError("IKS Error: " + exception.StackTrace);
            UnityEngine.Debug.Log("Failed to get correct advertisement " + exception.Message);
            return null;
        }
    }

    private static int GetCacheAge(WWW www)
    {
        string str;
        if ((www.responseHeaders != null) && www.responseHeaders.TryGetValue("CACHE-CONTROL", out str))
        {
            char[] separator = new char[] { ',' };
            foreach (string str2 in str.Split(separator))
            {
                string str3 = str2.ToLowerInvariant().Trim();
                if (str3.StartsWith("max-age"))
                {
                    int num2;
                    char[] chArray2 = new char[] { '=' };
                    string[] strArray3 = str3.Split(chArray2);
                    if ((strArray3.Length == 2) && int.TryParse(strArray3[1], out num2))
                    {
                        return num2;
                    }
                }
            }
        }
        return -1;
    }

    public bool HasAlreadySeenResponse()
    {
        return this.m_hasSeenResponse;
    }

    public static void Init()
    {
        if (s_instance == null)
        {
            s_instance = AssetLoader.Get().LoadGameObject("InnKeepersSpecial", true, false).GetComponent<InnKeepersSpecial>();
            OverlayUI.Get().AddGameObject(s_instance.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        }
    }

    public bool LoadedSuccessfully()
    {
        return this.m_loadedSuccessfully;
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (mode == SceneMgr.Mode.HUB)
        {
            StoreManager.Get().StartGeneralTransaction(this.m_storeMode);
            SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        }
    }

    public void Show(bool visible)
    {
        if (visible)
        {
            float num = 0.5f;
            this.content.SetActive(true);
            Color color = this.adImage.gameObject.GetComponent<Renderer>().material.color;
            color.a = 0f;
            this.adImage.gameObject.GetComponent<Renderer>().material.color = color;
            object[] args = new object[] { "amount", 1f, "time", num, "easeType", iTween.EaseType.linear };
            Hashtable hashtable = iTween.Hash(args);
            iTween.FadeTo(this.adImage.gameObject, hashtable);
            this.adTitle.Show();
            object[] objArray2 = new object[] { "from", 0f, "to", 1f, "time", num, "easeType", iTween.EaseType.linear, "onupdate", newVal => this.adTitle.TextAlpha = (float) newVal };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.ValueTo(this.adTitle.gameObject, hashtable2);
            this.adSubtitle.Show();
            object[] objArray3 = new object[] { "from", 0f, "to", 1f, "time", num, "easeType", iTween.EaseType.linear, "onupdate", newVal => this.adSubtitle.TextAlpha = (float) newVal };
            Hashtable hashtable3 = iTween.Hash(objArray3);
            iTween.ValueTo(this.adSubtitle.gameObject, hashtable3);
        }
        else
        {
            this.content.SetActive(false);
            this.adTitle.Hide();
            this.adSubtitle.Hide();
        }
    }

    private void ShowStore(GeneralStoreMode mode = 0)
    {
        this.m_storeMode = mode;
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB)
        {
            StoreManager.Get().StartGeneralTransaction(this.m_storeMode);
        }
        else
        {
            SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        }
    }

    private void Update()
    {
        string adUrlOverride;
        if (!string.IsNullOrEmpty(this.adUrlOverride))
        {
            adUrlOverride = this.adUrlOverride;
        }
        else
        {
            adUrlOverride = this.m_url;
        }
        if ((adUrlOverride != this.m_lastUrl) && !string.IsNullOrEmpty(adUrlOverride))
        {
            this.m_lastUrl = adUrlOverride;
            this.m_link = null;
            this.Show(false);
            base.StartCoroutine(this.UpdateAdJson(adUrlOverride));
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateAdJson(string url)
    {
        return new <UpdateAdJson>c__Iterator1B4 { url = url, <$>url = url, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator UpdateAdTexture()
    {
        return new <UpdateAdTexture>c__Iterator1B5 { <>f__this = this };
    }

    private void UpdateText(JSONNode rawMetadata)
    {
        if (this.m_adMetadata.ContainsKey("gameAction"))
        {
            this.m_gameAction = this.m_adMetadata["gameAction"];
        }
        if (this.m_adMetadata.ContainsKey("buttonText"))
        {
            this.adButtonText.GameStringLookup = false;
            this.adButtonText.Text = this.m_adMetadata["buttonText"];
        }
        Vector3 localPosition = this.adTitle.transform.localPosition;
        if (this.m_adMetadata.ContainsKey("titleOffsetX"))
        {
            localPosition.x += this.m_adMetadata["titleOffsetX"].AsFloat;
        }
        if (this.m_adMetadata.ContainsKey("titleOffsetY"))
        {
            localPosition.y += this.m_adMetadata["titleOffsetY"].AsFloat;
        }
        this.adTitle.transform.localPosition = localPosition;
        Vector3 vector2 = this.adSubtitle.transform.localPosition;
        if (this.m_adMetadata.ContainsKey("subtitleOffsetX"))
        {
            vector2.x += this.m_adMetadata["subtitleOffsetX"].AsFloat;
        }
        if (this.m_adMetadata.ContainsKey("subtitleOffsetY"))
        {
            vector2.y += this.m_adMetadata["subtitleOffsetY"].AsFloat;
        }
        this.adSubtitle.transform.localPosition = vector2;
        if (this.m_adMetadata.ContainsKey("titleFontSize"))
        {
            this.adTitle.FontSize = this.m_adMetadata["titleFontSize"].AsInt;
        }
        if (this.m_adMetadata.ContainsKey("subtitleFontSize"))
        {
            this.adSubtitle.FontSize = this.m_adMetadata["subtitleFontSize"].AsInt;
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateAdJson>c__Iterator1B4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>url;
        internal InnKeepersSpecial <>f__this;
        internal int <cacheAge>__0;
        internal string <cachedResponse>__5;
        internal Exception <e>__9;
        internal ulong <epochNow>__1;
        internal bool <fail>__8;
        internal string <hexString>__10;
        internal string <hexString>__7;
        internal ulong <lastDownloadTime>__2;
        internal string <response>__4;
        internal ulong <secondsSinceLastDownload>__3;
        internal WWW <urlWWW>__6;
        internal string url;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    Log.InnKeepersSpecial.Print("requesting url " + this.url, new object[0]);
                    this.<cacheAge>__0 = Options.Get().GetInt(Option.IKS_CACHE_AGE);
                    this.<epochNow>__1 = InnKeepersSpecial.DateTimeToUnixTimeStamp(DateTime.Now);
                    this.<lastDownloadTime>__2 = Options.Get().GetULong(Option.IKS_LAST_DOWNLOAD_TIME, this.<epochNow>__1);
                    this.<secondsSinceLastDownload>__3 = this.<epochNow>__1 - this.<lastDownloadTime>__2;
                    this.<response>__4 = string.Empty;
                    this.<cachedResponse>__5 = TextUtils.FromHexString(Options.Get().GetString(Option.IKS_LAST_DOWNLOAD_RESPONSE));
                    Log.InnKeepersSpecial.Print(string.Concat(new object[] { "last download ", this.<secondsSinceLastDownload>__3, "s ago; (will refresh after: ", this.<cacheAge>__0, "s)" }), new object[0]);
                    if ((this.<secondsSinceLastDownload>__3 > this.<cacheAge>__0) || string.IsNullOrEmpty(this.<cachedResponse>__5))
                    {
                        this.<urlWWW>__6 = new WWW(this.url, null, this.<>f__this.m_headers);
                        this.$current = this.<urlWWW>__6;
                        this.$PC = 1;
                        return true;
                    }
                    Log.InnKeepersSpecial.Print("Using cached response: " + this.<cachedResponse>__5, new object[0]);
                    this.<response>__4 = this.<cachedResponse>__5;
                    break;

                case 1:
                    if (string.IsNullOrEmpty(this.<urlWWW>__6.error))
                    {
                        this.<response>__4 = this.<urlWWW>__6.text;
                        this.<cacheAge>__0 = InnKeepersSpecial.GetCacheAge(this.<urlWWW>__6);
                        this.<hexString>__7 = TextUtils.ToHexString(this.<response>__4);
                        Options.Get().SetString(Option.IKS_LAST_DOWNLOAD_RESPONSE, this.<hexString>__7);
                        Options.Get().SetULong(Option.IKS_LAST_DOWNLOAD_TIME, this.<epochNow>__1);
                        Options.Get().SetInt(Option.IKS_CACHE_AGE, this.<cacheAge>__0);
                        break;
                    }
                    UnityEngine.Debug.LogWarning("Failed to download url for Inkeeper's Special: " + this.url);
                    UnityEngine.Debug.LogWarning(this.<urlWWW>__6.error);
                    goto Label_037D;

                default:
                    goto Label_037D;
            }
            Log.InnKeepersSpecial.Print("url text is " + this.<response>__4, new object[0]);
            this.<fail>__8 = false;
            try
            {
                this.<>f__this.m_response = JSON.Parse(this.<response>__4);
            }
            catch (Exception exception)
            {
                this.<e>__9 = exception;
                this.<fail>__8 = true;
                UnityEngine.Debug.LogError(this.<e>__9.StackTrace);
            }
            if (!this.<fail>__8)
            {
                this.<>f__this.m_adToDisplay = this.<>f__this.GetAdToDisplay(this.<>f__this.m_response);
                if (this.<>f__this.m_adToDisplay != null)
                {
                    this.<hexString>__10 = TextUtils.ToHexString(this.<>f__this.m_adToDisplay.ToString());
                    this.<>f__this.m_hasSeenResponse = this.<hexString>__10 == Options.Get().GetString(Option.IKS_LAST_SHOWN_AD);
                    Options.Get().SetString(Option.IKS_LAST_SHOWN_AD, this.<hexString>__10);
                    Log.InnKeepersSpecial.Print("Ad to display :" + ((string) this.<>f__this.m_adToDisplay["link"]), new object[0]);
                    this.<>f__this.StartCoroutine(this.<>f__this.UpdateAdTexture());
                }
                this.$PC = -1;
            }
        Label_037D:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateAdTexture>c__Iterator1B5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal InnKeepersSpecial <>f__this;
        internal JSONNode <content>__0;
        internal string <imageUrl>__4;
        internal JSONNode <media>__3;
        internal JSONNode <metadata>__6;
        internal string <subtitle>__2;
        internal string <title>__1;
        internal bool <visible>__5;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.m_link = (string) this.<>f__this.m_adToDisplay["link"];
                    this.<content>__0 = this.<>f__this.m_adToDisplay["contents"].AsArray[0];
                    this.<title>__1 = (string) this.<content>__0["title"];
                    this.<subtitle>__2 = (string) this.<content>__0["subtitle"];
                    if (this.<content>__0["link"] != null)
                    {
                        this.<>f__this.m_link = (string) this.<content>__0["link"];
                    }
                    if (this.<title>__1 == null)
                    {
                        this.<title>__1 = string.Empty;
                    }
                    this.<>f__this.adTitle.Text = this.<title>__1.Replace(@"\n", "\n");
                    if (this.<subtitle>__2 == null)
                    {
                        this.<subtitle>__2 = string.Empty;
                    }
                    this.<>f__this.adSubtitle.Text = this.<subtitle>__2.Replace(@"\n", "\n");
                    this.<media>__3 = this.<content>__0["media"];
                    this.<imageUrl>__4 = string.Empty;
                    if ((this.<media>__3 != null) && (this.<media>__3["url"] != null))
                    {
                        this.<imageUrl>__4 = "http:" + ((string) this.<media>__3["url"]);
                    }
                    Log.InnKeepersSpecial.Print("image url is " + this.<imageUrl>__4, new object[0]);
                    this.<>f__this.m_textureWWW = new WWW(this.<imageUrl>__4);
                    this.$current = this.<>f__this.m_textureWWW;
                    this.$PC = 1;
                    return true;

                case 1:
                    if (string.IsNullOrEmpty(this.<>f__this.m_textureWWW.error))
                    {
                        this.<visible>__5 = this.<>f__this.content.activeSelf;
                        this.<>f__this.Show(true);
                        this.<>f__this.adImage.GetComponent<Renderer>().material.mainTexture = this.<>f__this.m_textureWWW.texture;
                        this.<>f__this.adImage.GetComponent<Renderer>().material.mainTexture.wrapMode = TextureWrapMode.Clamp;
                        this.<metadata>__6 = this.<>f__this.m_adToDisplay["metadata"];
                        this.<>f__this.UpdateText(this.<metadata>__6);
                        this.<>f__this.Show(this.<visible>__5);
                        this.<>f__this.m_loadedSuccessfully = true;
                        this.$PC = -1;
                        break;
                    }
                    UnityEngine.Debug.LogError("Failed to download image for Inkeeper's Special: " + this.<imageUrl>__4);
                    UnityEngine.Debug.LogError(this.<>f__this.m_textureWWW.error);
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

