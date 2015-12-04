using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TB07_FactionWars : MissionEntity
{
    private Notification GameOverPopup;
    private Vector3 popUpPos;
    private string textID;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator186 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    public override void PreloadAssets()
    {
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator186 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal TB07_FactionWars <>f__this;
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
                    this.<>f__this.textID = string.Empty;
                    break;

                case 1:
                    break;

                case 2:
                    GameState.Get().SetBusy(false);
                    this.<>f__this.popUpPos = new Vector3(1.27f, 0f, 19f);
                    this.<>f__this.GameOverPopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 1.25f), GameStrings.Get(this.<>f__this.textID), false);
                    NotificationManager.Get().DestroyNotification(this.<>f__this.GameOverPopup, 4f);
                    GameState.Get().SetBusy(true);
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 3;
                    goto Label_0185;

                case 3:
                    GameState.Get().SetBusy(false);
                    goto Label_017C;

                default:
                    goto Label_0183;
            }
            if (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0185;
            }
            if (this.missionEvent == 1)
            {
                this.<>f__this.textID = "TB_SINGLEPLAYERTRIAL_PLAYERDIED";
            }
            if (this.<>f__this.textID.Length > 0)
            {
                GameState.Get().SetBusy(true);
                this.$current = new WaitForSeconds(0.5f);
                this.$PC = 2;
                goto Label_0185;
            }
        Label_017C:
            this.$PC = -1;
        Label_0183:
            return false;
        Label_0185:
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

