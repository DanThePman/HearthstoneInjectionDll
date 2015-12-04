using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE12_Naga : LOE_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map17;
    private bool m_pearlLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator145 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator143 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator144 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_LOE_12_RESPONSE",
            m_stringTag = "VO_LOE_12_RESPONSE"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOE_12_RESPONSE");
        base.PreloadSound("VO_LOE_12_NAZJAR_TURN_1");
        base.PreloadSound("VO_LOE_12_NAZJAR_TURN_1_FINLEY");
        base.PreloadSound("VO_LOE_12_NAZJAR_TURN_3_FINLEY");
        base.PreloadSound("VO_LOE_NAZJAR_TURN_3_CARTOGRAPHER");
        base.PreloadSound("VO_LOE_12_NAZJAR_TURN_5");
        base.PreloadSound("VO_LOE_12_NAZJAR_TURN_5_FINLEY");
        base.PreloadSound("VO_LOE_12_WIN");
        base.PreloadSound("VO_LOE_12_WIN_2");
        base.PreloadSound("VO_LOE_12_NAZJAR_PEARL");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator142 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator145 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE12_Naga <>f__this;
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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Blaggh_Quote", "VO_LOE_12_WIN", "VO_LOE_12_WIN", 0f, false, false));
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator143 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal LOE12_Naga <>f__this;
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
                        GameState.Get().GetFriendlySidePlayer().WipeZzzs();
                        GameState.Get().GetOpposingSidePlayer().WipeZzzs();
                        break;
                    }
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator144 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE12_Naga <>f__this;
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
                case 1:
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_023E;
                    }
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    switch (this.turn)
                    {
                        case 1:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_12_NAZJAR_TURN_1", "VO_LOE_12_NAZJAR_TURN_1", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            this.$current = new WaitForSeconds(6.7f);
                            this.$PC = 2;
                            goto Label_023E;

                        case 5:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOE_12_NAZJAR_TURN_3_FINLEY", 3f, 1f, false));
                            this.$PC = 4;
                            goto Label_023E;

                        case 9:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_12_NAZJAR_TURN_5", "VO_LOE_12_NAZJAR_TURN_5", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            this.$current = new WaitForSeconds(5.7f);
                            this.$PC = 6;
                            goto Label_023E;
                    }
                    break;

                case 2:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOE_12_NAZJAR_TURN_1_FINLEY", 3f, 1f, false));
                    this.$PC = 3;
                    goto Label_023E;

                case 3:
                case 5:
                case 7:
                    break;

                case 4:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_NAZJAR_TURN_3_CARTOGRAPHER", 3f, 1f, false));
                    this.$PC = 5;
                    goto Label_023E;

                case 6:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOE_12_NAZJAR_TURN_5_FINLEY", 3f, 1f, false));
                    this.$PC = 7;
                    goto Label_023E;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_023E:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator142 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal LOE12_Naga <>f__this;
        internal string <cardID>__0;
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
                        goto Label_0142;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_0139;

                default:
                    goto Label_0140;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0142;
            }
            this.<cardID>__0 = this.entity.GetCardId();
            string key = this.<cardID>__0;
            if (key != null)
            {
                int num2;
                if (LOE12_Naga.<>f__switch$map17 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(1);
                    dictionary.Add("LOEA12_3", 0);
                    LOE12_Naga.<>f__switch$map17 = dictionary;
                }
                if (LOE12_Naga.<>f__switch$map17.TryGetValue(key, out num2) && (num2 == 0))
                {
                    if (this.<>f__this.m_pearlLinePlayed)
                    {
                        goto Label_0140;
                    }
                    this.<>f__this.m_pearlLinePlayed = true;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOE_12_NAZJAR_PEARL", 3f, 1f, false));
                    this.$PC = 3;
                    goto Label_0142;
                }
            }
        Label_0139:
            this.$PC = -1;
        Label_0140:
            return false;
        Label_0142:
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

