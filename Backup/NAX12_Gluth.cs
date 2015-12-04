using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NAX12_Gluth : NAX_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map25;
    private bool m_achievementTauntPlayed;
    private bool m_cardLinePlayed;
    private int m_heroPowerLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator173 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator172 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX12_01_EMOTE_01",
            m_stringTag = "VO_NAX12_01_EMOTE_01"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX12_01_EMOTE_02",
            m_stringTag = "VO_NAX12_01_EMOTE_02"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX12_01_EMOTE_03",
            m_stringTag = "VO_NAX12_01_EMOTE_03"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_NAX12_01_HP_01");
        base.PreloadSound("VO_NAX12_01_EMOTE_03");
        base.PreloadSound("VO_NAX12_01_EMOTE_02");
        base.PreloadSound("VO_NAX12_01_EMOTE_01");
        base.PreloadSound("VO_NAX12_01_CARD_01");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator174 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator173 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal NAX12_Gluth <>f__this;
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
                    NotificationManager.Get().CreateKTQuote("VO_KT_GLUTH4_76", "VO_KT_GLUTH4_76", true);
                    break;

                default:
                    goto Label_00AC;
            }
            if ((this.gameResult == TAG_PLAYSTATE.LOST) && this.<>f__this.m_achievementTauntPlayed)
            {
                NotificationManager.Get().CreateKTQuote("VO_KT_GLUTH3_75", "VO_KT_GLUTH3_75", false);
                goto Label_00AC;
                this.$PC = -1;
            }
        Label_00AC:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator172 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal NAX12_Gluth <>f__this;
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
                switch (this.turn)
                {
                    case 1:
                        NotificationManager.Get().CreateKTQuote("VO_KT_GLUTH2_73", "VO_KT_GLUTH2_73", false);
                        break;

                    case 13:
                        this.<>f__this.m_achievementTauntPlayed = true;
                        NotificationManager.Get().CreateKTQuote("VO_KT_GLUTH2_ALT_74", "VO_KT_GLUTH2_ALT_74", false);
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator174 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal NAX12_Gluth <>f__this;
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
                        goto Label_0234;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    NotificationManager.Get().CreateKTQuote("VO_KT_GLUTH5_77", "VO_KT_GLUTH5_77", false);
                    goto Label_022B;

                case 4:
                    NotificationManager.Get().CreateKTQuote("VO_KT_GLUTH6_78", "VO_KT_GLUTH6_78", false);
                    goto Label_022B;

                default:
                    goto Label_0232;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0234;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (NAX12_Gluth.<>f__switch$map25 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                    dictionary.Add("NAX12_02", 0);
                    dictionary.Add("NAX12_04", 1);
                    NAX12_Gluth.<>f__switch$map25 = dictionary;
                }
                if (NAX12_Gluth.<>f__switch$map25.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        if (this.<>f__this.m_heroPowerLinePlayed > 2)
                        {
                            goto Label_0232;
                        }
                        this.<>f__this.m_heroPowerLinePlayed++;
                        if (this.<>f__this.m_heroPowerLinePlayed == 1)
                        {
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX12_01_HP_01", "VO_NAX12_01_HP_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            goto Label_022B;
                        }
                        this.$current = new WaitForSeconds(2f);
                        this.$PC = 3;
                        goto Label_0234;
                    }
                    if (num2 == 1)
                    {
                        if (this.<>f__this.m_cardLinePlayed)
                        {
                            goto Label_0232;
                        }
                        this.<>f__this.m_cardLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX12_01_CARD_01", "VO_NAX12_01_CARD_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                        this.$current = new WaitForSeconds(1f);
                        this.$PC = 4;
                        goto Label_0234;
                    }
                }
            }
        Label_022B:
            this.$PC = -1;
        Label_0232:
            return false;
        Label_0234:
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

