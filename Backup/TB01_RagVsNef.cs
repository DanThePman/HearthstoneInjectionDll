using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TB01_RagVsNef : MissionEntity
{
    private Card m_ragnarosCard;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator17D { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator17D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal Map<int, Player>.ValueCollection.Enumerator <$s_709>__1;
        internal TB01_RagVsNef <>f__this;
        internal float <clipLength>__6;
        internal Card <heroCard>__4;
        internal Entity <heroEntity>__3;
        internal Player <player>__2;
        internal Map<int, Player> <playerMap>__0;
        internal CardSoundSpell <spell>__5;
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
                    if (this.missionEvent == 1)
                    {
                        this.<playerMap>__0 = GameState.Get().GetPlayerMap();
                        this.<$s_709>__1 = this.<playerMap>__0.Values.GetEnumerator();
                        try
                        {
                            while (this.<$s_709>__1.MoveNext())
                            {
                                this.<player>__2 = this.<$s_709>__1.Current;
                                this.<heroEntity>__3 = this.<player>__2.GetHero();
                                this.<heroCard>__4 = this.<heroEntity>__3.GetCard();
                                if (this.<heroEntity>__3.GetCardId() == "TBA01_1")
                                {
                                    this.<>f__this.m_ragnarosCard = this.<heroCard>__4;
                                }
                            }
                        }
                        finally
                        {
                            this.<$s_709>__1.Dispose();
                        }
                        GameState.Get().SetBusy(true);
                        this.<spell>__5 = this.<>f__this.m_ragnarosCard.PlayEmote(EmoteType.THREATEN);
                        this.<clipLength>__6 = this.<spell>__5.m_CardSoundData.m_AudioSource.clip.length;
                        this.$current = new WaitForSeconds((float) (this.<clipLength>__6 * 0.8));
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    GameState.Get().SetBusy(false);
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
}

