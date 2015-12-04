using PegasusUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Hub : Scene
{
    [CompilerGenerated]
    private static System.Action <>f__am$cache2;
    private Notification m_PracticeNotification;
    public static bool s_hasAlreadyShownTBAnimation;

    [DebuggerHidden]
    private IEnumerator DoFirstTimeHubWelcome()
    {
        return new <DoFirstTimeHubWelcome>c__IteratorD1 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator DoTavernBrawlAnims()
    {
        return new <DoTavernBrawlAnims>c__IteratorD0();
    }

    private void DoTavernBrawlAnimsCB()
    {
        base.StartCoroutine(this.DoTavernBrawlAnims());
    }

    private static void DoTavernBrawlIntroVO()
    {
        if (!NotificationManager.Get().HasSoundPlayedThisSession("VO_INNKEEPER_TAVERNBRAWL_PUSH_32"))
        {
            if (<>f__am$cache2 == null)
            {
                <>f__am$cache2 = () => NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_TAVERNBRAWL_DESC1_29"), "VO_INNKEEPER_TAVERNBRAWL_DESC1_29", 0f, null);
            }
            System.Action finishCallback = <>f__am$cache2;
            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_TAVERNBRAWL_PUSH_32"), "VO_INNKEEPER_TAVERNBRAWL_PUSH_32", finishCallback);
            NotificationManager.Get().ForceAddSoundToPlayedList("VO_INNKEEPER_TAVERNBRAWL_PUSH_32");
        }
    }

    private void HideTooltipNotification(UIEvent e)
    {
        if (this.m_PracticeNotification != null)
        {
            NotificationManager.Get().DestroyNotification(this.m_PracticeNotification, 0f);
        }
    }

    private void OnAdventureBundlePurchase(Network.Bundle bundle, PaymentMethod purchaseMethod, object userData)
    {
        if ((bundle != null) && (bundle.Items != null))
        {
            foreach (Network.BundleItem item in bundle.Items)
            {
                if (item.Product == ProductType.PRODUCT_TYPE_NAXX)
                {
                    Options.Get().SetBool(Option.BUNDLE_JUST_PURCHASE_IN_HUB, true);
                    AdventureConfig.Get().SetSelectedAdventureMode(AdventureDbId.NAXXRAMAS, AdventureModeDbId.NORMAL);
                    break;
                }
            }
        }
    }

    private void OnBoxButtonPressed(Box.ButtonType buttonType, object userData)
    {
        if (buttonType == Box.ButtonType.TOURNAMENT)
        {
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.TOURNAMENT);
            Tournament.Get().NotifyOfBoxTransitionStart();
        }
        else if (buttonType == Box.ButtonType.FORGE)
        {
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.DRAFT);
        }
        else if (buttonType == Box.ButtonType.ADVENTURE)
        {
            AdventureConfig.Get().ResetSubScene();
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.ADVENTURE);
        }
        else if (buttonType == Box.ButtonType.COLLECTION)
        {
            CollectionManager.Get().NotifyOfBoxTransitionStart();
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.COLLECTIONMANAGER);
        }
        else if (buttonType == Box.ButtonType.OPEN_PACKS)
        {
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.PACKOPENING);
        }
        else if (buttonType == Box.ButtonType.TAVERN_BRAWL)
        {
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.TAVERN_BRAWL);
        }
    }

    private void OnDestroy()
    {
        TavernBrawlManager.Get().OnTavernBrawlUpdated -= new System.Action(this.DoTavernBrawlAnimsCB);
        NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheFeatures), new System.Action(this.DoTavernBrawlAnimsCB));
        NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheHeroLevels), new System.Action(this.DoTavernBrawlAnimsCB));
    }

    private void OnSceneUnloaded(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        SpecialEventType activeEventType = SpecialEventVisualMgr.GetActiveEventType();
        if (activeEventType != SpecialEventType.IGNORE)
        {
            SpecialEventVisualMgr.Get().UnloadEvent(activeEventType);
        }
        SceneMgr.Get().UnregisterSceneUnloadedEvent(new SceneMgr.SceneUnloadedCallback(this.OnSceneUnloaded));
    }

    private void Start()
    {
        if (CollectionManager.Get().GetPreconDeck(TAG_CLASS.MAGE) == null)
        {
            Error.AddFatalLoc("GLOBAL_ERROR_NO_MAGE_PRECON", new object[0]);
        }
        else
        {
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_BOX_SCREEN_VISIT);
            Enum[] args = new Enum[] { PresenceStatus.HUB };
            PresenceMgr.Get().SetStatus(args);
            Box box = Box.Get();
            box.AddButtonPressListener(new Box.ButtonPressCallback(this.OnBoxButtonPressed));
            box.m_QuestLogButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.HideTooltipNotification));
            box.m_StoreButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.HideTooltipNotification));
            if (UniversalInputManager.UsePhoneUI != null)
            {
                box.m_ribbonButtons.m_questLogRibbon.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.HideTooltipNotification));
                box.m_ribbonButtons.m_storeRibbon.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.HideTooltipNotification));
            }
            SceneMgr.Get().NotifySceneLoaded();
            if (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LOGIN)
            {
                MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MainTitle);
            }
            if (!Options.Get().GetBool(Option.HAS_SEEN_HUB, false))
            {
                base.StartCoroutine(this.DoFirstTimeHubWelcome());
            }
            else if (!Options.Get().GetBool(Option.HAS_SEEN_100g_REMINDER, false))
            {
                if (NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>().GetTotal() >= 100L)
                {
                    NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_FIRST_100_GOLD"), "VO_INNKEEPER_FIRST_100_GOLD", 0f, null);
                    Options.Get().SetBool(Option.HAS_SEEN_100g_REMINDER, true);
                }
            }
            else if (TavernBrawlManager.Get().IsFirstTimeSeeingThisFeature)
            {
                DoTavernBrawlIntroVO();
            }
            StoreManager.Get().RegisterSuccessfulPurchaseListener(new StoreManager.SuccessfulPurchaseCallback(this.OnAdventureBundlePurchase));
            SpecialEventType activeEventType = SpecialEventVisualMgr.GetActiveEventType();
            if ((activeEventType != SpecialEventType.IGNORE) && AchieveManager.Get().HasUnlockedFeature(Achievement.UnlockableFeature.FORGE))
            {
                SpecialEventVisualMgr.Get().LoadEvent(activeEventType);
                SceneMgr.Get().RegisterSceneUnloadedEvent(new SceneMgr.SceneUnloadedCallback(this.OnSceneUnloaded));
            }
            if (TavernBrawlManager.Get().IsFirstTimeSeeingCurrentSeason && !s_hasAlreadyShownTBAnimation)
            {
                s_hasAlreadyShownTBAnimation = true;
                base.StartCoroutine(this.DoTavernBrawlAnims());
            }
            TavernBrawlManager.Get().OnTavernBrawlUpdated += new System.Action(this.DoTavernBrawlAnimsCB);
            NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheFeatures), new System.Action(this.DoTavernBrawlAnimsCB));
            NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheHeroLevels), new System.Action(this.DoTavernBrawlAnimsCB));
        }
    }

    public override void Unload()
    {
        StoreManager.Get().RemoveSuccessfulPurchaseListener(new StoreManager.SuccessfulPurchaseCallback(this.OnAdventureBundlePurchase));
        this.HideTooltipNotification(null);
        Box box = Box.Get();
        box.m_QuestLogButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.HideTooltipNotification));
        box.m_StoreButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.HideTooltipNotification));
        box.RemoveButtonPressListener(new Box.ButtonPressCallback(this.OnBoxButtonPressed));
    }

    private void Update()
    {
        Network.Get().ProcessNetwork();
    }

    [CompilerGenerated]
    private sealed class <DoFirstTimeHubWelcome>c__IteratorD1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Hub <>f__this;

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
                    goto Label_01B8;

                case 1:
                case 2:
                    if (((StoreManager.Get() != null) && StoreManager.Get().IsShown()) || ((QuestLog.Get() != null) && QuestLog.Get().IsShown()))
                    {
                        this.$current = null;
                        this.$PC = 2;
                        goto Label_01B8;
                    }
                    break;

                case 3:
                    break;

                default:
                    goto Label_01B6;
            }
            while (AchieveManager.Get().HasActiveQuests(true) || (WelcomeQuests.Get() != null))
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_01B8;
            }
            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_1ST_HUB_06"), "VO_INNKEEPER_1ST_HUB_06", 3f, null);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.<>f__this.m_PracticeNotification = NotificationManager.Get().CreatePopupText(new Vector3(-30.46f, 33.5f, 3f), (Vector3) (25f * Vector3.one), GameStrings.Get("GLUE_PRACTICE_HINT"), true);
            }
            else
            {
                this.<>f__this.m_PracticeNotification = NotificationManager.Get().CreatePopupText(new Vector3(-33.62785f, 33.52365f, 3f), (Vector3) (15f * Vector3.one), GameStrings.Get("GLUE_PRACTICE_HINT"), true);
            }
            this.<>f__this.m_PracticeNotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
            Options.Get().SetBool(Option.HAS_SEEN_HUB, true);
            AdTrackingManager.Get().TrackFirstLogin();
            this.$PC = -1;
        Label_01B6:
            return false;
        Label_01B8:
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

    [CompilerGenerated]
    private sealed class <DoTavernBrawlAnims>c__IteratorD0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <isFirstTimeSeason>__2;
        internal bool <tavernBrawlEnabled>__1;
        internal Box <theBox>__0;

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
                    this.<theBox>__0 = Box.Get();
                    this.<tavernBrawlEnabled>__1 = this.<theBox>__0.UpdateTavernBrawlButtonState(true);
                    if (this.<tavernBrawlEnabled>__1)
                    {
                        if (!TavernBrawlManager.Get().IsTavernBrawlActive)
                        {
                            if (!this.<theBox>__0.IsTavernBrawlButtonDeactivated)
                            {
                                if (this.<theBox>__0.m_tavernBrawlDeactivateSound != string.Empty)
                                {
                                    SoundManager.Get().LoadAndPlay(Box.Get().m_tavernBrawlDeactivateSound);
                                }
                                this.<theBox>__0.PlayTavernBrawlButtonActivation(false, false);
                            }
                            goto Label_01AC;
                        }
                        this.<isFirstTimeSeason>__2 = TavernBrawlManager.Get().IsFirstTimeSeeingCurrentSeason;
                        if (!this.<isFirstTimeSeason>__2 && !this.<theBox>__0.IsTavernBrawlButtonDeactivated)
                        {
                            goto Label_01AC;
                        }
                        this.<theBox>__0.UpdateTavernBrawlButtonState(false);
                        if (this.<isFirstTimeSeason>__2)
                        {
                            this.$current = new WaitForSeconds(1.5f);
                            this.$PC = 1;
                            goto Label_01B5;
                        }
                        break;
                    }
                    goto Label_01B3;

                case 1:
                    break;

                case 2:
                    CameraShakeMgr.Shake(Camera.main, new Vector3(0.5f, 0.5f, 0.5f), 0.3f);
                    this.<theBox>__0.UpdateTavernBrawlButtonState(true);
                    goto Label_01AC;

                default:
                    goto Label_01B3;
            }
            if (TavernBrawlManager.Get().IsFirstTimeSeeingThisFeature)
            {
                Hub.DoTavernBrawlIntroVO();
            }
            if (this.<theBox>__0.m_tavernBrawlActivateSound != string.Empty)
            {
                SoundManager.Get().LoadAndPlay(this.<theBox>__0.m_tavernBrawlActivateSound);
            }
            this.<theBox>__0.PlayTavernBrawlButtonActivation(true, false);
            this.$current = new WaitForSeconds(0.65f);
            this.$PC = 2;
            goto Label_01B5;
        Label_01AC:
            this.$PC = -1;
        Label_01B3:
            return false;
        Label_01B5:
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

