using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TB_ChooseYourFateRandom : MissionEntity
{
    private Notification ChooseYourFatePopup;
    private string firstFate = "TB_PICKYOURFATE_RANDOM_FIRSTFATE";
    private string firstOpponenentFate = "TB_PICKYOURFATE_BUILDAROUND_OPPONENT_FIRSTFATE";
    private string newFate = "TB_PICKYOURFATE_RANDOM_NEWFATE";
    private string opponentFate = "TB_PICKYOURFATE_RANDOM_OPPONENTFATE";
    private Vector3 popUpPos;
    private HashSet<int> seen = new HashSet<int>();
    private string textID;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator182 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("tutorial_mission_hero_coin_mouse_away");
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator182 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal TB_ChooseYourFateRandom <>f__this;
        internal int <EntityId>__0;
        internal int <friendlyPlayerID>__1;
        internal int <opposingPlayerID>__2;
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
                        goto Label_04AF;
                    }
                    if (this.<>f__this.seen.Contains(this.missionEvent))
                    {
                        goto Label_04AD;
                    }
                    this.<>f__this.seen.Add(this.missionEvent);
                    this.<>f__this.popUpPos = new Vector3(-46f, 0f, 0f);
                    this.<EntityId>__0 = 0;
                    this.<friendlyPlayerID>__1 = GameState.Get().GetFriendlySidePlayer().GetEntityId();
                    this.<opposingPlayerID>__2 = GameState.Get().GetOpposingSidePlayer().GetEntityId();
                    if (this.missionEvent > 0x3e8)
                    {
                        this.<EntityId>__0 = this.missionEvent - 0x3e8;
                        this.missionEvent -= this.<EntityId>__0;
                        if (this.<EntityId>__0 == this.<friendlyPlayerID>__1)
                        {
                            this.<>f__this.popUpPos.z = -44f;
                            this.<>f__this.textID = this.<>f__this.newFate;
                            if (UniversalInputManager.UsePhoneUI != null)
                            {
                                this.<>f__this.popUpPos.x = -51f;
                                this.<>f__this.popUpPos.z = -62f;
                            }
                        }
                        if (this.<EntityId>__0 == this.<opposingPlayerID>__2)
                        {
                            this.<>f__this.popUpPos.z = 44f;
                            this.<>f__this.textID = this.<>f__this.opponentFate;
                            if (UniversalInputManager.UsePhoneUI != null)
                            {
                                this.<>f__this.popUpPos.x = -51f;
                                this.<>f__this.popUpPos.z = 53f;
                            }
                        }
                    }
                    switch (this.missionEvent)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 0x10:
                        case 0x11:
                        case 0x12:
                        case 0x13:
                        case 20:
                            if (GameState.Get().GetFriendlySidePlayer() != GameState.Get().GetCurrentPlayer())
                            {
                                this.<>f__this.textID = this.<>f__this.firstOpponenentFate;
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
                            this.<>f__this.textID = this.<>f__this.firstFate;
                            if (UniversalInputManager.UsePhoneUI == null)
                            {
                                this.<>f__this.popUpPos.x = -50.5f;
                                this.<>f__this.popUpPos.z = 29f;
                            }
                            else
                            {
                                this.<>f__this.popUpPos.x = -77f;
                                this.<>f__this.popUpPos.z = 30.5f;
                            }
                            goto Label_0403;

                        case 0x3e8:
                            this.<>f__this.ChooseYourFatePopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 1.5f), GameStrings.Get(this.<>f__this.textID), false);
                            this.<>f__this.PlaySound("tutorial_mission_hero_coin_mouse_away", 1f, true, false);
                            NotificationManager.Get().DestroyNotification(this.<>f__this.ChooseYourFatePopup, 5f);
                            this.<>f__this.ChooseYourFatePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
                            break;
                    }
                    goto Label_04A6;

                case 2:
                    this.<>f__this.ChooseYourFatePopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 1.5f), GameStrings.Get(this.<>f__this.textID), false);
                    this.<>f__this.PlaySound("tutorial_mission_hero_coin_mouse_away", 1f, true, false);
                    NotificationManager.Get().DestroyNotification(this.<>f__this.ChooseYourFatePopup, 3f);
                    this.<>f__this.ChooseYourFatePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
                    goto Label_04A6;

                default:
                    goto Label_04AD;
            }
        Label_0403:
            this.$current = new WaitForSeconds(1f);
            this.$PC = 2;
            goto Label_04AF;
        Label_04A6:
            this.$PC = -1;
        Label_04AD:
            return false;
        Label_04AF:
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

