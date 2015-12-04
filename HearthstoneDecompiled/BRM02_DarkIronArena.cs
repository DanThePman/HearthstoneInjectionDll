using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM02_DarkIronArena : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map3;
    private HashSet<string> m_linesPlayed = new HashSet<string>();
    private const float PLAY_CARD_DELAY = 0.7f;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__IteratorF5 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__IteratorF4 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA02_1_RESPONSE_04",
            m_stringTag = "VO_BRMA02_1_RESPONSE_04"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA02_1_RESPONSE_04");
        base.PreloadSound("VO_BRMA02_1_HERO_POWER_05");
        base.PreloadSound("VO_BRMA02_1_TURN1_02");
        base.PreloadSound("VO_BRMA02_1_TURN1_PT2_03");
        base.PreloadSound("VO_BRMA02_1_ALAKIR_34");
        base.PreloadSound("VO_BRMA02_1_ALEXSTRAZA_32");
        base.PreloadSound("VO_BRMA02_1_BEAST_22");
        base.PreloadSound("VO_BRMA02_1_BOOM_28");
        base.PreloadSound("VO_BRMA02_1_CAIRNE_20");
        base.PreloadSound("VO_BRMA02_1_CHO_07");
        base.PreloadSound("VO_BRMA02_1_DEATHWING_35");
        base.PreloadSound("VO_BRMA02_1_ETC_18");
        base.PreloadSound("VO_BRMA02_1_FEUGEN_15");
        base.PreloadSound("VO_BRMA02_1_FOEREAPER_29");
        base.PreloadSound("VO_BRMA02_1_GEDDON_13");
        base.PreloadSound("VO_BRMA02_1_GELBIN_21");
        base.PreloadSound("VO_BRMA02_1_GRUUL_31");
        base.PreloadSound("VO_BRMA02_1_HOGGER_27");
        base.PreloadSound("VO_BRMA02_1_ILLIDAN_23");
        base.PreloadSound("VO_BRMA02_1_LEVIATHAN_12");
        base.PreloadSound("VO_BRMA02_1_LOATHEB_16");
        base.PreloadSound("VO_BRMA02_1_MAEXXNA_24");
        base.PreloadSound("VO_BRMA02_1_MILLHOUSE_09");
        base.PreloadSound("VO_BRMA02_1_MOGOR_25");
        base.PreloadSound("VO_BRMA02_1_MUKLA_10");
        base.PreloadSound("VO_BRMA02_1_NOZDORMU_36");
        base.PreloadSound("VO_BRMA02_1_ONYXIA_33");
        base.PreloadSound("VO_BRMA02_1_PAGLE_08");
        base.PreloadSound("VO_BRMA02_1_SNEED_30");
        base.PreloadSound("VO_BRMA02_1_STALAGG_14");
        base.PreloadSound("VO_BRMA02_1_SYLVANAS_19");
        base.PreloadSound("VO_BRMA02_1_THALNOS_06");
        base.PreloadSound("VO_BRMA02_1_THAURISSAN_37");
        base.PreloadSound("VO_BRMA02_1_TINKMASTER_11");
        base.PreloadSound("VO_BRMA02_1_TOSHLEY_26");
        base.PreloadSound("VO_BRMA02_1_VOLJIN_17");
        base.PreloadSound("VO_NEFARIAN_GRIMSTONE_DEAD1_30");
        base.PreloadSound("VO_RAGNAROS_GRIMSTONE_DEAD2_66");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToWillPlayCardWithTiming(string cardId)
    {
        return new <RespondToWillPlayCardWithTiming>c__IteratorF3 { cardId = cardId, <$>cardId = cardId, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__IteratorF5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal BRM02_DarkIronArena <>f__this;
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
                    goto Label_015C;

                case 1:
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_GRIMSTONE_DEAD1_30"), string.Empty, true, 5f, CanvasAnchor.BOTTOM_LEFT);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_NEFARIAN_GRIMSTONE_DEAD1_30", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                    this.$PC = 2;
                    goto Label_015C;

                case 2:
                    NotificationManager.Get().DestroyActiveQuote(0f);
                    NotificationManager.Get().CreateCharacterQuote("Ragnaros_Quote", NotificationManager.ALT_ADVENTURE_SCREEN_POS, GameStrings.Get("VO_RAGNAROS_GRIMSTONE_DEAD2_66"), string.Empty, true, 7f, null, CanvasAnchor.BOTTOM_LEFT);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_RAGNAROS_GRIMSTONE_DEAD2_66", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                    this.$PC = 3;
                    goto Label_015C;

                case 3:
                    NotificationManager.Get().DestroyActiveQuote(0f);
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_015C:
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
    private sealed class <HandleStartOfTurnWithTiming>c__IteratorF4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM02_DarkIronArena <>f__this;
        internal Actor <enemyActor>__0;
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
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    if (this.turn == 1)
                    {
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_TURN1_02", "VO_BRMA02_1_TURN1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_TURN1_PT2_03", "VO_BRMA02_1_TURN1_PT2_03", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    break;

                default:
                    break;
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
    private sealed class <RespondToWillPlayCardWithTiming>c__IteratorF3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>cardId;
        internal BRM02_DarkIronArena <>f__this;
        internal Actor <enemyActor>__0;
        internal string cardId;

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
                    if (!this.<>f__this.m_linesPlayed.Contains(this.cardId))
                    {
                        this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                        if ((this.cardId == "BRMA02_2") || (this.cardId == "BRMA02_2H"))
                        {
                            if (this.<>f__this.m_enemySpeaking)
                            {
                                goto Label_0FAC;
                            }
                            GameState.Get().SetBusy(true);
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_HERO_POWER_05", "VO_BRMA02_1_HERO_POWER_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 1;
                            goto Label_0FAE;
                        }
                        GameState.Get().SetBusy(true);
                        break;
                    }
                    goto Label_0FAC;

                case 1:
                    GameState.Get().SetBusy(false);
                    goto Label_0FA5;

                case 2:
                    break;

                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 20:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 30:
                case 0x1f:
                case 0x20:
                case 0x21:
                case 0x22:
                    goto Label_0F9A;

                default:
                    goto Label_0FAC;
            }
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0FAE;
            }
            string cardId = this.cardId;
            if (cardId != null)
            {
                int num2;
                if (BRM02_DarkIronArena.<>f__switch$map3 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(0x20);
                    dictionary.Add("NEW1_010", 0);
                    dictionary.Add("EX1_561", 1);
                    dictionary.Add("EX1_577", 2);
                    dictionary.Add("GVG_110", 3);
                    dictionary.Add("EX1_110", 4);
                    dictionary.Add("EX1_100", 5);
                    dictionary.Add("NEW1_030", 6);
                    dictionary.Add("PRO_001", 7);
                    dictionary.Add("FP1_015", 8);
                    dictionary.Add("GVG_113", 9);
                    dictionary.Add("EX1_249", 10);
                    dictionary.Add("EX1_112", 11);
                    dictionary.Add("NEW1_038", 12);
                    dictionary.Add("NEW1_040", 13);
                    dictionary.Add("EX1_614", 14);
                    dictionary.Add("GVG_007", 15);
                    dictionary.Add("FP1_030", 0x10);
                    dictionary.Add("FP1_010", 0x11);
                    dictionary.Add("NEW1_029", 0x12);
                    dictionary.Add("GVG_112", 0x13);
                    dictionary.Add("EX1_014", 20);
                    dictionary.Add("EX1_560", 0x15);
                    dictionary.Add("EX1_562", 0x16);
                    dictionary.Add("EX1_557", 0x17);
                    dictionary.Add("GVG_114", 0x18);
                    dictionary.Add("FP1_014", 0x19);
                    dictionary.Add("EX1_016", 0x1a);
                    dictionary.Add("EX1_012", 0x1b);
                    dictionary.Add("BRM_028", 0x1c);
                    dictionary.Add("EX1_083", 0x1d);
                    dictionary.Add("GVG_115", 30);
                    dictionary.Add("GVG_014", 0x1f);
                    BRM02_DarkIronArena.<>f__switch$map3 = dictionary;
                }
                if (BRM02_DarkIronArena.<>f__switch$map3.TryGetValue(cardId, out num2))
                {
                    switch (num2)
                    {
                        case 0:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_ALAKIR_34", "VO_BRMA02_1_ALAKIR_34", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 3;
                            goto Label_0FAE;

                        case 1:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_ALEXSTRAZA_32", "VO_BRMA02_1_ALEXSTRAZA_32", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 4;
                            goto Label_0FAE;

                        case 2:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_BEAST_22", "VO_BRMA02_1_BEAST_22", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 5;
                            goto Label_0FAE;

                        case 3:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_BOOM_28", "VO_BRMA02_1_BOOM_28", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 6;
                            goto Label_0FAE;

                        case 4:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_CAIRNE_20", "VO_BRMA02_1_CAIRNE_20", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 7;
                            goto Label_0FAE;

                        case 5:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_CHO_07", "VO_BRMA02_1_CHO_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 8;
                            goto Label_0FAE;

                        case 6:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_DEATHWING_35", "VO_BRMA02_1_DEATHWING_35", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 9;
                            goto Label_0FAE;

                        case 7:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_ETC_18", "VO_BRMA02_1_ETC_18", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 10;
                            goto Label_0FAE;

                        case 8:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_FEUGEN_15", "VO_BRMA02_1_FEUGEN_15", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 11;
                            goto Label_0FAE;

                        case 9:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_FOEREAPER_29", "VO_BRMA02_1_FOEREAPER_29", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 12;
                            goto Label_0FAE;

                        case 10:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_GEDDON_13", "VO_BRMA02_1_GEDDON_13", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 13;
                            goto Label_0FAE;

                        case 11:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_GELBIN_21", "VO_BRMA02_1_GELBIN_21", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 14;
                            goto Label_0FAE;

                        case 12:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_GRUUL_31", "VO_BRMA02_1_GRUUL_31", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 15;
                            goto Label_0FAE;

                        case 13:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_HOGGER_27", "VO_BRMA02_1_HOGGER_27", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x10;
                            goto Label_0FAE;

                        case 14:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_ILLIDAN_23", "VO_BRMA02_1_ILLIDAN_23", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x11;
                            goto Label_0FAE;

                        case 15:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_LEVIATHAN_12", "VO_BRMA02_1_LEVIATHAN_12", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x12;
                            goto Label_0FAE;

                        case 0x10:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_LOATHEB_16", "VO_BRMA02_1_LOATHEB_16", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x13;
                            goto Label_0FAE;

                        case 0x11:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_MAEXXNA_24", "VO_BRMA02_1_MAEXXNA_24", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 20;
                            goto Label_0FAE;

                        case 0x12:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_MILLHOUSE_09", "VO_BRMA02_1_MILLHOUSE_09", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x15;
                            goto Label_0FAE;

                        case 0x13:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_MOGOR_25", "VO_BRMA02_1_MOGOR_25", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x16;
                            goto Label_0FAE;

                        case 20:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_MUKLA_10", "VO_BRMA02_1_MUKLA_10", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x17;
                            goto Label_0FAE;

                        case 0x15:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_NOZDORMU_36", "VO_BRMA02_1_NOZDORMU_36", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x18;
                            goto Label_0FAE;

                        case 0x16:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_ONYXIA_33", "VO_BRMA02_1_ONYXIA_33", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x19;
                            goto Label_0FAE;

                        case 0x17:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_PAGLE_08", "VO_BRMA02_1_PAGLE_08", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x1a;
                            goto Label_0FAE;

                        case 0x18:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_SNEED_30", "VO_BRMA02_1_SNEED_30", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x1b;
                            goto Label_0FAE;

                        case 0x19:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_STALAGG_14", "VO_BRMA02_1_STALAGG_14", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x1c;
                            goto Label_0FAE;

                        case 0x1a:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_SYLVANAS_19", "VO_BRMA02_1_SYLVANAS_19", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x1d;
                            goto Label_0FAE;

                        case 0x1b:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_THALNOS_06", "VO_BRMA02_1_THALNOS_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 30;
                            goto Label_0FAE;

                        case 0x1c:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_THAURISSAN_37", "VO_BRMA02_1_THAURISSAN_37", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x1f;
                            goto Label_0FAE;

                        case 0x1d:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_TINKMASTER_11", "VO_BRMA02_1_TINKMASTER_11", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x20;
                            goto Label_0FAE;

                        case 30:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_TOSHLEY_26", "VO_BRMA02_1_TOSHLEY_26", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x21;
                            goto Label_0FAE;

                        case 0x1f:
                            this.<>f__this.m_linesPlayed.Add(this.cardId);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA02_1_VOLJIN_17", "VO_BRMA02_1_VOLJIN_17", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 0.7f, true, true));
                            this.$PC = 0x22;
                            goto Label_0FAE;
                    }
                }
            }
        Label_0F9A:
            GameState.Get().SetBusy(false);
        Label_0FA5:
            this.$PC = -1;
        Label_0FAC:
            return false;
        Label_0FAE:
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

