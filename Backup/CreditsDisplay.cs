using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CreditsDisplay : MonoBehaviour
{
    private const float CREDITS_SCROLL_SPEED = 2.5f;
    private Vector3 creditsRootStartLocalPosition;
    private Vector3 creditsText1StartLocalPosition;
    private Vector3 creditsText2StartLocalPosition;
    public Transform m_cardBone;
    private string[] m_creditLines;
    private List<FullDef> m_creditsDefs;
    private bool m_creditsDone;
    public GameObject m_creditsRoot;
    public UberText m_creditsText1;
    public UberText m_creditsText2;
    private bool m_creditsTextLoaded;
    private bool m_creditsTextLoadSucceeded;
    private int m_currentLine;
    private UberText m_currentText;
    private bool m_displayingLatestYear = true;
    public GameObject m_doneArrowInButton;
    public StandardPegButtonNew m_doneButton;
    private List<Actor> m_fakeCards;
    public Transform m_flopPoint;
    private int m_lastCard = 1;
    public Transform m_offscreenCardBone;
    private Actor m_shownCreditsCard;
    public StandardPegButtonNew m_yearButton;
    private const int MAX_LINES_PER_CHUNK = 70;
    private static CreditsDisplay s_instance;
    private const string SHOW_NEW_CARD_COROUTINE = "ShowNewCard";
    private const string START_CREDITS_COROUTINE = "StartCredits";
    private bool started;

    private void ActorLoadedCallback(string name, GameObject go, object callbackData)
    {
        this.m_fakeCards.Add(go.GetComponent<Actor>());
    }

    private void Awake()
    {
        s_instance = this;
        this.m_fakeCards = new List<Actor>();
        this.m_creditsDefs = new List<FullDef>();
        this.creditsRootStartLocalPosition = this.m_creditsRoot.transform.localPosition;
        this.creditsText1StartLocalPosition = this.m_creditsText1.transform.localPosition;
        this.creditsText2StartLocalPosition = this.m_creditsText2.transform.localPosition;
        this.m_doneButton.SetText(GameStrings.Get("GLOBAL_BACK"));
        this.m_doneButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDonePressed));
        this.m_yearButton.SetText(!this.m_displayingLatestYear ? "2015" : "2014");
        this.m_yearButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnYearPressed));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            Box.Get().m_tableTop.SetActive(false);
            Box.Get().m_letterboxingContainer.SetActive(false);
            this.m_doneButton.SetText(string.Empty);
            this.m_doneArrowInButton.SetActive(true);
        }
        AssetLoader.Get().LoadActor("Card_Hand_Ally", new AssetLoader.GameObjectCallback(this.ActorLoadedCallback), null, false);
        AssetLoader.Get().LoadActor("Card_Hand_Ally", new AssetLoader.GameObjectCallback(this.ActorLoadedCallback), null, false);
        this.LoadAllCreditsCards();
        this.LoadCreditsText();
        Navigation.Push(new Navigation.NavigateBackHandler(this.EndCredits));
    }

    private void DropText()
    {
        UberText text = this.m_creditsText1;
        if (this.m_currentText == this.m_creditsText1)
        {
            text = this.m_creditsText2;
        }
        float z = 1.8649f;
        TransformUtil.SetPoint(this.m_currentText.gameObject, Anchor.FRONT, text.gameObject, Anchor.BACK, new Vector3(0f, 0f, z));
    }

    private bool EndCredits()
    {
        iTween.FadeTo(this.m_creditsText1.gameObject, 0f, 0.1f);
        iTween.FadeTo(this.m_creditsText2.gameObject, 0f, 0.1f);
        SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
        return true;
    }

    [DebuggerHidden]
    private IEnumerator EndCreditsTimer()
    {
        return new <EndCreditsTimer>c__Iterator5B();
    }

    private void FlopCredits()
    {
        if (this.m_currentText == this.m_creditsText1)
        {
            this.m_currentText = this.m_creditsText2;
        }
        else
        {
            this.m_currentText = this.m_creditsText1;
        }
        this.m_currentText.Text = this.GetNextCreditsChunk();
        this.DropText();
    }

    public static CreditsDisplay Get()
    {
        return s_instance;
    }

    private string GetFilePath()
    {
        Locale[] loadOrder = Localization.GetLoadOrder(false);
        for (int i = 0; i < loadOrder.Length; i++)
        {
            string fileName = "CREDITS_" + (!this.m_displayingLatestYear ? "2014" : "2015") + ".txt";
            string assetPath = GameStrings.GetAssetPath(loadOrder[i], fileName);
            if (System.IO.File.Exists(assetPath))
            {
                return assetPath;
            }
        }
        return null;
    }

    private string GetNextCreditsChunk()
    {
        string str = string.Empty;
        int currentLine = this.m_currentLine;
        int num2 = 70;
        for (int i = 0; i < num2; i++)
        {
            if (this.m_creditLines.Length < ((i + currentLine) + 1))
            {
                this.m_creditsDone = true;
                this.StartEndCreditsTimer();
                return str;
            }
            string str2 = this.m_creditLines[i + currentLine];
            if (str2.Length > 0x26)
            {
                num2 -= Mathf.CeilToInt((float) (str2.Length / 0x26));
                if ((i > num2) && (i > 60))
                {
                    return str;
                }
            }
            str = str + str2 + Environment.NewLine;
            this.m_currentLine++;
        }
        return str;
    }

    private float GetTopOfCurrentCredits()
    {
        Bounds textWorldSpaceBounds = this.m_currentText.GetTextWorldSpaceBounds();
        return (textWorldSpaceBounds.center.z + textWorldSpaceBounds.extents.z);
    }

    private void LoadAllCreditsCards()
    {
        if (this.m_displayingLatestYear)
        {
            DefLoader.Get().LoadFullDef("CRED_01", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_02", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_03", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_04", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_05", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_06", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_07", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_08", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_09", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_10", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_11", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_12", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_14", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_15", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_16", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_18", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_19", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_20", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_21", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_22", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_23", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_24", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_25", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_26", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_27", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_28", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_29", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_30", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_31", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_32", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_33", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_34", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_35", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_36", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_37", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_38", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_39", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_40", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_41", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_42", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_43", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_44", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_45", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_46", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
        }
        else
        {
            DefLoader.Get().LoadFullDef("CRED_01", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_02", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_03", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_04", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_05", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_06", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_07", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_08", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_09", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_10", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_11", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_12", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_13", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_14", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_15", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_16", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            DefLoader.Get().LoadFullDef("CRED_17", new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
        }
    }

    private void LoadCreditsText()
    {
        this.m_creditsTextLoadSucceeded = false;
        string filePath = this.GetFilePath();
        if (filePath == null)
        {
            Error.AddDevWarning("Credits Error", "CreditsDisplay.LoadCreditsText() - Failed to find file for CREDITS.", new object[0]);
            this.m_creditsTextLoaded = true;
        }
        else
        {
            try
            {
                this.m_creditLines = System.IO.File.ReadAllLines(filePath);
                this.m_creditsTextLoadSucceeded = true;
            }
            catch (Exception exception)
            {
                object[] messageArgs = new object[] { filePath, exception.Message };
                Error.AddDevWarning("Credits Error", "CreditsDisplay.LoadCreditsText() - Failed to read \"{0}\".\n\nException: {1}", messageArgs);
            }
            this.m_creditsTextLoaded = true;
        }
    }

    private void NewCard()
    {
        base.StartCoroutine("ShowNewCard");
    }

    [DebuggerHidden]
    private IEnumerator NotifySceneLoadedWhenReady()
    {
        return new <NotifySceneLoadedWhenReady>c__Iterator57 { <>f__this = this };
    }

    private void OnBoxOpened(object userData)
    {
        Box.Get().RemoveTransitionFinishedListener(new Box.TransitionFinishedCallback(this.OnBoxOpened));
        if (!this.m_creditsTextLoadSucceeded)
        {
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
        }
        else
        {
            MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_Credits);
            base.StartCoroutine("StartCredits");
        }
    }

    private void OnDestoy()
    {
        s_instance = null;
    }

    private void OnDonePressed(UIEvent e)
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            Box.Get().m_letterboxingContainer.SetActive(true);
        }
        Navigation.GoBack();
    }

    private void OnFullDefLoaded(string cardID, FullDef def, object userData)
    {
        this.m_creditsDefs.Add(def);
    }

    private void OnYearPressed(UIEvent e)
    {
        base.StopCoroutine("StartCredits");
        base.StopCoroutine("ShowNewCard");
        if (this.m_shownCreditsCard != null)
        {
            this.m_shownCreditsCard.ActivateSpell(SpellType.BURN);
            SoundManager.Get().LoadAndPlay("credits_card_embers_" + UnityEngine.Random.Range(1, 3).ToString());
            this.m_shownCreditsCard = null;
        }
        this.m_displayingLatestYear = !this.m_displayingLatestYear;
        base.StartCoroutine(this.ResetCredits());
    }

    [DebuggerHidden]
    private IEnumerator ResetCredits()
    {
        return new <ResetCredits>c__Iterator5A { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ShowNewCard()
    {
        return new <ShowNewCard>c__Iterator59 { <>f__this = this };
    }

    private void Start()
    {
        base.StartCoroutine(this.NotifySceneLoadedWhenReady());
    }

    [DebuggerHidden]
    private IEnumerator StartCredits()
    {
        return new <StartCredits>c__Iterator58 { <>f__this = this };
    }

    private void StartEndCreditsTimer()
    {
        base.StartCoroutine(this.EndCreditsTimer());
    }

    public void Unload()
    {
        DefLoader.Get().ClearCardDefs();
    }

    private void Update()
    {
        Network.Get().ProcessNetwork();
        if (this.started)
        {
            Transform transform = this.m_creditsRoot.transform;
            transform.localPosition += new Vector3(0f, 0f, 2.5f * UnityEngine.Time.deltaTime);
            if ((!this.m_creditsDone && (this.m_currentText != null)) && (this.GetTopOfCurrentCredits() > this.m_flopPoint.position.z))
            {
                this.FlopCredits();
            }
        }
    }

    [CompilerGenerated]
    private sealed class <EndCreditsTimer>c__Iterator5B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
                    this.$current = new WaitForSeconds(300f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB) && (SceneMgr.Get().GetMode() == SceneMgr.Mode.CREDITS))
                    {
                        Navigation.GoBack();
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

    [CompilerGenerated]
    private sealed class <NotifySceneLoadedWhenReady>c__Iterator57 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CreditsDisplay <>f__this;

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
                    if (this.<>f__this.m_fakeCards.Count < 2)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_00AE;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_00AC;
            }
            while (!this.<>f__this.m_creditsTextLoaded)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00AE;
            }
            Box.Get().AddTransitionFinishedListener(new Box.TransitionFinishedCallback(this.<>f__this.OnBoxOpened));
            SceneMgr.Get().NotifySceneLoaded();
            goto Label_00AC;
            this.$PC = -1;
        Label_00AC:
            return false;
        Label_00AE:
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
    private sealed class <ResetCredits>c__Iterator5A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CreditsDisplay <>f__this;

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
                    this.<>f__this.m_currentText = null;
                    this.<>f__this.m_creditsText1.Text = string.Empty;
                    this.<>f__this.m_creditsText2.Text = string.Empty;
                    this.<>f__this.started = false;
                    this.<>f__this.m_creditsTextLoaded = false;
                    this.<>f__this.m_creditsTextLoadSucceeded = false;
                    this.<>f__this.m_currentLine = 0;
                    this.<>f__this.m_creditLines = null;
                    this.<>f__this.m_yearButton.SetText(!this.<>f__this.m_displayingLatestYear ? "2015" : "2014");
                    this.<>f__this.m_creditsText1.transform.localPosition = this.<>f__this.creditsText1StartLocalPosition;
                    this.<>f__this.m_creditsText2.transform.localPosition = this.<>f__this.creditsText2StartLocalPosition;
                    this.<>f__this.m_creditsRoot.transform.localPosition = this.<>f__this.creditsRootStartLocalPosition;
                    this.<>f__this.m_lastCard = 1;
                    this.<>f__this.m_creditsDefs = new List<FullDef>();
                    this.<>f__this.LoadAllCreditsCards();
                    this.<>f__this.LoadCreditsText();
                    break;

                case 1:
                    break;

                default:
                    goto Label_0194;
            }
            if (!this.<>f__this.m_creditsTextLoaded)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.StartCoroutine("StartCredits");
            this.$PC = -1;
        Label_0194:
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
    private sealed class <ShowNewCard>c__Iterator59 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CreditsDisplay <>f__this;
        internal float <CARD_MOVE_TIME>__0;
        internal EntityDef <entityDef>__3;
        internal int <index>__1;
        internal bool <isSchweitz>__4;
        internal int <newDefIndex>__2;
        internal Actor <oldActor>__5;

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
                    this.<CARD_MOVE_TIME>__0 = 1f;
                    this.<index>__1 = 0;
                    if (this.<>f__this.m_lastCard == 0)
                    {
                        this.<index>__1 = 1;
                    }
                    this.<>f__this.m_lastCard = this.<index>__1;
                    this.<>f__this.m_shownCreditsCard = this.<>f__this.m_fakeCards[this.<index>__1];
                    this.<newDefIndex>__2 = UnityEngine.Random.Range(0, this.<>f__this.m_creditsDefs.Count);
                    this.<>f__this.m_shownCreditsCard.SetCardDef(this.<>f__this.m_creditsDefs[this.<newDefIndex>__2].GetCardDef());
                    this.<entityDef>__3 = this.<>f__this.m_creditsDefs[this.<newDefIndex>__2].GetEntityDef();
                    this.<isSchweitz>__4 = this.<entityDef>__3.GetCardId() == "CRED_10";
                    if (this.<isSchweitz>__4)
                    {
                        this.<entityDef>__3.SetTag<TAG_RACE>(GAME_TAG.CARDRACE, TAG_RACE.PIRATE);
                    }
                    this.<>f__this.m_shownCreditsCard.SetEntityDef(this.<entityDef>__3);
                    this.<>f__this.m_creditsDefs.RemoveAt(this.<newDefIndex>__2);
                    this.<>f__this.m_shownCreditsCard.UpdateAllComponents();
                    this.<>f__this.m_shownCreditsCard.Show();
                    if (this.<isSchweitz>__4)
                    {
                        this.<>f__this.m_shownCreditsCard.GetRaceText().Text = GameStrings.Get("GLUE_NINJA");
                    }
                    this.<>f__this.m_shownCreditsCard.transform.position = this.<>f__this.m_offscreenCardBone.position;
                    this.<>f__this.m_shownCreditsCard.transform.localScale = this.<>f__this.m_offscreenCardBone.localScale;
                    this.<>f__this.m_shownCreditsCard.transform.localEulerAngles = this.<>f__this.m_offscreenCardBone.localEulerAngles;
                    SoundManager.Get().LoadAndPlay("credits_card_enter_" + UnityEngine.Random.Range(1, 3).ToString());
                    iTween.MoveTo(this.<>f__this.m_shownCreditsCard.gameObject, this.<>f__this.m_cardBone.position, this.<CARD_MOVE_TIME>__0);
                    iTween.RotateTo(this.<>f__this.m_shownCreditsCard.gameObject, this.<>f__this.m_cardBone.localEulerAngles, this.<CARD_MOVE_TIME>__0);
                    this.<oldActor>__5 = this.<>f__this.m_shownCreditsCard;
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    goto Label_0350;

                case 1:
                    SoundManager.Get().LoadAndPlay("tavern_crowd_play_reaction_positive_" + UnityEngine.Random.Range(1, 5).ToString());
                    this.$current = new WaitForSeconds(7.5f);
                    this.$PC = 2;
                    goto Label_0350;

                case 2:
                    this.<>f__this.m_shownCreditsCard.ActivateSpell(SpellType.BURN);
                    SoundManager.Get().LoadAndPlay("credits_card_embers_" + UnityEngine.Random.Range(1, 3).ToString());
                    if (this.<>f__this.m_shownCreditsCard == this.<oldActor>__5)
                    {
                        this.<>f__this.m_shownCreditsCard = null;
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0350:
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
    private sealed class <StartCredits>c__Iterator58 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CreditsDisplay <>f__this;

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
                    this.<>f__this.m_creditsText2.Text = this.<>f__this.GetNextCreditsChunk();
                    this.<>f__this.m_currentText = this.<>f__this.m_creditsText2;
                    this.<>f__this.FlopCredits();
                    this.<>f__this.started = true;
                    this.<>f__this.m_creditsRoot.SetActive(true);
                    this.$current = new WaitForSeconds(4f);
                    this.$PC = 1;
                    goto Label_00E5;

                case 1:
                case 2:
                    if (this.<>f__this.m_creditsDefs.Count > 0)
                    {
                        this.<>f__this.NewCard();
                        this.$current = new WaitForSeconds(11f);
                        this.$PC = 2;
                        goto Label_00E5;
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00E5:
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
}

