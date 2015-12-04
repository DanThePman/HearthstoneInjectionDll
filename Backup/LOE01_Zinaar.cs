using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE01_Zinaar : LOE_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map12;
    private bool m_wishMoreWishesLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator12B { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator128 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator12A { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_LOE_02_RESPONSE",
            m_stringTag = "VO_LOE_02_RESPONSE"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOE_02_RESPONSE");
        base.PreloadSound("VO_LOE_02_WISH");
        base.PreloadSound("VO_LOE_02_START2");
        base.PreloadSound("VO_LOE_02_START3");
        base.PreloadSound("VO_LOE_02_TURN_6");
        base.PreloadSound("VO_LOE_ZINAAR_TURN_6_CARTOGRAPHER_2");
        base.PreloadSound("VO_LOE_02_TURN_6_2");
        base.PreloadSound("VO_LOE_ZINAAR_TURN_6_CARTOGRAPHER_2_ALT");
        base.PreloadSound("VO_LOE_02_TURN_10");
        base.PreloadSound("VO_LOE_ZINAAR_TURN_10_CARTOGRAPHER_2");
        base.PreloadSound("VO_LOE_02_WIN");
        base.PreloadSound("VO_LOE_02_MORE_WISHES");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToFriendlyPlayedCardWithTiming>c__Iterator129 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator12B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE01_Zinaar <>f__this;
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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Reno_Quote", "VO_LOE_02_WIN", 0f, false, false));
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator128 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal LOE01_Zinaar <>f__this;
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
                    if (this.missionEvent == 2)
                    {
                        GameState.Get().SetBusy(true);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_02_WISH", 3f, 1f, false));
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    GameState.Get().SetBusy(false);
                    break;

                default:
                    goto Label_0092;
            }
            this.$PC = -1;
        Label_0092:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator12A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE01_Zinaar <>f__this;
        internal int turn;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            int turn;
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
                        goto Label_029B;
                    }
                    turn = this.turn;
                    switch (turn)
                    {
                        case 7:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_02_TURN_6", 3f, 1f, false));
                            this.$PC = 4;
                            goto Label_029B;

                        case 9:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_02_TURN_6_2", 3f, 1f, false));
                            this.$PC = 6;
                            goto Label_029B;
                    }
                    break;

                case 2:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_02_START3", 3f, 1f, false));
                    this.$PC = 3;
                    goto Label_029B;

                case 4:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_ZINAAR_TURN_6_CARTOGRAPHER_2", 3f, 1f, false));
                    this.$PC = 5;
                    goto Label_029B;

                case 6:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_ZINAAR_TURN_6_CARTOGRAPHER_2_ALT", 3f, 1f, false));
                    this.$PC = 7;
                    goto Label_029B;

                case 8:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_ZINAAR_TURN_10_CARTOGRAPHER_2", 3f, 1f, false));
                    this.$PC = 9;
                    goto Label_029B;

                default:
                    goto Label_0299;
            }
            if (turn == 1)
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_02_START2", 3f, 1f, false));
                this.$PC = 2;
                goto Label_029B;
                this.$PC = -1;
            }
            else if (turn == 13)
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_02_TURN_10", 3f, 1f, false));
                this.$PC = 8;
                goto Label_029B;
            }
        Label_0299:
            return false;
        Label_029B:
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
    private sealed class <RespondToFriendlyPlayedCardWithTiming>c__Iterator129 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal LOE01_Zinaar <>f__this;
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
                if (LOE01_Zinaar.<>f__switch$map12 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(1);
                    dictionary.Add("LOEA02_06", 0);
                    LOE01_Zinaar.<>f__switch$map12 = dictionary;
                }
                if (LOE01_Zinaar.<>f__switch$map12.TryGetValue(key, out num2) && (num2 == 0))
                {
                    if (this.<>f__this.m_wishMoreWishesLinePlayed)
                    {
                        goto Label_0140;
                    }
                    this.<>f__this.m_wishMoreWishesLinePlayed = true;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_02_MORE_WISHES", 3f, 1f, false));
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

