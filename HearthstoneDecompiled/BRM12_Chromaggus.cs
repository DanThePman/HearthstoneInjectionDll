using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM12_Chromaggus : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$mapC;
    private bool m_cardLinePlayed;
    private bool m_heroPowerLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator117 { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator116 { turn = turn, <$>turn = turn };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "ChromaggusBoss_EmoteResponse_1",
            m_stringTag = "ChromaggusBoss_EmoteResponse_1"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("ChromaggusBoss_EmoteResponse_1");
        base.PreloadSound("VO_NEFARIAN_CHROMAGGUS_DEAD_63");
        base.PreloadSound("VO_NEFARIAN_CHROMAGGUS1_59");
        base.PreloadSound("VO_NEFARIAN_CHROMAGGUS2_60");
        base.PreloadSound("VO_NEFARIAN_CHROMAGGUS3_61");
        base.PreloadSound("VO_NEFARIAN_CHROMAGGUS4_62");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator115 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator117 : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_CHROMAGGUS_DEAD_63"), "VO_NEFARIAN_CHROMAGGUS_DEAD_63", true, 0f, CanvasAnchor.BOTTOM_LEFT);
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator116 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal Vector3 <quotePos>__0;
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
                this.<quotePos>__0 = new Vector3(95f, NotificationManager.DEPTH, 36.8f);
                switch (this.turn)
                {
                    case 2:
                        NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_CHROMAGGUS1_59"), "VO_NEFARIAN_CHROMAGGUS1_59", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                        break;

                    case 6:
                        NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_CHROMAGGUS2_60"), "VO_NEFARIAN_CHROMAGGUS2_60", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator115 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM12_Chromaggus <>f__this;
        internal Vector3 <quotePos>__0;
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
                    this.<quotePos>__0 = new Vector3(95f, NotificationManager.DEPTH, 36.8f);
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_007F;

                default:
                    goto Label_01AD;
            }
            if (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_01AF;
            }
        Label_007F:
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01AF;
            }
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM12_Chromaggus.<>f__switch$mapC == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                    dictionary.Add("BRMA12_2", 0);
                    dictionary.Add("BRMA12_2H", 0);
                    dictionary.Add("BRMA12_8", 1);
                    BRM12_Chromaggus.<>f__switch$mapC = dictionary;
                }
                if (BRM12_Chromaggus.<>f__switch$mapC.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        if (this.<>f__this.m_heroPowerLinePlayed)
                        {
                            goto Label_01AD;
                        }
                        this.<>f__this.m_heroPowerLinePlayed = true;
                        NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_CHROMAGGUS4_62"), "VO_NEFARIAN_CHROMAGGUS4_62", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                    }
                    else if (num2 == 1)
                    {
                        if (this.<>f__this.m_cardLinePlayed)
                        {
                            goto Label_01AD;
                        }
                        this.<>f__this.m_cardLinePlayed = true;
                        NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_CHROMAGGUS3_61"), "VO_NEFARIAN_CHROMAGGUS3_61", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                    }
                }
            }
            this.$PC = -1;
        Label_01AD:
            return false;
        Label_01AF:
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

