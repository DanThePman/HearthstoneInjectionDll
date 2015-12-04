using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE02_Sun_Raider_Phaerix : LOE_MissionEntity
{
    private bool m_damageLinePlayed;
    private int m_staffLinesPlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator12E { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator12C { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator12D { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_LOE_01_RESPONSE",
            m_stringTag = "VO_LOE_01_RESPONSE"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOE_01_RESPONSE");
        base.PreloadSound("VO_LOE_01_WOUNDED");
        base.PreloadSound("VO_LOE_01_STAFF");
        base.PreloadSound("VO_LOE_01_STAFF_2");
        base.PreloadSound("VO_LOE_02_PHAERIX_STAFF_RECOVER");
        base.PreloadSound("VO_LOE_01_STAFF_2_RENO");
        base.PreloadSound("VO_LOE_01_WIN_2");
        base.PreloadSound("VO_LOE_01_WIN_2_ALT_2");
        base.PreloadSound("VO_LOE_01_START");
        base.PreloadSound("VO_LOE_01_WIN");
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator12E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE02_Sun_Raider_Phaerix <>f__this;
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
                    goto Label_00E8;

                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Reno_Quote", "VO_LOE_01_WIN", "VO_LOE_01_WIN", 0f, false, false));
                    this.$PC = 2;
                    goto Label_00E8;

                case 2:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Reno_Quote", "VO_LOE_01_WIN_2_ALT_2", "VO_LOE_01_WIN_2_ALT_2", 0f, false, false));
                    this.$PC = 3;
                    goto Label_00E8;

                case 3:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00E8:
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator12C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal LOE02_Sun_Raider_Phaerix <>f__this;
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
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    if (this.<>f__this.m_staffLinesPlayed >= this.missionEvent)
                    {
                        break;
                    }
                    if (this.missionEvent <= 9)
                    {
                        this.<>f__this.m_staffLinesPlayed = this.missionEvent;
                        switch (this.missionEvent)
                        {
                            case 1:
                                GameState.Get().SetBusy(true);
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_01_STAFF", 3f, 1f, false));
                                this.$PC = 2;
                                goto Label_0227;

                            case 2:
                                GameState.Get().SetBusy(true);
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_02_PHAERIX_STAFF_RECOVER", "VO_LOE_02_PHAERIX_STAFF_RECOVER", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                GameState.Get().SetBusy(false);
                                goto Label_0225;

                            case 3:
                                GameState.Get().SetBusy(true);
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_01_STAFF_2_RENO", 3f, 1f, false));
                                this.$PC = 3;
                                goto Label_0227;
                        }
                        break;
                    }
                    if (this.<>f__this.m_damageLinePlayed)
                    {
                        break;
                    }
                    this.<>f__this.m_damageLinePlayed = true;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_01_WOUNDED", 3f, 1f, false));
                    this.$PC = 1;
                    goto Label_0227;

                case 1:
                    break;

                case 2:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_01_STAFF_2", "VO_LOE_01_STAFF_2", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    GameState.Get().SetBusy(false);
                    break;

                case 3:
                    GameState.Get().SetBusy(false);
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
        Label_0225:
            return false;
        Label_0227:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator12D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE02_Sun_Raider_Phaerix <>f__this;
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
                    if (this.turn == 1)
                    {
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_LOE_01_START", "VO_LOE_01_START", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                        this.$current = new WaitForSeconds(4f);
                        this.$PC = 1;
                        goto Label_00EC;
                    }
                    break;

                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Reno_BigQuote", "VO_LOE_01_WIN_2", 3f, 1f, true, false));
                    this.$PC = 2;
                    goto Label_00EC;

                case 2:
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00EC:
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

