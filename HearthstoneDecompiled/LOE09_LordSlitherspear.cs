using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE09_LordSlitherspear : LOE_MissionEntity
{
    private Card m_cauldronCard;
    private bool m_finley_death_line;
    private bool m_finley_saved;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator13D { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator13B { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator13C { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_LOEA09_1_RESPONSE",
            m_stringTag = "VO_LOEA09_1_RESPONSE"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOEA09_1_RESPONSE");
        base.PreloadSound("VO_LOEA09_UNSTABLE");
        base.PreloadSound("VO_LOEA09_HERO_POWER");
        base.PreloadSound("FX_MinionSummon_Cast");
        base.PreloadSound("VO_LOEA09_QUOTE1");
        base.PreloadSound("VO_LOEA09_FINLEY_DEATH");
        base.PreloadSound("VO_LOEA09_HERO_POWER1");
        base.PreloadSound("VO_LOEA09_HERO_POWER2");
        base.PreloadSound("VO_LOEA09_HERO_POWER3");
        base.PreloadSound("VO_LOEA09_HERO_POWER4");
        base.PreloadSound("VO_LOEA09_HERO_POWER5");
        base.PreloadSound("VO_LOEA09_HERO_POWER6");
        base.PreloadSound("VO_LOEA09_WIN");
    }

    public override void StartGameplaySoundtracks()
    {
        MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_LOE_Wing3);
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator13D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE09_LordSlitherspear <>f__this;
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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Blaggh_Quote", "VO_LOEA09_WIN", "VO_LOEA09_WIN", 0f, false, false));
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator13B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal LOE09_LordSlitherspear <>f__this;
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
                    break;

                case 1:
                    break;

                case 2:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA09_UNSTABLE", "VO_LOEA09_UNSTABLE", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    this.$PC = 3;
                    goto Label_01B2;

                case 3:
                    GameState.Get().SetBusy(false);
                    goto Label_01A9;

                case 4:
                    GameState.Get().SetBusy(false);
                    goto Label_01A9;

                default:
                    goto Label_01B0;
            }
            if (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_01B2;
            }
            switch (this.missionEvent)
            {
                case 1:
                    this.<>f__this.m_finley_saved = true;
                    GameState.Get().SetBusy(true);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOEA09_QUOTE1", 3f, 1f, false));
                    this.$PC = 2;
                    goto Label_01B2;

                case 3:
                    if (!this.<>f__this.m_finley_death_line)
                    {
                        this.<>f__this.m_finley_death_line = true;
                        GameState.Get().SetBusy(true);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOEA09_FINLEY_DEATH", 3f, 1f, false));
                        this.$PC = 4;
                        goto Label_01B2;
                    }
                    goto Label_01B0;
            }
        Label_01A9:
            this.$PC = -1;
        Label_01B0:
            return false;
        Label_01B2:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator13C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE09_LordSlitherspear <>f__this;
        internal Entity <cauldron>__1;
        internal int <cauldronId>__0;
        internal Actor <enemyActor>__2;
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
                    if (!this.<>f__this.m_finley_saved)
                    {
                        if (this.<>f__this.m_cauldronCard == null)
                        {
                            this.<cauldronId>__0 = GameState.Get().GetGameEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_1);
                            this.<cauldron>__1 = GameState.Get().GetEntity(this.<cauldronId>__0);
                            if (this.<cauldron>__1 != null)
                            {
                                this.<>f__this.m_cauldronCard = this.<cauldron>__1.GetCard();
                            }
                        }
                        if (((this.<>f__this.m_cauldronCard != null) || (this.turn <= 1)) && ((this.<>f__this.m_cauldronCard == null) || (this.<>f__this.m_cauldronCard.GetEntity().GetZone() == TAG_ZONE.PLAY)))
                        {
                            this.<enemyActor>__2 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                            break;
                        }
                    }
                    goto Label_0366;

                case 1:
                    break;

                case 2:
                    if (this.<>f__this.m_cauldronCard != null)
                    {
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOEA09_HERO_POWER1", "VO_LOEA09_HERO_POWER1", 3f, 1f, false));
                        this.$PC = 3;
                        goto Label_0368;
                    }
                    goto Label_0366;

                default:
                    goto Label_0366;
            }
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0368;
            }
            switch (this.turn)
            {
                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA09_HERO_POWER", "VO_LOEA09_HERO_POWER", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__2, 1f, true, false));
                    this.$PC = 2;
                    goto Label_0368;

                case 2:
                case 3:
                case 5:
                case 7:
                case 9:
                case 11:
                    break;

                case 4:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA09_HERO_POWER2", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_cauldronCard.GetActor(), 3f, 1f, true, false));
                    break;

                case 6:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA09_HERO_POWER3", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_cauldronCard.GetActor(), 3f, 1f, true, false));
                    break;

                case 8:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA09_HERO_POWER4", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_cauldronCard.GetActor(), 3f, 1f, true, false));
                    break;

                case 10:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA09_HERO_POWER5", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_cauldronCard.GetActor(), 3f, 1f, true, false));
                    break;

                case 12:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA09_HERO_POWER6", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_cauldronCard.GetActor(), 3f, 1f, true, false));
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
        Label_0366:
            return false;
        Label_0368:
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

