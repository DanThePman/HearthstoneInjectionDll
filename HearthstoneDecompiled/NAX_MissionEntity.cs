using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class NAX_MissionEntity : MissionEntity
{
    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator154 { missionEvent = missionEvent, <$>missionEvent = missionEvent };
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator154 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal int missionEvent;

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
                switch (this.missionEvent)
                {
                    case 0x3e9:
                        NotificationManager.Get().CreateKTQuote("VO_KT_ANTI_CHEESE1_65", "VO_KT_ANTI_CHEESE1_65", true);
                        break;

                    case 0x3ea:
                        NotificationManager.Get().CreateKTQuote("VO_KT_ANTI_CHEESE2_66", "VO_KT_ANTI_CHEESE2_66", true);
                        break;

                    case 0x3eb:
                        NotificationManager.Get().CreateKTQuote("VO_KT_ANTI_CHEESE3_67", "VO_KT_ANTI_CHEESE3_67", true);
                        break;
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

