using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TutorialEntity : MissionEntity
{
    public static readonly Vector3 HELP_POPUP_SCALE = ((Vector3) (16f * Vector3.one));
    private KeywordHelpPanel historyTooltip;
    private Notification manaReminder;
    protected Notification startingPopup;
    private Notification thatsABadPlayPopup;
    public static readonly Vector3 TUTORIAL_DIALOG_SCALE_PHONE = ((Vector3) (1.5f * Vector3.one));
    private bool userHitStartButton;

    protected bool DidLoseTutorial(TutorialProgress val)
    {
        int @int = Options.Get().GetInt(Option.TUTORIAL_LOST_PROGRESS);
        bool flag = false;
        if ((@int & (((int) 1) << ((int) val))) > 0)
        {
            flag = true;
        }
        return flag;
    }

    [DebuggerHidden]
    private IEnumerator DisplayManaReminder(string reminderText)
    {
        return new <DisplayManaReminder>c__Iterator187 { reminderText = reminderText, <$>reminderText = reminderText, <>f__this = this };
    }

    protected void HandleGameStartEvent()
    {
        MulliganManager.Get().ForceMulliganActive(true);
        MulliganManager.Get().SkipCardChoosing();
        TurnStartManager.Get().BeginListeningForTurnEvents();
    }

    protected override void HandleMulliganTagChange()
    {
    }

    private void HighlightTaunters()
    {
        foreach (Card card in GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetCards())
        {
            if (card.GetEntity().HasTaunt() && !card.GetEntity().IsStealthed())
            {
                NotificationManager.Get().DestroyAllPopUps();
                Vector3 position = new Vector3(card.transform.position.x - 2f, card.transform.position.y, card.transform.position.z);
                Notification notification = NotificationManager.Get().CreateFadeArrow(position, new Vector3(0f, 270f, 0f));
                NotificationManager.Get().DestroyNotification(notification, 3f);
                break;
            }
        }
    }

    public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
    {
        if (!clickedEntity.IsControlledByLocalUser())
        {
            return true;
        }
        Network.Options.Option selectedNetworkOption = GameState.Get().GetSelectedNetworkOption();
        if ((selectedNetworkOption == null) || (selectedNetworkOption.Main == null))
        {
            return true;
        }
        Entity entity = GameState.Get().GetEntity(selectedNetworkOption.Main.ID);
        if (!wasInTargetMode || (entity == null))
        {
            return true;
        }
        if (clickedEntity == entity)
        {
            return true;
        }
        string cardId = entity.GetCardId();
        if ((!(cardId == "CS2_022") && !(cardId == "CS2_029")) && !(cardId == "CS2_034"))
        {
            return true;
        }
        this.ShowDontHurtYourselfPopup(clickedEntity.GetCard().transform.position);
        return false;
    }

    public override void NotifyOfHeroesFinishedAnimatingInMulligan()
    {
        Board.Get().FindCollider("DragPlane").GetComponent<Collider>().enabled = false;
        base.HandleMissionEvent(0x36);
    }

    public override void NotifyOfHistoryTokenMousedOut()
    {
        if (this.historyTooltip != null)
        {
            UnityEngine.Object.Destroy(this.historyTooltip.gameObject);
        }
    }

    public override void NotifyOfHistoryTokenMousedOver(GameObject mousedOverTile)
    {
        Vector3 vector;
        this.historyTooltip = KeywordHelpPanelManager.Get().CreateKeywordPanel(0);
        this.historyTooltip.Reset();
        this.historyTooltip.Initialize(GameStrings.Get("TUTORIAL_TOOLTIP_HISTORY_HEADLINE"), GameStrings.Get("TUTORIAL_TOOLTIP_HISTORY_DESC"));
        if (UniversalInputManager.Get().IsTouchMode())
        {
            vector = new Vector3(1f, 0.1916952f, 1.2f);
        }
        else
        {
            vector = new Vector3(-1.140343f, 0.1916952f, 0.4895353f);
        }
        this.historyTooltip.transform.parent = mousedOverTile.GetComponent<HistoryCard>().m_mainCardActor.transform;
        float x = 0.4792188f;
        this.historyTooltip.transform.localPosition = vector;
        this.historyTooltip.transform.localScale = new Vector3(x, x, x);
    }

    protected virtual void NotifyOfManaError()
    {
    }

    public override bool NotifyOfPlayError(PlayErrors.ErrorType error, Entity errorSource)
    {
        if (error == PlayErrors.ErrorType.REQ_ENOUGH_MANA)
        {
            Actor actor = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
            if (errorSource.GetCost() > GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.RESOURCES))
            {
                Notification notification = NotificationManager.Get().CreateSpeechBubble(GameStrings.Get("TUTORIAL02_JAINA_05"), Notification.SpeechBubbleDirection.BottomLeft, actor, true, true);
                SoundManager.Get().LoadAndPlay("VO_TUTORIAL_02_JAINA_05_20");
                NotificationManager.Get().DestroyNotification(notification, 3.5f);
                Gameplay.Get().StartCoroutine(this.DisplayManaReminder(GameStrings.Get("TUTORIAL02_HELP_01")));
            }
            else
            {
                Notification notification2 = NotificationManager.Get().CreateSpeechBubble(GameStrings.Get("TUTORIAL02_JAINA_04"), Notification.SpeechBubbleDirection.BottomLeft, actor, true, true);
                SoundManager.Get().LoadAndPlay("VO_TUTORIAL_02_JAINA_04_19");
                NotificationManager.Get().DestroyNotification(notification2, 3.5f);
                Gameplay.Get().StartCoroutine(this.DisplayManaReminder(GameStrings.Get("TUTORIAL02_HELP_03")));
            }
            return true;
        }
        if ((error == PlayErrors.ErrorType.REQ_ATTACK_GREATER_THAN_0) && (errorSource.GetCardId() == "TU4a_006"))
        {
            return true;
        }
        if (error == PlayErrors.ErrorType.REQ_TARGET_TAUNTER)
        {
            SoundManager.Get().LoadAndPlay("UI_no_can_do");
            GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_TAUNT);
            GameState.Get().ShowEnemyTauntCharacters();
            this.HighlightTaunters();
            return true;
        }
        return false;
    }

    public override bool NotifyOfTooltipDisplay(TooltipZone tooltip)
    {
        string str;
        string str2;
        ZoneDeck component = tooltip.targetObject.GetComponent<ZoneDeck>();
        if (component == null)
        {
            return false;
        }
        if (component.m_Side == Player.Side.FRIENDLY)
        {
            str = GameStrings.Get("GAMEPLAY_TOOLTIP_DECK_HEADLINE");
            str2 = GameStrings.Get("TUTORIAL_TOOLTIP_DECK_DESCRIPTION");
            if (UniversalInputManager.UsePhoneUI != null)
            {
                tooltip.ShowGameplayTooltipLarge(str, str2);
            }
            else
            {
                tooltip.ShowGameplayTooltip(str, str2);
            }
            return true;
        }
        if (component.m_Side != Player.Side.OPPOSING)
        {
            return false;
        }
        str = GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYDECK_HEADLINE");
        str2 = GameStrings.Get("TUTORIAL_TOOLTIP_ENEMYDECK_DESC");
        if (UniversalInputManager.UsePhoneUI != null)
        {
            tooltip.ShowGameplayTooltipLarge(str, str2);
        }
        else
        {
            tooltip.ShowGameplayTooltip(str, str2);
        }
        return true;
    }

    protected void ResetTutorialLostProgress()
    {
        Options.Get().SetInt(Option.TUTORIAL_LOST_PROGRESS, 0);
    }

    protected void SetTutorialLostProgress(TutorialProgress val)
    {
        int num = Options.Get().GetInt(Option.TUTORIAL_LOST_PROGRESS) | (((int) 1) << ((int) val));
        Options.Get().SetInt(Option.TUTORIAL_LOST_PROGRESS, num);
    }

    protected void SetTutorialProgress(TutorialProgress val)
    {
        if (!GameMgr.Get().IsSpectator())
        {
            if (!Network.ShouldBeConnectedToAurora())
            {
                if (GameUtils.AreAllTutorialsComplete(val))
                {
                    Options.Get().SetBool(Option.CONNECT_TO_AURORA, true);
                }
                Options.Get().SetEnum<TutorialProgress>(Option.LOCAL_TUTORIAL_PROGRESS, val);
            }
            AdTrackingManager.Get().TrackTutorialProgress(val.ToString());
            NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>().CampaignProgress = val;
            NetCache.Get().NetCacheChanged<NetCache.NetCacheProfileProgress>();
        }
    }

    public override bool ShouldDoOpeningTaunts()
    {
        return false;
    }

    public override bool ShouldHandleCoin()
    {
        return false;
    }

    private void ShowDontHurtYourselfPopup(Vector3 origin)
    {
        if (this.thatsABadPlayPopup != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.thatsABadPlayPopup);
        }
        Vector3 position = new Vector3(origin.x - 3f, origin.y, origin.z);
        this.thatsABadPlayPopup = NotificationManager.Get().CreatePopupText(position, HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_07"), true);
        NotificationManager.Get().DestroyNotification(this.thatsABadPlayPopup, 2.5f);
    }

    protected void UserPressedStartButton(UIEvent e)
    {
        if (!this.userHitStartButton)
        {
            this.userHitStartButton = true;
            if (this.startingPopup != null)
            {
                NotificationManager.Get().DestroyNotification(this.startingPopup, 0f);
            }
            base.HandleMissionEvent(0x37);
        }
    }

    [CompilerGenerated]
    private sealed class <DisplayManaReminder>c__Iterator187 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>reminderText;
        internal TutorialEntity <>f__this;
        internal Notification.PopUpArrowDirection <direction>__2;
        internal Vector3 <manaPopupPosition>__1;
        internal Vector3 <manaPosition>__0;
        internal string reminderText;

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
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.<>f__this.manaReminder != null)
                    {
                        NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.<>f__this.manaReminder);
                    }
                    this.<>f__this.NotifyOfManaError();
                    this.<manaPosition>__0 = ManaCrystalMgr.Get().GetManaCrystalSpawnPosition();
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<manaPopupPosition>__1 = new Vector3(this.<manaPosition>__0.x - 0.7f, this.<manaPosition>__0.y + 1.14f, this.<manaPosition>__0.z + 4.33f);
                        this.<direction>__2 = Notification.PopUpArrowDirection.RightDown;
                    }
                    else
                    {
                        this.<manaPopupPosition>__1 = new Vector3(this.<manaPosition>__0.x - 0.02f, this.<manaPosition>__0.y + 0.2f, this.<manaPosition>__0.z + 1.93f);
                        this.<direction>__2 = Notification.PopUpArrowDirection.Down;
                    }
                    this.<>f__this.manaReminder = NotificationManager.Get().CreatePopupText(this.<manaPopupPosition>__1, TutorialEntity.HELP_POPUP_SCALE, this.reminderText, true);
                    this.<>f__this.manaReminder.ShowPopUpArrow(this.<direction>__2);
                    NotificationManager.Get().DestroyNotification(this.<>f__this.manaReminder, 4f);
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

