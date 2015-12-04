using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CollectionDeckBoxVisual : PegUIElement
{
    private const float ADJUST_Y_OFFSET_ANIM_TIME = 0.05f;
    private static readonly Map<TAG_CLASS, Color> BANNER_COLORS;
    private static readonly Color BASIC_SET_COLOR_IN_PROGRESS = new Color(0.97f, 0.82f, 0.22f);
    private const float BUTTON_POP_SPEED = 6f;
    private static readonly Map<TAG_CLASS, Vector2> CLASS_ICON_OFFSETS;
    private const string DECKBOX_POPDOWN_ANIM_NAME = "Deck_PopDown";
    private const string DECKBOX_POPUP_ANIM_NAME = "Deck_PopUp";
    private bool m_animateButtonPress = true;
    private Material m_bannerMaterial;
    public CustomDeckBones m_bones;
    private CardDef m_cardDef;
    public int m_classBannerMaterialIndex;
    private Material m_classIconMaterial;
    public int m_classIconMaterialIndex;
    public GameObject m_classObject;
    private Transform m_customDeckTransform;
    private long m_deckID = -1L;
    public UberText m_deckName;
    public PegUIElement m_deleteButton;
    public GameObject m_editDecal;
    private EntityDef m_entityDef;
    private FullDef m_fullDef;
    private string m_heroCardID = string.Empty;
    public GameObject m_highlight;
    private HighlightState m_highlightState;
    public GameObject m_invalidDeckX;
    private bool m_isPoppedUp;
    private bool m_isShown;
    private bool m_isValid = true;
    public GameObject m_labelGradient;
    private Vector3 m_originalButtonPosition;
    public int m_portraitMaterialIndex;
    public GameObject m_portraitObject;
    private int m_positionIndex;
    public GameObject m_pressedBone;
    public UberText m_setProgressLabel;
    private bool m_wasTouchModeEnabled;
    public static readonly Vector3 POPPED_DOWN_LOCAL_POS = new Vector3(0f, -0.8598533f, 0f);
    public static readonly float POPPED_UP_LOCAL_Z = 0f;
    private const float SCALE_TIME = 0.2f;
    private static readonly Vector3 SCALED_DOWN_LOCAL_SCALE = new Vector3(0.95f, 0.95f, 0.95f);
    private const float SCALED_DOWN_LOCAL_Y_OFFSET = 1.273138f;
    private Vector3 SCALED_UP_DECK_OFFSET = new Vector3(0f, 0f, 0f);
    private Vector3 SCALED_UP_LOCAL_SCALE = new Vector3(1.195666f, 1.195666f, 1.195666f);
    private const float SCALED_UP_LOCAL_Y_OFFSET = 3.238702f;

    static CollectionDeckBoxVisual()
    {
        Map<TAG_CLASS, Color> map = new Map<TAG_CLASS, Color>();
        map.Add(TAG_CLASS.MAGE, new Color(0.1294118f, 0.2666667f, 0.3882353f));
        map.Add(TAG_CLASS.PALADIN, new Color(0.4392157f, 0.2941177f, 0.09019608f));
        map.Add(TAG_CLASS.PRIEST, new Color(0.5215687f, 0.5215687f, 0.5215687f));
        map.Add(TAG_CLASS.ROGUE, new Color(0.09019608f, 0.07450981f, 0.07450981f));
        map.Add(TAG_CLASS.SHAMAN, new Color(0.1294118f, 0.172549f, 0.372549f));
        map.Add(TAG_CLASS.WARLOCK, new Color(0.2117647f, 0.1098039f, 0.282353f));
        map.Add(TAG_CLASS.WARRIOR, new Color(0.2745098f, 0.05098039f, 0.08235294f));
        map.Add(TAG_CLASS.DRUID, new Color(0.2313726f, 0.1607843f, 0.08627451f));
        map.Add(TAG_CLASS.HUNTER, new Color(0.07450981f, 0.2313726f, 0.06666667f));
        map.Add(TAG_CLASS.INVALID, new Color(0f, 0f, 0f));
        BANNER_COLORS = map;
        Map<TAG_CLASS, Vector2> map2 = new Map<TAG_CLASS, Vector2>();
        map2.Add(TAG_CLASS.MAGE, new Vector2(0f, 0f));
        map2.Add(TAG_CLASS.PALADIN, new Vector2(0.2f, 0f));
        map2.Add(TAG_CLASS.PRIEST, new Vector2(0.39f, 0f));
        map2.Add(TAG_CLASS.ROGUE, new Vector2(0.578f, 0f));
        map2.Add(TAG_CLASS.SHAMAN, new Vector2(0.77f, 0f));
        map2.Add(TAG_CLASS.WARLOCK, new Vector2(0f, 0.8f));
        map2.Add(TAG_CLASS.WARRIOR, new Vector2(0.2f, 0.8f));
        map2.Add(TAG_CLASS.DRUID, new Vector2(0.39f, 0.8f));
        map2.Add(TAG_CLASS.HUNTER, new Vector2(0.578f, 0.8f));
        map2.Add(TAG_CLASS.INVALID, new Vector2(0f, 0f));
        CLASS_ICON_OFFSETS = map2;
    }

    public void AssignFromCollectionDeck(CollectionDeck deck)
    {
        if (deck != null)
        {
            this.SetDeckName(deck.Name);
            this.SetDeckID(deck.ID);
            this.SetHeroCardID(deck.HeroCardID);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        base.SetEnabled(false);
        this.m_deleteButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDeleteButtonPressed));
        this.m_deleteButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnDeleteButtonOver));
        this.m_deleteButton.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnDeleteButtonRollout));
        this.ShowEditDecal(false);
        this.ShowDeleteButton(false);
        this.m_invalidDeckX.SetActive(false);
        this.m_deckName.RichText = false;
        SoundManager.Get().Load("tiny_button_press_1");
        SoundManager.Get().Load("tiny_button_mouseover_1");
        this.m_customDeckTransform = base.transform.FindChild("CustomDeck");
        if (PlatformSettings.s_screen == ScreenCategory.Phone)
        {
            this.SCALED_UP_LOCAL_SCALE = new Vector3(1.1f, 1.1f, 1.1f);
            this.SCALED_UP_DECK_OFFSET = new Vector3(0f, -0.2f, 0f);
        }
        this.m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
    }

    [DebuggerHidden]
    private IEnumerator ChangeHighlightStateWhenReady(ActorStateType stateType)
    {
        return new <ChangeHighlightStateWhenReady>c__Iterator2B { stateType = stateType, <$>stateType = stateType, <>f__this = this };
    }

    public void DisableButtonAnimation()
    {
        this.m_animateButtonPress = false;
    }

    public void EnableButtonAnimation()
    {
        this.m_animateButtonPress = true;
    }

    public CardDef GetCardDef()
    {
        return this.m_cardDef;
    }

    public TAG_CLASS GetClass()
    {
        if (this.m_entityDef == null)
        {
            return TAG_CLASS.INVALID;
        }
        return this.m_entityDef.GetClass();
    }

    public long GetDeckID()
    {
        return this.m_deckID;
    }

    public UberText GetDeckNameText()
    {
        return this.m_deckName;
    }

    public FullDef GetFullDef()
    {
        return this.m_fullDef;
    }

    public string GetHeroCardID()
    {
        return this.m_heroCardID;
    }

    public Texture GetHeroPortraitTexture()
    {
        if (this.m_cardDef == null)
        {
            return null;
        }
        return this.m_cardDef.GetPortraitTexture();
    }

    public int GetPositionIndex()
    {
        return this.m_positionIndex;
    }

    public void Hide()
    {
        base.gameObject.SetActive(false);
        this.m_isShown = false;
    }

    public void HideBanner()
    {
        this.m_classObject.SetActive(false);
    }

    public void HideDeckName()
    {
        this.m_deckName.gameObject.SetActive(false);
    }

    public bool IsShown()
    {
        return this.m_isShown;
    }

    public bool IsValid()
    {
        return this.m_isValid;
    }

    private void OnDeleteButtonConfirmationResponse(AlertPopup.Response response, object userData)
    {
        if (response != AlertPopup.Response.CANCEL)
        {
            base.SetEnabled(false);
            CollectionDeckTray.Get().GetDecksContent().DeleteDeck(this.GetDeckID());
        }
    }

    private void OnDeleteButtonOver(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("tiny_button_mouseover_1", base.gameObject);
    }

    private void OnDeleteButtonPressed(UIEvent e)
    {
        if (!CollectionDeckTray.Get().IsShowingDeckContents())
        {
            SoundManager.Get().LoadAndPlay("tiny_button_press_1", base.gameObject);
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_HEADER"),
                m_text = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_DESC"),
                m_showAlertIcon = false,
                m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                m_responseCallback = new AlertPopup.ResponseCallback(this.OnDeleteButtonConfirmationResponse)
            };
            DialogManager.Get().ShowPopup(info);
        }
    }

    private void OnDeleteButtonRollout(UIEvent e)
    {
        this.ShowDeleteButton(false);
    }

    private void OnHeroFullDefLoaded(string cardID, FullDef def, object userData)
    {
        object[] args = new object[] { cardID, this.m_heroCardID };
        Log.Kyle.Print("OnHeroFullDefLoaded cardID: {0},  m_heroCardID: {1}", args);
        if (cardID.Equals(this.m_heroCardID))
        {
            this.m_fullDef = def;
            this.m_entityDef = def.GetEntityDef();
            this.m_cardDef = def.GetCardDef();
            this.SetPortrait(this.m_cardDef.GetCustomDeckPortrait());
            TAG_CLASS classTag = this.m_entityDef.GetClass();
            this.SetClassDisplay(classTag);
            this.SetBasicSetProgress(classTag);
        }
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.OnOutEvent();
    }

    private void OnOutEvent()
    {
        this.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
        object[] args = new object[] { "position", this.m_originalButtonPosition, "isLocal", true, "time", 0.1, "easeType", iTween.EaseType.linear };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_portraitObject, hashtable);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        this.OnOverEvent();
    }

    private void OnOverEvent()
    {
        SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over", base.gameObject);
        this.SetHighlightState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
    }

    protected override void OnPress()
    {
        if (this.m_animateButtonPress)
        {
            this.OnPressEvent();
        }
    }

    private void OnPressEvent()
    {
        object[] args = new object[] { "position", this.m_pressedBone.transform.localPosition, "isLocal", true, "time", 0.1, "easeType", iTween.EaseType.linear };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_portraitObject, hashtable);
    }

    protected override void OnRelease()
    {
        if ((SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL) || UniversalInputManager.Get().IsTouchMode())
        {
            SoundManager.Get().LoadAndPlay("tournament_screen_select_hero", base.gameObject);
        }
        this.OnReleaseEvent();
    }

    private void OnReleaseEvent()
    {
        object[] args = new object[] { "position", this.m_originalButtonPosition, "isLocal", true, "time", 0.1, "easeType", iTween.EaseType.linear };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_portraitObject, hashtable);
    }

    private void OnScaleComplete(OnScaleFinishedCallbackData callbackData)
    {
        if (callbackData.m_callback != null)
        {
            callbackData.m_callback(callbackData.m_callbackData);
        }
    }

    private void OnScaledDown(object callbackData)
    {
        OnScaleFinishedCallbackData data = callbackData as OnScaleFinishedCallbackData;
        Vector3 localPosition = base.transform.localPosition;
        localPosition.y = 1.273138f;
        object[] args = new object[] { "position", localPosition, "isLocal", true, "time", 0.05f, "easetype", iTween.EaseType.linear, "oncomplete", "ScaleDownComplete", "oncompletetarget", base.gameObject, "oncompleteparams", data };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(base.gameObject, hashtable);
    }

    private void PlayPopAnimation(string animationName)
    {
        this.PlayPopAnimation(animationName, null, null);
    }

    private void PlayPopAnimation(string animationName, DelOnAnimationFinished callback, object callbackData)
    {
        base.GetComponent<Animation>().Play(animationName);
        OnPopAnimationFinishedCallbackData data = new OnPopAnimationFinishedCallbackData {
            m_callback = callback,
            m_callbackData = callbackData,
            m_animationName = animationName
        };
        base.StopCoroutine("WaitThenCallAnimationCallback");
        base.StartCoroutine("WaitThenCallAnimationCallback", data);
    }

    public void PlayPopDownAnimation()
    {
        this.PlayPopDownAnimation(null);
    }

    public void PlayPopDownAnimation(DelOnAnimationFinished callback)
    {
        this.PlayPopDownAnimation(callback, null);
    }

    public void PlayPopDownAnimation(DelOnAnimationFinished callback, object callbackData)
    {
        if (!this.m_isPoppedUp)
        {
            if (callback != null)
            {
                callback(callbackData);
            }
        }
        else
        {
            this.m_isPoppedUp = false;
            if (this.m_customDeckTransform != null)
            {
                this.m_customDeckTransform.localPosition -= this.SCALED_UP_DECK_OFFSET;
            }
            base.GetComponent<Animation>()["Deck_PopDown"].time = 0f;
            base.GetComponent<Animation>()["Deck_PopDown"].speed = 6f;
            this.PlayPopAnimation("Deck_PopDown", callback, callbackData);
        }
    }

    public void PlayPopDownAnimationImmediately()
    {
        this.PlayPopDownAnimationImmediately(null);
    }

    public void PlayPopDownAnimationImmediately(DelOnAnimationFinished callback)
    {
        this.PlayPopDownAnimationImmediately(callback, null);
    }

    public void PlayPopDownAnimationImmediately(DelOnAnimationFinished callback, object callbackData)
    {
        if (!this.m_isPoppedUp)
        {
            if (callback != null)
            {
                callback(callbackData);
            }
        }
        else
        {
            this.m_isPoppedUp = false;
            base.GetComponent<Animation>()["Deck_PopDown"].time = base.GetComponent<Animation>()["Deck_PopDown"].length;
            base.GetComponent<Animation>()["Deck_PopDown"].speed = 1f;
            this.PlayPopAnimation("Deck_PopDown", callback, callbackData);
        }
    }

    public void PlayPopUpAnimation()
    {
        this.PlayPopUpAnimation(null);
    }

    public void PlayPopUpAnimation(DelOnAnimationFinished callback)
    {
        this.PlayPopUpAnimation(callback, null);
    }

    public void PlayPopUpAnimation(DelOnAnimationFinished callback, object callbackData)
    {
        if (this.m_isPoppedUp)
        {
            if (callback != null)
            {
                callback(callbackData);
            }
        }
        else
        {
            this.m_isPoppedUp = true;
            if (this.m_customDeckTransform != null)
            {
                this.m_customDeckTransform.localPosition += this.SCALED_UP_DECK_OFFSET;
            }
            base.GetComponent<Animation>()["Deck_PopUp"].time = 0f;
            base.GetComponent<Animation>()["Deck_PopUp"].speed = 6f;
            this.PlayPopAnimation("Deck_PopUp", callback, callbackData);
        }
    }

    public void PlayScaleDownAnimation()
    {
        this.PlayScaleDownAnimation(null);
    }

    public void PlayScaleDownAnimation(DelOnAnimationFinished callback)
    {
        this.PlayScaleDownAnimation(callback, null);
    }

    public void PlayScaleDownAnimation(DelOnAnimationFinished callback, object callbackData)
    {
        OnScaleFinishedCallbackData data = new OnScaleFinishedCallbackData {
            m_callback = callback,
            m_callbackData = callbackData
        };
        this.ScaleDeckBox(false, new DelOnAnimationFinished(this.OnScaledDown), data);
    }

    public void PlayScaleUpAnimation()
    {
        this.PlayScaleUpAnimation(null);
    }

    public void PlayScaleUpAnimation(DelOnAnimationFinished callback)
    {
        this.PlayScaleUpAnimation(callback, null);
    }

    public void PlayScaleUpAnimation(DelOnAnimationFinished callback, object callbackData)
    {
        OnScaleFinishedCallbackData data = new OnScaleFinishedCallbackData {
            m_callback = callback,
            m_callbackData = callbackData
        };
        Vector3 localPosition = base.transform.localPosition;
        localPosition.y = 3.238702f;
        object[] args = new object[] { "position", localPosition, "isLocal", true, "time", 0.05f, "easetype", iTween.EaseType.linear, "oncomplete", "ScaleUpNow", "oncompletetarget", base.gameObject, "oncompleteparams", data };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(base.gameObject, hashtable);
    }

    private void ScaleDeckBox(bool scaleUp, DelOnAnimationFinished callback, object callbackData)
    {
        OnScaleFinishedCallbackData data = new OnScaleFinishedCallbackData {
            m_callback = callback,
            m_callbackData = callbackData
        };
        Vector3 vector = !scaleUp ? SCALED_DOWN_LOCAL_SCALE : this.SCALED_UP_LOCAL_SCALE;
        object[] args = new object[] { "scale", vector, "isLocal", true, "time", 0.2f, "easetype", iTween.EaseType.linear, "oncomplete", "OnScaleComplete", "oncompletetarget", base.gameObject, "oncompleteparams", data, "name", "scale" };
        Hashtable hashtable = iTween.Hash(args);
        iTween.StopByName(base.gameObject, "scale");
        iTween.ScaleTo(base.gameObject, hashtable);
    }

    private void ScaleDownComplete(OnScaleFinishedCallbackData onScaledDownData)
    {
        if (onScaledDownData.m_callback != null)
        {
            onScaledDownData.m_callback(onScaledDownData.m_callbackData);
        }
    }

    private void ScaleUpNow(OnScaleFinishedCallbackData readyToScaleUpData)
    {
        this.ScaleDeckBox(true, readyToScaleUpData.m_callback, readyToScaleUpData.m_callbackData);
    }

    public void SetBasicSetProgress(TAG_CLASS classTag)
    {
        int basicCardsIOwn = CollectionManager.Get().GetBasicCardsIOwn(classTag);
        int num2 = 20;
        if ((basicCardsIOwn == num2) || (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER))
        {
            this.m_deckName.transform.position = this.m_bones.m_deckLabelOneLine.position;
            this.m_labelGradient.transform.parent = this.m_bones.m_gradientOneLine;
            this.m_labelGradient.transform.localPosition = Vector3.zero;
            this.m_labelGradient.transform.localScale = Vector3.one;
            this.m_setProgressLabel.gameObject.SetActive(false);
        }
        else
        {
            this.m_deckName.transform.position = this.m_bones.m_deckLabelTwoLine.position;
            this.m_labelGradient.transform.parent = this.m_bones.m_gradientTwoLine;
            this.m_labelGradient.transform.localPosition = Vector3.zero;
            this.m_labelGradient.transform.localScale = Vector3.one;
            this.m_setProgressLabel.gameObject.SetActive(true);
            object[] args = new object[] { basicCardsIOwn, num2 };
            this.m_setProgressLabel.Text = GameStrings.Format((UniversalInputManager.UsePhoneUI == null) ? "GLUE_BASIC_SET_PROGRESS" : "GLUE_BASIC_SET_PROGRESS_PHONE", args);
            this.m_setProgressLabel.TextColor = BASIC_SET_COLOR_IN_PROGRESS;
        }
    }

    private void SetClassDisplay(TAG_CLASS classTag)
    {
        MeshRenderer component = this.m_classObject.GetComponent<MeshRenderer>();
        if (component != null)
        {
            this.m_classIconMaterial = component.materials[this.m_classIconMaterialIndex];
            this.m_bannerMaterial = component.materials[this.m_classBannerMaterialIndex];
            if ((this.m_classIconMaterial != null) && (this.m_bannerMaterial != null))
            {
                this.m_classIconMaterial.mainTextureOffset = CLASS_ICON_OFFSETS[classTag];
                this.m_bannerMaterial.color = BANNER_COLORS[classTag];
            }
        }
    }

    public void SetDeckID(long id)
    {
        this.m_deckID = id;
    }

    public void SetDeckName(string deckName)
    {
        this.m_deckName.Text = deckName;
    }

    public void SetHeroCardID(string heroCardID)
    {
        this.m_heroCardID = heroCardID;
        DefLoader.Get().LoadFullDef(heroCardID, new DefLoader.LoadDefCallback<FullDef>(this.OnHeroFullDefLoaded));
    }

    public void SetHighlightState(ActorStateType stateType)
    {
        if (this.m_highlightState == null)
        {
            this.m_highlightState = base.gameObject.transform.parent.GetComponentInChildren<HighlightState>();
        }
        if (this.m_highlightState != null)
        {
            if (!this.m_highlightState.IsReady())
            {
                base.StartCoroutine(this.ChangeHighlightStateWhenReady(stateType));
            }
            else
            {
                this.m_highlightState.ChangeState(stateType);
            }
        }
    }

    public void SetIsValid(bool isValid)
    {
        if (this.m_isValid != isValid)
        {
            this.m_isValid = isValid;
            this.m_invalidDeckX.SetActive(!this.m_isValid);
        }
    }

    public void SetOriginalButtonPosition()
    {
        this.m_originalButtonPosition = this.m_portraitObject.transform.localPosition;
    }

    private void SetPortrait(Material portraitMaterial)
    {
        if (portraitMaterial == null)
        {
            UnityEngine.Debug.LogError("Custom Deck Portrait Material is null!");
        }
        else
        {
            RenderUtils.SetSharedMaterial(this.m_portraitObject, this.m_portraitMaterialIndex, portraitMaterial);
            if ((this.m_cardDef != null) && ((CollectionManager.Get().GetBestHeroFlairOwned(this.m_heroCardID).Premium == TAG_PREMIUM.GOLDEN) || (this.m_entityDef.GetCardSet() == TAG_CARD_SET.HERO_SKINS)))
            {
                Material material = this.m_portraitObject.GetComponent<Renderer>().materials[this.m_portraitMaterialIndex];
                Texture texture = null;
                if (material.HasProperty("_ShadowTex"))
                {
                    texture = material.GetTexture("_ShadowTex");
                }
                RenderUtils.SetMaterial(this.m_portraitObject, this.m_portraitMaterialIndex, this.m_cardDef.GetPremiumPortraitMaterial());
                this.m_portraitObject.GetComponent<Renderer>().materials[this.m_portraitMaterialIndex].SetTexture("_ShadowTex", texture);
            }
        }
    }

    public void SetPositionIndex(int idx)
    {
        this.m_positionIndex = idx;
    }

    public void Show()
    {
        base.gameObject.SetActive(true);
        this.m_isShown = true;
    }

    public void ShowBanner()
    {
        this.m_classObject.SetActive(true);
    }

    public void ShowDeckName()
    {
        this.m_deckName.gameObject.SetActive(true);
    }

    public void ShowDeleteButton(bool show)
    {
        this.m_deleteButton.gameObject.SetActive(show);
    }

    public void ShowEditDecal(bool show)
    {
        this.m_editDecal.SetActive(show);
    }

    private void Update()
    {
        if (this.m_wasTouchModeEnabled != UniversalInputManager.Get().IsTouchMode())
        {
            PegUIElement.InteractionState interactionState = base.GetInteractionState();
            if (this.m_wasTouchModeEnabled)
            {
                switch (interactionState)
                {
                    case PegUIElement.InteractionState.Down:
                        this.OnPressEvent();
                        break;

                    case PegUIElement.InteractionState.Over:
                        this.OnOverEvent();
                        break;
                }
            }
            else
            {
                switch (interactionState)
                {
                    case PegUIElement.InteractionState.Down:
                        this.OnReleaseEvent();
                        break;

                    case PegUIElement.InteractionState.Over:
                        this.OnOutEvent();
                        break;
                }
                this.ShowDeleteButton(false);
            }
            this.m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitThenCallAnimationCallback(OnPopAnimationFinishedCallbackData callbackData)
    {
        return new <WaitThenCallAnimationCallback>c__Iterator2C { callbackData = callbackData, <$>callbackData = callbackData, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <ChangeHighlightStateWhenReady>c__Iterator2B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ActorStateType <$>stateType;
        internal CollectionDeckBoxVisual <>f__this;
        internal ActorStateType stateType;

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
                case 1:
                    if (!this.<>f__this.m_highlightState.IsReady())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.m_highlightState.ChangeState(this.stateType);
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

    [CompilerGenerated]
    private sealed class <WaitThenCallAnimationCallback>c__Iterator2C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionDeckBoxVisual.OnPopAnimationFinishedCallbackData <$>callbackData;
        internal CollectionDeckBoxVisual <>f__this;
        internal bool <enableInput>__0;
        internal CollectionDeckBoxVisual.OnPopAnimationFinishedCallbackData callbackData;

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
                    this.$current = new WaitForSeconds(this.<>f__this.GetComponent<Animation>()[this.callbackData.m_animationName].length / this.<>f__this.GetComponent<Animation>()[this.callbackData.m_animationName].speed);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<enableInput>__0 = this.callbackData.m_animationName.Equals("Deck_PopUp");
                    this.<>f__this.SetEnabled(this.<enableInput>__0);
                    if (this.callbackData.m_callback != null)
                    {
                        this.callbackData.m_callback(this.callbackData.m_callbackData);
                        this.$PC = -1;
                        break;
                    }
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

    public delegate void DelOnAnimationFinished(object callbackData);

    private class OnPopAnimationFinishedCallbackData
    {
        public string m_animationName;
        public CollectionDeckBoxVisual.DelOnAnimationFinished m_callback;
        public object m_callbackData;
    }

    private class OnScaleFinishedCallbackData
    {
        public CollectionDeckBoxVisual.DelOnAnimationFinished m_callback;
        public object m_callbackData;
    }
}

