using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE04_Scarvash : LOE_MissionEntity
{
    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator133 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator132 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_LOE_04_RESPONSE",
            m_stringTag = "VO_LOE_04_RESPONSE"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOEA04_BRANN_TURN_1");
        base.PreloadSound("VO_LOE_04_SCARVASH_TURN_2");
        base.PreloadSound("VO_BRANN_MITHRIL_ALT_02");
        base.PreloadSound("VO_LOE_SCARVASH_TURN_6_CARTOGRAPHER");
        base.PreloadSound("VO_LOE_04_RESPONSE");
        base.PreloadSound("VO_LOE_04_WIN");
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator133 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE04_Scarvash <>f__this;
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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Brann_Quote", "VO_LOE_04_WIN", 0f, false, false));
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator132 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE04_Scarvash <>f__this;
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
                    switch (this.turn)
                    {
                        case 1:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA04_BRANN_TURN_1", 3f, 1f, false));
                            this.$PC = 1;
                            goto Label_0157;

                        case 7:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_BRANN_MITHRIL_ALT_02", 3f, 1f, false));
                            this.$PC = 2;
                            goto Label_0157;
                    }
                    break;

                case 1:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_04_SCARVASH_TURN_2", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 3f, 1f, true, false));
                    break;

                case 2:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_SCARVASH_TURN_6_CARTOGRAPHER", 3f, 1f, false));
                    this.$PC = 3;
                    goto Label_0157;

                case 3:
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0157:
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

