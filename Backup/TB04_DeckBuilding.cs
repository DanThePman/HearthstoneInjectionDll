using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TB04_DeckBuilding : MissionEntity
{
    private Notification CardPlayedPopup;
    private Notification EndOfTurnPopup;
    private Notification PickThreePopup;
    private Vector3 popUpPos;
    private Notification StartOfTurnPopup;
    private string textID;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator183 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("tutorial_mission_hero_coin_mouse_away");
    }

    public override bool ShouldDoAlternateMulliganIntro()
    {
        return true;
    }

    public override bool ShouldHandleCoin()
    {
        return false;
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator183 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal TB04_DeckBuilding <>f__this;
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
                        goto Label_07EE;
                    }
                    this.<>f__this.popUpPos = new Vector3(0f, 0f, 0f);
                    switch (this.missionEvent)
                    {
                        case 1:
                            if (GameState.Get().GetFriendlySidePlayer() == GameState.Get().GetCurrentPlayer())
                            {
                                GameState.Get().SetBusy(true);
                                this.$current = new WaitForSeconds(2f);
                                this.$PC = 2;
                                goto Label_07EE;
                            }
                            goto Label_07EC;

                        case 2:
                            if (this.<>f__this.PickThreePopup != null)
                            {
                                NotificationManager.Get().DestroyNotification(this.<>f__this.PickThreePopup, 0.25f);
                            }
                            break;

                        case 3:
                            if (GameState.Get().GetFriendlySidePlayer() == GameState.Get().GetCurrentPlayer())
                            {
                                this.<>f__this.textID = "TB_DECKBUILDING_FIRSTENDTURN";
                                if (UniversalInputManager.UsePhoneUI != null)
                                {
                                    this.<>f__this.popUpPos.x = 82f;
                                    this.<>f__this.popUpPos.z = -28f;
                                }
                                else
                                {
                                    this.<>f__this.popUpPos.z = -36f;
                                }
                                GameState.Get().SetBusy(true);
                                this.<>f__this.EndOfTurnPopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 1.5f), GameStrings.Get(this.<>f__this.textID), false);
                                this.<>f__this.PlaySound("tutorial_mission_hero_coin_mouse_away", 1f, true, false);
                                NotificationManager.Get().DestroyNotification(this.<>f__this.EndOfTurnPopup, 5f);
                                this.<>f__this.EndOfTurnPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Down);
                                NotificationManager.Get().DestroyNotification(this.<>f__this.CardPlayedPopup, 0f);
                                this.$current = new WaitForSeconds(3.5f);
                                this.$PC = 4;
                                goto Label_07EE;
                            }
                            goto Label_07EC;

                        case 4:
                            if (GameState.Get().GetFriendlySidePlayer() == GameState.Get().GetCurrentPlayer())
                            {
                                this.<>f__this.textID = "TB_DECKBUILDING_FIRSTCARDPLAYED";
                                if (UniversalInputManager.UsePhoneUI != null)
                                {
                                    this.<>f__this.popUpPos.x = 82f;
                                    this.<>f__this.popUpPos.y = 0f;
                                    this.<>f__this.popUpPos.z = -28f;
                                }
                                else
                                {
                                    this.<>f__this.popUpPos.x = 51f;
                                    this.<>f__this.popUpPos.y = 0f;
                                    this.<>f__this.popUpPos.z = -15.5f;
                                }
                                GameState.Get().SetBusy(true);
                                this.<>f__this.CardPlayedPopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 1.5f), GameStrings.Get(this.<>f__this.textID), false);
                                this.<>f__this.CardPlayedPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
                                this.<>f__this.PlaySound("tutorial_mission_hero_coin_mouse_away", 1f, true, false);
                                NotificationManager.Get().DestroyNotification(this.<>f__this.CardPlayedPopup, 10f);
                                this.$current = new WaitForSeconds(3f);
                                this.$PC = 5;
                                goto Label_07EE;
                            }
                            goto Label_07EC;

                        case 10:
                            if (GameState.Get().IsFriendlySidePlayerTurn())
                            {
                                TurnStartManager.Get().BeginListeningForTurnEvents();
                            }
                            GameState.Get().SetBusy(true);
                            this.$current = new WaitForSeconds(2f);
                            this.$PC = 6;
                            goto Label_07EE;

                        case 11:
                            NotificationManager.Get().DestroyNotification(this.<>f__this.StartOfTurnPopup, 0f);
                            NotificationManager.Get().DestroyNotification(this.<>f__this.EndOfTurnPopup, 0f);
                            NotificationManager.Get().DestroyNotification(this.<>f__this.CardPlayedPopup, 0f);
                            NotificationManager.Get().DestroyNotification(this.<>f__this.PickThreePopup, 0f);
                            break;
                    }
                    goto Label_07E5;

                case 2:
                    GameState.Get().SetBusy(false);
                    this.<>f__this.textID = "TB_DECKBUILDING_FIRSTPICKTHREE";
                    if (UniversalInputManager.UsePhoneUI == null)
                    {
                        this.<>f__this.popUpPos.z = -44f;
                        break;
                    }
                    this.<>f__this.popUpPos.z = -66f;
                    break;

                case 3:
                    GameState.Get().SetBusy(false);
                    goto Label_07E5;

                case 4:
                    GameState.Get().SetBusy(false);
                    goto Label_07E5;

                case 5:
                    GameState.Get().SetBusy(false);
                    if (this.<>f__this.CardPlayedPopup != null)
                    {
                        object[] args = new object[] { "amount", new Vector3(2f, 2f, 2f), "time", 1f };
                        iTween.PunchScale(this.<>f__this.CardPlayedPopup.gameObject, iTween.Hash(args));
                    }
                    goto Label_07E5;

                case 6:
                    GameState.Get().SetBusy(false);
                    GameState.Get().SetBusy(true);
                    this.$current = new WaitForSeconds(2f);
                    this.$PC = 7;
                    goto Label_07EE;

                case 7:
                    GameState.Get().SetBusy(false);
                    this.<>f__this.textID = "TB_DECKBUILDING_STARTOFGAME";
                    if (UniversalInputManager.UsePhoneUI == null)
                    {
                        this.<>f__this.popUpPos.x = 0f;
                        this.<>f__this.popUpPos.y = 0f;
                        this.<>f__this.popUpPos.z = 0f;
                    }
                    else
                    {
                        this.<>f__this.popUpPos.x = 0f;
                        this.<>f__this.popUpPos.y = 0f;
                        this.<>f__this.popUpPos.z = 0f;
                    }
                    GameState.Get().SetBusy(true);
                    this.<>f__this.StartOfTurnPopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 2f), GameStrings.Get(this.<>f__this.textID), false);
                    this.<>f__this.PlaySound("tutorial_mission_hero_coin_mouse_away", 1f, true, false);
                    NotificationManager.Get().DestroyNotification(this.<>f__this.StartOfTurnPopup, 3f);
                    this.$current = new WaitForSeconds(3f);
                    this.$PC = 8;
                    goto Label_07EE;

                case 8:
                    GameState.Get().SetBusy(false);
                    GameState.Get().SetBusy(true);
                    this.$current = new WaitForSeconds(3f);
                    this.$PC = 9;
                    goto Label_07EE;

                case 9:
                    GameState.Get().SetBusy(false);
                    goto Label_07E5;

                default:
                    goto Label_07EC;
            }
            this.<>f__this.PickThreePopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 1.25f), GameStrings.Get(this.<>f__this.textID), false);
            this.<>f__this.PlaySound("tutorial_mission_hero_coin_mouse_away", 1f, true, false);
            NotificationManager.Get().DestroyNotification(this.<>f__this.PickThreePopup, 12f);
            GameState.Get().SetBusy(true);
            this.$current = new WaitForSeconds(1f);
            this.$PC = 3;
            goto Label_07EE;
        Label_07E5:
            this.$PC = -1;
        Label_07EC:
            return false;
        Label_07EE:
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

