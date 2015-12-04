using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM01_GrimGuzzler : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map2;
    private bool m_cardLinePlayed;
    private bool m_disableSpecialCardVO = true;
    private bool m_eTCLinePlayed;
    private bool m_heroPowerLinePlayed;
    private bool m_succubusLinePlayed;
    private bool m_warGolemLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__IteratorF2 { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__IteratorF0 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__IteratorF1 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA01_1_RESPONSE_03",
            m_stringTag = "VO_BRMA01_1_RESPONSE_03"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA01_1_RESPONSE_03");
        base.PreloadSound("VO_BRMA01_1_HERO_POWER_04");
        base.PreloadSound("VO_BRMA01_1_CARD_05");
        base.PreloadSound("VO_BRMA01_1_ETC_06");
        base.PreloadSound("VO_BRMA01_1_SUCCUBUS_08");
        base.PreloadSound("VO_BRMA01_1_WARGOLEM_07");
        base.PreloadSound("VO_BRMA01_1_TURN1_02");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__IteratorEF { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__IteratorF2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
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
                    return true;

                case 1:
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_COREN_DEAD_28"), "VO_NEFARIAN_COREN_DEAD_28", true, 0f, CanvasAnchor.BOTTOM_LEFT);
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
    private sealed class <HandleMissionEventWithTiming>c__IteratorF0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal BRM01_GrimGuzzler <>f__this;
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
                case 1:
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    switch (this.missionEvent)
                    {
                        case 1:
                            if (!this.<>f__this.m_eTCLinePlayed)
                            {
                                if (!this.<>f__this.m_disableSpecialCardVO)
                                {
                                    this.<>f__this.m_eTCLinePlayed = true;
                                    GameState.Get().SetBusy(true);
                                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA01_1_ETC_06", "VO_BRMA01_1_ETC_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                    GameState.Get().SetBusy(false);
                                }
                                break;
                            }
                            goto Label_0200;

                        case 2:
                            if (!this.<>f__this.m_succubusLinePlayed)
                            {
                                if (!this.<>f__this.m_disableSpecialCardVO)
                                {
                                    this.<>f__this.m_succubusLinePlayed = true;
                                    GameState.Get().SetBusy(true);
                                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA01_1_SUCCUBUS_08", "VO_BRMA01_1_SUCCUBUS_08", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                    GameState.Get().SetBusy(false);
                                }
                                break;
                            }
                            goto Label_0200;

                        case 3:
                            if (!this.<>f__this.m_warGolemLinePlayed)
                            {
                                if (!this.<>f__this.m_disableSpecialCardVO)
                                {
                                    this.<>f__this.m_warGolemLinePlayed = true;
                                    GameState.Get().SetBusy(true);
                                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA01_1_WARGOLEM_07", "VO_BRMA01_1_WARGOLEM_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                    GameState.Get().SetBusy(false);
                                }
                                break;
                            }
                            goto Label_0200;
                    }
                    break;

                default:
                    goto Label_0207;
            }
        Label_0200:
            this.$PC = -1;
        Label_0207:
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
    private sealed class <HandleStartOfTurnWithTiming>c__IteratorF1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM01_GrimGuzzler <>f__this;
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
                switch (this.turn)
                {
                    case 1:
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA01_1_TURN1_02", "VO_BRMA01_1_TURN1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                        break;

                    case 3:
                        this.<>f__this.m_disableSpecialCardVO = false;
                        break;
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
    private sealed class <RespondToPlayedCardWithTiming>c__IteratorEF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM01_GrimGuzzler <>f__this;
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
                        goto Label_01B6;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_01B4;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01B6;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM01_GrimGuzzler.<>f__switch$map2 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                    dictionary.Add("BRMA01_2", 0);
                    dictionary.Add("BRMA01_2H", 0);
                    dictionary.Add("BRMA01_4", 1);
                    BRM01_GrimGuzzler.<>f__switch$map2 = dictionary;
                }
                if (BRM01_GrimGuzzler.<>f__switch$map2.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        if (this.<>f__this.m_heroPowerLinePlayed)
                        {
                            goto Label_01B4;
                        }
                        this.<>f__this.m_heroPowerLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA01_1_HERO_POWER_04", "VO_BRMA01_1_HERO_POWER_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                    else if (num2 == 1)
                    {
                        if (this.<>f__this.m_cardLinePlayed)
                        {
                            goto Label_01B4;
                        }
                        this.<>f__this.m_cardLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA01_1_CARD_05", "VO_BRMA01_1_CARD_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                }
            }
            this.$PC = -1;
        Label_01B4:
            return false;
        Label_01B6:
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

