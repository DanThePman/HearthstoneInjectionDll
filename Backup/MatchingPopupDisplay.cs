using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MatchingPopupDisplay : TransitionPopup
{
    private SceneMgr.Mode m_gameMode;
    public GameObject m_nameContainer;
    private List<GameObject> m_spinnerTexts = new List<GameObject>();
    public UberText m_tipOfTheDay;
    private const int NUM_SPINNER_ENTRIES = 10;

    protected override void Awake()
    {
        base.Awake();
        this.SetupSpinnerText();
        this.UpdateTipOfTheDay();
        this.GenerateRandomSpinnerTexts();
        base.m_title.Text = GameStrings.Get("GLUE_MATCHMAKER_FINDING_OPPONENT");
        this.m_nameContainer.SetActive(false);
        base.m_title.gameObject.SetActive(false);
        this.m_tipOfTheDay.gameObject.SetActive(false);
        SoundManager.Get().Load("FindOpponent_mechanism_start");
    }

    private void GenerateRandomSpinnerTexts()
    {
        int num = 1;
        List<string> list = new List<string>();
        while (true)
        {
            string item = GameStrings.Get("GLUE_SPINNER_" + num);
            if (item == ("GLUE_SPINNER_" + num))
            {
                break;
            }
            list.Add(item);
            num++;
        }
        SceneUtils.FindChild(base.gameObject, "NAME_PerfectOpponent").gameObject.GetComponent<UberText>().Text = GameStrings.Get("GLUE_MATCHMAKER_PERFECT_OPPONENT");
        for (num = 0; num < 10; num++)
        {
            int index = Mathf.FloorToInt(UnityEngine.Random.value * list.Count);
            this.m_spinnerTexts[num].GetComponent<UberText>().Text = list[index];
            list.RemoveAt(index);
        }
    }

    private void IncreaseTooltipProgress()
    {
        if (this.m_gameMode == SceneMgr.Mode.TOURNAMENT)
        {
            Options.Get().SetInt(Option.TIP_PLAY_PROGRESS, Options.Get().GetInt(Option.TIP_PLAY_PROGRESS, 0) + 1);
        }
        else if (this.m_gameMode == SceneMgr.Mode.DRAFT)
        {
            Options.Get().SetInt(Option.TIP_FORGE_PROGRESS, Options.Get().GetInt(Option.TIP_FORGE_PROGRESS, 0) + 1);
        }
    }

    protected override void OnAnimateShowFinished()
    {
        base.OnAnimateShowFinished();
        this.EnableCancelButtonIfPossible();
    }

    protected override void OnCancelButtonReleased(UIEvent e)
    {
        base.OnCancelButtonReleased(e);
        Navigation.GoBack();
    }

    protected override void OnGameCanceled(FindGameEventData eventData)
    {
        Navigation.PopUnique(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    protected override void OnGameConnecting(FindGameEventData eventData)
    {
        base.OnGameConnecting(eventData);
        this.IncreaseTooltipProgress();
    }

    protected override void OnGameDelayed(FindGameEventData eventData)
    {
        this.EnableCancelButtonIfPossible();
    }

    protected override void OnGameEntered(FindGameEventData eventData)
    {
        this.EnableCancelButtonIfPossible();
    }

    protected override void OnGameError(FindGameEventData eventData)
    {
        Navigation.PopUnique(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    protected override void OnGameplaySceneLoaded()
    {
        this.m_nameContainer.SetActive(true);
        base.GetComponent<PlayMakerFSM>().SendEvent("Death");
        base.StartCoroutine(this.StopSpinnerDelay());
        Navigation.Clear();
    }

    private bool OnNavigateBack()
    {
        if (!base.m_cancelButton.gameObject.activeSelf)
        {
            return false;
        }
        base.GetComponent<PlayMakerFSM>().SendEvent("Cancel");
        base.FireMatchCanceledEvent();
        return true;
    }

    private void SetupSpinnerText()
    {
        for (int i = 1; i <= 10; i++)
        {
            GameObject gameObject = SceneUtils.FindChild(base.gameObject, "NAME_" + i).gameObject;
            this.m_spinnerTexts.Add(gameObject);
        }
    }

    protected override void ShowPopup()
    {
        SoundManager.Get().LoadAndPlay("FindOpponent_mechanism_start");
        base.ShowPopup();
        PlayMakerFSM component = base.GetComponent<PlayMakerFSM>();
        FsmBool @bool = component.FsmVariables.FindFsmBool("PlaySpinningMusic");
        if (@bool != null)
        {
            @bool.Value = this.m_gameMode != SceneMgr.Mode.TAVERN_BRAWL;
        }
        component.SendEvent("Birth");
        SceneUtils.EnableRenderers(this.m_nameContainer, false);
        base.m_title.gameObject.SetActive(true);
        this.m_tipOfTheDay.gameObject.SetActive(true);
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    [DebuggerHidden]
    private IEnumerator StopSpinnerDelay()
    {
        return new <StopSpinnerDelay>c__IteratorD8 { <>f__this = this };
    }

    private void UpdateTipOfTheDay()
    {
        this.m_gameMode = SceneMgr.Get().GetMode();
        if (this.m_gameMode == SceneMgr.Mode.TOURNAMENT)
        {
            this.m_tipOfTheDay.Text = GameStrings.GetTip(TipCategory.PLAY, Options.Get().GetInt(Option.TIP_PLAY_PROGRESS, 0), TipCategory.DEFAULT);
        }
        else if (this.m_gameMode == SceneMgr.Mode.DRAFT)
        {
            this.m_tipOfTheDay.Text = GameStrings.GetTip(TipCategory.FORGE, Options.Get().GetInt(Option.TIP_FORGE_PROGRESS, 0), TipCategory.DEFAULT);
        }
        else if (this.m_gameMode == SceneMgr.Mode.TAVERN_BRAWL)
        {
            this.m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.TAVERNBRAWL);
        }
        else
        {
            this.m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.DEFAULT);
        }
    }

    [CompilerGenerated]
    private sealed class <StopSpinnerDelay>c__IteratorD8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MatchingPopupDisplay <>f__this;

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
                    this.$current = new WaitForSeconds(3.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.AnimateHide();
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

