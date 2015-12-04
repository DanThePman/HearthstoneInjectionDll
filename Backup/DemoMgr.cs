using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DemoMgr
{
    private const bool LOAD_STORED_SETTING = false;
    private Notification m_demoText;
    private DemoMode m_mode;
    private bool m_nextDemoTipIsNewArenaMatch;
    private bool m_nextTipUnclickable;
    private bool m_shouldGiveArenaInstruction;
    private static DemoMgr s_instance;

    public void ApplyAppleStoreDemoDefaults()
    {
        Options.Get().SetBool(Option.CONNECT_TO_AURORA, false);
        Options.Get().SetBool(Option.HAS_SEEN_CINEMATIC, true);
        Options.Get().SetEnum<TutorialProgress>(Option.LOCAL_TUTORIAL_PROGRESS, TutorialProgress.NOTHING_COMPLETE);
    }

    public bool ArenaIs1WinMode()
    {
        if (this.m_mode != DemoMode.BLIZZCON_2013)
        {
            return false;
        }
        return true;
    }

    public bool CantExitArena()
    {
        if (this.m_mode != DemoMode.BLIZZCON_2013)
        {
            return false;
        }
        return true;
    }

    public void ChangeDemoText(string demoText)
    {
        this.m_demoText.ChangeText(demoText);
    }

    [DebuggerHidden]
    public IEnumerator CompleteAppleStoreDemo()
    {
        return new <CompleteAppleStoreDemo>c__Iterator257();
    }

    public void CreateDemoText(string demoText)
    {
        this.CreateDemoText(demoText, false, false);
    }

    public void CreateDemoText(string demoText, bool unclickable)
    {
        this.CreateDemoText(demoText, unclickable, false);
    }

    public void CreateDemoText(string demoText, bool unclickable, bool shouldDoArenaInstruction)
    {
        if (this.m_demoText == null)
        {
            this.m_shouldGiveArenaInstruction = shouldDoArenaInstruction;
            this.m_nextTipUnclickable = unclickable;
            GameObject go = AssetLoader.Get().LoadGameObject("DemoText", true, false);
            OverlayUI.Get().AddGameObject(go, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
            this.m_demoText = go.GetComponent<Notification>();
            this.m_demoText.ChangeText(demoText);
            UniversalInputManager.Get().SetSystemDialogActive(true);
            go.transform.GetComponentInChildren<PegUIElement>().AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.RemoveDemoTextDialog));
            if (this.m_nextTipUnclickable)
            {
                this.m_nextTipUnclickable = false;
                this.MakeDemoTextClickable(false);
            }
        }
    }

    private void DemoTextLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        this.m_demoText = actorObject.GetComponent<Notification>();
        this.m_demoText.ChangeText((string) callbackData);
        UniversalInputManager.Get().SetSystemDialogActive(true);
        actorObject.transform.GetComponentInChildren<PegUIElement>().AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.RemoveDemoTextDialog));
        if (this.m_nextTipUnclickable)
        {
            this.m_nextTipUnclickable = false;
            this.MakeDemoTextClickable(false);
        }
    }

    public static DemoMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new DemoMgr();
        }
        ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
        return s_instance;
    }

    public DemoMode GetMode()
    {
        return this.m_mode;
    }

    public DemoMode GetModeFromString(string modeString)
    {
        try
        {
            return EnumUtils.GetEnum<DemoMode>(modeString, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return DemoMode.NONE;
        }
    }

    private string GetStoredGameMode()
    {
        return null;
    }

    public void Initialize()
    {
        string storedGameMode = this.GetStoredGameMode();
        if (storedGameMode == null)
        {
            storedGameMode = Vars.Key("Demo.Mode").GetStr("NONE");
        }
        this.SetModeFromString(storedGameMode);
    }

    public bool IsCurrencyEnabled()
    {
        switch (this.m_mode)
        {
            case DemoMode.BLIZZCON_2013:
            case DemoMode.BLIZZCON_2014:
            case DemoMode.BLIZZCON_2015:
                return false;
        }
        return true;
    }

    public bool IsDemo()
    {
        return (this.m_mode != DemoMode.NONE);
    }

    public bool IsExpoDemo()
    {
        switch (this.m_mode)
        {
            case DemoMode.PAX_EAST_2013:
            case DemoMode.GAMESCOM_2013:
            case DemoMode.BLIZZCON_2013:
            case DemoMode.BLIZZCON_2014:
            case DemoMode.BLIZZCON_2015:
            case DemoMode.APPLE_STORE:
                return true;
        }
        return false;
    }

    public bool IsHubEscMenuEnabled()
    {
        switch (this.m_mode)
        {
            case DemoMode.BLIZZCON_2013:
            case DemoMode.BLIZZCON_2014:
            case DemoMode.BLIZZCON_2015:
            case DemoMode.APPLE_STORE:
                return false;
        }
        return true;
    }

    public bool IsSocialEnabled()
    {
        switch (this.m_mode)
        {
            case DemoMode.BLIZZCON_2013:
            case DemoMode.BLIZZCON_2015:
            case DemoMode.APPLE_STORE:
                return false;
        }
        return true;
    }

    public void MakeDemoTextClickable(bool clickable)
    {
        if (!clickable)
        {
            this.m_demoText.transform.GetComponentInChildren<BoxCollider>().enabled = false;
            this.m_demoText.transform.FindChild("continue").gameObject.SetActive(false);
        }
        else
        {
            this.m_demoText.transform.GetComponentInChildren<BoxCollider>().enabled = true;
            this.m_demoText.transform.FindChild("continue").gameObject.SetActive(true);
        }
    }

    public void NextDemoTipIsNewArenaMatch()
    {
        this.m_nextDemoTipIsNewArenaMatch = true;
    }

    public void RemoveDemoTextDialog()
    {
        UniversalInputManager.Get().SetSystemDialogActive(false);
        UnityEngine.Object.DestroyImmediate(this.m_demoText.gameObject);
        if (this.m_shouldGiveArenaInstruction)
        {
            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_FORGE_INST1_19"), "VO_INNKEEPER_FORGE_INST1_19", 3f, null);
            this.m_shouldGiveArenaInstruction = false;
        }
        if (this.m_nextDemoTipIsNewArenaMatch)
        {
            this.m_nextDemoTipIsNewArenaMatch = false;
            this.CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA"), false, true);
        }
    }

    private void RemoveDemoTextDialog(UIEvent e)
    {
        this.RemoveDemoTextDialog();
    }

    public void SetMode(DemoMode mode)
    {
        this.m_mode = mode;
    }

    public void SetModeFromString(string modeString)
    {
        this.m_mode = this.GetModeFromString(modeString);
    }

    public bool ShouldShowWelcomeQuests()
    {
        switch (this.m_mode)
        {
            case DemoMode.BLIZZCON_2013:
            case DemoMode.BLIZZCON_2014:
            case DemoMode.BLIZZCON_2015:
                return false;
        }
        return true;
    }

    public bool ShowOnlyCustomDecks()
    {
        DemoMode mode = this.m_mode;
        if ((mode != DemoMode.BLIZZCON_2014) && (mode != DemoMode.BLIZZCON_2015))
        {
            return false;
        }
        return true;
    }

    private void WillReset()
    {
        if (this.m_mode == DemoMode.APPLE_STORE)
        {
            this.ApplyAppleStoreDemoDefaults();
        }
    }

    [CompilerGenerated]
    private sealed class <CompleteAppleStoreDemo>c__Iterator257 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        private static AlertPopup.ResponseCallback <>f__am$cache3;
        internal AlertPopup.PopupInfo <info>__0;

        private static void <>m__257(AlertPopup.Response response, object userData)
        {
            ApplicationMgr.Get().Reset();
        }

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
                    this.$current = new WaitForSeconds(3f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<info>__0 = new AlertPopup.PopupInfo();
                    this.<info>__0.m_headerText = GameStrings.Get("GLOBAL_DEMO_COMPLETE_HEADER");
                    this.<info>__0.m_text = GameStrings.Get("GLOBAL_DEMO_COMPLETE_BODY");
                    this.<info>__0.m_showAlertIcon = false;
                    this.<info>__0.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
                    if (<>f__am$cache3 == null)
                    {
                        <>f__am$cache3 = new AlertPopup.ResponseCallback(DemoMgr.<CompleteAppleStoreDemo>c__Iterator257.<>m__257);
                    }
                    this.<info>__0.m_responseCallback = <>f__am$cache3;
                    DialogManager.Get().ShowPopup(this.<info>__0);
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
}

