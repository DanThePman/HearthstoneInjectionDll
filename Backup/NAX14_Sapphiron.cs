using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NAX14_Sapphiron : NAX_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map27;
    private bool m_cardKtLinePlayed;
    private int m_numTimesFrostBreathMisses;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator178 { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator179 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator177 { turn = turn, <$>turn = turn };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX14_01_EMOTE_01",
            m_stringTag = "VO_NAX14_01_EMOTE_01"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX14_01_EMOTE_02",
            m_stringTag = "VO_NAX14_01_EMOTE_02"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX14_01_EMOTE_03",
            m_stringTag = "VO_NAX14_01_EMOTE_03"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_NAX14_01_EMOTE_01");
        base.PreloadSound("VO_NAX14_01_EMOTE_02");
        base.PreloadSound("VO_NAX14_01_EMOTE_03");
        base.PreloadSound("VO_NAX14_01_CARD_01");
        base.PreloadSound("VO_NAX14_01_HP_01");
        base.PreloadSound("VO_KT_SAPPHIRON2_84");
        base.PreloadSound("VO_KT_SAPPHIRON3_85");
        base.PreloadSound("VO_KT_SAPPHIRON4_ALT_87");
        base.PreloadSound("VO_KT_SAPPHIRON5_88");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator17A { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator178 : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateKTQuote("VO_KT_SAPPHIRON5_88", "VO_KT_SAPPHIRON5_88", true);
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator179 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal NAX14_Sapphiron <>f__this;
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
                    return true;

                case 1:
                    if (this.missionEvent == 1)
                    {
                        this.<>f__this.m_numTimesFrostBreathMisses++;
                        if (this.<>f__this.m_numTimesFrostBreathMisses == 4)
                        {
                            NotificationManager.Get().CreateKTQuote("VO_KT_SAPPHIRON3_85", "VO_KT_SAPPHIRON3_85", true);
                            break;
                            this.$PC = -1;
                        }
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator177 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal int turn;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            this.$PC = -1;
            if (((this.$PC == 0) && (this.turn == 1)) && (GameState.Get().GetOpposingSidePlayer().GetHero().GetCardId() != "NAX14_01H"))
            {
                NotificationManager.Get().CreateKTQuote("VO_KT_SAPPHIRON2_84", "VO_KT_SAPPHIRON2_84", false);
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator17A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal NAX14_Sapphiron <>f__this;
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
                        goto Label_01BF;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    if (!this.<>f__this.m_cardKtLinePlayed)
                    {
                        NotificationManager.Get().CreateKTQuote("VO_KT_SAPPHIRON4_ALT_87", "VO_KT_SAPPHIRON4_ALT_87", false);
                        this.<>f__this.m_cardKtLinePlayed = true;
                    }
                    else
                    {
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX14_01_CARD_01", "VO_NAX14_01_CARD_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                    goto Label_01B6;

                default:
                    goto Label_01BD;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01BF;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (NAX14_Sapphiron.<>f__switch$map27 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                    dictionary.Add("NAX14_02", 0);
                    dictionary.Add("NAX14_04", 1);
                    NAX14_Sapphiron.<>f__switch$map27 = dictionary;
                }
                if (NAX14_Sapphiron.<>f__switch$map27.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX14_01_HP_01", "VO_NAX14_01_HP_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                    else if (num2 == 1)
                    {
                        this.$current = new WaitForSeconds(1f);
                        this.$PC = 3;
                        goto Label_01BF;
                    }
                }
            }
        Label_01B6:
            this.$PC = -1;
        Label_01BD:
            return false;
        Label_01BF:
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

