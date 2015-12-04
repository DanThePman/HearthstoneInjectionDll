using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM03_Thaurissan : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map4;
    private bool m_cardLinePlayed;
    private bool m_heroPowerLinePlayed;
    private bool m_moiraDead;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__IteratorF9 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__IteratorF8 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__IteratorF7 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA03_1_RESPONSE_03",
            m_stringTag = "VO_BRMA03_1_RESPONSE_03"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA03_1_RESPONSE_03");
        base.PreloadSound("VO_BRMA03_1_HERO_POWER_06");
        base.PreloadSound("VO_BRMA03_1_CARD_04");
        base.PreloadSound("VO_BRMA03_1_MOIRA_DEATH_05");
        base.PreloadSound("VO_BRMA03_1_VS_RAG_07");
        base.PreloadSound("VO_BRMA03_1_TURN1_02");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__IteratorF6 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__IteratorF9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal BRM03_Thaurissan <>f__this;
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
                    if (!this.<>f__this.m_moiraDead)
                    {
                        this.$current = new WaitForSeconds(5f);
                        this.$PC = 2;
                    }
                    else
                    {
                        this.$current = new WaitForSeconds(5f);
                        this.$PC = 1;
                    }
                    return true;

                case 1:
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_THAURISSAN_DEAD2_33"), "VO_NEFARIAN_THAURISSAN_DEAD2_33", true, 0f, CanvasAnchor.BOTTOM_LEFT);
                    break;

                case 2:
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_THAURISSAN_DEAD_32"), "VO_NEFARIAN_THAURISSAN_DEAD_32", true, 0f, CanvasAnchor.BOTTOM_LEFT);
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
    private sealed class <HandleMissionEventWithTiming>c__IteratorF8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal BRM03_Thaurissan <>f__this;
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
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    switch (this.missionEvent)
                    {
                        case 1:
                            this.<>f__this.m_moiraDead = true;
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA03_1_MOIRA_DEATH_05", "VO_BRMA03_1_MOIRA_DEATH_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            this.$PC = 1;
                            return true;

                        case 2:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA03_1_VS_RAG_07", "VO_BRMA03_1_VS_RAG_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            goto Label_00EE;
                    }
                    break;

                case 1:
                    GameState.Get().SetBusy(false);
                    break;

                default:
                    goto Label_00F5;
            }
        Label_00EE:
            this.$PC = -1;
        Label_00F5:
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
    private sealed class <HandleStartOfTurnWithTiming>c__IteratorF7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM03_Thaurissan <>f__this;
        internal Actor <enemyActor>__0;
        internal int turn;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            this.$PC = -1;
            if (this.$PC == 0)
            {
                this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                if (this.turn == 1)
                {
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA03_1_TURN1_02", "VO_BRMA03_1_TURN1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                }
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
    private sealed class <RespondToPlayedCardWithTiming>c__IteratorF6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM03_Thaurissan <>f__this;
        internal Actor <enemyActor>__0;
        internal Entity entity;

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
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_01AA;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_01A8;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01AA;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM03_Thaurissan.<>f__switch$map4 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                    dictionary.Add("BRMA03_2", 0);
                    dictionary.Add("BRMA_01", 1);
                    BRM03_Thaurissan.<>f__switch$map4 = dictionary;
                }
                if (BRM03_Thaurissan.<>f__switch$map4.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        if (this.<>f__this.m_heroPowerLinePlayed)
                        {
                            goto Label_01A8;
                        }
                        this.<>f__this.m_heroPowerLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA03_1_HERO_POWER_06", "VO_BRMA03_1_HERO_POWER_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                    else if (num2 == 1)
                    {
                        if (this.<>f__this.m_cardLinePlayed)
                        {
                            goto Label_01A8;
                        }
                        this.<>f__this.m_cardLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA03_1_CARD_04", "VO_BRMA03_1_CARD_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                }
            }
            this.$PC = -1;
        Label_01A8:
            return false;
        Label_01AA:
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

