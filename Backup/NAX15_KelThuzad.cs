using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NAX15_KelThuzad : NAX_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map28;
    private bool m_bigglesLinePlayed;
    private bool m_frostHeroPowerLinePlayed;
    private bool m_hurryLinePlayed;
    private int m_numTimesMindControlPlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator17B { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator17C { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    public override void HandleRealTimeMissionEvent(int missionEvent)
    {
        if (missionEvent == 1)
        {
            AssetLoader.Get().LoadGameObject("KelThuzad_StealTurn", new AssetLoader.GameObjectCallback(this.OnStealTurnSpellLoaded), null, false);
        }
    }

    public override void OnPlayThinkEmote()
    {
        if (!this.m_hurryLinePlayed && !base.m_enemySpeaking)
        {
            Player currentPlayer = GameState.Get().GetCurrentPlayer();
            if (currentPlayer.IsLocalUser() && !currentPlayer.GetHeroCard().HasActiveEmoteSound())
            {
                this.m_hurryLinePlayed = true;
                Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_HURRY_31", "VO_NAX15_01_HURRY_31", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
            }
        }
    }

    private void OnStealTurnSpellLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            if (TurnTimer.Get() != null)
            {
                TurnTimer.Get().OnEndTurnRequested();
            }
            EndTurnButton.Get().OnEndTurnRequested();
        }
        else
        {
            go.transform.position = EndTurnButton.Get().transform.position;
            Spell component = go.GetComponent<Spell>();
            if (component == null)
            {
                if (TurnTimer.Get() != null)
                {
                    TurnTimer.Get().OnEndTurnRequested();
                }
                EndTurnButton.Get().OnEndTurnRequested();
            }
            else
            {
                Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                component.ActivateState(SpellStateType.ACTION);
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_PHASE2_ALT_11", "VO_NAX15_01_PHASE2_ALT_11", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
            }
        }
    }

    protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
    {
        Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
        switch (emoteType)
        {
            case EmoteType.GREETINGS:
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_EMOTE_HELLO_26", "VO_NAX15_01_EMOTE_HELLO_26", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                break;

            case EmoteType.WELL_PLAYED:
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_EMOTE_WP_25", "VO_NAX15_01_EMOTE_WP_25", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                break;

            case EmoteType.OOPS:
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_EMOTE_OOPS_29", "VO_NAX15_01_EMOTE_OOPS_29", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                break;

            case EmoteType.THREATEN:
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_EMOTE_THREATEN_30", "VO_NAX15_01_EMOTE_THREATEN_30", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                break;

            case EmoteType.THANKS:
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_EMOTE_THANKS_27", "VO_NAX15_01_EMOTE_THANKS_27", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                break;

            case EmoteType.SORRY:
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_EMOTE_SORRY_28", "VO_NAX15_01_EMOTE_SORRY_28", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                break;

            case EmoteType.START:
            {
                string cardId = GameState.Get().GetFriendlySidePlayer().GetHero().GetCardId();
                if (cardId != null)
                {
                    int num;
                    if (<>f__switch$map28 == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(9);
                        dictionary.Add("HERO_01", 0);
                        dictionary.Add("HERO_02", 1);
                        dictionary.Add("HERO_03", 2);
                        dictionary.Add("HERO_04", 3);
                        dictionary.Add("HERO_05", 4);
                        dictionary.Add("HERO_06", 5);
                        dictionary.Add("HERO_07", 6);
                        dictionary.Add("HERO_08", 7);
                        dictionary.Add("HERO_09", 8);
                        <>f__switch$map28 = dictionary;
                    }
                    if (<>f__switch$map28.TryGetValue(cardId, out num))
                    {
                        switch (num)
                        {
                            case 0:
                                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_RESPOND_GARROSH_15", "VO_NAX15_01_RESPOND_GARROSH_15", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                                return;

                            case 1:
                                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_RESPOND_THRALL_17", "VO_NAX15_01_RESPOND_THRALL_17", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                                return;

                            case 2:
                                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_RESPOND_VALEERA_18", "VO_NAX15_01_RESPOND_VALEERA_18", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                                return;

                            case 3:
                                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_RESPOND_UTHER_14", "VO_NAX15_01_RESPOND_UTHER_14", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                                return;

                            case 4:
                                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_RESPOND_REXXAR_19", "VO_NAX15_01_RESPOND_REXXAR_19", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                                return;

                            case 5:
                                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_RESPOND_MALFURION_ALT_21", "VO_NAX15_01_RESPOND_MALFURION_ALT_21", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                                return;

                            case 6:
                                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_RESPOND_GULDAN_22", "VO_NAX15_01_RESPOND_GULDAN_22", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                                return;

                            case 7:
                                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_RESPOND_JAINA_23", "VO_NAX15_01_RESPOND_JAINA_23", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                                return;

                            case 8:
                                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_NAX15_01_RESPOND_ANDUIN_24", "VO_NAX15_01_RESPOND_ANDUIN_24", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                                return;
                        }
                    }
                }
                break;
            }
        }
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_NAX15_01_SUMMON_ADDS_12");
        base.PreloadSound("VO_NAX15_01_PHASE2_10");
        base.PreloadSound("VO_NAX15_01_HP_07");
        base.PreloadSound("VO_NAX15_01_HP2_05");
        base.PreloadSound("VO_NAX15_01_HP3_06");
        base.PreloadSound("VO_NAX15_01_PHASE2_ALT_11");
        base.PreloadSound("VO_NAX15_01_EMOTE_HELLO_26");
        base.PreloadSound("VO_NAX15_01_EMOTE_WP_25");
        base.PreloadSound("VO_NAX15_01_EMOTE_OOPS_29");
        base.PreloadSound("VO_NAX15_01_EMOTE_SORRY_28");
        base.PreloadSound("VO_NAX15_01_EMOTE_THANKS_27");
        base.PreloadSound("VO_NAX15_01_EMOTE_THREATEN_30");
        base.PreloadSound("VO_NAX15_01_RESPOND_GARROSH_15");
        base.PreloadSound("VO_NAX15_01_RESPOND_THRALL_17");
        base.PreloadSound("VO_NAX15_01_RESPOND_VALEERA_18");
        base.PreloadSound("VO_NAX15_01_RESPOND_UTHER_14");
        base.PreloadSound("VO_NAX15_01_RESPOND_REXXAR_19");
        base.PreloadSound("VO_NAX15_01_RESPOND_MALFURION_ALT_21");
        base.PreloadSound("VO_NAX15_01_RESPOND_GULDAN_22");
        base.PreloadSound("VO_NAX15_01_RESPOND_JAINA_23");
        base.PreloadSound("VO_NAX15_01_RESPOND_ANDUIN_24");
        base.PreloadSound("VO_NAX15_01_BIGGLES_32");
        base.PreloadSound("VO_NAX15_01_HURRY_31");
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator17B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal int <KTgloat>__0;
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
                    if (this.gameResult != TAG_PLAYSTATE.LOST)
                    {
                        goto Label_0146;
                    }
                    this.<KTgloat>__0 = Options.Get().GetInt(Option.KELTHUZADTAUNTS);
                    this.$current = new WaitForSeconds(5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    switch (this.<KTgloat>__0)
                    {
                        case 0:
                            NotificationManager.Get().CreateKTQuote("VO_NAX15_01_GLOAT1_33", "VO_NAX15_01_GLOAT1_33", true);
                            goto Label_0108;

                        case 1:
                            NotificationManager.Get().CreateKTQuote("VO_NAX15_01_GLOAT2_34", "VO_NAX15_01_GLOAT2_34", true);
                            goto Label_0108;

                        case 2:
                            NotificationManager.Get().CreateKTQuote("VO_NAX15_01_GLOAT3_35", "VO_NAX15_01_GLOAT3_35", true);
                            goto Label_0108;

                        case 3:
                            NotificationManager.Get().CreateKTQuote("VO_NAX15_01_GLOAT4_36", "VO_NAX15_01_GLOAT4_36", true);
                            goto Label_0108;

                        case 4:
                            NotificationManager.Get().CreateKTQuote("VO_NAX15_01_GLOAT5_37", "VO_NAX15_01_GLOAT5_37", true);
                            goto Label_0108;
                    }
                    break;

                default:
                    goto Label_0146;
            }
        Label_0108:
            if (this.<KTgloat>__0 >= 4)
            {
                Options.Get().SetInt(Option.KELTHUZADTAUNTS, 0);
            }
            else
            {
                Options.Get().SetInt(Option.KELTHUZADTAUNTS, this.<KTgloat>__0 + 1);
                goto Label_0146;
                this.$PC = -1;
            }
        Label_0146:
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator17C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal NAX15_KelThuzad <>f__this;
        internal Actor <enemyActor>__0;
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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.HandleMissionEventWithTiming(this.missionEvent));
                    this.$PC = 1;
                    goto Label_0356;

                case 1:
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    switch (this.missionEvent)
                    {
                        case 1:
                            GameState.Get().SetBusy(true);
                            goto Label_00CF;

                        case 2:
                            this.<>f__this.m_enemySpeaking = true;
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX15_01_PHASE2_10", "VO_NAX15_01_PHASE2_10", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            this.$PC = 4;
                            goto Label_0356;

                        case 3:
                            if (this.<>f__this.m_frostHeroPowerLinePlayed)
                            {
                                break;
                            }
                            this.<>f__this.m_frostHeroPowerLinePlayed = true;
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX15_01_HP_07", "VO_NAX15_01_HP_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            this.$PC = 6;
                            goto Label_0356;

                        case 4:
                            if (this.<>f__this.m_numTimesMindControlPlayed == 0)
                            {
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX15_01_HP2_05", "VO_NAX15_01_HP2_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 7;
                                goto Label_0356;
                            }
                            if (this.<>f__this.m_numTimesMindControlPlayed == 1)
                            {
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX15_01_HP3_06", "VO_NAX15_01_HP3_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 8;
                                goto Label_0356;
                            }
                            goto Label_02DC;

                        case 5:
                            if (!this.<>f__this.m_bigglesLinePlayed)
                            {
                                this.<>f__this.m_bigglesLinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX15_01_BIGGLES_32", "VO_NAX15_01_BIGGLES_32", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                                this.$PC = -1;
                            }
                            break;
                    }
                    goto Label_0354;

                case 2:
                    break;

                case 4:
                    GameState.Get().SetBusy(false);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX15_01_SUMMON_ADDS_12", "VO_NAX15_01_SUMMON_ADDS_12", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    this.$PC = 5;
                    goto Label_0356;

                case 7:
                case 8:
                    goto Label_02DC;

                default:
                    goto Label_0354;
            }
        Label_00CF:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0356;
            }
            GameState.Get().SetBusy(false);
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX15_01_SUMMON_ADDS_12", "VO_NAX15_01_SUMMON_ADDS_12", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
            this.$PC = 3;
            goto Label_0356;
        Label_02DC:
            this.<>f__this.m_numTimesMindControlPlayed++;
        Label_0354:
            return false;
        Label_0356:
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

