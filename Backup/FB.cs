using Facebook;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public sealed class FB : ScriptableObject
{
    private static string appId;
    private static string authResponse;
    private static bool cookie;
    private static IFacebook facebook;
    private static bool frictionlessRequests;
    private static bool isInitCalled;
    private static bool logging;
    public static HideUnityDelegate OnHideUnity;
    public static InitDelegate OnInitComplete;
    private static bool status;
    private static bool xfbml;

    public static void ActivateApp()
    {
        FacebookImpl.ActivateApp(AppId);
    }

    public static void API(string query, HttpMethod method, FacebookDelegate callback = null, Dictionary<string, string> formData = null)
    {
        FacebookImpl.API(query, method, formData, callback);
    }

    public static void API(string query, HttpMethod method, FacebookDelegate callback, WWWForm formData)
    {
        FacebookImpl.API(query, method, formData, callback);
    }

    public static void AppRequest(string message, OGActionType actionType, string objectId, string[] to, string data = "", string title = "", FacebookDelegate callback = null)
    {
        FacebookImpl.AppRequest(message, actionType, objectId, to, null, null, null, data, title, callback);
    }

    public static void AppRequest(string message, string[] to = null, List<object> filters = null, string[] excludeIds = null, int? maxRecipients = new int?(), string data = "", string title = "", FacebookDelegate callback = null)
    {
        FacebookImpl.AppRequest(message, null, null, to, filters, excludeIds, maxRecipients, data, title, callback);
    }

    public static void AppRequest(string message, OGActionType actionType, string objectId, List<object> filters = null, string[] excludeIds = null, int? maxRecipients = new int?(), string data = "", string title = "", FacebookDelegate callback = null)
    {
        FacebookImpl.AppRequest(message, actionType, objectId, null, filters, excludeIds, maxRecipients, data, title, callback);
    }

    public static void Feed(string toId = "", string link = "", string linkName = "", string linkCaption = "", string linkDescription = "", string picture = "", string mediaSource = "", string actionName = "", string actionLink = "", string reference = "", Dictionary<string, string[]> properties = null, FacebookDelegate callback = null)
    {
        FacebookImpl.FeedRequest(toId, link, linkName, linkCaption, linkDescription, picture, mediaSource, actionName, actionLink, reference, properties, callback);
    }

    public static void GameGroupCreate(string name, string description, string privacy = "CLOSED", FacebookDelegate callback = null)
    {
        FacebookImpl.GameGroupCreate(name, description, privacy, callback);
    }

    public static void GameGroupJoin(string id, FacebookDelegate callback = null)
    {
        FacebookImpl.GameGroupJoin(id, callback);
    }

    public static void GetDeepLink(FacebookDelegate callback)
    {
        FacebookImpl.GetDeepLink(callback);
    }

    public static void Init(InitDelegate onInitComplete, HideUnityDelegate onHideUnity = null, string authResponse = null)
    {
        Init(onInitComplete, FBSettings.AppId, FBSettings.Cookie, FBSettings.Logging, FBSettings.Status, FBSettings.Xfbml, FBSettings.FrictionlessRequests, onHideUnity, authResponse);
    }

    public static void Init(InitDelegate onInitComplete, string appId, bool cookie = true, bool logging = true, bool status = true, bool xfbml = false, bool frictionlessRequests = true, HideUnityDelegate onHideUnity = null, string authResponse = null)
    {
        FB.appId = appId;
        FB.cookie = cookie;
        FB.logging = logging;
        FB.status = status;
        FB.xfbml = xfbml;
        FB.frictionlessRequests = frictionlessRequests;
        FB.authResponse = authResponse;
        OnInitComplete = onInitComplete;
        OnHideUnity = onHideUnity;
        if (!isInitCalled)
        {
            FBBuildVersionAttribute versionAttributeOfType = FBBuildVersionAttribute.GetVersionAttributeOfType(typeof(IFacebook));
            if (versionAttributeOfType == null)
            {
                FbDebugOverride.Warn("Cannot find Facebook SDK Version");
            }
            else
            {
                FbDebugOverride.Info(string.Format("Using SDK {0}, Build {1}", versionAttributeOfType.SdkVersion, versionAttributeOfType.BuildVersion));
            }
            throw new NotImplementedException("Facebook API does not yet support this platform");
        }
        FbDebugOverride.Warn("FB.Init() has already been called.  You only need to call this once and only once.");
        if (FacebookImpl != null)
        {
            OnDllLoaded();
        }
    }

    public static void Login(string scope = "", FacebookDelegate callback = null)
    {
        FacebookImpl.Login(scope, callback);
    }

    public static void Logout()
    {
        FacebookImpl.Logout();
    }

    private static void OnDllLoaded()
    {
        FBBuildVersionAttribute versionAttributeOfType = FBBuildVersionAttribute.GetVersionAttributeOfType(FacebookImpl.GetType());
        if (versionAttributeOfType != null)
        {
            FbDebugOverride.Log(string.Format("Finished loading Facebook dll. Version {0} Build {1}", versionAttributeOfType.SdkVersion, versionAttributeOfType.BuildVersion));
        }
        FacebookImpl.Init(OnInitComplete, appId, cookie, logging, status, xfbml, FBSettings.ChannelUrl, authResponse, frictionlessRequests, OnHideUnity);
    }

    [Obsolete("use FB.ActivateApp()")]
    public static void PublishInstall(FacebookDelegate callback = null)
    {
        FacebookImpl.PublishInstall(AppId, callback);
    }

    public static string AccessToken
    {
        get
        {
            return ((facebook == null) ? string.Empty : facebook.AccessToken);
        }
    }

    public static DateTime AccessTokenExpiresAt
    {
        get
        {
            return ((facebook == null) ? DateTime.MinValue : facebook.AccessTokenExpiresAt);
        }
    }

    public static string AppId
    {
        get
        {
            return appId;
        }
    }

    private static IFacebook FacebookImpl
    {
        get
        {
            if (facebook == null)
            {
                throw new NullReferenceException("Facebook object is not yet loaded.  Did you call FB.Init()?");
            }
            return facebook;
        }
    }

    public static bool IsInitialized
    {
        get
        {
            return ((facebook != null) && facebook.IsInitialized);
        }
    }

    public static bool IsLoggedIn
    {
        get
        {
            return ((facebook != null) && facebook.IsLoggedIn);
        }
    }

    public static string UserId
    {
        get
        {
            return ((facebook == null) ? string.Empty : facebook.UserId);
        }
    }

    public sealed class Android
    {
        public static string KeyHash
        {
            get
            {
                AndroidFacebook facebook = FB.facebook as AndroidFacebook;
                return ((facebook == null) ? string.Empty : facebook.KeyHash);
            }
        }
    }

    public sealed class AppEvents
    {
        public static void LogEvent(string logEvent, float? valueToSum = new float?(), Dictionary<string, object> parameters = null)
        {
            FB.FacebookImpl.AppEventsLogEvent(logEvent, valueToSum, parameters);
        }

        public static void LogPurchase(float logPurchase, string currency = "USD", Dictionary<string, object> parameters = null)
        {
            FB.FacebookImpl.AppEventsLogPurchase(logPurchase, currency, parameters);
        }

        public static bool LimitEventUsage
        {
            get
            {
                return ((FB.facebook != null) && FB.facebook.LimitEventUsage);
            }
            set
            {
                FB.facebook.LimitEventUsage = value;
            }
        }
    }

    public sealed class Canvas
    {
        public static void Pay(string product, string action = "purchaseitem", int quantity = 1, int? quantityMin = new int?(), int? quantityMax = new int?(), string requestId = null, string pricepointId = null, string testCurrency = null, FacebookDelegate callback = null)
        {
            FB.FacebookImpl.Pay(product, action, quantity, quantityMin, quantityMax, requestId, pricepointId, testCurrency, callback);
        }

        public static void SetAspectRatio(int width, int height, params FBScreen.Layout[] layoutParams)
        {
            FBScreen.SetAspectRatio(width, height, layoutParams);
        }

        public static void SetResolution(int width, int height, bool fullscreen, int preferredRefreshRate = 0, params FBScreen.Layout[] layoutParams)
        {
            FBScreen.SetResolution(width, height, fullscreen, preferredRefreshRate, layoutParams);
        }
    }

    public abstract class CompiledFacebookLoader : MonoBehaviour
    {
        protected CompiledFacebookLoader()
        {
        }

        private void Start()
        {
            FB.facebook = this.fb;
            FB.OnDllLoaded();
            UnityEngine.Object.Destroy(this);
        }

        protected abstract IFacebook fb { get; }
    }

    public abstract class RemoteFacebookLoader : MonoBehaviour
    {
        private const string facebookNamespace = "Facebook.";
        private const int maxRetryLoadCount = 3;
        private static int retryLoadCount;

        protected RemoteFacebookLoader()
        {
        }

        [DebuggerHidden]
        public static IEnumerator LoadFacebookClass(string className, LoadedDllCallback callback)
        {
            return new <LoadFacebookClass>c__Iterator1EF { className = className, callback = callback, <$>className = className, <$>callback = callback };
        }

        private void OnDllLoaded(IFacebook fb)
        {
            FB.facebook = fb;
            FB.OnDllLoaded();
        }

        [DebuggerHidden]
        private IEnumerator Start()
        {
            return new <Start>c__Iterator1F0 { <>f__this = this };
        }

        protected abstract string className { get; }

        [CompilerGenerated]
        private sealed class <LoadFacebookClass>c__Iterator1EF : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal FB.RemoteFacebookLoader.LoadedDllCallback <$>callback;
            internal string <$>className;
            internal Assembly <assembly>__3;
            internal WWW <authTokenWww>__2;
            internal System.Type <facebookClass>__4;
            internal IFacebook <fb>__5;
            internal string <url>__0;
            internal WWW <www>__1;
            internal FB.RemoteFacebookLoader.LoadedDllCallback callback;
            internal string className;

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
                        this.<url>__0 = string.Format(IntegratedPluginCanvasLocation.DllUrl, this.className);
                        this.<www>__1 = new WWW(this.<url>__0);
                        FbDebugOverride.Log("loading dll: " + this.<url>__0);
                        this.$current = this.<www>__1;
                        this.$PC = 1;
                        goto Label_026B;

                    case 1:
                        if (this.<www>__1.error == null)
                        {
                            this.<authTokenWww>__2 = new WWW(IntegratedPluginCanvasLocation.KeyUrl);
                            this.$current = this.<authTokenWww>__2;
                            this.$PC = 2;
                            goto Label_026B;
                        }
                        FbDebugOverride.Error(this.<www>__1.error);
                        if (FB.RemoteFacebookLoader.retryLoadCount < 3)
                        {
                            FB.RemoteFacebookLoader.retryLoadCount++;
                        }
                        this.<www>__1.Dispose();
                        break;

                    case 2:
                        if (this.<authTokenWww>__2.error == null)
                        {
                            this.<assembly>__3 = Security.LoadAndVerifyAssembly(this.<www>__1.bytes, this.<authTokenWww>__2.text);
                            if (this.<assembly>__3 == null)
                            {
                                FbDebugOverride.Error("Could not securely load assembly from " + this.<url>__0);
                                this.<www>__1.Dispose();
                            }
                            else
                            {
                                this.<facebookClass>__4 = this.<assembly>__3.GetType("Facebook." + this.className);
                                if (this.<facebookClass>__4 == null)
                                {
                                    FbDebugOverride.Error(this.className + " not found in assembly!");
                                    this.<www>__1.Dispose();
                                }
                                else
                                {
                                    System.Type[] typeArguments = new System.Type[] { this.<facebookClass>__4 };
                                    object[] parameters = new object[] { IfNotExist.AddNew };
                                    this.<fb>__5 = typeof(FBComponentFactory).GetMethod("GetComponent").MakeGenericMethod(typeArguments).Invoke(null, parameters) as IFacebook;
                                    if (this.<fb>__5 == null)
                                    {
                                        FbDebugOverride.Error(this.className + " couldn't be created.");
                                        this.<www>__1.Dispose();
                                    }
                                    else
                                    {
                                        this.callback(this.<fb>__5);
                                        this.<www>__1.Dispose();
                                        this.$PC = -1;
                                    }
                                }
                            }
                            break;
                        }
                        FbDebugOverride.Error("Cannot load from " + IntegratedPluginCanvasLocation.KeyUrl + ": " + this.<authTokenWww>__2.error);
                        this.<authTokenWww>__2.Dispose();
                        break;
                }
                return false;
            Label_026B:
                return true;
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
        private sealed class <Start>c__Iterator1F0 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal FB.RemoteFacebookLoader <>f__this;
            internal IEnumerator <loader>__0;

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
                        this.<loader>__0 = FB.RemoteFacebookLoader.LoadFacebookClass(this.<>f__this.className, new FB.RemoteFacebookLoader.LoadedDllCallback(this.<>f__this.OnDllLoaded));
                        break;

                    case 1:
                        break;

                    default:
                        goto Label_008C;
                }
                if (this.<loader>__0.MoveNext())
                {
                    this.$current = this.<loader>__0.Current;
                    this.$PC = 1;
                    return true;
                }
                UnityEngine.Object.Destroy(this.<>f__this);
                this.$PC = -1;
            Label_008C:
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

        public delegate void LoadedDllCallback(IFacebook fb);
    }
}

