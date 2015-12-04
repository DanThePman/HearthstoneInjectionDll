using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TB06_SinglePlayerTrial : MissionEntity
{
    private Notification GameOverPopup;
    private Vector3 popUpPos;
    private string textID;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator185 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_GIFT1");
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator185 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal TB06_SinglePlayerTrial <>f__this;
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
                    goto Label_01BE;

                case 3:
                    GameState.Get().SetBusy(false);
                    goto Label_01B5;

                default:
                    goto Label_01BC;
            }
            if (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_01BE;
            }
            switch (this.missionEvent)
            {
                case 1:
                    this.<>f__this.textID = "TB_SINGLEPLAYERTRIAL_PLAYERDIED";
                    break;

                case 2:
                    this.<>f__this.textID = "TB_SINGLEPLAYERTRIAL_COMPUTERDIED";
                    break;

                case 10:
                    this.<>f__this.textID = "TB_SINGLEPLAYERTRIAL_START";
                    break;
            }
            if (this.<>f__this.textID.Length > 0)
            {
                GameState.Get().SetBusy(true);
                this.$current = new WaitForSeconds(0.5f);
                this.$PC = 2;
                goto Label_01BE;
            }
        Label_01B5:
            this.$PC = -1;
        Label_01BC:
            return false;
        Label_01BE:
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

