using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE08_Archaedas : LOE_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map15;
    private HashSet<string> m_playedLines = new HashSet<string>();

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator13A { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator138 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_LOE_08_RESPONSE",
            m_stringTag = "VO_LOE_08_RESPONSE"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOE_08_RESPONSE");
        base.PreloadSound("VO_LOEA08_TURN_1_BRANN");
        base.PreloadSound("VO_LOE_ARCHAEDAS_TURN_1_CARTOGRAPHER");
        base.PreloadSound("VO_LOE_08_LANDSLIDE");
        base.PreloadSound("VO_LOE_08_ANIMATE_STONE");
        base.PreloadSound("VO_LOE_08_WIN");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator139 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator13A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE08_Archaedas <>f__this;
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
                    goto Label_00A7;

                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Brann_Quote", "VO_LOE_08_WIN", "VO_LOE_08_WIN", 0f, false, false));
                    this.$PC = 2;
                    goto Label_00A7;

                case 2:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00A7:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator138 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE08_Archaedas <>f__this;
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
                    if (this.turn == 1)
                    {
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA08_TURN_1_BRANN", 3f, 1f, false));
                        this.$PC = 1;
                        goto Label_00C3;
                    }
                    break;

                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_ARCHAEDAS_TURN_1_CARTOGRAPHER", 5f, 1f, false));
                    this.$PC = 2;
                    goto Label_00C3;

                case 2:
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00C3:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator139 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal LOE08_Archaedas <>f__this;
        internal string <cardID>__1;
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
                        goto Label_01C7;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_01C5;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01C7;
            }
            if (!this.<>f__this.m_playedLines.Contains(this.entity.GetCardId()))
            {
                this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                this.<cardID>__1 = this.entity.GetCardId();
                string key = this.<cardID>__1;
                if (key != null)
                {
                    int num2;
                    if (LOE08_Archaedas.<>f__switch$map15 == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                        dictionary.Add("LOEA06_04", 0);
                        dictionary.Add("LOEA06_03", 1);
                        LOE08_Archaedas.<>f__switch$map15 = dictionary;
                    }
                    if (LOE08_Archaedas.<>f__switch$map15.TryGetValue(key, out num2))
                    {
                        if (num2 == 0)
                        {
                            this.<>f__this.m_playedLines.Add(this.<cardID>__1);
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_08_LANDSLIDE", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 3f, 1f, true, false));
                        }
                        else if (num2 == 1)
                        {
                            this.<>f__this.m_playedLines.Add(this.<cardID>__1);
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_08_ANIMATE_STONE", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 3f, 1f, true, false));
                        }
                    }
                }
                this.$PC = -1;
            }
        Label_01C5:
            return false;
        Label_01C7:
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

