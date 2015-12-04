using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE13_Skelesaurus : LOE_MissionEntity
{
    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator147 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator146 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "LOEA13_1_SkelesaurusHex_EmoteResponse",
            m_stringTag = "VO_LOE_13_RESPONSE"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOE_13_TURN_1");
        base.PreloadSound("VO_LOE_13_TURN_1_2");
        base.PreloadSound("VO_LOE_13_TURN_5");
        base.PreloadSound("VO_LOE_13_TURN_5_2");
        base.PreloadSound("VO_LOE_13_TURN_9");
        base.PreloadSound("VO_LOE_13_TURN_9_2");
        base.PreloadSound("VO_LOE_13_WIN");
        base.PreloadSound("LOEA13_1_SkelesaurusHex_EmoteResponse");
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator147 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE13_Skelesaurus <>f__this;
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
                    goto Label_00A2;

                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Cartographer_Quote", "VO_LOE_13_WIN", 0f, false, false));
                    this.$PC = 2;
                    goto Label_00A2;

                case 2:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00A2:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator146 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE13_Skelesaurus <>f__this;
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
                case 1:
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0203;
                    }
                    switch (this.turn)
                    {
                        case 1:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_13_TURN_1", 3f, 1f, false));
                            this.$PC = 2;
                            goto Label_0203;

                        case 5:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Rafaam_wrap_BigQuote", "VO_LOE_13_TURN_5", 3f, 1f, false));
                            this.$PC = 4;
                            goto Label_0203;

                        case 9:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOE_13_TURN_9", 3f, 1f, false));
                            this.$PC = 6;
                            goto Label_0203;
                    }
                    break;

                case 2:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Rafaam_wrap_BigQuote", "VO_LOE_13_TURN_1_2", 3f, 1f, false));
                    this.$PC = 3;
                    goto Label_0203;

                case 3:
                case 5:
                case 7:
                    break;

                case 4:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_13_TURN_5_2", 3f, 1f, false));
                    this.$PC = 5;
                    goto Label_0203;

                case 6:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_13_TURN_9_2", 3f, 1f, false));
                    this.$PC = 7;
                    goto Label_0203;

                default:
                    goto Label_0201;
            }
            this.$PC = -1;
        Label_0201:
            return false;
        Label_0203:
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

