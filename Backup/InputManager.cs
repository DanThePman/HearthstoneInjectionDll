using PegasusGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private const float DOUBLE_CLICK_TIME = 0.175f;
    private Card m_battlecrySourceCard;
    private CardBackManager.LoadCardBackData m_bigViewCardBackFriendly;
    private CardBackManager.LoadCardBackData m_bigViewCardBackOpposing;
    private List<Card> m_cancelingBattlecryCards;
    private bool m_cardWasInsideHandLastFrame;
    private bool m_checkForInput;
    private object m_clickObject;
    private float m_clickStartTime;
    private bool m_doubleClickActive;
    private ClickHandler m_doubleClickHandler;
    private bool m_dragging;
    public DragRotatorInfo m_DragRotatorInfo;
    private Card m_heldCard;
    private bool m_hideHandAfterPlayingCard;
    private bool m_ignoreMouseUp;
    private bool m_isInBattleCryEffect;
    private Vector3 m_lastMouseDownPosition;
    private GameObject m_lastObjectMousedDown;
    private GameObject m_lastObjectRightMousedDown;
    private Card m_lastPreviewedCard;
    private ZoneChangeList m_lastZoneChangeList;
    private bool m_leftMouseButtonIsDown;
    private List<Actor> m_mobileTargettingEffectActors;
    private Card m_mousedOverCard;
    private HistoryCard m_mousedOverHistoryCard;
    private GameObject m_mousedOverObject;
    private float m_mousedOverTimer;
    public float m_MouseOverDelay = 0.4f;
    private ZoneHand m_myHandZone;
    private ZonePlay m_myPlayZone;
    private ZoneWeapon m_myWeaponZone;
    private List<PhoneHandHiddenListener> m_phoneHandHiddenListener;
    private List<PhoneHandShownListener> m_phoneHandShownListener;
    private ClickHandler m_singleClickHandler;
    private uint m_spectatorNotifyCurrentToken;
    private bool m_targettingHeroPower;
    private bool m_touchDraggingCard;
    private bool m_touchedDownOnSmallHand;
    private bool m_useHandEnlarge;
    private readonly PlatformDependentValue<float> MIN_GRAB_Y;
    private const float MOBILE_TARGETTING_XY_SCALE = 1.08f;
    private const float MOBILE_TARGETTING_Y_OFFSET = 0.8f;
    private static InputManager s_instance;

    public InputManager()
    {
        DragRotatorInfo info = new DragRotatorInfo();
        DragRotatorAxisInfo info2 = new DragRotatorAxisInfo {
            m_ForceMultiplier = 25f,
            m_MinDegrees = -40f,
            m_MaxDegrees = 40f,
            m_RestSeconds = 2f
        };
        info.m_PitchInfo = info2;
        info2 = new DragRotatorAxisInfo {
            m_ForceMultiplier = 25f,
            m_MinDegrees = -45f,
            m_MaxDegrees = 45f,
            m_RestSeconds = 2f
        };
        info.m_RollInfo = info2;
        this.m_DragRotatorInfo = info;
        PlatformDependentValue<float> value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            Tablet = 80f,
            Phone = 80f
        };
        this.MIN_GRAB_Y = value2;
        this.m_cancelingBattlecryCards = new List<Card>();
        this.m_mobileTargettingEffectActors = new List<Actor>();
        this.m_phoneHandShownListener = new List<PhoneHandShownListener>();
        this.m_phoneHandHiddenListener = new List<PhoneHandHiddenListener>();
    }

    private bool ActivatePlaySpell(Card card)
    {
        if (card == null)
        {
            return false;
        }
        Spell playSpell = card.GetPlaySpell(true);
        if (playSpell == null)
        {
            return false;
        }
        playSpell.ActivateState(SpellStateType.BIRTH);
        return true;
    }

    private bool ActivatePowerUpSpell(Card card)
    {
        if (card == null)
        {
            return false;
        }
        Spell actorSpell = card.GetActorSpell(SpellType.POWER_UP);
        if (actorSpell == null)
        {
            return false;
        }
        actorSpell.ActivateState(SpellStateType.BIRTH);
        return true;
    }

    private void ApplyMobileTargettingEffectToActor(Actor actor)
    {
        if ((actor != null) && (actor.gameObject != null))
        {
            SceneUtils.SetLayer(actor.gameObject, GameLayer.IgnoreFullScreenEffects);
            object[] args = new object[] { "y", 0.8f, "time", 0.4f, "easeType", iTween.EaseType.easeOutQuad, "name", "position", "isLocal", true };
            Hashtable hashtable = iTween.Hash(args);
            object[] objArray2 = new object[] { "x", 1.08f, "z", 1.08f, "time", 0.4f, "easeType", iTween.EaseType.easeOutQuad, "name", "scale" };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.StopByName(actor.gameObject, "position");
            iTween.StopByName(actor.gameObject, "scale");
            iTween.MoveTo(actor.gameObject, hashtable);
            iTween.ScaleTo(actor.gameObject, hashtable2);
        }
    }

    private void Awake()
    {
        s_instance = this;
        this.m_useHandEnlarge = (bool) UniversalInputManager.UsePhoneUI;
    }

    private bool CancelOption()
    {
        bool flag = false;
        GameState state = GameState.Get();
        if (state.IsInMainOptionMode())
        {
            state.CancelCurrentOptionMode();
        }
        if (this.CancelTargetMode())
        {
            flag = true;
        }
        if (this.CancelSubOptionMode())
        {
            flag = true;
        }
        if (this.DropHeldCard(true))
        {
            flag = true;
        }
        if (this.m_mousedOverCard != null)
        {
            this.m_mousedOverCard.UpdateProposedManaUsage();
        }
        return flag;
    }

    private bool CancelSubOptionMode()
    {
        if (!GameState.Get().IsInSubOptionMode())
        {
            return false;
        }
        this.CancelSubOptions();
        GameState.Get().CancelCurrentOptionMode();
        return true;
    }

    private void CancelSubOptions()
    {
        Card subOptionParentCard = ChoiceCardMgr.Get().GetSubOptionParentCard();
        if (subOptionParentCard != null)
        {
            ChoiceCardMgr.Get().CancelSubOptions();
            Entity entity = subOptionParentCard.GetEntity();
            if (!entity.IsHeroPower())
            {
                ZoneMgr.Get().CancelLocalZoneChange(this.m_lastZoneChangeList, null, null);
                this.m_lastZoneChangeList = null;
            }
            this.ReverseManaUpdate(entity);
            this.DropSubOptionParentCard();
        }
    }

    private bool CancelTargetMode()
    {
        if (!GameState.Get().IsInTargetMode())
        {
            return false;
        }
        SoundManager.Get().LoadAndPlay("CancelAttack");
        if (this.m_mousedOverCard != null)
        {
            this.DisableSkullIfNeeded(this.m_mousedOverCard);
        }
        if (TargetReticleManager.Get() != null)
        {
            TargetReticleManager.Get().DestroyFriendlyTargetArrow(true);
        }
        this.ResetBattlecrySourceCard();
        this.CancelSubOptions();
        GameState.Get().CancelCurrentOptionMode();
        return true;
    }

    private bool CheckDoubleClick()
    {
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        if (this.m_doubleClickActive && ((realtimeSinceStartup - this.m_clickStartTime) < 0.175f))
        {
            this.m_doubleClickHandler(this.m_clickObject);
            base.StopCoroutine("FireSingleClickAfterDelay");
            return true;
        }
        return false;
    }

    private void ClearBattlecrySourceCard()
    {
        if (this.m_isInBattleCryEffect && (this.m_battlecrySourceCard != null))
        {
            this.EndBattleCryEffect();
        }
        this.m_battlecrySourceCard = null;
        RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
        if (this.m_useHandEnlarge)
        {
            this.m_myHandZone.SetFriendlyHeroTargetingMode(false);
            this.m_myHandZone.UpdateLayout(-1, true);
        }
    }

    public void DisableInput()
    {
        this.m_checkForInput = false;
        this.HandleMouseOff();
        if (TargetReticleManager.Get() != null)
        {
            TargetReticleManager.Get().DestroyFriendlyTargetArrow(false);
        }
    }

    private void DisableSkullIfNeeded(Card mousedOverCard)
    {
        Spell actorSpell = mousedOverCard.GetActorSpell(SpellType.SKULL);
        if (actorSpell != null)
        {
            iTween.Stop(actorSpell.gameObject);
            actorSpell.transform.localScale = Vector3.zero;
            actorSpell.Deactivate();
        }
        Network.Options.Option.SubOption selectedNetworkSubOption = GameState.Get().GetSelectedNetworkSubOption();
        if (selectedNetworkSubOption != null)
        {
            Entity entity = GameState.Get().GetEntity(selectedNetworkSubOption.ID);
            if (entity != null)
            {
                Card card = entity.GetCard();
                if (card != null)
                {
                    actorSpell = card.GetActorSpell(SpellType.SKULL);
                    if (actorSpell != null)
                    {
                        iTween.Stop(actorSpell.gameObject);
                        actorSpell.transform.localScale = Vector3.zero;
                        actorSpell.Deactivate();
                    }
                }
            }
        }
    }

    public void DoEndTurnButton()
    {
        if (!GameMgr.Get().IsSpectator())
        {
            GameState state = GameState.Get();
            if (!state.IsResponsePacketBlocked() && !EndTurnButton.Get().IsInputBlocked())
            {
                switch (state.GetResponseMode())
                {
                    case GameState.ResponseMode.OPTION:
                    {
                        Network.Options optionsPacket = state.GetOptionsPacket();
                        for (int i = 0; i < optionsPacket.List.Count; i++)
                        {
                            Network.Options.Option option = optionsPacket.List[i];
                            if ((option.Type == Network.Options.Option.OptionType.END_TURN) || (option.Type == Network.Options.Option.OptionType.PASS))
                            {
                                if (state.GetGameEntity().NotifyOfEndTurnButtonPushed())
                                {
                                    state.SetSelectedOption(i);
                                    state.SendOption();
                                    this.HidePhoneHand();
                                    this.DoEndTurnButton_Option_OnEndTurnRequested();
                                }
                                break;
                            }
                        }
                        break;
                    }
                    case GameState.ResponseMode.CHOICE:
                    {
                        Network.EntityChoices friendlyEntityChoices = state.GetFriendlyEntityChoices();
                        List<Entity> chosenEntities = state.GetChosenEntities();
                        if (chosenEntities.Count >= friendlyEntityChoices.CountMin)
                        {
                            ChoiceCardMgr.Get().OnSendChoices(friendlyEntityChoices, chosenEntities);
                            state.SendChoices();
                        }
                        break;
                    }
                }
            }
        }
    }

    private void DoEndTurnButton_Option_OnEndTurnRequested()
    {
        if (TurnTimer.Get() != null)
        {
            TurnTimer.Get().OnEndTurnRequested();
        }
        EndTurnButton.Get().OnEndTurnRequested();
    }

    public bool DoesHideHandAfterPlayingCard()
    {
        return this.m_hideHandAfterPlayingCard;
    }

    private bool DoNetworkChoice(Entity entity)
    {
        GameState state = GameState.Get();
        if (!state.IsChoosableEntity(entity))
        {
            return false;
        }
        if (!state.RemoveChosenEntity(entity))
        {
            state.AddChosenEntity(entity);
            Network.EntityChoices friendlyEntityChoices = state.GetFriendlyEntityChoices();
            if (friendlyEntityChoices.CountMax == 1)
            {
                List<Entity> chosenEntities = state.GetChosenEntities();
                ChoiceCardMgr.Get().OnSendChoices(friendlyEntityChoices, chosenEntities);
                state.SendChoices();
            }
        }
        return true;
    }

    private bool DoNetworkOptions(Entity entity)
    {
        int entityId = entity.GetEntityId();
        GameState state = GameState.Get();
        Network.Options optionsPacket = state.GetOptionsPacket();
        for (int i = 0; i < optionsPacket.List.Count; i++)
        {
            Network.Options.Option option = optionsPacket.List[i];
            if ((option.Type == Network.Options.Option.OptionType.POWER) && (option.Main.ID == entityId))
            {
                state.SetSelectedOption(i);
                if (option.Subs.Count == 0)
                {
                    if ((option.Main.Targets == null) || (option.Main.Targets.Count == 0))
                    {
                        state.SendOption();
                    }
                    else
                    {
                        this.EnterOptionTargetMode();
                    }
                }
                else
                {
                    state.EnterSubOptionMode();
                    Card parentCard = entity.GetCard();
                    ChoiceCardMgr.Get().ShowSubOptions(parentCard);
                }
                return true;
            }
        }
        if (!UniversalInputManager.Get().IsTouchMode() || !entity.GetCard().IsShowingTooltip())
        {
            PlayErrors.DisplayPlayError(PlayErrors.GetPlayEntityError(entity), entity);
        }
        return false;
    }

    private bool DoNetworkOptionTarget(Entity entity, Entity simulatedSourceEntity = null)
    {
        bool flag = simulatedSourceEntity == null;
        int entityId = entity.GetEntityId();
        GameState state = GameState.Get();
        Network.Options.Option.SubOption selectedNetworkSubOption = state.GetSelectedNetworkSubOption();
        Entity errorSource = !flag ? simulatedSourceEntity : state.GetEntity(selectedNetworkSubOption.ID);
        if (flag && !selectedNetworkSubOption.Targets.Contains(entityId))
        {
            Entity target = state.GetEntity(entityId);
            PlayErrors.DisplayPlayError(PlayErrors.GetTargetEntityError(errorSource, target), errorSource);
            return false;
        }
        if (TargetReticleManager.Get() != null)
        {
            TargetReticleManager.Get().DestroyFriendlyTargetArrow(false);
        }
        this.FinishBattlecrySourceCard();
        this.FinishSubOptions();
        if (errorSource.IsHeroPower())
        {
            errorSource.SetTagAndHandleChange<int>(GAME_TAG.EXHAUSTED, 1);
            this.ForceManaUpdate(errorSource);
        }
        state.SetSelectedOptionTarget(entityId);
        state.SendOption();
        return true;
    }

    public bool DoNetworkResponse(Entity entity, bool checkValidInput = true)
    {
        if (ThinkEmoteManager.Get() != null)
        {
            ThinkEmoteManager.Get().NotifyOfActivity();
        }
        GameState state = GameState.Get();
        if (checkValidInput && !state.IsEntityInputEnabled(entity))
        {
            return false;
        }
        GameState.ResponseMode responseMode = state.GetResponseMode();
        bool flag = false;
        switch (responseMode)
        {
            case GameState.ResponseMode.OPTION:
                flag = this.DoNetworkOptions(entity);
                break;

            case GameState.ResponseMode.SUB_OPTION:
                flag = this.DoNetworkSubOptions(entity);
                break;

            case GameState.ResponseMode.OPTION_TARGET:
                flag = this.DoNetworkOptionTarget(entity, null);
                break;

            case GameState.ResponseMode.CHOICE:
                flag = this.DoNetworkChoice(entity);
                break;
        }
        if (flag)
        {
            entity.GetCard().UpdateActorState();
        }
        return flag;
    }

    private bool DoNetworkSubOptions(Entity entity)
    {
        int entityId = entity.GetEntityId();
        GameState state = GameState.Get();
        Network.Options.Option selectedNetworkOption = state.GetSelectedNetworkOption();
        for (int i = 0; i < selectedNetworkOption.Subs.Count; i++)
        {
            Network.Options.Option.SubOption option2 = selectedNetworkOption.Subs[i];
            if (option2.ID == entityId)
            {
                state.SetSelectedSubOption(i);
                if ((option2.Targets == null) || (option2.Targets.Count == 0))
                {
                    state.SendOption();
                }
                else
                {
                    this.EnterOptionTargetMode();
                }
                return true;
            }
        }
        return false;
    }

    public void DoubleClickCard(object obj)
    {
        this.m_mousedOverCard = null;
        FullScreenFXMgr.Get().Desaturate(0.9f, 0.4f, iTween.EaseType.easeInOutQuad, null);
        FullScreenFXMgr.Get().SetBlurDesaturation(0.5f);
        FullScreenFXMgr.Get().Blur(1f, 0.5f, iTween.EaseType.easeInCirc, null);
    }

    private void DropCanceledHeldCard(Card card)
    {
        this.m_heldCard = null;
        RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
        this.m_myHandZone.UpdateLayout(-1, true);
        this.m_myPlayZone.SortWithSpotForHeldCard(-1);
    }

    public bool DropHeldCard()
    {
        return this.DropHeldCard(false);
    }

    private bool DropHeldCard(bool wasCancelled)
    {
        Log.Hand.Print("DropHeldCard - cancelled? " + wasCancelled, new object[0]);
        PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
        if (this.m_useHandEnlarge)
        {
            this.m_myHandZone.SetFriendlyHeroTargetingMode(false);
            if (this.m_hideHandAfterPlayingCard)
            {
                this.HidePhoneHand();
            }
            else
            {
                this.m_myHandZone.UpdateLayout(-1, true);
            }
        }
        if (this.m_heldCard == null)
        {
            return false;
        }
        Card heldCard = this.m_heldCard;
        heldCard.SetDoNotSort(false);
        iTween.Stop(this.m_heldCard.gameObject);
        Entity entity = heldCard.GetEntity();
        heldCard.NotifyLeftPlayfield();
        GameState.Get().GetGameEntity().NotifyOfCardDropped(entity);
        DragCardSoundEffects component = heldCard.GetComponent<DragCardSoundEffects>();
        if (component != null)
        {
            component.Disable();
        }
        UnityEngine.Object.Destroy(this.m_heldCard.GetComponent<DragRotator>());
        this.m_heldCard = null;
        ProjectedShadow componentInChildren = heldCard.GetActor().GetComponentInChildren<ProjectedShadow>();
        if (componentInChildren != null)
        {
            componentInChildren.DisableShadow();
        }
        if (wasCancelled)
        {
            this.DropCanceledHeldCard(heldCard);
            return true;
        }
        bool flag = false;
        if (this.IsInZone(heldCard, TAG_ZONE.HAND))
        {
            bool flag2 = entity.IsMinion();
            bool flag3 = entity.IsWeapon();
            if (flag2 || flag3)
            {
                RaycastHit hit;
                if (UniversalInputManager.Get().GetInputHitInfo(Camera.main, GameLayer.InvisibleHitBox2, out hit))
                {
                    Zone zone = !flag3 ? ((Zone) this.m_myPlayZone) : ((Zone) this.m_myWeaponZone);
                    if (zone != null)
                    {
                        GameState state = GameState.Get();
                        int pos = 0;
                        int position = 0;
                        if (flag2)
                        {
                            pos = this.PlayZoneSlotMousedOver(heldCard) + 1;
                            position = ZoneMgr.Get().PredictZonePosition(zone, pos);
                            state.SetSelectedOptionPosition(position);
                        }
                        if (this.DoNetworkResponse(entity, true))
                        {
                            this.m_lastZoneChangeList = ZoneMgr.Get().AddPredictedLocalZoneChange(heldCard, zone, pos, position);
                            this.ForceManaUpdate(entity);
                            if (flag2 && state.EntityHasTargets(entity))
                            {
                                flag = true;
                                bool showArrow = !UniversalInputManager.Get().IsTouchMode();
                                if (TargetReticleManager.Get() != null)
                                {
                                    TargetReticleManager.Get().CreateFriendlyTargetArrow(entity, entity, true, showArrow, null, false);
                                }
                                this.m_battlecrySourceCard = heldCard;
                                if (UniversalInputManager.Get().IsTouchMode())
                                {
                                    this.StartBattleCryEffect(entity);
                                }
                            }
                        }
                        else
                        {
                            state.SetSelectedOptionPosition(0);
                        }
                    }
                }
            }
            else if (entity.IsSpell())
            {
                RaycastHit hit2;
                if (GameState.Get().EntityHasTargets(entity))
                {
                    this.DropCanceledHeldCard(entity.GetCard());
                    return true;
                }
                if (UniversalInputManager.Get().GetInputHitInfo(Camera.main, GameLayer.InvisibleHitBox2, out hit2))
                {
                    if (!GameState.Get().HasResponse(entity))
                    {
                        PlayErrors.DisplayPlayError(PlayErrors.GetPlayEntityError(entity), entity);
                    }
                    else
                    {
                        this.DoNetworkResponse(entity, true);
                        this.m_lastZoneChangeList = ZoneMgr.Get().AddLocalZoneChange(heldCard, TAG_ZONE.PLAY);
                        this.ForceManaUpdate(entity);
                        if (!GameState.Get().HasSubOptions(entity))
                        {
                            this.ActivatePowerUpSpell(heldCard);
                        }
                        this.ActivatePlaySpell(heldCard);
                    }
                }
            }
            this.m_myHandZone.UpdateLayout(-1, true);
            this.m_myPlayZone.SortWithSpotForHeldCard(-1);
        }
        if (flag)
        {
            if (RemoteActionHandler.Get() != null)
            {
                RemoteActionHandler.Get().NotifyOpponentOfTargetModeBegin(heldCard);
            }
        }
        else if (GameState.Get().GetResponseMode() != GameState.ResponseMode.SUB_OPTION)
        {
            RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
        }
        return true;
    }

    public void DropSubOptionParentCard()
    {
        Log.Hand.Print("DropSubOptionParentCard()", new object[0]);
        ChoiceCardMgr.Get().ClearSubOptions();
        RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
        if (this.m_useHandEnlarge)
        {
            this.m_myHandZone.SetFriendlyHeroTargetingMode(false);
            this.m_myHandZone.UpdateLayout(-1, true);
        }
        if (UniversalInputManager.Get().IsTouchMode())
        {
            this.EndMobileTargetingEffect();
        }
    }

    public void EnableInput()
    {
        this.m_checkForInput = true;
    }

    private void EndBattleCryEffect()
    {
        this.m_isInBattleCryEffect = false;
        this.EndMobileTargetingEffect();
        this.m_battlecrySourceCard.SetBattleCrySource(false);
    }

    private void EndMobileTargetingEffect()
    {
        foreach (Actor actor in this.m_mobileTargettingEffectActors)
        {
            this.RemoveMobileTargettingEffectFromActor(actor);
        }
        FullScreenFXMgr.Get().StopDesaturate(0.4f, iTween.EaseType.easeInOutQuad, null);
    }

    private void EnterOptionTargetMode()
    {
        GameState state = GameState.Get();
        state.EnterOptionTargetMode();
        if (this.m_useHandEnlarge)
        {
            this.m_myHandZone.SetFriendlyHeroTargetingMode(state.FriendlyHeroIsTargetable());
            this.m_myHandZone.UpdateLayout(-1, true);
        }
    }

    private void FinishBattlecrySourceCard()
    {
        if (this.m_battlecrySourceCard != null)
        {
            this.ClearBattlecrySourceCard();
        }
    }

    [DebuggerHidden]
    private IEnumerator FinishSpectatorNotify_SubOption(uint myToken, SpectatorNotify notify, ChooseOption echoPacket, Network.Options.Option chosenOption, Entity subEntity)
    {
        return new <FinishSpectatorNotify_SubOption>c__Iterator9E { echoPacket = echoPacket, myToken = myToken, notify = notify, chosenOption = chosenOption, subEntity = subEntity, <$>echoPacket = echoPacket, <$>myToken = myToken, <$>notify = notify, <$>chosenOption = chosenOption, <$>subEntity = subEntity, <>f__this = this };
    }

    private void FinishSubOptions()
    {
        if (ChoiceCardMgr.Get().GetSubOptionParentCard() != null)
        {
            this.DropSubOptionParentCard();
        }
    }

    [DebuggerHidden]
    private IEnumerator FireSingleClickAfterDelay()
    {
        return new <FireSingleClickAfterDelay>c__Iterator9D { <>f__this = this };
    }

    private void ForceManaUpdate(Entity entity)
    {
        Player friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
        int spentMana = entity.GetRealTimeCost() - friendlySidePlayer.GetRealTimeTempMana();
        if (friendlySidePlayer.GetRealTimeTempMana() > 0)
        {
            int usedMana = Mathf.Clamp(entity.GetRealTimeCost(), 0, friendlySidePlayer.GetRealTimeTempMana());
            friendlySidePlayer.NotifyOfUsedTempMana(usedMana);
            ManaCrystalMgr.Get().DestroyTempManaCrystals(usedMana);
        }
        if (spentMana > 0)
        {
            friendlySidePlayer.NotifyOfSpentMana(spentMana);
            ManaCrystalMgr.Get().UpdateSpentMana(spentMana);
        }
        friendlySidePlayer.UpdateManaCounter();
    }

    public static InputManager Get()
    {
        return s_instance;
    }

    public Card GetBattlecrySourceCard()
    {
        return this.m_battlecrySourceCard;
    }

    public ZoneHand GetFriendlyHand()
    {
        return this.m_myHandZone;
    }

    public Card GetHeldCard()
    {
        return this.m_heldCard;
    }

    public Card GetMousedOverCard()
    {
        return this.m_mousedOverCard;
    }

    private void GrabCard(GameObject cardObject)
    {
        if (!GameMgr.Get().IsSpectator())
        {
            Card component = cardObject.GetComponent<Card>();
            if (component.IsInputEnabled() && GameState.Get().GetGameEntity().ShouldAllowCardGrab(component.GetEntity()))
            {
                Zone zone = component.GetZone();
                if (zone.IsInputEnabled())
                {
                    component.SetDoNotSort(true);
                    if ((zone is ZoneHand) && !UniversalInputManager.Get().IsTouchMode())
                    {
                        ((ZoneHand) zone).UpdateLayout(-1);
                    }
                    this.m_heldCard = component;
                    SoundManager.Get().LoadAndPlay("FX_MinionSummon01_DrawFromHand_01", cardObject);
                    DragCardSoundEffects effects = this.m_heldCard.GetComponent<DragCardSoundEffects>();
                    if (effects != null)
                    {
                        effects.enabled = true;
                    }
                    else
                    {
                        effects = cardObject.AddComponent<DragCardSoundEffects>();
                    }
                    effects.Restart();
                    cardObject.AddComponent<DragRotator>().SetInfo(this.m_DragRotatorInfo);
                    ProjectedShadow componentInChildren = component.GetActor().GetComponentInChildren<ProjectedShadow>();
                    if (componentInChildren != null)
                    {
                        componentInChildren.EnableShadow(0.15f);
                    }
                    iTween.Stop(cardObject);
                    float x = 0.7f;
                    iTween.ScaleTo(cardObject, new Vector3(x, x, x), 0.2f);
                    KeywordHelpPanelManager.Get().HideKeywordHelp();
                    if (CardTypeBanner.Get() != null)
                    {
                        CardTypeBanner.Get().Hide();
                    }
                    component.NotifyPickedUp();
                    GameState.Get().GetGameEntity().NotifyOfCardGrabbed(component.GetEntity());
                    SceneUtils.SetLayer(component, GameLayer.Default);
                }
            }
        }
    }

    private void HandleClickOnCard(GameObject upClickedCard, bool wasMouseDownTarget)
    {
        if (EmoteHandler.Get() != null)
        {
            if (EmoteHandler.Get().IsMouseOverEmoteOption())
            {
                return;
            }
            EmoteHandler.Get().HideEmotes();
        }
        if (EnemyEmoteHandler.Get() != null)
        {
            if (EnemyEmoteHandler.Get().IsMouseOverEmoteOption())
            {
                return;
            }
            EnemyEmoteHandler.Get().HideEmotes();
        }
        Card component = upClickedCard.GetComponent<Card>();
        Entity entity = component.GetEntity();
        Log.Hand.Print("HandleClickOnCard - Card zone: " + component.GetZone(), new object[0]);
        if ((UniversalInputManager.Get().IsTouchMode() && entity.IsHero()) && (!GameState.Get().IsInTargetMode() && wasMouseDownTarget))
        {
            if (entity.IsControlledByLocalUser())
            {
                if (EmoteHandler.Get() != null)
                {
                    EmoteHandler.Get().ShowEmotes();
                }
                return;
            }
            if (!GameMgr.Get().IsSpectator() && (EnemyEmoteHandler.Get() != null))
            {
                EnemyEmoteHandler.Get().ShowEmotes();
                return;
            }
        }
        if (component == ChoiceCardMgr.Get().GetSubOptionParentCard())
        {
            this.CancelOption();
        }
        else
        {
            GameState.ResponseMode responseMode = GameState.Get().GetResponseMode();
            if (this.IsInZone(component, TAG_ZONE.HAND))
            {
                if (GameState.Get().IsMulliganManagerActive())
                {
                    if (!GameMgr.Get().IsSpectator())
                    {
                        MulliganManager.Get().ToggleHoldState(component);
                    }
                }
                else if (!UniversalInputManager.Get().IsTouchMode() && (!ChoiceCardMgr.Get().IsFriendlyShown() && (this.GetBattlecrySourceCard() == null)))
                {
                    this.GrabCard(upClickedCard);
                }
            }
            else
            {
                switch (responseMode)
                {
                    case GameState.ResponseMode.SUB_OPTION:
                        this.HandleClickOnSubOption(entity, false);
                        return;

                    case GameState.ResponseMode.CHOICE:
                        this.HandleClickOnChoice(entity);
                        return;
                }
                if (this.IsInZone(component, TAG_ZONE.PLAY))
                {
                    this.HandleClickOnCardInBattlefield(entity);
                }
            }
        }
    }

    private void HandleClickOnCardInBattlefield(Entity clickedEntity)
    {
        if (!GameMgr.Get().IsSpectator())
        {
            PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
            GameState state = GameState.Get();
            Card mousedOverCard = clickedEntity.GetCard();
            if (((!UniversalInputManager.Get().IsTouchMode() || !clickedEntity.IsHeroPower()) || (this.m_mousedOverTimer <= this.m_MouseOverDelay)) && state.GetGameEntity().NotifyOfBattlefieldCardClicked(clickedEntity, state.IsInTargetMode()))
            {
                if (state.IsInTargetMode())
                {
                    this.DisableSkullIfNeeded(mousedOverCard);
                    if (state.GetSelectedNetworkSubOption().ID == clickedEntity.GetEntityId())
                    {
                        this.CancelOption();
                    }
                    else if (this.DoNetworkResponse(clickedEntity, true) && (this.m_heldCard != null))
                    {
                        Card heldCard = this.m_heldCard;
                        this.m_heldCard = null;
                        heldCard.SetDoNotSort(false);
                        this.m_lastZoneChangeList = ZoneMgr.Get().AddLocalZoneChange(heldCard, TAG_ZONE.PLAY);
                    }
                }
                else if ((UniversalInputManager.Get().IsTouchMode() && UniversalInputManager.Get().GetMouseButtonUp(0)) && state.EntityHasTargets(clickedEntity))
                {
                    if (!mousedOverCard.IsShowingTooltip() && state.IsFriendlySidePlayerTurn())
                    {
                        PlayErrors.DisplayPlayError(PlayErrors.ErrorType.REQ_DRAG_TO_PLAY, clickedEntity);
                    }
                }
                else if (clickedEntity.IsWeapon() && clickedEntity.IsControlledByLocalUser())
                {
                    this.HandleClickOnCardInBattlefield(state.GetFriendlySidePlayer().GetHero());
                }
                else if (this.DoNetworkResponse(clickedEntity, true))
                {
                    if (!state.IsInTargetMode())
                    {
                        if (clickedEntity.IsHeroPower())
                        {
                            this.ActivatePlaySpell(mousedOverCard);
                            clickedEntity.SetTagAndHandleChange<int>(GAME_TAG.EXHAUSTED, 1);
                            this.ForceManaUpdate(clickedEntity);
                        }
                    }
                    else
                    {
                        RemoteActionHandler.Get().NotifyOpponentOfTargetModeBegin(mousedOverCard);
                        if (TargetReticleManager.Get() != null)
                        {
                            TargetReticleManager.Get().CreateFriendlyTargetArrow(clickedEntity, clickedEntity, false, true, null, false);
                        }
                        if (clickedEntity.IsHeroPower())
                        {
                            this.m_targettingHeroPower = true;
                            this.ActivatePlaySpell(mousedOverCard);
                        }
                        else if (clickedEntity.IsCharacter())
                        {
                            mousedOverCard.ActivateCharacterAttackEffects();
                            state.ShowEnemyTauntCharacters();
                            if (!mousedOverCard.IsAttacking())
                            {
                                Spell actorAttackSpellForInput = mousedOverCard.GetActorAttackSpellForInput();
                                if (actorAttackSpellForInput != null)
                                {
                                    if (clickedEntity.HasTag(GAME_TAG.IMMUNE_WHILE_ATTACKING))
                                    {
                                        mousedOverCard.GetActor().ActivateSpell(SpellType.IMMUNE);
                                    }
                                    actorAttackSpellForInput.ActivateState(SpellStateType.BIRTH);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void HandleClickOnChoice(Entity entity)
    {
        if (!GameMgr.Get().IsSpectator())
        {
            if (this.DoNetworkResponse(entity, true))
            {
                SoundManager.Get().LoadAndPlay("HeroDropItem1");
            }
            else
            {
                PlayErrors.DisplayPlayError(PlayErrors.GetPlayEntityError(entity), entity);
            }
        }
    }

    private void HandleClickOnSubOption(Entity entity, bool isSimulated = false)
    {
        if (isSimulated || GameState.Get().HasResponse(entity))
        {
            bool flag = false;
            if (!isSimulated)
            {
                flag = GameState.Get().SubEntityHasTargets(entity);
                if (flag)
                {
                    Card subOptionParentCard = ChoiceCardMgr.Get().GetSubOptionParentCard();
                    RemoteActionHandler.Get().NotifyOpponentOfTargetModeBegin(subOptionParentCard);
                    Entity hero = entity.GetHero();
                    Entity sourceEntity = subOptionParentCard.GetEntity();
                    TargetReticleManager.Get().CreateFriendlyTargetArrow(hero, sourceEntity, true, !UniversalInputManager.Get().IsTouchMode(), entity.GetCardTextInHand(), false);
                }
            }
            Card card2 = entity.GetCard();
            if (!isSimulated)
            {
                this.DoNetworkResponse(entity, true);
            }
            this.ActivatePowerUpSpell(card2);
            this.ActivatePlaySpell(card2);
            if (entity.IsMinion())
            {
                card2.HideCard();
            }
            ChoiceCardMgr.Get().OnSubOptionClicked(entity);
            if (!isSimulated && !flag)
            {
                this.FinishSubOptions();
            }
            if ((UniversalInputManager.Get().IsTouchMode() && !isSimulated) && flag)
            {
                this.StartMobileTargetingEffect(GameState.Get().GetSelectedNetworkSubOption().Targets);
            }
        }
        else
        {
            PlayErrors.DisplayPlayError(PlayErrors.GetPlayEntityError(entity), entity);
        }
    }

    private bool HandleGameHotkeys()
    {
        if ((GameState.Get() != null) && GameState.Get().IsMulliganManagerActive())
        {
            return false;
        }
        return (Input.GetKeyUp(KeyCode.Escape) && this.CancelOption());
    }

    public bool HandleKeyboardInput()
    {
        if (this.HandleUniversalHotkeys())
        {
            return true;
        }
        if ((GameState.Get() != null) && GameState.Get().IsMulliganManagerActive())
        {
            return this.HandleMulliganHotkeys();
        }
        return this.HandleGameHotkeys();
    }

    private void HandleLeftMouseDown()
    {
        this.m_touchedDownOnSmallHand = false;
        if (this.CheckDoubleClick())
        {
            this.m_ignoreMouseUp = true;
        }
        else
        {
            RaycastHit hit;
            GameObject gameObject = null;
            if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast, out hit))
            {
                gameObject = hit.collider.gameObject;
                if (gameObject.GetComponent<EndTurnButtonReminder>() != null)
                {
                    return;
                }
                CardStandIn @in = SceneUtils.FindComponentInParents<CardStandIn>(hit.transform);
                if (((@in != null) && (GameState.Get() != null)) && !GameState.Get().IsMulliganManagerActive())
                {
                    Card linkedCard = @in.linkedCard;
                    if (!this.IsCancelingBattlecryCard(linkedCard))
                    {
                        if (this.m_useHandEnlarge && !this.m_myHandZone.HandEnlarged())
                        {
                            this.m_leftMouseButtonIsDown = true;
                            this.m_touchedDownOnSmallHand = true;
                            return;
                        }
                        this.m_lastObjectMousedDown = @in.gameObject;
                        this.m_lastMouseDownPosition = UniversalInputManager.Get().GetMousePosition();
                        this.m_leftMouseButtonIsDown = true;
                        if (UniversalInputManager.Get().IsTouchMode())
                        {
                            this.m_touchDraggingCard = this.m_myHandZone.TouchReceived();
                            this.m_lastPreviewedCard = @in.linkedCard;
                        }
                    }
                    return;
                }
                if ((gameObject.GetComponent<EndTurnButton>() != null) && !GameMgr.Get().IsSpectator())
                {
                    EndTurnButton.Get().PlayPushDownAnimation();
                    this.m_lastObjectMousedDown = hit.collider.gameObject;
                    return;
                }
                if (gameObject.GetComponent<GameOpenPack>() != null)
                {
                    this.m_lastObjectMousedDown = hit.collider.gameObject;
                    return;
                }
                Actor actor = SceneUtils.FindComponentInParents<Actor>(hit.transform);
                if (actor == null)
                {
                    return;
                }
                Card card2 = actor.GetCard();
                if ((UniversalInputManager.Get().IsTouchMode() && (this.m_battlecrySourceCard != null)) && (card2 == this.m_battlecrySourceCard))
                {
                    this.m_dragging = true;
                    TargetReticleManager.Get().ShowArrow(true);
                    return;
                }
                if (this.IsCancelingBattlecryCard(card2))
                {
                    return;
                }
                if (card2 != null)
                {
                    this.m_lastObjectMousedDown = card2.gameObject;
                }
                else if (actor.GetHistoryCard() != null)
                {
                    this.m_lastObjectMousedDown = actor.transform.parent.gameObject;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("You clicked on something that is not being handled by InputManager.  Alert The Brode!");
                }
                this.m_lastMouseDownPosition = UniversalInputManager.Get().GetMousePosition();
                this.m_leftMouseButtonIsDown = true;
            }
            if ((this.m_useHandEnlarge && this.m_myHandZone.HandEnlarged()) && ((ChoiceCardMgr.Get().GetSubOptionParentCard() == null) && (gameObject == null)))
            {
                this.HidePhoneHand();
            }
            this.HandleMemberClick();
        }
    }

    private void HandleLeftMouseUp()
    {
        if (this.m_ignoreMouseUp)
        {
            this.m_ignoreMouseUp = false;
        }
        else
        {
            PegCursor.Get().SetMode(PegCursor.Mode.UP);
            bool dragging = this.m_dragging;
            this.m_dragging = false;
            this.m_leftMouseButtonIsDown = false;
            this.m_targettingHeroPower = false;
            GameObject lastObjectMousedDown = this.m_lastObjectMousedDown;
            this.m_lastObjectMousedDown = null;
            if (UniversalInputManager.Get().WasTouchCanceled())
            {
                this.CancelOption();
                this.m_heldCard = null;
            }
            else if ((this.m_heldCard != null) && ((GameState.Get().GetResponseMode() == GameState.ResponseMode.OPTION) || (GameState.Get().GetResponseMode() == GameState.ResponseMode.NONE)))
            {
                this.DropHeldCard();
            }
            else
            {
                RaycastHit hit;
                bool flag2 = UniversalInputManager.Get().IsTouchMode() && GameState.Get().IsInTargetMode();
                bool flag3 = ChoiceCardMgr.Get().GetSubOptionParentCard() != null;
                if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast, out hit))
                {
                    GameObject gameObject = hit.collider.gameObject;
                    if (gameObject.GetComponent<EndTurnButtonReminder>() != null)
                    {
                        return;
                    }
                    if (((gameObject.GetComponent<EndTurnButton>() != null) && (gameObject == lastObjectMousedDown)) && !GameMgr.Get().IsSpectator())
                    {
                        EndTurnButton.Get().PlayButtonUpAnimation();
                        this.DoEndTurnButton();
                    }
                    else if ((gameObject.GetComponent<GameOpenPack>() != null) && (gameObject == lastObjectMousedDown))
                    {
                        gameObject.GetComponent<GameOpenPack>().HandleClick();
                    }
                    else
                    {
                        Actor actor = SceneUtils.FindComponentInParents<Actor>(hit.transform);
                        if (actor != null)
                        {
                            Card card = actor.GetCard();
                            if (card != null)
                            {
                                if (((card.gameObject == lastObjectMousedDown) || dragging) && !this.IsCancelingBattlecryCard(card))
                                {
                                    this.HandleClickOnCard(actor.GetCard().gameObject, card.gameObject == lastObjectMousedDown);
                                }
                            }
                            else if (actor.GetHistoryCard() != null)
                            {
                                HistoryManager.Get().HandleClickOnBigCard(actor.GetHistoryCard());
                            }
                        }
                        CardStandIn @in = SceneUtils.FindComponentInParents<CardStandIn>(hit.transform);
                        if (@in != null)
                        {
                            if (this.m_useHandEnlarge && this.m_touchedDownOnSmallHand)
                            {
                                this.ShowPhoneHand();
                            }
                            if (((lastObjectMousedDown == @in.gameObject) && (GameState.Get() != null)) && (!GameState.Get().IsMulliganManagerActive() && !this.IsCancelingBattlecryCard(@in.linkedCard)))
                            {
                                this.HandleClickOnCard(@in.linkedCard.gameObject, true);
                            }
                        }
                        if ((UniversalInputManager.Get().IsTouchMode() && (actor != null)) && (ChoiceCardMgr.Get().GetSubOptionParentCard() != null))
                        {
                            foreach (Card card2 in ChoiceCardMgr.Get().GetFriendlyCards())
                            {
                                if (card2 == actor.GetCard())
                                {
                                    flag3 = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (flag2)
                {
                    this.CancelOption();
                }
                if ((UniversalInputManager.Get().IsTouchMode() && flag3) && (ChoiceCardMgr.Get().GetSubOptionParentCard() != null))
                {
                    this.CancelSubOptionMode();
                }
            }
        }
    }

    public void HandleMemberClick()
    {
        if (this.m_mousedOverObject == null)
        {
            RaycastHit hit;
            if (UniversalInputManager.Get().GetInputHitInfo(Camera.main, GameLayer.PlayAreaCollision, out hit))
            {
                RaycastHit hit2;
                if (((GameState.Get() != null) && !GameState.Get().IsMulliganManagerActive()) && !UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast, out hit2))
                {
                    GameObject mouseClickDustEffectPrefab = Board.Get().GetMouseClickDustEffectPrefab();
                    if (mouseClickDustEffectPrefab != null)
                    {
                        GameObject parent = UnityEngine.Object.Instantiate<GameObject>(mouseClickDustEffectPrefab);
                        parent.transform.position = hit.point;
                        ParticleSystem[] componentsInChildren = parent.GetComponentsInChildren<ParticleSystem>();
                        if (componentsInChildren != null)
                        {
                            Vector3 euler = new Vector3(Input.GetAxis("Mouse Y") * 40f, Input.GetAxis("Mouse X") * 40f, 0f);
                            foreach (ParticleSystem system in componentsInChildren)
                            {
                                if (system.name == "Rocks")
                                {
                                    system.transform.localRotation = Quaternion.Euler(euler);
                                }
                                system.Play();
                            }
                            switch (UnityEngine.Random.Range(1, 5))
                            {
                                case 1:
                                    SoundManager.Get().LoadAndPlay("board_common_dirt_poke_1", parent);
                                    break;

                                case 2:
                                    SoundManager.Get().LoadAndPlay("board_common_dirt_poke_2", parent);
                                    break;

                                case 3:
                                    SoundManager.Get().LoadAndPlay("board_common_dirt_poke_3", parent);
                                    break;

                                case 4:
                                    SoundManager.Get().LoadAndPlay("board_common_dirt_poke_4", parent);
                                    break;

                                case 5:
                                    SoundManager.Get().LoadAndPlay("board_common_dirt_poke_5", parent);
                                    break;
                            }
                        }
                    }
                }
            }
            else if (Gameplay.Get() != null)
            {
                SoundManager.Get().LoadAndPlay("UI_MouseClick_01");
            }
        }
    }

    private void HandleMouseMove()
    {
        if (GameState.Get().IsInTargetMode())
        {
            this.HandleUpdateWhileNotHoldingCard();
        }
    }

    private void HandleMouseOff()
    {
        if (this.m_mousedOverCard != null)
        {
            this.HandleMouseOffCard();
        }
        if (this.m_mousedOverObject != null)
        {
            this.HandleMouseOffLastObject();
        }
    }

    private void HandleMouseOffCard()
    {
        PegCursor.Get().SetMode(PegCursor.Mode.UP);
        Card mousedOverCard = this.m_mousedOverCard;
        this.m_mousedOverCard = null;
        mousedOverCard.HideTooltip();
        mousedOverCard.NotifyMousedOut();
        this.ShowBullseyeIfNeeded();
        this.DisableSkullIfNeeded(mousedOverCard);
    }

    private void HandleMouseOffLastObject()
    {
        if (this.m_mousedOverObject.GetComponent<EndTurnButton>() != null)
        {
            this.m_mousedOverObject.GetComponent<EndTurnButton>().HandleMouseOut();
            this.m_lastObjectMousedDown = null;
        }
        else if (this.m_mousedOverObject.GetComponent<EndTurnButtonReminder>() != null)
        {
            this.m_lastObjectMousedDown = null;
        }
        else if (this.m_mousedOverObject.GetComponent<TooltipZone>() != null)
        {
            this.m_mousedOverObject.GetComponent<TooltipZone>().HideTooltip();
            this.m_lastObjectMousedDown = null;
        }
        else if (this.m_mousedOverObject.GetComponent<HistoryManager>() != null)
        {
            HistoryManager.Get().NotifyOfMouseOff();
        }
        else if (this.m_mousedOverObject.GetComponent<GameOpenPack>() != null)
        {
            this.m_mousedOverObject.GetComponent<GameOpenPack>().NotifyOfMouseOff();
            this.m_lastObjectMousedDown = null;
        }
        this.m_mousedOverObject = null;
        this.HideBigViewCardBacks();
    }

    private void HandleMouseOverCard(Card card)
    {
        if (card.IsInputEnabled())
        {
            GameState state = GameState.Get();
            this.m_mousedOverCard = card;
            bool flag2 = (state.IsFriendlySidePlayerTurn() && (TargetReticleManager.Get() != null)) && TargetReticleManager.Get().IsActive();
            if ((GameMgr.Get() != null) && GameMgr.Get().IsSpectator())
            {
                flag2 = false;
            }
            if (((state.IsMainPhase() && (this.m_heldCard == null)) && (!ChoiceCardMgr.Get().HasSubOption() && !flag2)) && (!UniversalInputManager.Get().IsTouchMode() || (card.gameObject == this.m_lastObjectMousedDown)))
            {
                this.SetShouldShowTooltip();
            }
            card.NotifyMousedOver();
            if ((state.IsMulliganManagerActive() && card.GetEntity().IsControlledByFriendlySidePlayer()) && (UniversalInputManager.UsePhoneUI == null))
            {
                KeywordHelpPanelManager.Get().UpdateKeywordHelpForMulliganCard(card.GetEntity(), card.GetActor());
            }
            this.ShowBullseyeIfNeeded();
            this.ShowSkullIfNeeded();
        }
    }

    private void HandleMouseOverObjectWhileNotHoldingCard(RaycastHit hitInfo)
    {
        GameObject gameObject = hitInfo.collider.gameObject;
        if (this.m_mousedOverCard != null)
        {
            this.HandleMouseOffCard();
        }
        if (UniversalInputManager.Get().IsTouchMode() && !UniversalInputManager.Get().GetMouseButton(0))
        {
            if (this.m_mousedOverObject != null)
            {
                this.HandleMouseOffLastObject();
            }
        }
        else
        {
            bool flag = (TargetReticleManager.Get() != null) && TargetReticleManager.Get().IsLocalArrowActive();
            if ((GameMgr.Get() != null) && GameMgr.Get().IsSpectator())
            {
                flag = false;
            }
            if ((gameObject.GetComponent<HistoryManager>() != null) && !flag)
            {
                this.m_mousedOverObject = gameObject;
                HistoryManager.Get().NotifyOfInput(hitInfo.point.z);
            }
            else if (this.m_mousedOverObject != gameObject)
            {
                if (this.m_mousedOverObject != null)
                {
                    this.HandleMouseOffLastObject();
                }
                if ((EndTurnButton.Get() != null) && !GameMgr.Get().IsSpectator())
                {
                    if (gameObject.GetComponent<EndTurnButton>() != null)
                    {
                        this.m_mousedOverObject = gameObject;
                        EndTurnButton.Get().HandleMouseOver();
                    }
                    else if ((gameObject.GetComponent<EndTurnButtonReminder>() != null) && gameObject.GetComponent<EndTurnButtonReminder>().ShowFriendlySidePlayerTurnReminder())
                    {
                        this.m_mousedOverObject = gameObject;
                    }
                }
                TooltipZone component = gameObject.GetComponent<TooltipZone>();
                if (component != null)
                {
                    this.m_mousedOverObject = gameObject;
                    this.ShowTooltipZone(gameObject, component);
                }
                GameOpenPack pack = gameObject.GetComponent<GameOpenPack>();
                if (pack != null)
                {
                    this.m_mousedOverObject = gameObject;
                    pack.NotifyOfMouseOver();
                }
                if ((this.GetBattlecrySourceCard() == null) && (UniversalInputManager.Get().InputHitAnyObject(Camera.main, GameLayer.InvisibleHitBox1) && ChoiceCardMgr.Get().HasSubOption()))
                {
                    this.CancelSubOptionMode();
                }
            }
        }
    }

    private bool HandleMulliganHotkeys()
    {
        if ((MulliganManager.Get() != null) && (((ApplicationMgr.IsInternal() && Input.GetKeyUp(KeyCode.Escape)) && (!GameMgr.Get().IsTutorial() && (PlatformSettings.OS != OSCategory.iOS))) && (PlatformSettings.OS != OSCategory.Android)))
        {
            MulliganManager.Get().SetAllMulliganCardsToHold();
            this.DoEndTurnButton();
            TurnStartManager.Get().BeginListeningForTurnEvents();
            MulliganManager.Get().SkipMulliganForDev();
            return true;
        }
        return false;
    }

    private void HandleRightClick()
    {
        if (!this.CancelOption())
        {
            if ((EmoteHandler.Get() != null) && EmoteHandler.Get().AreEmotesActive())
            {
                EmoteHandler.Get().HideEmotes();
            }
            if ((EnemyEmoteHandler.Get() != null) && EnemyEmoteHandler.Get().AreEmotesActive())
            {
                EnemyEmoteHandler.Get().HideEmotes();
            }
        }
    }

    private void HandleRightClickOnCard(Card card)
    {
        if ((GameState.Get().IsInTargetMode() || GameState.Get().IsInSubOptionMode()) || (this.m_heldCard != null))
        {
            this.HandleRightClick();
        }
        else if (card.GetEntity().IsHero())
        {
            if (card.GetEntity().IsControlledByLocalUser())
            {
                if (EmoteHandler.Get() != null)
                {
                    if (EmoteHandler.Get().AreEmotesActive())
                    {
                        EmoteHandler.Get().HideEmotes();
                    }
                    else
                    {
                        EmoteHandler.Get().ShowEmotes();
                    }
                }
            }
            else
            {
                bool flag = EnemyEmoteHandler.Get() != null;
                if (GameMgr.Get().IsSpectator() && (card.GetEntity().GetControllerSide() != Player.Side.OPPOSING))
                {
                    flag = false;
                }
                if (flag)
                {
                    if (EnemyEmoteHandler.Get().AreEmotesActive())
                    {
                        EnemyEmoteHandler.Get().HideEmotes();
                    }
                    else
                    {
                        EnemyEmoteHandler.Get().ShowEmotes();
                    }
                }
            }
        }
    }

    private void HandleRightMouseDown()
    {
        RaycastHit hit;
        if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast, out hit))
        {
            GameObject gameObject = hit.collider.gameObject;
            if ((gameObject.GetComponent<EndTurnButtonReminder>() == null) && (gameObject.GetComponent<EndTurnButton>() == null))
            {
                Actor actor = SceneUtils.FindComponentInParents<Actor>(hit.transform);
                if (actor != null)
                {
                    if (actor.GetCard() != null)
                    {
                        this.m_lastObjectRightMousedDown = actor.GetCard().gameObject;
                    }
                    else if (actor.GetHistoryCard() != null)
                    {
                        this.m_lastObjectRightMousedDown = actor.transform.parent.gameObject;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("You clicked on something that is not being handled by InputManager.  Alert The Brode!");
                    }
                }
            }
        }
    }

    private void HandleRightMouseUp()
    {
        RaycastHit hit;
        PegCursor.Get().SetMode(PegCursor.Mode.UP);
        GameObject lastObjectRightMousedDown = this.m_lastObjectRightMousedDown;
        this.m_lastObjectRightMousedDown = null;
        this.m_lastObjectMousedDown = null;
        this.m_leftMouseButtonIsDown = false;
        this.m_dragging = false;
        if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast, out hit))
        {
            Actor actor = SceneUtils.FindComponentInParents<Actor>(hit.transform);
            if ((actor == null) || (actor.GetCard() == null))
            {
                this.HandleRightClick();
            }
            else if (actor.GetCard().gameObject == lastObjectRightMousedDown)
            {
                this.HandleRightClickOnCard(actor.GetCard());
            }
            else
            {
                this.HandleRightClick();
            }
        }
        else
        {
            this.HandleRightClick();
        }
    }

    private bool HandleUniversalHotkeys()
    {
        return false;
    }

    private void HandleUpdateWhileHoldingCard(bool hitBattlefield)
    {
        PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
        Card heldCard = this.m_heldCard;
        if (!heldCard.IsInputEnabled())
        {
            this.DropHeldCard();
        }
        else
        {
            Entity entity = heldCard.GetEntity();
            if (((hitBattlefield && (TargetReticleManager.Get() != null)) && (!TargetReticleManager.Get().IsActive() && GameState.Get().EntityHasTargets(entity))) && (entity.GetCardType() != TAG_CARDTYPE.MINION))
            {
                if (!this.DoNetworkResponse(entity, true))
                {
                    this.PositionHeldCard();
                    return;
                }
                DragCardSoundEffects component = heldCard.GetComponent<DragCardSoundEffects>();
                if (component != null)
                {
                    component.Disable();
                }
                RemoteActionHandler.Get().NotifyOpponentOfCardPickedUp(heldCard);
                RemoteActionHandler.Get().NotifyOpponentOfTargetModeBegin(heldCard);
                Entity hero = entity.GetHero();
                TargetReticleManager.Get().CreateFriendlyTargetArrow(hero, entity, true, true, null, false);
                this.ActivatePowerUpSpell(heldCard);
                this.ActivatePlaySpell(heldCard);
            }
            else
            {
                if (hitBattlefield && this.m_cardWasInsideHandLastFrame)
                {
                    RemoteActionHandler.Get().NotifyOpponentOfCardPickedUp(heldCard);
                    this.m_cardWasInsideHandLastFrame = false;
                }
                else if (!hitBattlefield)
                {
                    this.m_cardWasInsideHandLastFrame = true;
                }
                this.PositionHeldCard();
                if (GameState.Get().GetResponseMode() == GameState.ResponseMode.SUB_OPTION)
                {
                    this.CancelSubOptionMode();
                }
            }
            if ((UniversalInputManager.Get().IsTouchMode() && !hitBattlefield) && ((this.m_heldCard != null) && ((UniversalInputManager.Get().GetMousePosition().y - this.m_lastMouseDownPosition.y) < this.MIN_GRAB_Y)))
            {
                PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
                this.ReturnHeldCardToHand();
            }
        }
    }

    private void HandleUpdateWhileLeftMouseButtonIsDown()
    {
        if (UniversalInputManager.Get().IsTouchMode() && (this.m_heldCard == null))
        {
            if (this.GetBattlecrySourceCard() == null)
            {
                this.m_myHandZone.HandleInput();
            }
            Card card = (this.m_myHandZone.CurrentStandIn == null) ? null : this.m_myHandZone.CurrentStandIn.linkedCard;
            if (card != this.m_lastPreviewedCard)
            {
                if (card != null)
                {
                    this.m_lastMouseDownPosition.y = UniversalInputManager.Get().GetMousePosition().y;
                }
                this.m_lastPreviewedCard = card;
            }
        }
        if (!this.m_dragging && (this.m_lastObjectMousedDown != null))
        {
            if (this.m_lastObjectMousedDown.GetComponent<HistoryCard>() != null)
            {
                this.m_lastObjectMousedDown = null;
                this.m_leftMouseButtonIsDown = false;
            }
            else
            {
                float num = UniversalInputManager.Get().GetMousePosition().y - this.m_lastMouseDownPosition.y;
                float num2 = UniversalInputManager.Get().GetMousePosition().x - this.m_lastMouseDownPosition.x;
                if (((num2 <= -20f) || (num2 >= 20f)) || ((num <= -20f) || (num >= 20f)))
                {
                    bool flag = !UniversalInputManager.Get().IsTouchMode() || (num > this.MIN_GRAB_Y);
                    CardStandIn component = this.m_lastObjectMousedDown.GetComponent<CardStandIn>();
                    if (((component != null) && (GameState.Get() != null)) && !GameState.Get().IsMulliganManagerActive())
                    {
                        if (UniversalInputManager.Get().IsTouchMode())
                        {
                            if (!flag)
                            {
                                return;
                            }
                            component = this.m_myHandZone.CurrentStandIn;
                            if (component == null)
                            {
                                return;
                            }
                        }
                        if ((!ChoiceCardMgr.Get().IsFriendlyShown() && (this.GetBattlecrySourceCard() == null)) && this.IsInZone(component.linkedCard, TAG_ZONE.HAND))
                        {
                            this.m_dragging = true;
                            this.GrabCard(component.linkedCard.gameObject);
                        }
                    }
                    else if (!GameState.Get().IsMulliganManagerActive() && !GameState.Get().IsInTargetMode())
                    {
                        Card card2 = this.m_lastObjectMousedDown.GetComponent<Card>();
                        Entity entity = card2.GetEntity();
                        if (entity.IsControlledByLocalUser())
                        {
                            if (this.IsInZone(card2, TAG_ZONE.HAND))
                            {
                                if ((flag && (!UniversalInputManager.Get().IsTouchMode() || GameState.Get().HasResponse(entity))) && (ChoiceCardMgr.Get().IsFriendlyShown() && (this.GetBattlecrySourceCard() == null)))
                                {
                                    this.m_dragging = true;
                                    this.GrabCard(this.m_lastObjectMousedDown);
                                }
                            }
                            else if (this.IsInZone(card2, TAG_ZONE.PLAY) && (!entity.IsHeroPower() || (entity.IsHeroPower() && GameState.Get().EntityHasTargets(entity))))
                            {
                                this.m_dragging = true;
                                this.HandleClickOnCardInBattlefield(entity);
                            }
                        }
                    }
                }
            }
        }
    }

    private void HandleUpdateWhileNotHoldingCard()
    {
        RaycastHit hit;
        if (!UniversalInputManager.Get().IsTouchMode() || !TargetReticleManager.Get().IsLocalArrowActive())
        {
            this.m_myHandZone.HandleInput();
        }
        bool flag = UniversalInputManager.Get().IsTouchMode() && !UniversalInputManager.Get().GetMouseButton(0);
        bool inputHitInfo = UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast, out hit);
        if (!flag && inputHitInfo)
        {
            CardStandIn @in = SceneUtils.FindComponentInParents<CardStandIn>(hit.transform);
            Actor actor = SceneUtils.FindComponentInParents<Actor>(hit.transform);
            if ((actor == null) && (@in == null))
            {
                this.HandleMouseOverObjectWhileNotHoldingCard(hit);
            }
            else
            {
                if (this.m_mousedOverObject != null)
                {
                    this.HandleMouseOffLastObject();
                }
                Card linkedCard = null;
                if (actor != null)
                {
                    linkedCard = actor.GetCard();
                }
                if (linkedCard == null)
                {
                    if (@in == null)
                    {
                        return;
                    }
                    if ((GameState.Get() == null) || GameState.Get().IsMulliganManagerActive())
                    {
                        return;
                    }
                    linkedCard = @in.linkedCard;
                }
                if (!this.IsCancelingBattlecryCard(linkedCard))
                {
                    if ((linkedCard != this.m_mousedOverCard) && ((linkedCard.GetZone() != this.m_myHandZone) || GameState.Get().IsMulliganManagerActive()))
                    {
                        if (this.m_mousedOverCard != null)
                        {
                            this.HandleMouseOffCard();
                        }
                        this.HandleMouseOverCard(linkedCard);
                    }
                    PegCursor.Get().SetMode(PegCursor.Mode.OVER);
                }
            }
        }
        else
        {
            if (this.m_mousedOverCard != null)
            {
                this.HandleMouseOffCard();
            }
            this.HandleMouseOff();
        }
    }

    private bool HasLocalHandPlays()
    {
        List<Card> cards = this.m_myHandZone.GetCards();
        if (cards.Count != 0)
        {
            int spendableManaCrystals = ManaCrystalMgr.Get().GetSpendableManaCrystals();
            foreach (Card card in cards)
            {
                if (card.GetEntity().GetRealTimeCost() <= spendableManaCrystals)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void HideBigViewCardBacks()
    {
    }

    private void HidePhoneHand()
    {
        if ((this.m_useHandEnlarge && (this.m_myHandZone != null)) && this.m_myHandZone.HandEnlarged())
        {
            this.m_myHandZone.SetHandEnlarged(false);
            foreach (PhoneHandHiddenListener listener in this.m_phoneHandHiddenListener.ToArray())
            {
                listener.Fire();
            }
        }
    }

    private void HidePhoneHandIfOutOfServerPlays()
    {
        if (!GameState.Get().HasHandPlays())
        {
            this.HidePhoneHand();
        }
    }

    private bool IsCancelingBattlecryCard(Card card)
    {
        return this.m_cancelingBattlecryCards.Contains(card);
    }

    private bool IsInZone(Card card, TAG_ZONE zoneTag)
    {
        if (card.GetZone() == null)
        {
            return false;
        }
        return (GameUtils.GetFinalZoneForEntity(card.GetEntity()) == zoneTag);
    }

    private bool IsInZone(Entity entity, TAG_ZONE zoneTag)
    {
        return this.IsInZone(entity.GetCard(), zoneTag);
    }

    public bool MouseIsMoving()
    {
        return this.MouseIsMoving(0f);
    }

    public bool MouseIsMoving(float tolerance)
    {
        if ((Mathf.Abs(Input.GetAxis("Mouse X")) <= tolerance) && (Mathf.Abs(Input.GetAxis("Mouse Y")) <= tolerance))
        {
            return false;
        }
        return true;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnGameOver(object userData)
    {
        this.CancelOption();
    }

    private void OnHandEnlargeComplete(Zone zone, object userData)
    {
        zone.RemoveUpdateLayoutCompleteCallback(new Zone.UpdateLayoutCompleteCallback(this.OnHandEnlargeComplete));
        if (this.m_leftMouseButtonIsDown && UniversalInputManager.Get().InputHitAnyObject(GameLayer.CardRaycast))
        {
            this.HandleLeftMouseDown();
        }
    }

    public void OnManaCrystalMgrManaSpent()
    {
        if (this.m_mousedOverCard != null)
        {
            this.m_mousedOverCard.UpdateProposedManaUsage();
        }
    }

    public void OnMulliganEnded()
    {
        if (this.m_mousedOverCard != null)
        {
            this.SetShouldShowTooltip();
        }
    }

    private void OnOptionsReceived(object userData)
    {
        if (this.m_mousedOverCard != null)
        {
            this.m_mousedOverCard.UpdateProposedManaUsage();
        }
        this.HidePhoneHandIfOutOfServerPlays();
    }

    private void OnSpectatorNotifyEvent(SpectatorNotify notify, object userData)
    {
        if (GameMgr.Get().IsSpectator())
        {
            GameState state = GameState.Get();
            if ((((state != null) && state.IsGameCreatedOrCreating()) && !state.IsGameOverNowOrPending()) && notify.HasChooseOption)
            {
                bool flag = true;
                ChooseOption chooseOption = notify.ChooseOption;
                if (notify.PlayerId != state.GetCurrentPlayer().GetPlayerId())
                {
                    object[] args = new object[] { notify.PlayerId, chooseOption.Id, state.GetCurrentPlayer().GetPlayerId() };
                    Log.Power.Print("Spectator received ChooseOption for wrong player turn receivedPlayerId={0} receivedId={1} currentTurnPlayerId={2}", args);
                }
                else
                {
                    Network.Options optionsPacket = state.GetOptionsPacket();
                    if (optionsPacket == null)
                    {
                        string format = string.Format("Spectator received SpectatorNotify while options is null receivedPlayerId={0} receivedId={1} currentTurnPlayerId={2}", notify.PlayerId, chooseOption.Id, state.GetCurrentPlayer().GetPlayerId());
                        Log.Power.Print(format, new object[0]);
                        UnityEngine.Debug.LogError(format);
                    }
                    else
                    {
                        Network.Options.Option chosenOption = null;
                        if ((chooseOption.Index >= 0) && (chooseOption.Index < optionsPacket.List.Count))
                        {
                            chosenOption = optionsPacket.List[chooseOption.Index];
                        }
                        if (((optionsPacket == null) || (optionsPacket.ID != chooseOption.Id)) || (chosenOption == null))
                        {
                            object[] objArray2 = new object[] { notify.PlayerId, chooseOption.Id, chooseOption.Index, (optionsPacket != null) ? optionsPacket.ID.ToString() : "NULL", (optionsPacket != null) ? optionsPacket.List.Count.ToString() : "NULL" };
                            Log.Power.Print("Spectator received unexpected ChooseOption playerId={0} receivedId={1} receivedIndex={2} availId={3} availCount={4}", objArray2);
                        }
                        else
                        {
                            state.SetSelectedOption(chooseOption);
                            Entity entity = state.GetEntity(chosenOption.Main.ID);
                            if ((chosenOption.Type == Network.Options.Option.OptionType.END_TURN) || (chosenOption.Type == Network.Options.Option.OptionType.PASS))
                            {
                                state.GetGameEntity().NotifyOfEndTurnButtonPushed();
                                GameState.Get().ClearResponseMode();
                                this.OnSpectatorNotifyEvent_UpdateHighlights();
                                if (this.m_mousedOverCard != null)
                                {
                                    GameState.Get().GetFriendlySidePlayer().CancelAllProposedMana(this.m_mousedOverCard.GetEntity());
                                }
                                this.DoEndTurnButton_Option_OnEndTurnRequested();
                            }
                            else
                            {
                                if (entity == null)
                                {
                                    object[] objArray3 = new object[] { notify.PlayerId, chooseOption.Id, chosenOption.Main.ID };
                                    Log.Power.Print("Spectator received unknown entity in ChooseOption playerId={0} receivedId={1} entityId={2}", objArray3);
                                }
                                RemoteActionHandler handler = RemoteActionHandler.Get();
                                if (((handler != null) && (handler.GetFriendlyHoverCard() != null)) && (handler.GetFriendlyHoverCard() == entity.GetCard()))
                                {
                                    handler.GetFriendlyHoverCard().NotifyMousedOut();
                                }
                                if (chooseOption.HasSubOption && (chooseOption.SubOption >= 0))
                                {
                                    if ((chosenOption.Subs == null) || (chooseOption.SubOption >= chosenOption.Subs.Count))
                                    {
                                        object[] objArray4 = new object[] { notify.PlayerId, chooseOption.Id, chooseOption.Index, chooseOption.SubOption, (chosenOption.Subs != null) ? chosenOption.Subs.Count : 0 };
                                        Log.Power.Print("Spectator received unexpected ChooseOption SubOption playerId={0} receivedId={1} option={2} subOption={3} availSubOptions={4}", objArray4);
                                        this.OnSpectatorNotifyEvent_UpdateHighlights();
                                        return;
                                    }
                                    this.DoNetworkResponse(entity, false);
                                    Network.Options.Option.SubOption option3 = chosenOption.Subs[chooseOption.SubOption];
                                    Entity subEntity = state.GetEntity(option3.ID);
                                    if (subEntity == null)
                                    {
                                        object[] objArray5 = new object[] { notify.PlayerId, chooseOption.Id, chosenOption.Main.ID, option3.ID };
                                        Log.Power.Print("Spectator received unknown entity in ChooseOption SubOption playerId={0} receivedId={1} mainEntityId={2} subEntityId={3}", objArray5);
                                        this.OnSpectatorNotifyEvent_UpdateHighlights();
                                        return;
                                    }
                                    this.m_spectatorNotifyCurrentToken++;
                                    base.StartCoroutine(this.FinishSpectatorNotify_SubOption(this.m_spectatorNotifyCurrentToken, notify, chooseOption, chosenOption, subEntity));
                                    flag = false;
                                }
                                else if (chooseOption.Target > 0)
                                {
                                    Entity entity3 = state.GetEntity(chooseOption.Target);
                                    if (entity3 == null)
                                    {
                                        object[] objArray6 = new object[] { notify.PlayerId, chooseOption.Id, chosenOption.Main.ID, chooseOption.Target };
                                        Log.Power.Print("Spectator received unknown target entity in ChooseOption playerId={0} receivedId={1} mainEntityId={2} targetEntityId={3}", objArray6);
                                        this.OnSpectatorNotifyEvent_UpdateHighlights();
                                        return;
                                    }
                                    this.DoNetworkOptionTarget(entity3, entity);
                                }
                                if (flag)
                                {
                                    this.OnSpectatorNotifyEvent_UpdateHighlights();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnSpectatorNotifyEvent_UpdateHighlights()
    {
        Network.Options optionsPacket = GameState.Get().GetOptionsPacket();
        if (optionsPacket == null)
        {
            optionsPacket = GameState.Get().GetLastOptions();
        }
        GameState.Get().UpdateOptionHighlights(optionsPacket);
    }

    private void OnTurnTimerUpdate(TurnTimerUpdate update, object userData)
    {
        if (update.GetSecondsRemaining() <= Mathf.Epsilon)
        {
            this.CancelOption();
        }
    }

    private int PlayZoneSlotMousedOver(Card card)
    {
        RaycastHit hit;
        int num = 0;
        if (!UniversalInputManager.Get().GetInputHitInfo(Camera.main, GameLayer.InvisibleHitBox2, out hit))
        {
            return num;
        }
        float slotWidth = this.m_myPlayZone.GetSlotWidth();
        float num3 = this.m_myPlayZone.transform.position.x - (((this.m_myPlayZone.GetCards().Count + 1) * slotWidth) / 2f);
        num = (int) Mathf.Ceil(((hit.point.x - num3) / slotWidth) - (slotWidth / 2f));
        int count = this.m_myPlayZone.GetCards().Count;
        if ((num >= 0) && (num <= count))
        {
            return num;
        }
        if (card.transform.position.x < this.m_myPlayZone.transform.position.x)
        {
            return 0;
        }
        return count;
    }

    private void PositionHeldCard()
    {
        RaycastHit hit;
        RaycastHit hit2;
        Card heldCard = this.m_heldCard;
        Entity entity = heldCard.GetEntity();
        if (UniversalInputManager.Get().GetInputHitInfo(Camera.main, GameLayer.InvisibleHitBox2, out hit))
        {
            if (!heldCard.IsOverPlayfield())
            {
                if (!GameState.Get().HasResponse(entity))
                {
                    this.m_leftMouseButtonIsDown = false;
                    this.m_lastObjectMousedDown = null;
                    this.m_dragging = false;
                    this.DropHeldCard();
                    return;
                }
                heldCard.NotifyOverPlayfield();
            }
            if (entity.IsMinion())
            {
                int slot = this.PlayZoneSlotMousedOver(heldCard);
                if (slot != this.m_myPlayZone.GetSlotMousedOver())
                {
                    this.m_myPlayZone.SortWithSpotForHeldCard(slot);
                }
            }
        }
        else if (heldCard.IsOverPlayfield())
        {
            heldCard.NotifyLeftPlayfield();
            this.m_myPlayZone.SortWithSpotForHeldCard(-1);
        }
        if (UniversalInputManager.Get().GetInputHitInfo(Camera.main, GameLayer.DragPlane, out hit2))
        {
            heldCard.transform.position = hit2.point;
        }
    }

    public bool RegisterPhoneHandHiddenListener(PhoneHandHiddenCallback callback)
    {
        return this.RegisterPhoneHandHiddenListener(callback, null);
    }

    public bool RegisterPhoneHandHiddenListener(PhoneHandHiddenCallback callback, object userData)
    {
        PhoneHandHiddenListener item = new PhoneHandHiddenListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_phoneHandHiddenListener.Contains(item))
        {
            return false;
        }
        this.m_phoneHandHiddenListener.Add(item);
        return true;
    }

    public bool RegisterPhoneHandShownListener(PhoneHandShownCallback callback)
    {
        return this.RegisterPhoneHandShownListener(callback, null);
    }

    public bool RegisterPhoneHandShownListener(PhoneHandShownCallback callback, object userData)
    {
        PhoneHandShownListener item = new PhoneHandShownListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_phoneHandShownListener.Contains(item))
        {
            return false;
        }
        this.m_phoneHandShownListener.Add(item);
        return true;
    }

    private void RemoveMobileTargettingEffectFromActor(Actor actor)
    {
        if ((actor != null) && (actor.gameObject != null))
        {
            SceneUtils.SetLayer(actor.gameObject, GameLayer.Default);
            SceneUtils.SetLayer(actor.GetMeshRenderer().gameObject, GameLayer.CardRaycast);
            object[] args = new object[] { "x", 0f, "y", 0f, "z", 0f, "time", 0.5f, "easeType", iTween.EaseType.easeOutQuad, "name", "position", "isLocal", true };
            Hashtable hashtable = iTween.Hash(args);
            object[] objArray2 = new object[] { "x", 1f, "z", 1f, "time", 0.4f, "easeType", iTween.EaseType.easeOutQuad, "name", "scale" };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.StopByName(actor.gameObject, "position");
            iTween.StopByName(actor.gameObject, "scale");
            iTween.MoveTo(actor.gameObject, hashtable);
            iTween.ScaleTo(actor.gameObject, hashtable2);
        }
    }

    public bool RemovePhoneHandHiddenListener(PhoneHandHiddenCallback callback)
    {
        return this.RemovePhoneHandHiddenListener(callback, null);
    }

    public bool RemovePhoneHandHiddenListener(PhoneHandHiddenCallback callback, object userData)
    {
        PhoneHandHiddenListener item = new PhoneHandHiddenListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_phoneHandHiddenListener.Remove(item);
    }

    public bool RemovePhoneHandShownListener(PhoneHandShownCallback callback)
    {
        return this.RemovePhoneHandShownListener(callback, null);
    }

    public bool RemovePhoneHandShownListener(PhoneHandShownCallback callback, object userData)
    {
        PhoneHandShownListener item = new PhoneHandShownListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_phoneHandShownListener.Remove(item);
    }

    public void ResetBattlecrySourceCard()
    {
        if (this.m_battlecrySourceCard != null)
        {
            if (UniversalInputManager.Get().IsTouchMode())
            {
                string message = GameStrings.Get("GAMEPLAY_MOBILE_BATTLECRY_CANCELED");
                GameplayErrorManager.Get().DisplayMessage(message);
            }
            this.m_cancelingBattlecryCards.Add(this.m_battlecrySourceCard);
            Entity entity = this.m_battlecrySourceCard.GetEntity();
            Spell actorSpell = this.m_battlecrySourceCard.GetActorSpell(SpellType.BATTLECRY);
            if (actorSpell != null)
            {
                actorSpell.ActivateState(SpellStateType.CANCEL);
            }
            Spell playSpell = this.m_battlecrySourceCard.GetPlaySpell(true);
            if (playSpell != null)
            {
                playSpell.ActivateState(SpellStateType.CANCEL);
            }
            Spell customSummonSpell = this.m_battlecrySourceCard.GetCustomSummonSpell();
            if (customSummonSpell != null)
            {
                customSummonSpell.ActivateState(SpellStateType.CANCEL);
            }
            ZoneMgr.ChangeCompleteCallback callback = delegate (ZoneChangeList changeList, object userData) {
                Card item = (Card) userData;
                this.m_cancelingBattlecryCards.Remove(item);
            };
            ZoneMgr.Get().CancelLocalZoneChange(this.m_lastZoneChangeList, callback, this.m_battlecrySourceCard);
            this.m_lastZoneChangeList = null;
            this.ReverseManaUpdate(entity);
            this.ClearBattlecrySourceCard();
        }
    }

    public void ReturnHeldCardToHand()
    {
        if (this.m_heldCard != null)
        {
            Log.Hand.Print("ReturnHeldCardToHand()", new object[0]);
            Card heldCard = this.m_heldCard;
            heldCard.SetDoNotSort(false);
            iTween.Stop(this.m_heldCard.gameObject);
            Entity entity = heldCard.GetEntity();
            heldCard.NotifyLeftPlayfield();
            GameState.Get().GetGameEntity().NotifyOfCardDropped(entity);
            DragCardSoundEffects component = heldCard.GetComponent<DragCardSoundEffects>();
            if (component != null)
            {
                component.Disable();
            }
            UnityEngine.Object.Destroy(this.m_heldCard.GetComponent<DragRotator>());
            ProjectedShadow componentInChildren = heldCard.GetActor().GetComponentInChildren<ProjectedShadow>();
            if (componentInChildren != null)
            {
                componentInChildren.DisableShadow();
            }
            RemoteActionHandler.Get().NotifyOpponentOfCardDropped();
            if (this.m_useHandEnlarge)
            {
                this.m_myHandZone.SetFriendlyHeroTargetingMode(false);
            }
            this.m_myHandZone.UpdateLayout(this.m_myHandZone.GetLastMousedOverCard(), true);
            this.m_dragging = false;
            this.m_heldCard = null;
        }
    }

    public void ReverseManaUpdate(Entity entity)
    {
        Player friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
        int spentMana = -entity.GetRealTimeCost() + friendlySidePlayer.GetRealTimeTempMana();
        if (friendlySidePlayer.GetRealTimeTempMana() > 0)
        {
            int numCrystals = Mathf.Clamp(entity.GetRealTimeCost(), 0, friendlySidePlayer.GetRealTimeTempMana());
            friendlySidePlayer.NotifyOfUsedTempMana(-numCrystals);
            ManaCrystalMgr.Get().AddTempManaCrystals(numCrystals);
        }
        if (spentMana < 0)
        {
            friendlySidePlayer.NotifyOfSpentMana(spentMana);
            ManaCrystalMgr.Get().UpdateSpentMana(spentMana);
        }
        friendlySidePlayer.UpdateManaCounter();
    }

    public void SetHandEnlarge(bool set)
    {
        this.m_useHandEnlarge = set;
    }

    public void SetHideHandAfterPlayingCard(bool set)
    {
        this.m_hideHandAfterPlayingCard = set;
    }

    public void SetMousedOverCard(Card card)
    {
        if (this.m_mousedOverCard != card)
        {
            if ((this.m_mousedOverCard != null) && !(this.m_mousedOverCard.GetZone() is ZoneHand))
            {
                this.HandleMouseOffCard();
            }
            if (card.IsInputEnabled())
            {
                this.m_mousedOverCard = card;
                card.NotifyMousedOver();
            }
        }
    }

    private void SetShouldShowTooltip()
    {
        this.m_mousedOverTimer = 0f;
        this.m_mousedOverCard.SetShouldShowTooltip();
    }

    private void ShowBullseyeIfNeeded()
    {
        if ((TargetReticleManager.Get() != null) && TargetReticleManager.Get().IsActive())
        {
            bool show = (this.m_mousedOverCard != null) && GameState.Get().IsValidOptionTarget(this.m_mousedOverCard.GetEntity());
            TargetReticleManager.Get().ShowBullseye(show);
        }
    }

    private void ShowPhoneHand()
    {
        if (!GameState.Get().IsMulliganPhaseNowOrPending() && (this.m_useHandEnlarge && !this.m_myHandZone.HandEnlarged()))
        {
            this.m_myHandZone.AddUpdateLayoutCompleteCallback(new Zone.UpdateLayoutCompleteCallback(this.OnHandEnlargeComplete));
            this.m_myHandZone.SetHandEnlarged(true);
            foreach (PhoneHandShownListener listener in this.m_phoneHandShownListener.ToArray())
            {
                listener.Fire();
            }
        }
    }

    private void ShowSkullIfNeeded()
    {
        if (this.GetBattlecrySourceCard() == null)
        {
            Network.Options.Option.SubOption selectedNetworkSubOption = GameState.Get().GetSelectedNetworkSubOption();
            if (selectedNetworkSubOption != null)
            {
                Entity entity = GameState.Get().GetEntity(selectedNetworkSubOption.ID);
                if ((entity != null) && (entity.IsMinion() || entity.IsHero()))
                {
                    Entity entity2 = this.m_mousedOverCard.GetEntity();
                    if (((entity2.IsMinion() || entity2.IsHero()) && GameState.Get().IsValidOptionTarget(entity2)) && !entity2.IsObfuscated())
                    {
                        if ((entity2.CanBeDamaged() && (entity.GetRealTimeAttack() >= entity2.GetRealTimeRemainingHP())) || (entity.IsPoisonous() && entity2.IsMinion()))
                        {
                            Spell spell = this.m_mousedOverCard.ActivateActorSpell(SpellType.SKULL);
                            if (spell != null)
                            {
                                spell.transform.localScale = Vector3.zero;
                                object[] args = new object[] { "scale", Vector3.one, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic };
                                iTween.ScaleTo(spell.gameObject, iTween.Hash(args));
                            }
                        }
                        if ((entity.CanBeDamaged() && (entity2.GetRealTimeAttack() >= entity.GetRealTimeRemainingHP())) || (entity2.IsPoisonous() && entity.IsMinion()))
                        {
                            Spell spell2 = entity.GetCard().ActivateActorSpell(SpellType.SKULL);
                            if (spell2 != null)
                            {
                                spell2.transform.localScale = Vector3.zero;
                                object[] objArray2 = new object[] { "scale", Vector3.one, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic };
                                iTween.ScaleTo(spell2.gameObject, iTween.Hash(objArray2));
                            }
                        }
                    }
                }
            }
        }
    }

    private void ShowTooltipIfNecessary()
    {
        if ((this.m_mousedOverCard != null) && this.m_mousedOverCard.GetShouldShowTooltip())
        {
            this.m_mousedOverTimer += UnityEngine.Time.unscaledDeltaTime;
            if (this.m_mousedOverCard.IsActorReady())
            {
                if (GameState.Get().GetGameEntity().IsMouseOverDelayOverriden())
                {
                    this.m_mousedOverCard.ShowTooltip();
                }
                else if (this.m_mousedOverCard.GetZone() is ZoneHand)
                {
                    this.m_mousedOverCard.ShowTooltip();
                }
                else if (this.m_mousedOverTimer >= this.m_MouseOverDelay)
                {
                    this.m_mousedOverCard.ShowTooltip();
                }
            }
        }
    }

    private void ShowTooltipZone(GameObject hitObject, TooltipZone tooltip)
    {
        this.HideBigViewCardBacks();
        GameState state = GameState.Get();
        if (!state.IsMulliganManagerActive())
        {
            GameEntity gameEntity = state.GetGameEntity();
            if (((gameEntity != null) && !gameEntity.AreTooltipsDisabled()) && !gameEntity.NotifyOfTooltipDisplay(tooltip))
            {
                if (tooltip.targetObject.GetComponent<ManaCrystalMgr>() != null)
                {
                    if (ManaCrystalMgr.Get().ShouldShowOverloadTooltip())
                    {
                        this.ShowTooltipZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_MANA_OVERLOAD_HEADLINE"), GameStrings.Get("GAMEPLAY_TOOLTIP_MANA_OVERLOAD_DESCRIPTION"));
                    }
                    else
                    {
                        this.ShowTooltipZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_MANA_HEADLINE"), GameStrings.Get("GAMEPLAY_TOOLTIP_MANA_DESCRIPTION"));
                    }
                }
                else
                {
                    ZoneDeck component = tooltip.targetObject.GetComponent<ZoneDeck>();
                    if (component != null)
                    {
                        if (component.m_Side == Player.Side.FRIENDLY)
                        {
                            if (component.IsFatigued())
                            {
                                this.ShowTooltipZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_FATIGUE_DECK_HEADLINE"), GameStrings.Get("GAMEPLAY_TOOLTIP_FATIGUE_DECK_DESCRIPTION"));
                            }
                            else
                            {
                                object[] args = new object[] { component.GetCards().Count };
                                this.ShowTooltipZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_DECK_HEADLINE"), GameStrings.Format("GAMEPLAY_TOOLTIP_DECK_DESCRIPTION", args));
                            }
                        }
                        else if (component.m_Side == Player.Side.OPPOSING)
                        {
                            if (component.IsFatigued())
                            {
                                this.ShowTooltipZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_FATIGUE_ENEMYDECK_HEADLINE"), GameStrings.Get("GAMEPLAY_TOOLTIP_FATIGUE_ENEMYDECK_DESCRIPTION"));
                            }
                            else
                            {
                                object[] objArray2 = new object[] { component.GetCards().Count };
                                this.ShowTooltipZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYDECK_HEADLINE"), GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYDECK_DESC", objArray2));
                            }
                        }
                    }
                    else
                    {
                        ZoneHand hand = tooltip.targetObject.GetComponent<ZoneHand>();
                        if ((hand != null) && (hand.m_Side == Player.Side.OPPOSING))
                        {
                            if (GameMgr.Get().IsTutorial())
                            {
                                this.ShowTooltipZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYHAND_HEADLINE"), GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYHAND_DESC_TUT"));
                            }
                            else
                            {
                                int cardCount = hand.GetCardCount();
                                if (cardCount == 1)
                                {
                                    object[] objArray3 = new object[] { cardCount };
                                    this.ShowTooltipZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYHAND_HEADLINE"), GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYHAND_DESC_SINGLE", objArray3));
                                }
                                else
                                {
                                    object[] objArray4 = new object[] { cardCount };
                                    this.ShowTooltipZone(tooltip, GameStrings.Get("GAMEPLAY_TOOLTIP_ENEMYHAND_HEADLINE"), GameStrings.Format("GAMEPLAY_TOOLTIP_ENEMYHAND_DESC", objArray4));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void ShowTooltipZone(TooltipZone tooltip, string headline, string description)
    {
        GameState.Get().GetGameEntity().NotifyOfTooltipZoneMouseOver(tooltip);
        if (UniversalInputManager.Get().IsTouchMode())
        {
            tooltip.ShowGameplayTooltipLarge(headline, description);
        }
        else
        {
            tooltip.ShowGameplayTooltip(headline, description);
        }
    }

    private void Start()
    {
        if (GameState.Get() != null)
        {
            GameState.Get().RegisterOptionsReceivedListener(new GameState.OptionsReceivedCallback(this.OnOptionsReceived));
            GameState.Get().RegisterTurnTimerUpdateListener(new GameState.TurnTimerUpdateCallback(this.OnTurnTimerUpdate));
            GameState.Get().RegisterSpectatorNotifyListener(new GameState.SpectatorNotifyEventCallback(this.OnSpectatorNotifyEvent), null);
            GameState.Get().RegisterGameOverListener(new GameState.GameOverCallback(this.OnGameOver), null);
        }
    }

    private void StartBattleCryEffect(Entity entity)
    {
        this.m_isInBattleCryEffect = true;
        Network.Options.Option selectedNetworkOption = GameState.Get().GetSelectedNetworkOption();
        if (selectedNetworkOption == null)
        {
            UnityEngine.Debug.LogError("No targets for BattleCry.");
        }
        else
        {
            this.StartMobileTargetingEffect(selectedNetworkOption.Main.Targets);
            this.m_battlecrySourceCard.SetBattleCrySource(true);
        }
    }

    private void StartDoubleClick(ClickHandler singleClick, ClickHandler doubleClick, object clickObject)
    {
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        if (this.m_doubleClickActive && ((realtimeSinceStartup - this.m_clickStartTime) < 0.175f))
        {
            this.m_doubleClickHandler(this.m_clickObject);
            base.StopCoroutine("FireSingleClickAfterDelay");
        }
        else
        {
            this.m_doubleClickActive = true;
            this.m_clickStartTime = realtimeSinceStartup;
            this.m_singleClickHandler = singleClick;
            this.m_doubleClickHandler = doubleClick;
            this.m_clickObject = clickObject;
            if (this.m_singleClickHandler != null)
            {
                base.StartCoroutine("FireSingleClickAfterDelay");
            }
        }
    }

    private void StartMobileTargetingEffect(List<int> targets)
    {
        if ((targets != null) && (targets.Count != 0))
        {
            this.m_mobileTargettingEffectActors.Clear();
            foreach (int num in targets)
            {
                Entity entity = GameState.Get().GetEntity(num);
                if (entity.GetCard() != null)
                {
                    Actor item = entity.GetCard().GetActor();
                    this.m_mobileTargettingEffectActors.Add(item);
                    this.ApplyMobileTargettingEffectToActor(item);
                }
            }
            FullScreenFXMgr.Get().Desaturate(0.9f, 0.4f, iTween.EaseType.easeInOutQuad, null);
        }
    }

    public void StartWatchingForInput()
    {
        if (!this.m_checkForInput)
        {
            this.m_checkForInput = true;
            foreach (Zone zone in ZoneMgr.Get().GetZones())
            {
                if (zone.m_Side == Player.Side.FRIENDLY)
                {
                    if (zone is ZoneHand)
                    {
                        this.m_myHandZone = (ZoneHand) zone;
                    }
                    else if (zone is ZonePlay)
                    {
                        this.m_myPlayZone = (ZonePlay) zone;
                    }
                    else if (zone is ZoneWeapon)
                    {
                        this.m_myWeaponZone = (ZoneWeapon) zone;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (this.m_checkForInput)
        {
            if (UniversalInputManager.Get().GetMouseButtonDown(0))
            {
                this.HandleLeftMouseDown();
            }
            if (UniversalInputManager.Get().GetMouseButtonUp(0))
            {
                this.m_touchDraggingCard = false;
                this.HandleLeftMouseUp();
            }
            if (UniversalInputManager.Get().GetMouseButtonDown(1))
            {
                this.HandleRightMouseDown();
            }
            if (UniversalInputManager.Get().GetMouseButtonUp(1))
            {
                this.HandleRightMouseUp();
            }
            this.HandleMouseMove();
            if (this.m_leftMouseButtonIsDown && (this.m_heldCard == null))
            {
                this.HandleUpdateWhileLeftMouseButtonIsDown();
                if (UniversalInputManager.Get().IsTouchMode() && !this.m_touchDraggingCard)
                {
                    this.HandleUpdateWhileNotHoldingCard();
                }
            }
            else if (this.m_heldCard == null)
            {
                this.HandleUpdateWhileNotHoldingCard();
            }
            if (((GameState.Get() == null) || (GameState.Get().GetFriendlySidePlayer() == null)) || GameState.Get().GetFriendlySidePlayer().IsLocalUser())
            {
                bool hitBattlefield = UniversalInputManager.Get().InputHitAnyObject(Camera.main, GameLayer.InvisibleHitBox2);
                if ((TargetReticleManager.Get() != null) && TargetReticleManager.Get().IsActive())
                {
                    if (((!hitBattlefield && (this.GetBattlecrySourceCard() == null)) && (ChoiceCardMgr.Get().GetSubOptionParentCard() == null)) && ((UniversalInputManager.UsePhoneUI == null) || (!this.m_targettingHeroPower && !GameState.Get().IsSelectedOptionFriendlyHero())))
                    {
                        this.CancelOption();
                        if (this.m_useHandEnlarge)
                        {
                            this.m_myHandZone.SetFriendlyHeroTargetingMode(false);
                        }
                        if (this.m_heldCard != null)
                        {
                            this.PositionHeldCard();
                        }
                    }
                    else
                    {
                        TargetReticleManager.Get().UpdateArrowPosition();
                    }
                }
                else if (this.m_heldCard != null)
                {
                    this.HandleUpdateWhileHoldingCard(hitBattlefield);
                }
            }
            if ((EmoteHandler.Get() != null) && EmoteHandler.Get().AreEmotesActive())
            {
                EmoteHandler.Get().HandleInput();
            }
            if ((EnemyEmoteHandler.Get() != null) && EnemyEmoteHandler.Get().AreEmotesActive())
            {
                EnemyEmoteHandler.Get().HandleInput();
            }
            this.ShowTooltipIfNecessary();
        }
    }

    public bool UseHandEnlarge()
    {
        return this.m_useHandEnlarge;
    }

    public Vector3 LastMouseDownPosition
    {
        get
        {
            return this.m_lastMouseDownPosition;
        }
    }

    public bool LeftMouseButtonDown
    {
        get
        {
            return this.m_leftMouseButtonIsDown;
        }
    }

    [CompilerGenerated]
    private sealed class <FinishSpectatorNotify_SubOption>c__Iterator9E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Network.Options.Option <$>chosenOption;
        internal ChooseOption <$>echoPacket;
        internal uint <$>myToken;
        internal SpectatorNotify <$>notify;
        internal Entity <$>subEntity;
        internal List<Card>.Enumerator <$s_578>__4;
        internal List<Card>.Enumerator <$s_579>__7;
        internal InputManager <>f__this;
        internal List<Card> <actualChoiceCards>__2;
        internal Card <card>__5;
        internal Card <card>__8;
        internal List<Card> <choiceCards>__3;
        internal int <intendedOptionPacketId>__1;
        internal Network.Options <lastOptions>__9;
        internal GameState <state>__0;
        internal Entity <targetEntity>__6;
        internal Network.Options.Option chosenOption;
        internal ChooseOption echoPacket;
        internal uint myToken;
        internal SpectatorNotify notify;
        internal Entity subEntity;

        internal bool <>m__FB(Card c)
        {
            return !this.<state>__0.IsEntityInputEnabled(c.GetEntity());
        }

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
                    this.<state>__0 = GameState.Get();
                    this.<intendedOptionPacketId>__1 = this.echoPacket.Id;
                    break;

                case 1:
                    if ((((ChoiceCardMgr.Get() != null) && ChoiceCardMgr.Get().HasSubOption()) && ((this.myToken == this.<>f__this.m_spectatorNotifyCurrentToken) && (this.<state>__0.GetOptionsPacket() != null))) && (this.<state>__0.GetOptionsPacket().ID == this.<intendedOptionPacketId>__1))
                    {
                        break;
                    }
                    goto Label_044A;

                case 2:
                    goto Label_01B5;

                case 3:
                    if (this.subEntity.GetCard() != null)
                    {
                        this.subEntity.GetCard().SetInputEnabled(true);
                    }
                    this.<state>__0 = GameState.Get();
                    if (((this.<state>__0 != null) && !this.<state>__0.IsGameOver()) && (((this.myToken == this.<>f__this.m_spectatorNotifyCurrentToken) && (this.<state>__0.GetOptionsPacket() != null)) && (this.<state>__0.GetOptionsPacket().ID <= (this.<intendedOptionPacketId>__1 + 1))))
                    {
                        this.<>f__this.HandleClickOnSubOption(this.subEntity, true);
                        if (this.<state>__0.GetOptionsPacket().ID == this.<intendedOptionPacketId>__1)
                        {
                            this.<>f__this.OnSpectatorNotifyEvent_UpdateHighlights();
                        }
                        else if (this.<state>__0.GetLastOptions() != null)
                        {
                            this.<lastOptions>__9 = this.<state>__0.GetLastOptions();
                            this.<state>__0.UpdateOptionHighlights(this.<lastOptions>__9);
                        }
                        this.$PC = -1;
                    }
                    else
                    {
                        this.<$s_579>__7 = this.<choiceCards>__3.GetEnumerator();
                        try
                        {
                            while (this.<$s_579>__7.MoveNext())
                            {
                                this.<card>__8 = this.<$s_579>__7.Current;
                                this.<card>__8.HideCard();
                            }
                        }
                        finally
                        {
                            this.<$s_579>__7.Dispose();
                        }
                        this.<>f__this.OnSpectatorNotifyEvent_UpdateHighlights();
                    }
                    goto Label_044A;

                default:
                    goto Label_044A;
            }
            if (ChoiceCardMgr.Get().IsWaitingToShowSubOptions())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_044C;
            }
            this.<actualChoiceCards>__2 = ChoiceCardMgr.Get().GetFriendlyCards();
            this.<choiceCards>__3 = new List<Card>(this.<actualChoiceCards>__2);
        Label_01B5:
            while (Enumerable.Any<Card>(this.<choiceCards>__3, new Func<Card, bool>(this.<>m__FB)))
            {
                if (((this.myToken != this.<>f__this.m_spectatorNotifyCurrentToken) || (this.<state>__0.GetOptionsPacket() == null)) || (this.<state>__0.GetOptionsPacket().ID > (this.<intendedOptionPacketId>__1 + 1)))
                {
                    this.<$s_578>__4 = this.<choiceCards>__3.GetEnumerator();
                    try
                    {
                        while (this.<$s_578>__4.MoveNext())
                        {
                            this.<card>__5 = this.<$s_578>__4.Current;
                            this.<card>__5.HideCard();
                        }
                    }
                    finally
                    {
                        this.<$s_578>__4.Dispose();
                    }
                    this.<>f__this.OnSpectatorNotifyEvent_UpdateHighlights();
                    goto Label_044A;
                }
                this.$current = null;
                this.$PC = 2;
                goto Label_044C;
            }
            this.<targetEntity>__6 = null;
            if (this.echoPacket.Target > 0)
            {
                this.<targetEntity>__6 = this.<state>__0.GetEntity(this.echoPacket.Target);
                if (this.<targetEntity>__6 == null)
                {
                    object[] args = new object[] { this.notify.PlayerId, this.echoPacket.Id, this.chosenOption.Main.ID, this.subEntity.GetEntityId(), this.echoPacket.Target };
                    Log.Power.Print("Spectator received unknown target entity in ChooseOption SubOption playerId={0} receivedId={1} mainEntityId={2} subEntityId={3} targetEntityId={4}", args);
                    this.<>f__this.OnSpectatorNotifyEvent_UpdateHighlights();
                    goto Label_044A;
                }
            }
            if (this.subEntity.GetCard() != null)
            {
                this.subEntity.GetCard().SetInputEnabled(false);
            }
            this.$current = new WaitForSeconds(1f);
            this.$PC = 3;
            goto Label_044C;
        Label_044A:
            return false;
        Label_044C:
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
    private sealed class <FireSingleClickAfterDelay>c__Iterator9D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal InputManager <>f__this;

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
                    this.$current = new WaitForSeconds(0.175f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_doubleClickActive = false;
                    if (this.<>f__this.m_singleClickHandler != null)
                    {
                        this.<>f__this.m_singleClickHandler(this.<>f__this.m_clickObject);
                    }
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

    private delegate void ClickHandler(object obj);

    public delegate void PhoneHandHiddenCallback(object userData);

    private class PhoneHandHiddenListener : EventListener<InputManager.PhoneHandHiddenCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    public delegate void PhoneHandShownCallback(object userData);

    private class PhoneHandShownListener : EventListener<InputManager.PhoneHandShownCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }
}

