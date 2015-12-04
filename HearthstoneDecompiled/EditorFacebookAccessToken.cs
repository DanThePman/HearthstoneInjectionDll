using Facebook;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EditorFacebookAccessToken : MonoBehaviour
{
    private string accessToken = string.Empty;
    private static GUISkin fbSkin;
    private GUIStyle greyButton;
    private bool isLoggingIn;
    private float windowHeight = 200f;
    private const float windowWidth = 592f;

    private void OnGUI()
    {
        float top = (Screen.height / 2) - (this.windowHeight / 2f);
        float left = (Screen.width / 2) - 296f;
        if (fbSkin != null)
        {
            GUI.skin = fbSkin;
            this.greyButton = fbSkin.GetStyle("greyButton");
        }
        else
        {
            this.greyButton = GUI.skin.button;
        }
        GUI.ModalWindow(this.GetHashCode(), new Rect(left, top, 592f, this.windowHeight), new GUI.WindowFunction(this.OnGUIDialog), "Unity Editor Facebook Login");
    }

    private void OnGUIDialog(int windowId)
    {
        GUI.enabled = !this.isLoggingIn;
        GUILayout.Space(10f);
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        GUILayout.BeginVertical(new GUILayoutOption[0]);
        GUILayout.Space(10f);
        GUILayout.Label("User Access Token:", new GUILayoutOption[0]);
        GUILayout.EndVertical();
        GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.MinWidth(400f) };
        this.accessToken = GUILayout.TextField(this.accessToken, GUI.skin.textArea, options);
        GUILayout.EndHorizontal();
        GUILayout.Space(20f);
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        if (GUILayout.Button("Find Access Token", new GUILayoutOption[0]))
        {
            Application.OpenURL(string.Format("https://developers.facebook.com/tools/accesstoken/?app_id={0}", FB.AppId));
        }
        GUILayout.FlexibleSpace();
        GUIContent content = new GUIContent("Login");
        if (GUI.Button(GUILayoutUtility.GetRect(content, GUI.skin.button), content))
        {
            EditorFacebook component = FBComponentFactory.GetComponent<EditorFacebook>(IfNotExist.AddNew);
            component.AccessToken = this.accessToken;
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData["batch"] = "[{\"method\":\"GET\", \"relative_url\":\"me?fields=id\"},{\"method\":\"GET\", \"relative_url\":\"app?fields=id\"}]";
            formData["method"] = "POST";
            formData["access_token"] = this.accessToken;
            FB.API("/", HttpMethod.GET, new FacebookDelegate(component.MockLoginCallback), formData);
            this.isLoggingIn = true;
        }
        GUI.enabled = true;
        GUIContent content2 = new GUIContent("Cancel");
        Rect rect = GUILayoutUtility.GetRect(content2, this.greyButton);
        if (GUI.Button(rect, content2, this.greyButton))
        {
            FBComponentFactory.GetComponent<EditorFacebook>(IfNotExist.AddNew).MockCancelledLoginCallback();
            UnityEngine.Object.Destroy(this);
        }
        GUILayout.EndHorizontal();
        if (Event.current.type == EventType.Repaint)
        {
            this.windowHeight = (rect.y + rect.height) + GUI.skin.window.padding.bottom;
        }
    }

    [DebuggerHidden]
    private IEnumerator Start()
    {
        return new <Start>c__Iterator1F2();
    }

    [CompilerGenerated]
    private sealed class <Start>c__Iterator1F2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <downloadUrl>__0;
        internal WWW <www>__1;

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
                    if (EditorFacebookAccessToken.fbSkin == null)
                    {
                        this.<downloadUrl>__0 = IntegratedPluginCanvasLocation.FbSkinUrl;
                        this.<www>__1 = new WWW(this.<downloadUrl>__0);
                        this.$current = this.<www>__1;
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    if (this.<www>__1.error == null)
                    {
                        EditorFacebookAccessToken.fbSkin = this.<www>__1.assetBundle.mainAsset as GUISkin;
                        this.<www>__1.assetBundle.Unload(false);
                        this.$PC = -1;
                        break;
                    }
                    FbDebugOverride.Error("Could not find the Facebook Skin: " + this.<www>__1.error);
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

