using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE03_AncientTemple : LOE_MissionEntity
{
    private int m_mostRecentMissionEvent;
    private TempleArt m_templeArt;
    private Notification m_turnCounter;

    public override string CustomChoiceBannerText()
    {
        if (((TAG_STEP) base.GetTag<TAG_STEP>(GAME_TAG.STEP)) == TAG_STEP.MAIN_START)
        {
            string key = null;
            switch (this.m_mostRecentMissionEvent)
            {
                case 10:
                    key = "MISSION_GLOWING_POOL";
                    break;

                case 11:
                    key = "MISSION_PIT_OF_SPIKES";
                    break;

                case 12:
                    key = "MISSION_TAKE_THE_SHORTCUT";
                    break;

                case 4:
                    key = "MISSION_STATUES_EYE";
                    break;
            }
            if (key != null)
            {
                return GameStrings.Get(key);
            }
        }
        return null;
    }

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator131 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator12F { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator130 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    private void InitTempleArt(int cost)
    {
        this.m_templeArt = AssetLoader.Get().LoadGameObject("TempleArt", true, false).GetComponent<TempleArt>();
        this.UpdateTempleArt(cost);
    }

    private void InitTurnCounter(int cost)
    {
        this.m_turnCounter = AssetLoader.Get().LoadGameObject("LOE_Turn_Timer", true, false).GetComponent<Notification>();
        PlayMakerFSM component = this.m_turnCounter.GetComponent<PlayMakerFSM>();
        component.FsmVariables.GetFsmBool("RunningMan").Value = true;
        component.FsmVariables.GetFsmBool("MineCart").Value = false;
        component.SendEvent("Birth");
        Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
        this.m_turnCounter.transform.parent = actor.gameObject.transform;
        this.m_turnCounter.transform.localPosition = new Vector3(-1.4f, 0.187f, -0.11f);
        this.m_turnCounter.transform.localScale = (Vector3) (Vector3.one * 0.52f);
        this.UpdateTurnCounterText(cost);
    }

    private void InitVisuals()
    {
        int cost = base.GetCost();
        int tag = base.GetTag(GAME_TAG.TURN);
        this.InitTempleArt(cost);
        if ((tag >= 1) && GameState.Get().IsPastBeginPhase())
        {
            this.InitTurnCounter(cost);
        }
    }

    public override void NotifyOfMulliganEnded()
    {
        base.NotifyOfMulliganEnded();
        GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor().GetHealthObject().Hide();
    }

    public override void NotifyOfMulliganInitialized()
    {
        base.NotifyOfMulliganInitialized();
        this.m_mostRecentMissionEvent = base.GetTag(GAME_TAG.MISSION_EVENT);
        this.InitVisuals();
    }

    public override void OnTagChanged(TagDelta change)
    {
        base.OnTagChanged(change);
        if (change.tag == 0x30)
        {
            this.UpdateVisuals(change.newValue);
        }
    }

    protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
    {
        if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
        {
            Gameplay.Get().StartCoroutine(base.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_03_RESPONSE", 3f, 1f, true, false));
        }
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOE_03_TURN_5_4");
        base.PreloadSound("VO_LOE_03_TURN_6");
        base.PreloadSound("VO_LOE_03_TURN_9");
        base.PreloadSound("VO_LOE_03_TURN_5");
        base.PreloadSound("VO_LOE_03_TURN_4_GOOD");
        base.PreloadSound("VO_LOE_03_TURN_4_BAD");
        base.PreloadSound("VO_LOE_03_TURN_6_2");
        base.PreloadSound("VO_LOE_03_TURN_4_NEITHER");
        base.PreloadSound("VO_LOE_03_TURN_3_WARNING");
        base.PreloadSound("VO_LOE_03_TURN_2");
        base.PreloadSound("VO_LOE_03_TURN_2_2");
        base.PreloadSound("VO_LOE_03_TURN_4");
        base.PreloadSound("VO_LOE_03_TURN_7");
        base.PreloadSound("VO_LOE_03_TURN_7_2");
        base.PreloadSound("VO_LOE_03_TURN_3_BOULDER");
        base.PreloadSound("VO_LOE_03_TURN_1");
        base.PreloadSound("VO_LOE_03_TURN_8");
        base.PreloadSound("VO_LOE_03_TURN_10");
        base.PreloadSound("VO_LOE_03_WIN");
        base.PreloadSound("VO_LOE_WING_1_WIN_2");
        base.PreloadSound("VO_LOE_03_RESPONSE");
    }

    public override bool ShouldHandleCoin()
    {
        return false;
    }

    private void UpdateTempleArt(int cost)
    {
        Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
        this.m_templeArt.DoPortraitSwap(actor, cost);
    }

    private void UpdateTurnCounter(int cost)
    {
        this.m_turnCounter.GetComponent<PlayMakerFSM>().SendEvent("Action");
        this.UpdateTurnCounterText(cost);
    }

    private void UpdateTurnCounterText(int cost)
    {
        GameStrings.PluralNumber[] numberArray1 = new GameStrings.PluralNumber[1];
        GameStrings.PluralNumber number = new GameStrings.PluralNumber {
            m_number = cost
        };
        numberArray1[0] = number;
        GameStrings.PluralNumber[] pluralNumbers = numberArray1;
        string headlineString = GameStrings.FormatPlurals("MISSION_DEFAULTCOUNTERNAME", pluralNumbers, new object[0]);
        this.m_turnCounter.ChangeDialogText(headlineString, cost.ToString(), string.Empty, string.Empty);
    }

    private void UpdateVisuals(int cost)
    {
        this.UpdateTempleArt(cost);
        if (this.m_turnCounter != null)
        {
            this.UpdateTurnCounter(cost);
        }
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator131 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE03_AncientTemple <>f__this;
        internal TAG_PLAYSTATE gameResult;

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
                    if ((this.gameResult != TAG_PLAYSTATE.WON) || GameMgr.Get().IsClassChallengeMission())
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(5f);
                    this.$PC = 1;
                    goto Label_00A7;

                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Reno_Quote", "VO_LOE_03_WIN", "VO_LOE_03_WIN", 0f, false, false));
                    this.$PC = 2;
                    goto Label_00A7;

                case 2:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00A7:
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator12F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal LOE03_AncientTemple <>f__this;
        internal int missionEvent;

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
                    this.<>f__this.m_mostRecentMissionEvent = this.missionEvent;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.HandleMissionEventWithTiming(this.missionEvent));
                    this.$PC = 1;
                    goto Label_0736;

                case 1:
                    switch (this.missionEvent)
                    {
                        case 1:
                            GameState.Get().SetBusy(true);
                            goto Label_012A;

                        case 2:
                            GameState.Get().SetBusy(true);
                            goto Label_01AA;

                        case 3:
                            GameState.Get().SetBusy(true);
                            goto Label_0229;

                        case 4:
                            GameState.Get().SetBusy(true);
                            goto Label_02A8;

                        case 5:
                            GameState.Get().SetBusy(true);
                            goto Label_032A;

                        case 6:
                            GameState.Get().SetBusy(true);
                            goto Label_03AC;

                        case 7:
                            goto Label_0422;

                        case 8:
                            GameState.Get().SetBusy(true);
                            goto Label_04D6;

                        case 9:
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_03_TURN_3_WARNING", 3f, 1f, true, false));
                            this.$PC = 0x13;
                            goto Label_0736;

                        case 10:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_03_TURN_2", 3f, 1f, true, false));
                            this.$PC = 20;
                            goto Label_0736;

                        case 11:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_03_TURN_4", 3f, 1f, true, false));
                            this.$PC = 0x16;
                            goto Label_0736;

                        case 12:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_03_TURN_7", 3f, 1f, true, false));
                            this.$PC = 0x17;
                            goto Label_0736;

                        case 13:
                            GameState.Get().SetBusy(false);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_03_TURN_3_BOULDER", 3f, 1f, true, false));
                            this.$PC = 0x19;
                            goto Label_0736;
                    }
                    goto Label_0734;

                case 2:
                    break;

                case 4:
                    goto Label_01AA;

                case 5:
                    GameState.Get().SetBusy(false);
                    goto Label_0734;

                case 6:
                    goto Label_0229;

                case 8:
                    goto Label_02A8;

                case 9:
                    GameState.Get().SetBusy(false);
                    goto Label_0734;

                case 10:
                    goto Label_032A;

                case 12:
                    goto Label_03AC;

                case 14:
                    goto Label_0422;

                case 15:
                    goto Label_0455;

                case 0x11:
                    goto Label_04D6;

                case 0x13:
                    GameState.Get().SetBusy(false);
                    goto Label_0734;

                case 20:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_03_TURN_2_2", 3f, 1f, false));
                    this.$PC = 0x15;
                    goto Label_0736;

                case 0x17:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_03_TURN_7_2", 3f, 1f, false));
                    this.$PC = 0x18;
                    goto Label_0736;

                case 0x19:
                    GameState.Get().SetBusy(false);
                    goto Label_0734;

                default:
                    goto Label_0734;
            }
        Label_012A:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0736;
            }
            GameState.Get().SetBusy(false);
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_03_TURN_5_4", 3f, 1f, true, false));
            this.$PC = 3;
            goto Label_0736;
        Label_01AA:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_0736;
            }
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_03_TURN_6", 3f, 1f, false));
            this.$PC = 5;
            goto Label_0736;
        Label_0229:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 6;
                goto Label_0736;
            }
            GameState.Get().SetBusy(false);
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_03_TURN_9", 3f, 1f, false));
            this.$PC = 7;
            goto Label_0736;
        Label_02A8:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 8;
                goto Label_0736;
            }
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_03_TURN_5", 3f, 1f, true, false));
            this.$PC = 9;
            goto Label_0736;
        Label_032A:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 10;
                goto Label_0736;
            }
            GameState.Get().SetBusy(false);
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_03_TURN_4_GOOD", 3f, 1f, true, false));
            this.$PC = 11;
            goto Label_0736;
        Label_03AC:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 12;
                goto Label_0736;
            }
            GameState.Get().SetBusy(false);
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_03_TURN_4_BAD", 3f, 1f, false));
            this.$PC = 13;
            goto Label_0736;
        Label_0422:
            if (GameState.Get().IsBusy())
            {
                this.$current = null;
                this.$PC = 14;
                goto Label_0736;
            }
            GameState.Get().SetBusy(true);
        Label_0455:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 15;
                goto Label_0736;
            }
            GameState.Get().SetBusy(false);
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_03_TURN_6_2", 3f, 1f, false));
            this.$PC = 0x10;
            goto Label_0736;
        Label_04D6:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 0x11;
                goto Label_0736;
            }
            GameState.Get().SetBusy(false);
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_03_TURN_4_NEITHER", 3f, 1f, false));
            this.$PC = 0x12;
            goto Label_0736;
            this.$PC = -1;
        Label_0734:
            return false;
        Label_0736:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator130 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE03_AncientTemple <>f__this;
        internal int <cost>__1;
        internal Actor <mActor>__0;
        internal int turn;

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
                    this.<mActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    this.<mActor>__0.DeactivateSpell(SpellType.IMMUNE);
                    if (this.turn == 1)
                    {
                        this.<cost>__1 = this.<>f__this.GetCost();
                        this.<>f__this.InitTurnCounter(this.<cost>__1);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_03_TURN_1", 3f, 1f, false));
                        this.$PC = 1;
                        goto Label_0183;
                    }
                    break;

                case 1:
                    break;

                case 2:
                case 3:
                    goto Label_017A;

                default:
                    goto Label_0181;
            }
            if ((this.turn % 2) == 0)
            {
                switch (this.<>f__this.GetCost())
                {
                    case 1:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_03_TURN_10", 3f, 1f, false));
                        this.$PC = 3;
                        goto Label_0183;

                    case 3:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_03_TURN_8", 3f, 1f, false));
                        this.$PC = 2;
                        goto Label_0183;
                }
            }
        Label_017A:
            this.$PC = -1;
        Label_0181:
            return false;
        Label_0183:
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

