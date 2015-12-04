using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TB05_GiftExchange : MissionEntity
{
    private float delayTime;
    private string FirstGiftVO = "VO_TB_1503_FATHER_WINTER_GIFT1";
    private string FirstStolenVO = "VO_TB_1503_FATHER_WINTER_START";
    private Notification GameStartPopup;
    private Notification GiftSpawnedPopup;
    private Notification GiftStolenPopup;
    private string[] GiftVOList = new string[] { "VO_TB_1503_FATHER_WINTER_GIFT1", "VO_TB_1503_FATHER_WINTER_GIFT2", "VO_TB_1503_FATHER_WINTER_GIFT3", "VO_TB_1503_FATHER_WINTER_GIFT4" };
    private string NextStolenVO = "VO_TB_1503_FATHER_WINTER_LONG1";
    private string[] PissedVOList = new string[] { "VO_TB_1503_FATHER_WINTER_LONG2", "VO_TB_1503_FATHER_WINTER_LONG3", "VO_TB_1503_FATHER_WINTER_LONG4", "VO_TB_1503_FATHER_WINTER_LONG5", "VO_TB_1503_FATHER_WINTER_LONG6" };
    private Vector3 popUpPos;
    private string StartVO = "VO_TB_1503_FATHER_WINTER_LONG6";
    private string textID;
    private string VOChoice;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator184 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_GIFT1");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_GIFT2");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_GIFT3");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_GIFT4");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_LONG1");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_LONG2");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_LONG3");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_LONG4");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_LONG5");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_LONG6");
        base.PreloadSound("VO_TB_1503_FATHER_WINTER_START");
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator184 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal TB05_GiftExchange <>f__this;
        internal float <popupScale>__0;
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
                    this.<>f__this.VOChoice = string.Empty;
                    this.<>f__this.delayTime = 0f;
                    break;

                case 1:
                    break;

                case 2:
                    GameState.Get().SetBusy(false);
                    this.<>f__this.textID = "TB_GIFTEXCHANGE_GIFTSPAWNED";
                    this.<>f__this.popUpPos = new Vector3(1.27f, 0f, -9.32f);
                    if (GameState.Get().GetFriendlySidePlayer() == GameState.Get().GetCurrentPlayer())
                    {
                        this.<>f__this.popUpPos.z = 19f;
                    }
                    this.<popupScale>__0 = 1.25f;
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<popupScale>__0 = 1.75f;
                    }
                    this.<>f__this.GiftSpawnedPopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * this.<popupScale>__0), GameStrings.Get(this.<>f__this.textID), false);
                    NotificationManager.Get().DestroyNotification(this.<>f__this.GiftSpawnedPopup, 4f);
                    goto Label_04D9;

                case 3:
                    this.<>f__this.delayTime = 4f;
                    this.<>f__this.textID = "TB_GIFTEXCHANGE_GIFTSTOLEN";
                    this.<>f__this.popUpPos = new Vector3(22.2f, 0f, -44.6f);
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<>f__this.popUpPos.x = 61f;
                        this.<>f__this.popUpPos.z = -29f;
                    }
                    this.<>f__this.GiftStolenPopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 1.25f), GameStrings.Get(this.<>f__this.textID), false);
                    NotificationManager.Get().DestroyNotification(this.<>f__this.GiftStolenPopup, 4f);
                    goto Label_04D9;

                case 4:
                    GameState.Get().SetBusy(false);
                    this.$PC = -1;
                    goto Label_0535;

                default:
                    goto Label_0535;
            }
            if (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0537;
            }
            switch (this.missionEvent)
            {
                case 1:
                    if (this.<>f__this.FirstGiftVO.Length <= 0)
                    {
                        this.<>f__this.VOChoice = this.<>f__this.GiftVOList[UnityEngine.Random.Range(1, this.<>f__this.GiftVOList.Length)];
                        this.<>f__this.delayTime = 3f;
                        break;
                    }
                    this.<>f__this.VOChoice = this.<>f__this.FirstGiftVO;
                    this.<>f__this.FirstGiftVO = string.Empty;
                    this.<>f__this.delayTime = 4f;
                    GameState.Get().SetBusy(true);
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 2;
                    goto Label_0537;

                case 2:
                    this.<>f__this.VOChoice = this.<>f__this.PissedVOList[UnityEngine.Random.Range(1, this.<>f__this.PissedVOList.Length)];
                    this.<>f__this.delayTime = 2f;
                    break;

                case 10:
                    this.<>f__this.VOChoice = this.<>f__this.StartVO;
                    this.<>f__this.delayTime = 5f;
                    this.<>f__this.textID = "TB_GIFTEXCHANGE_START";
                    this.<>f__this.popUpPos = new Vector3(22.2f, 0f, -44.6f);
                    this.<>f__this.popUpPos = new Vector3(0f, 0f, 0f);
                    this.<>f__this.GameStartPopup = NotificationManager.Get().CreatePopupText(this.<>f__this.popUpPos, (Vector3) (TutorialEntity.HELP_POPUP_SCALE * 1.75f), GameStrings.Get(this.<>f__this.textID), false);
                    NotificationManager.Get().DestroyNotification(this.<>f__this.GameStartPopup, 3f);
                    break;

                case 11:
                    if (GameState.Get().GetFriendlySidePlayer() != GameState.Get().GetCurrentPlayer())
                    {
                        if (this.<>f__this.NextStolenVO.Length > 0)
                        {
                            this.<>f__this.VOChoice = this.<>f__this.NextStolenVO;
                            this.<>f__this.NextStolenVO = string.Empty;
                        }
                        break;
                    }
                    if (this.<>f__this.FirstStolenVO.Length <= 0)
                    {
                        break;
                    }
                    this.<>f__this.VOChoice = this.<>f__this.FirstStolenVO;
                    this.<>f__this.FirstStolenVO = string.Empty;
                    this.$current = new WaitForSeconds(1.5f);
                    this.$PC = 3;
                    goto Label_0537;
            }
        Label_04D9:
            this.<>f__this.PlaySound(this.<>f__this.VOChoice, 1f, true, false);
            GameState.Get().SetBusy(true);
            this.$current = new WaitForSeconds(this.<>f__this.delayTime);
            this.$PC = 4;
            goto Label_0537;
        Label_0535:
            return false;
        Label_0537:
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

