using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TB_ChooseYourFateBuildaround : MissionEntity
{
    private Notification ChooseYourFatePopup;
    private string friendlyFate = "TB_PICKYOURFATE_BUILDAROUND_NEWFATE";
    private string opposingFate = "TB_PICKYOURFATE_BUILDAROUND_OPPONENTFATE";
    private Vector3 popUpPos;
    private HashSet<int> seen = new HashSet<int>();
    private string textID;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator181 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("tutorial_mission_hero_coin_mouse_away");
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator181 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal TB_ChooseYourFateBuildaround <>f__this;
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
                        goto Label_0289;
                    }
                    if (this.<>f__this.seen.Contains(this.missionEvent))
                    {
                        goto Label_0287;
                    }
                    this.<>f__this.seen.Add(this.missionEvent);
                    this.<>f__this.popUpPos = new Vector3(0f, 0f, 0f);
                    switch (this.missionEvent)
                    {
                        case 1:
                        case 2:
                        case 3:
                            if (GameState.Get().GetFriendlySidePlayer() != GameState.Get().GetCurrentPlayer())
                            {
                                this.<>f__this.textID = this.<>f__this.opposingFate;
                                if (UniversalInputManager.UsePhoneUI != null)
                                {
                                    this.<>f__this.popUpPos.x = -34f;
                                    this.<>f__this.popUpPos.z = 12f;
                                }
                                else
                                {
                                    this.<>f__this.popUpPos.x = -7f;
                                    this.<>f__this.popUpPos.z = 9f;
                                }
                                break;
                            }
                            this.<>f__this.textID = this.<>f__this.friendlyFate;
                            if (UniversalInputManager.UsePhoneUI == null)
                            {
                                this.<>f__this.popUpPos.x = -50.5f;
                                this.<>f__this.popUpPos.z = 29f;
                            }
                            else
                            {
                                this.<>f__this.popUpPos.x = -75f;
                                this.<>f__this.popUpPos.z = 30.5f;
                            }
                            goto Label_01DD;
                    }
                    goto Label_0280;

                case 2:
                    this.<>f__this.ChooseYourFatePopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 1.5f), GameStrings.Get(this.<>f__this.textID), false);
                    this.<>f__this.PlaySound("tutorial_mission_hero_coin_mouse_away", 1f, true, false);
                    NotificationManager.Get().DestroyNotification(this.<>f__this.ChooseYourFatePopup, 3f);
                    this.<>f__this.ChooseYourFatePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
                    goto Label_0280;

                default:
                    goto Label_0287;
            }
        Label_01DD:
            this.$current = new WaitForSeconds(1f);
            this.$PC = 2;
            goto Label_0289;
        Label_0280:
            this.$PC = -1;
        Label_0287:
            return false;
        Label_0289:
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

