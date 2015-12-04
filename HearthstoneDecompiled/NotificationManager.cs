using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static readonly Vector3 ALT_ADVENTURE_SCREEN_POS = new Vector3(104.8f, DEPTH, 131.1f);
    private List<Notification> arrows;
    public GameObject bounceArrowPrefab;
    public static readonly Vector3 DEFAULT_CHARACTER_POS = new Vector3(100f, DEPTH, 24.7f);
    private const float DEFAULT_QUOTE_DURATION = 8f;
    public static readonly float DEPTH = -5f;
    public GameObject dialogBoxPrefab;
    public GameObject fadeArrowPrefab;
    public GameObject innkeeperQuotePrefab;
    public const string KT_PREFAB_NAME = "KT_Quote";
    private Notification m_quote;
    private List<string> m_quotesThisSession;
    public const string NORMAL_NEFARIAN_PREFAB_NAME = "NormalNefarian_Quote";
    private Vector3 NOTIFICATION_SCALE = ((Vector3) (0.163f * Vector3.one));
    private Vector3 NOTIFICATION_SCALE_PHONE = ((Vector3) (0.326f * Vector3.one));
    private List<Notification> notificationsToDestroyUponNewNotifier;
    public static readonly Vector3 PHONE_CHARACTER_POS = new Vector3(124.1f, DEPTH, 24.7f);
    private Notification popUpDialog;
    public GameObject popupTextPrefab;
    private List<Notification> popUpTexts;
    public const string RAGNAROS_PREFAB_NAME = "Ragnaros_Quote";
    private static NotificationManager s_instance;
    public GameObject speechBubblePrefab;
    public GameObject speechIndicatorPrefab;
    public const string TIRION_PREFAB_NAME = "Tirion_Quote";
    public const string ZOMBIE_NEFARIAN_PREFAB_NAME = "NefarianDragon_Quote";

    private void Awake()
    {
        s_instance = this;
        this.m_quotesThisSession = new List<string>();
    }

    private void ClickNotification(UIEvent e)
    {
        Notification data = (Notification) e.GetElement().GetData();
        this.NukeNotification(data);
        data.clickOff.RemoveEventListener(UIEventType.PRESS, new UIEvent.Handler(this.ClickNotification));
    }

    public Notification CreateBigCharacterQuote(string prefabName, string soundName, bool allowRepeatDuringSession = true, float durationSeconds = 0f, System.Action finishCallback = null)
    {
        return this.CreateBigCharacterQuote(prefabName, soundName, soundName, allowRepeatDuringSession, durationSeconds, finishCallback);
    }

    public Notification CreateBigCharacterQuote(string prefabName, string soundName, string textID, bool allowRepeatDuringSession = true, float durationSeconds = 0f, System.Action finishCallback = null)
    {
        if (!allowRepeatDuringSession && this.m_quotesThisSession.Contains(textID))
        {
            return null;
        }
        this.m_quotesThisSession.Add(textID);
        Notification quote = GameUtils.LoadGameObjectWithComponent<Notification>(prefabName);
        if (quote == null)
        {
            return null;
        }
        if (finishCallback != null)
        {
            quote.OnFinishDeathState = (System.Action) Delegate.Combine(quote.OnFinishDeathState, finishCallback);
        }
        this.PlayBigCharacterQuote(quote, GameStrings.Get(textID), soundName, durationSeconds);
        return quote;
    }

    public Notification CreateBouncingArrow(bool addToList)
    {
        Notification component = UnityEngine.Object.Instantiate<GameObject>(this.bounceArrowPrefab).GetComponent<Notification>();
        component.PlayBirth();
        if (addToList)
        {
            this.arrows.Add(component);
        }
        return component;
    }

    public Notification CreateBouncingArrow(Vector3 position, Vector3 rotation)
    {
        return this.CreateBouncingArrow(position, rotation, true);
    }

    public Notification CreateBouncingArrow(Vector3 position, Vector3 rotation, bool addToList)
    {
        Notification notification = this.CreateBouncingArrow(addToList);
        notification.transform.position = position;
        notification.transform.localEulerAngles = rotation;
        return notification;
    }

    public Notification CreateCharacterQuote(string prefabName, string text, string soundName, bool allowRepeatDuringSession = true, float durationSeconds = 0f, CanvasAnchor anchorPoint = 1)
    {
        return this.CreateCharacterQuote(prefabName, DEFAULT_CHARACTER_POS, text, soundName, allowRepeatDuringSession, durationSeconds, null, anchorPoint);
    }

    public Notification CreateCharacterQuote(string prefabName, Vector3 position, string text, string soundName, bool allowRepeatDuringSession = true, float durationSeconds = 0f, System.Action finishCallback = null, CanvasAnchor anchorPoint = 1)
    {
        if (!allowRepeatDuringSession && this.m_quotesThisSession.Contains(soundName))
        {
            return null;
        }
        this.m_quotesThisSession.Add(soundName);
        Notification quote = GameUtils.LoadGameObjectWithComponent<Notification>(prefabName);
        if (quote == null)
        {
            return null;
        }
        if (finishCallback != null)
        {
            quote.OnFinishDeathState = (System.Action) Delegate.Combine(quote.OnFinishDeathState, finishCallback);
        }
        this.PlayCharacterQuote(quote, position, text, soundName, durationSeconds, anchorPoint);
        return quote;
    }

    public Notification CreateFadeArrow(bool addToList)
    {
        Notification component = UnityEngine.Object.Instantiate<GameObject>(this.fadeArrowPrefab).GetComponent<Notification>();
        component.PlayBirth();
        if (addToList)
        {
            this.arrows.Add(component);
        }
        return component;
    }

    public Notification CreateFadeArrow(Vector3 position, Vector3 rotation)
    {
        return this.CreateFadeArrow(position, rotation, true);
    }

    public Notification CreateFadeArrow(Vector3 position, Vector3 rotation, bool addToList)
    {
        Notification notification = this.CreateFadeArrow(addToList);
        notification.transform.position = position;
        notification.transform.localEulerAngles = rotation;
        return notification;
    }

    public Notification CreateInnkeeperQuote(string text, string soundName, System.Action finishCallback)
    {
        return this.CreateInnkeeperQuote(DEFAULT_CHARACTER_POS, text, soundName, 0f, finishCallback);
    }

    public Notification CreateInnkeeperQuote(string text, string soundName, float durationSeconds = 0f, System.Action finishCallback = null)
    {
        return this.CreateInnkeeperQuote(DEFAULT_CHARACTER_POS, text, soundName, durationSeconds, finishCallback);
    }

    public Notification CreateInnkeeperQuote(Vector3 position, string text, string soundName, float durationSeconds = 0f, System.Action finishCallback = null)
    {
        Notification component = UnityEngine.Object.Instantiate<GameObject>(this.innkeeperQuotePrefab).GetComponent<Notification>();
        if (finishCallback != null)
        {
            component.OnFinishDeathState = (System.Action) Delegate.Combine(component.OnFinishDeathState, finishCallback);
        }
        this.PlayCharacterQuote(component, position, text, soundName, durationSeconds, CanvasAnchor.BOTTOM_LEFT);
        return component;
    }

    public Notification CreateKTQuote(string stringTag, string soundName, bool allowRepeatDuringSession = true)
    {
        return this.CreateKTQuote(DEFAULT_CHARACTER_POS, stringTag, soundName, allowRepeatDuringSession);
    }

    public Notification CreateKTQuote(Vector3 position, string stringTag, string soundName, bool allowRepeatDuringSession = true)
    {
        return this.CreateCharacterQuote("KT_Quote", position, GameStrings.Get(stringTag), soundName, allowRepeatDuringSession, 0f, null, CanvasAnchor.BOTTOM_LEFT);
    }

    public Notification CreatePopupDialog(string headlineText, string bodyText, string yesOrOKButtonText, string noButtonText)
    {
        return this.CreatePopupDialog(headlineText, bodyText, yesOrOKButtonText, noButtonText, new Vector3(0f, 0f, 0f));
    }

    public Notification CreatePopupDialog(string headlineText, string bodyText, string yesOrOKButtonText, string noButtonText, Vector3 offset)
    {
        if (this.popUpDialog != null)
        {
            UnityEngine.Object.Destroy(this.popUpDialog.gameObject);
        }
        GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(this.dialogBoxPrefab);
        Vector3 position = Camera.main.transform.position;
        obj2.transform.position = (position + new Vector3(-0.07040818f, -16.10709f, 1.79612f)) + offset;
        this.popUpDialog = obj2.GetComponent<Notification>();
        this.popUpDialog.ChangeDialogText(headlineText, bodyText, yesOrOKButtonText, noButtonText);
        this.popUpDialog.PlayBirth();
        UniversalInputManager.Get().SetGameDialogActive(true);
        return this.popUpDialog;
    }

    public void CreatePopupDialogFromObject(Notification passedInNotification, string headlineString, string bodyText, string OkButtonText)
    {
        if (this.popUpDialog != null)
        {
            UnityEngine.Object.Destroy(this.popUpDialog.gameObject);
        }
        TransformUtil.AttachAndPreserveLocalTransform(passedInNotification.gameObject.transform, OverlayUI.Get().m_heightScale.m_Center);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            passedInNotification.gameObject.transform.localScale = (Vector3) (1.5f * passedInNotification.gameObject.transform.localScale);
        }
        this.popUpDialog = passedInNotification;
        this.popUpDialog.ButtonStart.SetText(OkButtonText);
        this.popUpDialog.speechUberText.Text = bodyText;
        this.popUpDialog.headlineUberText.Text = headlineString;
        this.popUpDialog.PlayBirth();
        UniversalInputManager.Get().SetGameDialogActive(true);
    }

    public Notification CreatePopupText(Vector3 position, Vector3 scale, string text, bool convertLegacyPosition = true)
    {
        Vector3 vector = position;
        if (convertLegacyPosition)
        {
            Camera componentInChildren;
            if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
            {
                componentInChildren = BoardCameras.Get().GetComponentInChildren<Camera>();
            }
            else
            {
                componentInChildren = Box.Get().GetBoxCamera().GetComponent<Camera>();
            }
            vector = OverlayUI.Get().GetRelativePosition(position, componentInChildren, OverlayUI.Get().m_heightScale.m_Center, 0f);
        }
        GameObject go = UnityEngine.Object.Instantiate<GameObject>(this.popupTextPrefab);
        SceneUtils.SetLayer(go, GameLayer.UI);
        go.transform.localPosition = vector;
        go.transform.localScale = scale;
        OverlayUI.Get().AddGameObject(go, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        Notification component = go.GetComponent<Notification>();
        component.ChangeText(text);
        component.PlayBirth();
        this.popUpTexts.Add(component);
        return component;
    }

    public Notification CreateSpeechBubble(string speechText, Actor actor)
    {
        return this.CreateSpeechBubble(speechText, Notification.SpeechBubbleDirection.BottomLeft, actor, false, true);
    }

    public Notification CreateSpeechBubble(string speechText, Actor actor, bool bDestroyWhenNewCreated)
    {
        return this.CreateSpeechBubble(speechText, Notification.SpeechBubbleDirection.BottomLeft, actor, bDestroyWhenNewCreated, true);
    }

    public Notification CreateSpeechBubble(string speechText, Notification.SpeechBubbleDirection direction, Actor actor)
    {
        return this.CreateSpeechBubble(speechText, direction, actor, false, true);
    }

    public Notification CreateSpeechBubble(string speechText, Notification.SpeechBubbleDirection direction, Actor actor, bool bDestroyWhenNewCreated, bool parentToActor = true)
    {
        Notification component;
        this.DestroyOtherNotifications(direction);
        if (speechText == string.Empty)
        {
            component = UnityEngine.Object.Instantiate<GameObject>(this.speechIndicatorPrefab).GetComponent<Notification>();
            component.PlaySmallBirthForFakeBubble();
            component.SetPositionForSmallBubble(actor);
        }
        else
        {
            component = UnityEngine.Object.Instantiate<GameObject>(this.speechBubblePrefab).GetComponent<Notification>();
            component.ChangeText(speechText);
            component.FaceDirection(direction);
            component.PlayBirth();
            component.SetPosition(actor, direction);
        }
        if (bDestroyWhenNewCreated)
        {
            this.notificationsToDestroyUponNewNotifier.Add(component);
        }
        if (parentToActor)
        {
            component.transform.parent = actor.transform;
        }
        return component;
    }

    public Notification CreateTirionQuote(string stringTag, string soundName, bool allowRepeatDuringSession = true)
    {
        return this.CreateTirionQuote(DEFAULT_CHARACTER_POS, stringTag, soundName, allowRepeatDuringSession);
    }

    public Notification CreateTirionQuote(Vector3 position, string stringTag, string soundName, bool allowRepeatDuringSession = true)
    {
        return this.CreateCharacterQuote("Tirion_Quote", position, GameStrings.Get(stringTag), soundName, allowRepeatDuringSession, 0f, null, CanvasAnchor.BOTTOM_LEFT);
    }

    public Notification CreateZombieNefarianQuote(Vector3 position, string stringTag, string soundName, bool allowRepeatDuringSession)
    {
        return this.CreateCharacterQuote("NefarianDragon_Quote", position, GameStrings.Get(stringTag), soundName, allowRepeatDuringSession, 0f, null, CanvasAnchor.BOTTOM_LEFT);
    }

    public void DestroyActiveNotification(float delaySeconds)
    {
        if (this.popUpDialog != null)
        {
            if (delaySeconds == 0f)
            {
                this.NukeNotification(this.popUpDialog);
            }
            else
            {
                base.StartCoroutine(this.WaitAndThenDestroyNotification(this.popUpDialog, delaySeconds));
            }
        }
    }

    public void DestroyActiveQuote(float delaySeconds)
    {
        if (this.m_quote != null)
        {
            if (delaySeconds == 0f)
            {
                this.NukeNotification(this.m_quote);
            }
            else
            {
                base.StartCoroutine(this.WaitAndThenDestroyNotification(this.m_quote, delaySeconds));
            }
        }
    }

    public void DestroyAllArrows()
    {
        if (this.arrows.Count != 0)
        {
            for (int i = 0; i < this.arrows.Count; i++)
            {
                if (this.arrows[i] != null)
                {
                    this.NukeNotificationWithoutPlayingAnim(this.arrows[i]);
                }
            }
        }
    }

    public void DestroyAllPopUps()
    {
        if (this.popUpTexts.Count != 0)
        {
            for (int i = 0; i < this.popUpTexts.Count; i++)
            {
                if (this.popUpTexts[i] != null)
                {
                    this.NukeNotification(this.popUpTexts[i]);
                }
            }
            this.popUpTexts = new List<Notification>();
        }
    }

    public void DestroyNotification(Notification notification, float delaySeconds)
    {
        if (notification != null)
        {
            if (delaySeconds == 0f)
            {
                this.NukeNotification(notification);
            }
            else
            {
                base.StartCoroutine(this.WaitAndThenDestroyNotification(notification, delaySeconds));
            }
        }
    }

    public void DestroyNotificationNowWithNoAnim(Notification notification)
    {
        if (notification != null)
        {
            this.NukeNotificationWithoutPlayingAnim(notification);
        }
    }

    private void DestroyOtherNotifications(Notification.SpeechBubbleDirection direction)
    {
        if (this.notificationsToDestroyUponNewNotifier.Count != 0)
        {
            for (int i = 0; i < this.notificationsToDestroyUponNewNotifier.Count; i++)
            {
                if ((this.notificationsToDestroyUponNewNotifier[i] != null) && (this.notificationsToDestroyUponNewNotifier[i].GetSpeechBubbleDirection() == direction))
                {
                    this.NukeNotificationWithoutPlayingAnim(this.notificationsToDestroyUponNewNotifier[i]);
                }
            }
        }
    }

    public void ForceAddSoundToPlayedList(string soundName)
    {
        this.m_quotesThisSession.Add(soundName);
    }

    public void ForceRemoveSoundFromPlayedList(string soundName)
    {
        this.m_quotesThisSession.Remove(soundName);
    }

    public static NotificationManager Get()
    {
        return s_instance;
    }

    public bool HasSoundPlayedThisSession(string soundName)
    {
        return this.m_quotesThisSession.Contains(soundName);
    }

    public Notification MessageBox(string headlineText, string bodyText, string yesOrOKButtonText, UIEvent.Handler callback, Vector3 offset)
    {
        Notification notification = Get().CreatePopupDialog(headlineText, bodyText, yesOrOKButtonText, string.Empty, offset);
        notification.ButtonOK.AddEventListener(UIEventType.RELEASE, callback);
        return notification;
    }

    private void NukeNotification(Notification notification)
    {
        if (this.notificationsToDestroyUponNewNotifier.Contains(notification))
        {
            this.notificationsToDestroyUponNewNotifier.Remove(notification);
        }
        if (!notification.IsDying())
        {
            notification.PlayDeath();
            UniversalInputManager.Get().SetGameDialogActive(false);
        }
    }

    private void NukeNotificationWithoutPlayingAnim(Notification notification)
    {
        if (this.notificationsToDestroyUponNewNotifier.Contains(notification))
        {
            this.notificationsToDestroyUponNewNotifier.Remove(notification);
        }
        UnityEngine.Object.Destroy(notification.gameObject);
        UniversalInputManager.Get().SetGameDialogActive(false);
    }

    private void OnBigQuoteSoundLoaded(string name, GameObject go, object userData)
    {
        QuoteSoundCallbackData data = (QuoteSoundCallbackData) userData;
        if (data.m_quote == null)
        {
            UnityEngine.Object.Destroy(go);
        }
        else if (go == null)
        {
            UnityEngine.Debug.LogWarning("Quote Sound failed to load!");
            this.PlayQuoteWithoutSound((data.m_durationSeconds <= 0f) ? 8f : data.m_durationSeconds);
        }
        else
        {
            AudioSource component = go.GetComponent<AudioSource>();
            this.m_quote.AssignAudio(component);
            SoundManager.Get().PlayPreloaded(component);
            this.m_quote.PlayBirthWithForcedScale(Vector3.one);
            float delaySeconds = Mathf.Max(data.m_durationSeconds, component.clip.length);
            this.DestroyNotification(this.m_quote, delaySeconds);
            if (this.m_quote.clickOff != null)
            {
                this.m_quote.clickOff.SetData(this.m_quote);
                this.m_quote.clickOff.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.ClickNotification));
            }
        }
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnQuoteSoundLoaded(string name, GameObject go, object userData)
    {
        QuoteSoundCallbackData data = (QuoteSoundCallbackData) userData;
        if (data.m_quote == null)
        {
            UnityEngine.Object.Destroy(go);
        }
        else if (go == null)
        {
            UnityEngine.Debug.LogWarning("Quote Sound failed to load!");
            this.PlayQuoteWithoutSound((data.m_durationSeconds <= 0f) ? 8f : data.m_durationSeconds);
        }
        else
        {
            AudioSource component = go.GetComponent<AudioSource>();
            this.m_quote.AssignAudio(component);
            SoundManager.Get().PlayPreloaded(component);
            this.m_quote.PlayBirthWithForcedScale((UniversalInputManager.UsePhoneUI == null) ? this.NOTIFICATION_SCALE : this.NOTIFICATION_SCALE_PHONE);
            float delaySeconds = Mathf.Max(data.m_durationSeconds, component.clip.length);
            this.DestroyNotification(this.m_quote, delaySeconds);
            if (this.m_quote.clickOff != null)
            {
                this.m_quote.clickOff.SetData(this.m_quote);
                this.m_quote.clickOff.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.ClickNotification));
            }
        }
    }

    private void PlayBigCharacterQuote(Notification quote, string text, string soundName, float durationSeconds)
    {
        if (this.m_quote != null)
        {
            UnityEngine.Object.Destroy(this.m_quote.gameObject);
        }
        this.m_quote = quote;
        this.m_quote.ChangeText(text);
        TransformUtil.AttachAndPreserveLocalTransform(this.m_quote.transform, Board.Get().FindBone("OffScreenSpeaker1"));
        this.m_quote.transform.localPosition = Vector3.zero;
        this.m_quote.transform.localScale = (Vector3) (Vector3.one * 0.01f);
        this.m_quote.transform.localEulerAngles = Vector3.zero;
        if (!string.IsNullOrEmpty(soundName))
        {
            QuoteSoundCallbackData callbackData = new QuoteSoundCallbackData {
                m_quote = this.m_quote,
                m_durationSeconds = durationSeconds
            };
            AssetLoader.Get().LoadSound(soundName, new AssetLoader.GameObjectCallback(this.OnBigQuoteSoundLoaded), callbackData, false, SoundManager.Get().GetPlaceholderSound());
        }
        else
        {
            this.m_quote.PlayBirthWithForcedScale(Vector3.one);
            if (durationSeconds > 0f)
            {
                this.DestroyNotification(this.m_quote, durationSeconds);
            }
        }
    }

    private void PlayCharacterQuote(Notification quote, Vector3 position, string text, string soundName, float durationSeconds, CanvasAnchor anchorPoint = 1)
    {
        if (this.m_quote != null)
        {
            UnityEngine.Object.Destroy(this.m_quote.gameObject);
        }
        this.m_quote = quote;
        this.m_quote.ChangeText(text);
        this.m_quote.transform.position = position;
        this.m_quote.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        OverlayUI.Get().AddGameObject(this.m_quote.gameObject, anchorPoint, false, CanvasScaleMode.HEIGHT);
        if (!string.IsNullOrEmpty(soundName))
        {
            QuoteSoundCallbackData callbackData = new QuoteSoundCallbackData {
                m_quote = this.m_quote,
                m_durationSeconds = durationSeconds
            };
            AssetLoader.Get().LoadSound(soundName, new AssetLoader.GameObjectCallback(this.OnQuoteSoundLoaded), callbackData, false, SoundManager.Get().GetPlaceholderSound());
        }
        else
        {
            this.PlayQuoteWithoutSound(durationSeconds);
        }
    }

    private void PlayQuoteWithoutSound(float durationSeconds)
    {
        this.m_quote.PlayBirthWithForcedScale((UniversalInputManager.UsePhoneUI == null) ? this.NOTIFICATION_SCALE : this.NOTIFICATION_SCALE_PHONE);
        if (durationSeconds > 0f)
        {
            this.DestroyNotification(this.m_quote, durationSeconds);
        }
    }

    private void Start()
    {
        this.notificationsToDestroyUponNewNotifier = new List<Notification>();
        this.arrows = new List<Notification>();
        this.popUpTexts = new List<Notification>();
    }

    [DebuggerHidden]
    private IEnumerator WaitAndThenDestroyNotification(Notification notification, float amountSeconds)
    {
        return new <WaitAndThenDestroyNotification>c__Iterator262 { amountSeconds = amountSeconds, notification = notification, <$>amountSeconds = amountSeconds, <$>notification = notification, <>f__this = this };
    }

    public bool IsQuotePlaying
    {
        get
        {
            return (this.m_quote != null);
        }
    }

    [CompilerGenerated]
    private sealed class <WaitAndThenDestroyNotification>c__Iterator262 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>amountSeconds;
        internal Notification <$>notification;
        internal NotificationManager <>f__this;
        internal float amountSeconds;
        internal Notification notification;

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
                    this.$current = new WaitForSeconds(this.amountSeconds);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.notification != null)
                    {
                        this.<>f__this.NukeNotification(this.notification);
                    }
                    this.$PC = -1;
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

    private class QuoteSoundCallbackData
    {
        public float m_durationSeconds;
        public Notification m_quote;
    }
}

