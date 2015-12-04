using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class TB03_InspireVSJoust : MissionEntity
{
    [DebuggerHidden]
    public override IEnumerator PlayMissionIntroLineAndWait()
    {
        return new <PlayMissionIntroLineAndWait>c__Iterator180 { <>f__this = this };
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_TIRION_INTRO_03");
    }

    [CompilerGenerated]
    private sealed class <PlayMissionIntroLineAndWait>c__Iterator180 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TB03_InspireVSJoust <>f__this;

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
                    if (!NotificationManager.Get().HasSoundPlayedThisSession("VO_TIRION_INTRO_03"))
                    {
                        NotificationManager.Get().ForceAddSoundToPlayedList("VO_TIRION_INTRO_03");
                        NotificationManager.Get().CreateCharacterQuote("Tirion_Quote", NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get("VO_TIRION_INTRO_03"), string.Empty, false, 30f, null, CanvasAnchor.BOTTOM_LEFT);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TIRION_INTRO_03", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    NotificationManager.Get().DestroyActiveQuote(0f);
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

