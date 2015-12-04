using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NAX03_Maexxna : NAX_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map1C;
    private bool m_heroPowerLinePlayed;
    private bool m_seaGiantLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator15B { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator15A { turn = turn, <$>turn = turn };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX3_01_EMOTE_01",
            m_stringTag = "VO_NAX3_01_EMOTE_01"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX3_01_EMOTE_02",
            m_stringTag = "VO_NAX3_01_EMOTE_02"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX3_01_EMOTE_03",
            m_stringTag = "VO_NAX3_01_EMOTE_03"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX3_01_EMOTE_04",
            m_stringTag = "VO_NAX3_01_EMOTE_04"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX3_01_EMOTE_05",
            m_stringTag = "VO_NAX3_01_EMOTE_05"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_NAX3_01_EMOTE_01");
        base.PreloadSound("VO_NAX3_01_EMOTE_02");
        base.PreloadSound("VO_NAX3_01_EMOTE_03");
        base.PreloadSound("VO_NAX3_01_EMOTE_04");
        base.PreloadSound("VO_NAX3_01_EMOTE_05");
        base.PreloadSound("VO_NAX3_01_CARD_01");
        base.PreloadSound("VO_NAX3_01_HP_01");
        base.PreloadSound("VO_KT_MAEXXNA2_47");
        base.PreloadSound("VO_KT_MAEXXNA6_51");
        base.PreloadSound("VO_KT_MAEXXNA3_48");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator15C { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator15B : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateKTQuote("VO_KT_MAEXXNA4_49", "VO_KT_MAEXXNA4_49", true);
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator15A : IDisposable, IEnumerator, IEnumerator<object>
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
            if ((this.$PC == 0) && (this.turn == 1))
            {
                NotificationManager.Get().CreateKTQuote("VO_KT_MAEXXNA2_47", "VO_KT_MAEXXNA2_47", false);
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator15C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal NAX03_Maexxna <>f__this;
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
                        goto Label_027C;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_0182;

                case 4:
                case 5:
                    while (NotificationManager.Get().IsQuotePlaying)
                    {
                        this.$current = 0;
                        this.$PC = 5;
                        goto Label_027C;
                    }
                    NotificationManager.Get().CreateKTQuote("VO_KT_MAEXXNA3_48", "VO_KT_MAEXXNA3_48", false);
                    goto Label_0273;

                default:
                    goto Label_027A;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_027C;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (NAX03_Maexxna.<>f__switch$map1C == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                    dictionary.Add("NAX3_02", 0);
                    dictionary.Add("NAX3_03", 1);
                    dictionary.Add("EX1_586", 2);
                    NAX03_Maexxna.<>f__switch$map1C = dictionary;
                }
                if (NAX03_Maexxna.<>f__switch$map1C.TryGetValue(cardId, out num2))
                {
                    switch (num2)
                    {
                        case 0:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX3_01_HP_01", "VO_NAX3_01_HP_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            if (!this.<>f__this.m_heroPowerLinePlayed)
                            {
                                this.<>f__this.m_heroPowerLinePlayed = true;
                                goto Label_0182;
                            }
                            goto Label_027A;

                        case 1:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX3_01_CARD_01", "VO_NAX3_01_CARD_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            break;

                        case 2:
                            if (!this.<>f__this.m_seaGiantLinePlayed)
                            {
                                this.<>f__this.m_seaGiantLinePlayed = true;
                                this.$current = new WaitForSeconds(1f);
                                this.$PC = 4;
                                goto Label_027C;
                            }
                            goto Label_027A;
                    }
                }
            }
            goto Label_0273;
        Label_0182:
            while (this.<>f__this.m_enemySpeaking || NotificationManager.Get().IsQuotePlaying)
            {
                this.$current = 0;
                this.$PC = 3;
                goto Label_027C;
            }
            NotificationManager.Get().CreateKTQuote("VO_KT_MAEXXNA6_51", "VO_KT_MAEXXNA6_51", false);
        Label_0273:
            this.$PC = -1;
        Label_027A:
            return false;
        Label_027C:
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

