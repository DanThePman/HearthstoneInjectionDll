using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class TB02_CoOp : MissionEntity
{
    private Card m_bossCard;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator17F { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator17E { turn = turn, <$>turn = turn, <>f__this = this };
    }

    public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
    {
        if (gameResult != TAG_PLAYSTATE.WON)
        {
            base.NotifyOfGameOver(gameResult);
        }
        else
        {
            PegCursor.Get().SetMode(PegCursor.Mode.STOPWAITING);
            MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_EndGameScreen);
            GameEntity.EndGameScreenContext callbackData = new GameEntity.EndGameScreenContext();
            SoundManager.Get().LoadAndPlay("victory_jingle");
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_DISPLAYED_WIN_SCREEN);
            AssetLoader.Get().LoadActor("VictoryTwoScoop", new AssetLoader.GameObjectCallback(this.OnEndGameScreenLoaded), callbackData, false);
        }
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("FX_MinionSummon_Cast");
        base.PreloadSound("CleanMechSmall_Trigger_Underlay");
        base.PreloadSound("CleanMechLarge_Play_Underlay");
        base.PreloadSound("CleanMechLarge_Death_Underlay");
    }

    private void SetUpBossCard()
    {
        if (this.m_bossCard == null)
        {
            int tag = GameState.Get().GetGameEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_1);
            Entity entity = GameState.Get().GetEntity(tag);
            if (entity != null)
            {
                this.m_bossCard = entity.GetCard();
            }
        }
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator17F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal TB02_CoOp <>f__this;
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
                case 1:
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.SetUpBossCard();
                    if (this.<>f__this.m_bossCard == null)
                    {
                        goto Label_0136;
                    }
                    switch (this.missionEvent)
                    {
                        case 5:
                            GameState.Get().SetBusy(true);
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("CleanMechLarge_Play_Underlay", "VO_COOP02_ABILITY_05", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_bossCard.GetActor(), 1f, true, false));
                            GameState.Get().SetBusy(false);
                            goto Label_012F;

                        case 6:
                            GameState.Get().SetBusy(true);
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("CleanMechLarge_Death_Underlay", "VO_COOP02_ABILITY_06", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_bossCard.GetActor(), 1f, true, false));
                            GameState.Get().SetBusy(false);
                            goto Label_012F;
                    }
                    break;

                default:
                    goto Label_0136;
            }
        Label_012F:
            this.$PC = -1;
        Label_0136:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator17E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal TB02_CoOp <>f__this;
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
                this.<>f__this.SetUpBossCard();
                if ((this.<>f__this.m_bossCard != null) && (this.turn == 1))
                {
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("CleanMechSmall_Trigger_Underlay", "VO_COOP02_00", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_bossCard.GetActor(), 1f, true, false));
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
}

