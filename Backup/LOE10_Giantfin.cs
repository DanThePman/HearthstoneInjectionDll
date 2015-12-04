using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE10_Giantfin : LOE_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map16;
    private bool m_cardLinePlayed1;
    private bool m_cardLinePlayed2;
    private bool m_nyahLinePlayed;
    private int m_turnToPlayFoundLine = -1;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator141 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator13E { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator140 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_LOE_10_RESPONSE",
            m_stringTag = "VO_LOE_10_RESPONSE"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOEA10_1_MIDDLEFIN");
        base.PreloadSound("VO_LOE10_NYAH_FINLEY");
        base.PreloadSound("VO_LOE_10_NYAH");
        base.PreloadSound("VO_LOE_10_RESPONSE");
        base.PreloadSound("VO_LOE_10_START_2");
        base.PreloadSound("VO_LOE_10_TURN1");
        base.PreloadSound("VO_LOE_10_WIN");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator13F { entity = entity, <$>entity = entity, <>f__this = this };
    }

    public override void StartGameplaySoundtracks()
    {
        MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_LOE_Wing3);
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator141 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE10_Giantfin <>f__this;
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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Blaggh_Quote", "VO_LOE_10_WIN", "VO_LOE_10_WIN", 0f, false, false));
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator13E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal LOE10_Giantfin <>f__this;
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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.HandleMissionEventWithTiming(this.missionEvent));
                    this.$PC = 1;
                    goto Label_0136;

                case 1:
                case 2:
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 2;
                        goto Label_0136;
                    }
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    switch (this.missionEvent)
                    {
                        case 2:
                            if (!this.<>f__this.m_cardLinePlayed2)
                            {
                                this.<>f__this.m_cardLinePlayed2 = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA10_1_MIDDLEFIN", "VO_LOEA10_1_MIDDLEFIN", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            }
                            goto Label_0134;

                        case 3:
                            if (!this.<>f__this.m_cardLinePlayed1)
                            {
                                this.<>f__this.m_cardLinePlayed1 = true;
                                break;
                                this.$PC = -1;
                            }
                            goto Label_0134;
                    }
                    break;
            }
        Label_0134:
            return false;
        Label_0136:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator140 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE10_Giantfin <>f__this;
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
                    }
                    else
                    {
                        this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                        if (this.<>f__this.m_turnToPlayFoundLine == 5)
                        {
                            this.<>f__this.m_turnToPlayFoundLine = 7;
                        }
                        if (this.turn == this.<>f__this.m_turnToPlayFoundLine)
                        {
                            this.<>f__this.m_turnToPlayFoundLine = -1;
                            break;
                        }
                        if (this.turn != 1)
                        {
                            break;
                        }
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_10_TURN1", "VO_LOE_10_TURN1", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                        this.$PC = 2;
                    }
                    goto Label_0150;

                case 2:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOE_10_START_2", 3f, 1f, false));
                    this.$PC = 3;
                    goto Label_0150;

                case 3:
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0150:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator13F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal LOE10_Giantfin <>f__this;
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
                        goto Label_01AF;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOE10_NYAH_FINLEY", 3f, 1f, false));
                    this.$PC = 4;
                    goto Label_01AF;

                case 4:
                    goto Label_01A6;

                default:
                    goto Label_01AD;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01AF;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            this.<cardID>__1 = this.entity.GetCardId();
            string key = this.<cardID>__1;
            if (key != null)
            {
                int num2;
                if (LOE10_Giantfin.<>f__switch$map16 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(1);
                    dictionary.Add("LOEA10_5", 0);
                    LOE10_Giantfin.<>f__switch$map16 = dictionary;
                }
                if (LOE10_Giantfin.<>f__switch$map16.TryGetValue(key, out num2) && (num2 == 0))
                {
                    if (this.<>f__this.m_nyahLinePlayed)
                    {
                        goto Label_01AD;
                    }
                    this.<>f__this.m_nyahLinePlayed = true;
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_10_NYAH", "VO_LOE_10_NYAH", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    this.$current = new WaitForSeconds(4f);
                    this.$PC = 3;
                    goto Label_01AF;
                }
            }
        Label_01A6:
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

