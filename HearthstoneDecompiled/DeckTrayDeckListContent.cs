using PegasusShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class DeckTrayDeckListContent : DeckTrayContent
{
    private const float DECK_BUTTON_ROTATION_TIME = 0.1f;
    private const float DELETE_DECK_ANIM_TIME = 0.5f;
    private bool m_animatingExit;
    private List<BusyWithDeck> m_busyWithDeckListeners = new List<BusyWithDeck>();
    private int m_centeringDeckList = -1;
    private bool m_creatingNewDeck;
    [SerializeField]
    private Vector3 m_deckButtonOffset;
    private List<DeckCountChanged> m_deckCountChangedListeners = new List<DeckCountChanged>();
    [CustomEditField(Sections="Deck Tray Settings")]
    public Transform m_deckEditTopPos;
    [CustomEditField(Sections="Prefabs", T=EditType.GAME_OBJECT)]
    public string m_deckInfoActorPrefab;
    private CollectionDeckInfo m_deckInfoTooltip;
    [CustomEditField(Sections="Deck Tray Settings")]
    public GameObject m_deckInfoTooltipBone;
    private List<long> m_decksToDelete = new List<long>();
    [CustomEditField(Sections="Deck Button Settings")]
    public ParticleSystem m_deleteDeckPoof;
    private bool m_deletingDecks;
    private TraySection m_editingTraySection;
    private bool m_flagCreateNewDeck;
    private bool m_initialized;
    [CustomEditField(Sections="Deck Button Settings")]
    public CollectionNewDeckButton m_newDeckButton;
    [CustomEditField(Sections="Deck Button Settings")]
    public GameObject m_newDeckButtonContainer;
    private TraySection m_newlyCreatedTraySection;
    private string m_previousDeckName;
    [CustomEditField(Sections="Scroll Settings")]
    public UIBScrollable m_scrollbar;
    [CustomEditField(Sections="Prefabs")]
    public TraySection m_traySectionPrefab;
    private List<TraySection> m_traySections = new List<TraySection>();
    [CustomEditField(Sections="Deck Tray Settings")]
    public Transform m_traySectionStartPos;
    private bool m_waitingToDeleteDeck;
    private bool m_wasTouchModeEnabled;
    private const int MAX_NUM_DECKBOXES_AVAILABLE = 9;
    private const int NUM_DECKBOXES_TO_DISPLAY = 11;
    private const float TIME_BETWEEN_TRAY_DOOR_ANIMS = 0.015f;

    public override bool AnimateContentEntranceEnd()
    {
        if (this.m_editingTraySection != null)
        {
            return false;
        }
        if (this.m_flagCreateNewDeck)
        {
            this.StartCreateNewDeck();
        }
        else
        {
            this.ShowNewDeckButton(this.CanShowNewDeckButton(), null);
        }
        this.FireBusyWithDeckEvent(false);
        this.DeleteQueuedDecks(true);
        return true;
    }

    public override bool AnimateContentEntranceStart()
    {
        this.Initialize();
        this.UpdateAllTrays(SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL);
        if (this.m_editingTraySection != null)
        {
            this.FinishRenamingEditingDeck(null);
            this.m_editingTraySection.MoveDeckBoxBackToOriginalPosition(0.25f, delegate (object o) {
                this.m_editingTraySection = null;
            });
        }
        this.m_newDeckButton.SetIsUsable(this.CanShowNewDeckButton());
        this.FireBusyWithDeckEvent(true);
        this.FireDeckCountChangedEvent();
        CollectionManager.Get().DoneEditing();
        return true;
    }

    public override bool AnimateContentExitEnd()
    {
        return !this.m_animatingExit;
    }

    public override bool AnimateContentExitStart()
    {
        <AnimateContentExitStart>c__AnonStorey36F storeyf = new <AnimateContentExitStart>c__AnonStorey36F {
            <>f__this = this
        };
        this.m_animatingExit = true;
        this.FireBusyWithDeckEvent(true);
        float? speed = null;
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL)
        {
            speed = 500f;
        }
        this.ShowNewDeckButton(false, speed, null);
        storeyf.animationWaitTime = 0.5f;
        ApplicationMgr.Get().ScheduleCallback(0.5f, false, new ApplicationMgr.ScheduledCallback(storeyf.<>m__23E), null);
        return true;
    }

    private void Awake()
    {
        CollectionManager.Get().RegisterFavoriteHeroChangedListener(new CollectionManager.FavoriteHeroChangedCallback(this.OnFavoriteHeroChanged));
    }

    public void CancelRenameEditingDeck()
    {
        this.FinishRenamingEditingDeck(null);
    }

    public bool CanShowNewDeckButton()
    {
        return (CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK).Count < 9);
    }

    public void CreateNewDeckCancelled()
    {
        this.EndCreateNewDeck(false);
    }

    public void CreateNewDeckFromUserSelection(TAG_CLASS heroClass, string heroCardID, string customDeckName = null)
    {
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CM_NEW_DECK_CREATED);
        bool flag = SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL;
        DeckType deckType = DeckType.NORMAL_DECK;
        string str = customDeckName;
        if (flag)
        {
            deckType = DeckType.TAVERN_BRAWL_DECK;
            str = GameStrings.Get("GLUE_COLLECTION_TAVERN_BRAWL_DECKNAME");
        }
        else if (string.IsNullOrEmpty(str))
        {
            str = CollectionManager.Get().AutoGenerateDeckName(heroClass);
        }
        CollectionManager.Get().SendCreateDeck(deckType, str, heroCardID);
        this.EndCreateNewDeck(true);
    }

    private void CreateTraySections()
    {
        Vector3 localScale = this.m_traySectionStartPos.localScale;
        Vector3 localEulerAngles = this.m_traySectionStartPos.localEulerAngles;
        GameObject gameObject = this.m_traySectionStartPos.gameObject;
        for (int i = 0; i < 11; i++)
        {
            <CreateTraySections>c__AnonStorey370 storey = new <CreateTraySections>c__AnonStorey370 {
                <>f__this = this,
                traySection = (TraySection) GameUtils.Instantiate(this.m_traySectionPrefab, base.gameObject, false)
            };
            storey.traySection.transform.localScale = localScale;
            storey.traySection.transform.localEulerAngles = localEulerAngles;
            if (i == 0)
            {
                storey.traySection.transform.localPosition = this.m_traySectionStartPos.localPosition;
            }
            else
            {
                TransformUtil.SetPoint(storey.traySection.gameObject, Anchor.FRONT, gameObject, Anchor.BACK);
            }
            Material material = null;
            foreach (Material material2 in storey.traySection.m_door.GetComponent<Renderer>().materials)
            {
                if (material2.name.Equals("DeckTray", StringComparison.OrdinalIgnoreCase) || material2.name.Equals("DeckTray (Instance)", StringComparison.OrdinalIgnoreCase))
                {
                    material = material2;
                    break;
                }
            }
            UnityEngine.Vector2 vector3 = new UnityEngine.Vector2(0f, -0.0825f * i);
            storey.traySection.GetComponent<Renderer>().material.mainTextureOffset = vector3;
            if (material != null)
            {
                material.mainTextureOffset = vector3;
            }
            gameObject = storey.traySection.gameObject;
            storey.deckBox = storey.traySection.m_deckBox;
            storey.deckBox.SetPositionIndex(i);
            storey.deckBox.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(storey.<>m__245));
            storey.deckBox.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(storey.<>m__246));
            storey.deckBox.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(storey.<>m__247));
            storey.deckBox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storey.<>m__248));
            storey.deckBox.SetOriginalButtonPosition();
            storey.deckBox.HideBanner();
            this.m_traySections.Add(storey.traySection);
        }
    }

    public void DeleteDeck(long deckID)
    {
        this.m_decksToDelete.Add(deckID);
        CollectionDeck deck = CollectionManager.Get().GetDeck(deckID);
        if (deck != null)
        {
            deck.MarkBeingDeleted();
        }
        this.DeleteQueuedDecks(false);
    }

    [DebuggerHidden]
    private IEnumerator DeleteDeckAnimation(long deckID, System.Action callback = null)
    {
        return new <DeleteDeckAnimation>c__Iterator256 { deckID = deckID, callback = callback, <$>deckID = deckID, <$>callback = callback, <>f__this = this };
    }

    public void DeleteEditingDeck()
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        if (taggedDeck == null)
        {
            UnityEngine.Debug.LogWarning("No deck currently being edited!");
        }
        else
        {
            this.m_waitingToDeleteDeck = true;
            this.DeleteDeck(taggedDeck.ID);
        }
    }

    private void DeleteQueuedDecks(bool force = false)
    {
        if ((this.m_decksToDelete.Count != 0) && (base.IsModeActive() || force))
        {
            foreach (long num in this.m_decksToDelete)
            {
                CollectionManager.Get().SendDeleteDeck(num);
            }
            this.m_decksToDelete.Clear();
        }
    }

    private void EndCreateNewDeck(bool newDeck)
    {
        <EndCreateNewDeck>c__AnonStorey372 storey = new <EndCreateNewDeck>c__AnonStorey372 {
            newDeck = newDeck,
            <>f__this = this
        };
        this.m_creatingNewDeck = false;
        CollectionManagerDisplay.Get().ExitSelectNewDeckHeroMode();
        this.ShowNewDeckButton(true, new CollectionNewDeckButton.DelOnAnimationFinished(storey.<>m__24B));
    }

    private void FinishRenamingEditingDeck(string newDeckName = null)
    {
        if (this.m_editingTraySection != null)
        {
            CollectionDeckBoxVisual deckBox = this.m_editingTraySection.m_deckBox;
            CollectionDeck deck = this.UpdateRenamingEditingDeck(newDeckName);
            if ((deck != null) && (this.m_editingTraySection != null))
            {
                deckBox.SetDeckName(deck.Name);
            }
            if ((UniversalInputManager.Get() != null) && UniversalInputManager.Get().IsTextInputActive())
            {
                UniversalInputManager.Get().CancelTextInput(base.gameObject, false);
            }
            deckBox.ShowDeckName();
        }
    }

    private void FireBusyWithDeckEvent(bool busy)
    {
        foreach (BusyWithDeck deck in this.m_busyWithDeckListeners.ToArray())
        {
            deck(busy);
        }
    }

    private void FireDeckCountChangedEvent()
    {
        DeckCountChanged[] changedArray = this.m_deckCountChangedListeners.ToArray();
        int count = CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK).Count;
        foreach (DeckCountChanged changed in changedArray)
        {
            changed(count);
        }
    }

    public void FlagCreateNewDeck()
    {
        this.m_flagCreateNewDeck = true;
    }

    private TraySection GetExistingTrayFromDeck(CollectionDeck deck)
    {
        return this.GetExistingTrayFromDeck(deck.ID);
    }

    private TraySection GetExistingTrayFromDeck(long deckID)
    {
        foreach (TraySection section in this.m_traySections)
        {
            if (section.m_deckBox.GetDeckID() == deckID)
            {
                return section;
            }
        }
        return null;
    }

    private void GetIdealNewDeckButtonLocalPosition(TraySection setNewDeckButtonPosition, out Vector3 outPosition, out bool outActive)
    {
        TraySection lastUnusedTraySection = this.GetLastUnusedTraySection();
        TraySection section2 = (setNewDeckButtonPosition != null) ? setNewDeckButtonPosition : lastUnusedTraySection;
        outActive = lastUnusedTraySection != null;
        outPosition = ((section2 == null) ? this.m_traySectionStartPos.localPosition : section2.transform.localPosition) + this.m_deckButtonOffset;
    }

    private TraySection GetLastUnusedTraySection()
    {
        int num = 0;
        foreach (TraySection section in this.m_traySections)
        {
            if (num >= 9)
            {
                break;
            }
            if (section.m_deckBox.GetDeckID() == -1L)
            {
                return section;
            }
            num++;
        }
        return null;
    }

    private TraySection GetLastUsedTraySection()
    {
        int num = 0;
        TraySection section = null;
        foreach (TraySection section2 in this.m_traySections)
        {
            if (num >= 9)
            {
                break;
            }
            if (section2.m_deckBox.GetDeckID() == -1L)
            {
                return section;
            }
            section = section2;
            num++;
        }
        return null;
    }

    public Vector3 GetNewDeckButtonPosition()
    {
        return this.m_newDeckButton.transform.position;
    }

    private int GetTotalDeckBoxesInUse()
    {
        int num = 0;
        foreach (TraySection section in this.m_traySections)
        {
            if (section.m_deckBox.IsShown())
            {
                num++;
            }
        }
        return num;
    }

    private void Initialize()
    {
        if (!this.m_initialized)
        {
            this.m_newDeckButton.AddEventListener(UIEventType.RELEASE, e => this.OnNewDeckButtonPress());
            CollectionManager.Get().RegisterDeckDeletedListener(new CollectionManager.DelOnDeckDeleted(this.OnDeckDeleted));
            GameObject obj2 = AssetLoader.Get().LoadActor(FileUtils.GameAssetPathToName(this.m_deckInfoActorPrefab), false, false);
            if (obj2 == null)
            {
                UnityEngine.Debug.LogError(string.Format("Unable to load actor {0}: null", this.m_deckInfoActorPrefab), base.gameObject);
            }
            else
            {
                this.m_deckInfoTooltip = obj2.GetComponent<CollectionDeckInfo>();
                if (this.m_deckInfoTooltip == null)
                {
                    UnityEngine.Debug.LogError(string.Format("Actor {0} does not contain CollectionDeckInfo component.", this.m_deckInfoActorPrefab), base.gameObject);
                }
                else
                {
                    GameUtils.SetParent(this.m_deckInfoTooltip, this.m_deckInfoTooltipBone, false);
                    this.m_deckInfoTooltip.Hide();
                    this.m_deckInfoTooltip.RegisterHideListener(delegate {
                        if (UniversalInputManager.Get().IsTouchMode())
                        {
                            if (this.m_editingTraySection != null)
                            {
                                this.m_editingTraySection.m_deckBox.SetHighlightState(ActorStateType.NONE);
                                this.m_editingTraySection.m_deckBox.ShowDeckName();
                            }
                            this.FinishRenamingEditingDeck(null);
                        }
                    });
                    this.CreateTraySections();
                    this.m_initialized = true;
                }
            }
        }
    }

    private void InitializeTraysFromDecks()
    {
        if ((this.UpdateDeckTrayVisuals() > 0) && !Options.Get().GetBool(Option.HAS_STARTED_A_DECK, false))
        {
            Options.Get().SetBool(Option.HAS_STARTED_A_DECK, true);
        }
    }

    private bool IsEditingCards()
    {
        return (CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing) != null);
    }

    public bool IsWaitingToDeleteDeck()
    {
        return this.m_waitingToDeleteDeck;
    }

    private void OnDeckBoxVisualOut(CollectionDeckBoxVisual deckBox)
    {
        if (UniversalInputManager.Get().IsTouchMode())
        {
            if ((this.m_deckInfoTooltip != null) && this.m_deckInfoTooltip.IsShown())
            {
                deckBox.SetHighlightState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
            }
        }
        else
        {
            if (!UniversalInputManager.Get().InputIsOver(deckBox.m_deleteButton.gameObject))
            {
                deckBox.ShowDeleteButton(false);
            }
            if (this.m_deckInfoTooltip != null)
            {
                this.m_deckInfoTooltip.Hide();
            }
        }
    }

    private void OnDeckBoxVisualOver(CollectionDeckBoxVisual deckBox)
    {
        if (!UniversalInputManager.Get().IsTouchMode())
        {
            if (this.IsEditingCards() && (this.m_deckInfoTooltip != null))
            {
                this.m_deckInfoTooltip.UpdateManaCurve();
                this.m_deckInfoTooltip.Show();
            }
            else if (base.IsModeTryingOrActive())
            {
                deckBox.ShowDeleteButton(true);
            }
        }
    }

    private void OnDeckBoxVisualPress(CollectionDeckBoxVisual deckBox)
    {
        deckBox.enabled = false;
    }

    private void OnDeckBoxVisualRelease(TraySection traySection)
    {
        if (!this.m_creatingNewDeck)
        {
            CollectionDeckBoxVisual deckBox = traySection.m_deckBox;
            deckBox.enabled = true;
            if ((this.m_scrollbar == null) || !this.m_scrollbar.IsTouchDragging())
            {
                long deckID = deckBox.GetDeckID();
                CollectionDeck deck = CollectionManager.Get().GetDeck(deckID);
                if (deck.IsBeingDeleted())
                {
                    Log.JMac.Print(string.Format("CollectionDeckTray.DeckBoxVisualPress(): cannot edit deck {0}; it is being deleted", deck), new object[0]);
                }
                else if (deck.IsSavingChanges())
                {
                    Log.Rachelle.Print(string.Format("CollectionDeckTray.DeckBoxVisualPress(): cannot edit deck {0}; waiting for changes to be saved", deck), new object[0]);
                }
                else if (this.IsEditingCards())
                {
                    if (!UniversalInputManager.Get().IsTouchMode())
                    {
                        this.RenameCurrentlyEditingDeck();
                    }
                    else if ((this.m_deckInfoTooltip != null) && !this.m_deckInfoTooltip.IsShown())
                    {
                        this.m_deckInfoTooltip.UpdateManaCurve();
                        this.m_deckInfoTooltip.Show();
                    }
                }
                else if (base.IsModeActive())
                {
                    this.m_editingTraySection = traySection;
                    this.m_centeringDeckList = this.m_editingTraySection.m_deckBox.GetPositionIndex();
                    CollectionManagerDisplay.Get().RequestContentsToShowDeck(deckID);
                    this.ShowNewDeckButton(false, null);
                }
            }
        }
    }

    private void OnDeckDeleted(long deckID)
    {
        this.m_waitingToDeleteDeck = false;
        base.StartCoroutine(this.DeleteDeckAnimation(deckID, null));
    }

    private void OnDestroy()
    {
        CollectionManager manager = CollectionManager.Get();
        manager.RemoveFavoriteHeroChangedListener(new CollectionManager.FavoriteHeroChangedCallback(this.OnFavoriteHeroChanged));
        manager.RemoveDeckDeletedListener(new CollectionManager.DelOnDeckDeleted(this.OnDeckDeleted));
    }

    private void OnDrawGizmos()
    {
        if (this.m_editingTraySection != null)
        {
            Bounds bounds = this.m_editingTraySection.m_deckBox.GetDeckNameText().GetBounds();
            Gizmos.DrawWireSphere(bounds.min, 0.1f);
            Gizmos.DrawWireSphere(bounds.max, 0.1f);
        }
    }

    private void OnFavoriteHeroChanged(TAG_CLASS heroClass, NetCache.CardDefinition favoriteHero, object userData)
    {
        this.UpdateDeckTrayVisuals();
    }

    private void OnNewDeckButtonPress()
    {
        if (base.IsModeActive() && ((this.m_scrollbar == null) || !this.m_scrollbar.IsTouchDragging()))
        {
            SoundManager.Get().LoadAndPlay("Hub_Click");
            this.StartCreateNewDeck();
        }
    }

    public override void OnTaggedDeckChanged(CollectionManager.DeckTag tag, CollectionDeck newDeck, CollectionDeck oldDeck, bool isNewDeck)
    {
        if (tag == CollectionManager.DeckTag.Editing)
        {
            if ((newDeck != null) && (this.m_deckInfoTooltip != null))
            {
                this.m_deckInfoTooltip.SetDeck(newDeck);
            }
            if (base.IsModeActive())
            {
                this.InitializeTraysFromDecks();
            }
            if (isNewDeck && (newDeck != null))
            {
                this.m_newlyCreatedTraySection = this.GetExistingTrayFromDeck(newDeck);
                if (this.m_newlyCreatedTraySection != null)
                {
                    this.m_centeringDeckList = this.m_newlyCreatedTraySection.m_deckBox.GetPositionIndex();
                }
            }
        }
    }

    public override bool PostAnimateContentExit()
    {
        base.StartCoroutine(this.ShowTrayDoors(false));
        return true;
    }

    public override bool PreAnimateContentEntrance()
    {
        base.StartCoroutine(this.ShowTrayDoors(true));
        return true;
    }

    public override bool PreAnimateContentExit()
    {
        if (this.m_scrollbar == null)
        {
            return true;
        }
        if (this.m_centeringDeckList != -1)
        {
            float num2;
            int num = this.GetTotalDeckBoxesInUse() - 1;
            if (this.m_scrollbar.IsEnabledAndScrollable() && (num > 0))
            {
                num2 = ((float) this.m_centeringDeckList) / ((float) num);
            }
            else
            {
                num2 = 0f;
            }
            this.m_scrollbar.SetScroll(num2, delegate (float f) {
                this.m_animatingExit = false;
            }, iTween.EaseType.linear, this.m_scrollbar.m_ScrollTweenTime, true, true);
            this.m_animatingExit = true;
            this.m_centeringDeckList = -1;
        }
        return !this.m_animatingExit;
    }

    public void RegisterBusyWithDeck(BusyWithDeck dlg)
    {
        this.m_busyWithDeckListeners.Add(dlg);
    }

    public void RegisterDeckCountUpdated(DeckCountChanged dlg)
    {
        this.m_deckCountChangedListeners.Add(dlg);
    }

    public void RenameCurrentlyEditingDeck()
    {
        if (this.m_editingTraySection == null)
        {
            UnityEngine.Debug.LogWarning("Unable to rename deck. No deck currently being edited.", base.gameObject);
        }
        else if (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL)
        {
            CollectionDeckBoxVisual deckBox = this.m_editingTraySection.m_deckBox;
            deckBox.HideDeckName();
            Camera camera = Box.Get().GetCamera();
            Bounds bounds = deckBox.GetDeckNameText().GetBounds();
            Rect rect = CameraUtils.CreateGUIViewportRect(camera, bounds.min, bounds.max);
            Font localizedFont = deckBox.GetDeckNameText().GetLocalizedFont();
            this.m_previousDeckName = deckBox.GetDeckNameText().Text;
            UniversalInputManager.TextInputParams parms = new UniversalInputManager.TextInputParams {
                m_owner = base.gameObject,
                m_rect = rect,
                m_updatedCallback = newName => this.UpdateRenamingEditingDeck(newName),
                m_completedCallback = newName => this.FinishRenamingEditingDeck(newName),
                m_canceledCallback = (a1, a2) => this.FinishRenamingEditingDeck(this.m_previousDeckName),
                m_maxCharacters = 0x18,
                m_font = localizedFont,
                m_text = deckBox.GetDeckNameText().Text
            };
            UniversalInputManager.Get().UseTextInput(parms, false);
        }
    }

    public void SetEditingTraySection(int index)
    {
        this.m_editingTraySection = this.m_traySections[index];
        this.m_centeringDeckList = this.m_editingTraySection.m_deckBox.GetPositionIndex();
    }

    public void ShowNewDeckButton(bool newDeckButtonActive, CollectionNewDeckButton.DelOnAnimationFinished callback = null)
    {
        this.ShowNewDeckButton(newDeckButtonActive, null, callback);
    }

    public void ShowNewDeckButton(bool newDeckButtonActive, float? speed, CollectionNewDeckButton.DelOnAnimationFinished callback = null)
    {
        <ShowNewDeckButton>c__AnonStorey371 storey = new <ShowNewDeckButton>c__AnonStorey371 {
            callback = callback,
            <>f__this = this
        };
        if (this.m_newDeckButton.gameObject.activeSelf != newDeckButtonActive)
        {
            if (newDeckButtonActive)
            {
                this.m_newDeckButton.gameObject.SetActive(true);
                this.m_newDeckButton.PlayPopUpAnimation(new CollectionNewDeckButton.DelOnAnimationFinished(storey.<>m__249), null, speed);
            }
            else
            {
                this.m_newDeckButton.PlayPopDownAnimation(new CollectionNewDeckButton.DelOnAnimationFinished(storey.<>m__24A), null, speed);
            }
        }
        else if (storey.callback != null)
        {
            storey.callback(this);
        }
    }

    [DebuggerHidden]
    public IEnumerator ShowTrayDoors(bool show)
    {
        return new <ShowTrayDoors>c__Iterator254 { show = show, <$>show = show, <>f__this = this };
    }

    private void StartCreateNewDeck()
    {
        this.m_flagCreateNewDeck = false;
        Enum[] args = new Enum[] { PresenceStatus.DECKEDITOR };
        PresenceMgr.Get().SetStatus(args);
        this.ShowNewDeckButton(false, null);
        this.m_creatingNewDeck = true;
        CollectionManagerDisplay.Get().EnterSelectNewDeckHeroMode();
    }

    public void UnregisterBusyWithDeck(BusyWithDeck dlg)
    {
        this.m_busyWithDeckListeners.Remove(dlg);
    }

    public void UnregisterDeckCountUpdated(DeckCountChanged dlg)
    {
        this.m_deckCountChangedListeners.Remove(dlg);
    }

    private void Update()
    {
        if (this.m_wasTouchModeEnabled != UniversalInputManager.Get().IsTouchMode())
        {
            this.m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
            if (UniversalInputManager.Get().IsTouchMode() && (this.m_deckInfoTooltip != null))
            {
                this.m_deckInfoTooltip.Hide();
            }
        }
    }

    private void UpdateAllTrays(bool immediate = false)
    {
        this.InitializeTraysFromDecks();
        List<TraySection> showTraySections = new List<TraySection>();
        foreach (TraySection section in this.m_traySections)
        {
            if (section.m_deckBox.GetDeckID() == -1L)
            {
                section.HideDeckBox(immediate, null);
            }
            else if ((this.m_editingTraySection != section) && !section.IsOpen())
            {
                showTraySections.Add(section);
            }
        }
        base.StartCoroutine(this.UpdateAllTraysAnimation(showTraySections, immediate));
    }

    [DebuggerHidden]
    private IEnumerator UpdateAllTraysAnimation(List<TraySection> showTraySections, bool immediate)
    {
        return new <UpdateAllTraysAnimation>c__Iterator255 { showTraySections = showTraySections, immediate = immediate, <$>showTraySections = showTraySections, <$>immediate = immediate, <>f__this = this };
    }

    private int UpdateDeckTrayVisuals()
    {
        List<CollectionDeck> decks = null;
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL)
        {
            decks = CollectionManager.Get().GetDecks(DeckType.TAVERN_BRAWL_DECK);
        }
        else
        {
            decks = CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK);
        }
        for (int i = 0; i < decks.Count; i++)
        {
            if (i >= this.m_traySections.Count)
            {
                break;
            }
            CollectionDeck deck = decks[i];
            this.m_traySections[i].m_deckBox.AssignFromCollectionDeck(deck);
        }
        return decks.Count;
    }

    public void UpdateEditingDeckBoxVisual(string heroCardId)
    {
        if (this.m_editingTraySection != null)
        {
            this.m_editingTraySection.m_deckBox.SetHeroCardID(heroCardId);
        }
    }

    private void UpdateNewDeckButton(TraySection setNewDeckButtonPosition = null)
    {
        bool newDeckButtonActive = this.UpdateNewDeckButtonPosition(setNewDeckButtonPosition);
        this.ShowNewDeckButton(newDeckButtonActive, null);
    }

    private bool UpdateNewDeckButtonPosition(TraySection setNewDeckButtonPosition = null)
    {
        Vector3 vector;
        bool outActive = false;
        this.GetIdealNewDeckButtonLocalPosition(setNewDeckButtonPosition, out vector, out outActive);
        this.m_newDeckButtonContainer.transform.localPosition = vector;
        return outActive;
    }

    private CollectionDeck UpdateRenamingEditingDeck(string newDeckName)
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        if ((taggedDeck != null) && !string.IsNullOrEmpty(newDeckName))
        {
            taggedDeck.Name = newDeckName;
        }
        return taggedDeck;
    }

    [CustomEditField(Sections="Deck Button Settings")]
    public Vector3 DeckButtonOffset
    {
        get
        {
            return this.m_deckButtonOffset;
        }
        set
        {
            this.m_deckButtonOffset = value;
            this.UpdateNewDeckButton(null);
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateContentExitStart>c__AnonStorey36F
    {
        internal DeckTrayDeckListContent <>f__this;
        internal float animationWaitTime;

        internal void <>m__23E(object _0)
        {
            foreach (TraySection section in this.<>f__this.m_traySections)
            {
                if (this.<>f__this.m_editingTraySection != section)
                {
                    section.HideDeckBox(false, null);
                }
            }
            if (this.<>f__this.m_newlyCreatedTraySection != null)
            {
                <AnimateContentExitStart>c__AnonStorey36E storeye = new <AnimateContentExitStart>c__AnonStorey36E {
                    <>f__ref$879 = this,
                    animateTraySection = this.<>f__this.m_newlyCreatedTraySection
                };
                this.<>f__this.UpdateNewDeckButtonPosition(storeye.animateTraySection);
                this.<>f__this.ShowNewDeckButton(true, new CollectionNewDeckButton.DelOnAnimationFinished(storeye.<>m__24C));
                this.<>f__this.m_editingTraySection = this.<>f__this.m_newlyCreatedTraySection;
                this.<>f__this.m_newlyCreatedTraySection = null;
                this.animationWaitTime += 0.7f;
            }
            else if (this.<>f__this.m_editingTraySection != null)
            {
                this.<>f__this.m_editingTraySection.MoveDeckBoxToEditPosition(this.<>f__this.m_deckEditTopPos.position, 0.25f, null);
            }
            ApplicationMgr.Get().ScheduleCallback(this.animationWaitTime, false, delegate (object o) {
                this.<>f__this.m_animatingExit = false;
                this.<>f__this.FireBusyWithDeckEvent(false);
            }, null);
        }

        internal void <>m__24D(object o)
        {
            this.<>f__this.m_animatingExit = false;
            this.<>f__this.FireBusyWithDeckEvent(false);
        }

        private sealed class <AnimateContentExitStart>c__AnonStorey36E
        {
            internal DeckTrayDeckListContent.<AnimateContentExitStart>c__AnonStorey36F <>f__ref$879;
            internal TraySection animateTraySection;

            internal void <>m__24C(object _1)
            {
                this.animateTraySection.ShowDeckBox(true, delegate (object _2) {
                    this.animateTraySection.m_deckBox.gameObject.SetActive(false);
                    this.<>f__ref$879.<>f__this.m_newDeckButton.FlipHalfOverAndHide(0.1f, _3 => this.animateTraySection.FlipDeckBoxHalfOverToShow(0.1f, _4 => this.animateTraySection.MoveDeckBoxToEditPosition(this.<>f__ref$879.<>f__this.m_deckEditTopPos.position, 0.25f, null)));
                });
            }

            internal void <>m__24E(object _2)
            {
                this.animateTraySection.m_deckBox.gameObject.SetActive(false);
                this.<>f__ref$879.<>f__this.m_newDeckButton.FlipHalfOverAndHide(0.1f, _3 => this.animateTraySection.FlipDeckBoxHalfOverToShow(0.1f, _4 => this.animateTraySection.MoveDeckBoxToEditPosition(this.<>f__ref$879.<>f__this.m_deckEditTopPos.position, 0.25f, null)));
            }

            internal void <>m__24F(object _3)
            {
                this.animateTraySection.FlipDeckBoxHalfOverToShow(0.1f, _4 => this.animateTraySection.MoveDeckBoxToEditPosition(this.<>f__ref$879.<>f__this.m_deckEditTopPos.position, 0.25f, null));
            }

            internal void <>m__250(object _4)
            {
                this.animateTraySection.MoveDeckBoxToEditPosition(this.<>f__ref$879.<>f__this.m_deckEditTopPos.position, 0.25f, null);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <CreateTraySections>c__AnonStorey370
    {
        internal DeckTrayDeckListContent <>f__this;
        internal CollectionDeckBoxVisual deckBox;
        internal TraySection traySection;

        internal void <>m__245(UIEvent e)
        {
            this.<>f__this.OnDeckBoxVisualOver(this.deckBox);
        }

        internal void <>m__246(UIEvent e)
        {
            this.<>f__this.OnDeckBoxVisualOut(this.deckBox);
        }

        internal void <>m__247(UIEvent e)
        {
            this.<>f__this.OnDeckBoxVisualPress(this.deckBox);
        }

        internal void <>m__248(UIEvent e)
        {
            this.<>f__this.OnDeckBoxVisualRelease(this.traySection);
        }
    }

    [CompilerGenerated]
    private sealed class <DeleteDeckAnimation>c__Iterator256 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal System.Action <$>callback;
        internal long <$>deckID;
        internal DeckTrayDeckListContent <>f__this;
        internal int <delIndex>__0;
        internal TraySection <delTraySection>__1;
        internal long <existingDeckID>__5;
        internal int <i>__11;
        internal int <i>__3;
        internal int <itemsToMove>__9;
        internal bool <newDeckBtnActive>__7;
        internal Vector3 <newDeckBtnPos>__6;
        internal TraySection <newDeckButtonTrayLocation>__2;
        internal Action<object> <onAnimationsComplete>__10;
        internal Vector3 <prevTraySectionPosition>__8;
        internal TraySection <traySection>__12;
        internal TraySection <traySection>__4;
        internal Vector3 <traySectionPos>__13;
        internal System.Action callback;
        internal long deckID;

        internal void <>m__251(object _1)
        {
            this.<itemsToMove>__9--;
            if (this.<itemsToMove>__9 <= 0)
            {
                this.<>f__this.ShowNewDeckButton(this.<newDeckBtnActive>__7, null);
                this.<>f__this.FireBusyWithDeckEvent(false);
                if (this.callback != null)
                {
                    this.callback();
                }
                this.<>f__this.m_deletingDecks = false;
            }
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
                case 1:
                    if (this.<>f__this.m_deletingDecks)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<delIndex>__0 = 0;
                    this.<delTraySection>__1 = null;
                    this.<newDeckButtonTrayLocation>__2 = this.<>f__this.m_traySections[0];
                    this.<i>__3 = 0;
                    while (this.<i>__3 < this.<>f__this.m_traySections.Count)
                    {
                        this.<traySection>__4 = this.<>f__this.m_traySections[this.<i>__3];
                        this.<existingDeckID>__5 = this.<traySection>__4.m_deckBox.GetDeckID();
                        if (this.<existingDeckID>__5 == this.deckID)
                        {
                            this.<delIndex>__0 = this.<i>__3;
                            this.<delTraySection>__1 = this.<traySection>__4;
                        }
                        else if (this.<existingDeckID>__5 == -1L)
                        {
                            break;
                        }
                        this.<newDeckButtonTrayLocation>__2 = this.<traySection>__4;
                        this.<i>__3++;
                    }
                    break;

                default:
                    goto Label_04A0;
            }
            if (this.<delTraySection>__1 == null)
            {
                UnityEngine.Debug.LogWarning("Unable to delete deck with ID {0}. Not found in tray sections.", this.<>f__this.gameObject);
            }
            else
            {
                this.<>f__this.FireBusyWithDeckEvent(true);
                this.<>f__this.m_deletingDecks = true;
                this.<>f__this.FireDeckCountChangedEvent();
                this.<>f__this.m_traySections.RemoveAt(this.<delIndex>__0);
                this.<>f__this.GetIdealNewDeckButtonLocalPosition(this.<newDeckButtonTrayLocation>__2, out this.<newDeckBtnPos>__6, out this.<newDeckBtnActive>__7);
                this.<prevTraySectionPosition>__8 = this.<delTraySection>__1.transform.localPosition;
                SoundManager.Get().LoadAndPlay("collection_manager_delete_deck", this.<>f__this.gameObject);
                this.<>f__this.m_deleteDeckPoof.transform.position = this.<delTraySection>__1.m_deckBox.transform.position;
                this.<>f__this.m_deleteDeckPoof.Play(true);
                this.<delTraySection>__1.ClearDeckInfo();
                this.<delTraySection>__1.gameObject.SetActive(false);
                this.<itemsToMove>__9 = (this.<>f__this.m_traySections.Count - this.<delIndex>__0) + 1;
                this.<onAnimationsComplete>__10 = new Action<object>(this.<>m__251);
                this.<i>__11 = this.<delIndex>__0;
                while (this.<i>__11 < this.<>f__this.m_traySections.Count)
                {
                    this.<traySection>__12 = this.<>f__this.m_traySections[this.<i>__11];
                    this.<traySectionPos>__13 = this.<traySection>__12.transform.localPosition;
                    object[] args = new object[] { "position", this.<prevTraySectionPosition>__8, "isLocal", true, "time", 0.5f, "easeType", iTween.EaseType.easeOutBounce, "oncomplete", this.<onAnimationsComplete>__10, "name", "position" };
                    iTween.MoveTo(this.<traySection>__12.gameObject, iTween.Hash(args));
                    this.<prevTraySectionPosition>__8 = this.<traySectionPos>__13;
                    this.<i>__11++;
                }
                this.<>f__this.m_traySections.Add(this.<delTraySection>__1);
                this.<>f__this.m_newDeckButton.SetIsUsable(this.<>f__this.CanShowNewDeckButton());
                this.<delTraySection>__1.gameObject.SetActive(true);
                this.<delTraySection>__1.HideDeckBox(true, null);
                this.<delTraySection>__1.transform.localPosition = this.<prevTraySectionPosition>__8;
                if (this.<>f__this.m_newDeckButton.gameObject.activeSelf)
                {
                    object[] objArray2 = new object[] { "position", this.<newDeckBtnPos>__6, "isLocal", true, "time", 0.5f, "easeType", iTween.EaseType.easeOutBounce, "oncomplete", this.<onAnimationsComplete>__10, "name", "position" };
                    iTween.MoveTo(this.<>f__this.m_newDeckButtonContainer, iTween.Hash(objArray2));
                }
                else
                {
                    this.<>f__this.m_newDeckButtonContainer.transform.localPosition = this.<newDeckBtnPos>__6;
                    this.<onAnimationsComplete>__10(null);
                }
                this.$PC = -1;
            }
        Label_04A0:
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

    [CompilerGenerated]
    private sealed class <EndCreateNewDeck>c__AnonStorey372
    {
        internal DeckTrayDeckListContent <>f__this;
        internal bool newDeck;

        internal void <>m__24B(object o)
        {
            if (this.newDeck)
            {
                this.<>f__this.UpdateAllTrays(true);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ShowNewDeckButton>c__AnonStorey371
    {
        internal DeckTrayDeckListContent <>f__this;
        internal CollectionNewDeckButton.DelOnAnimationFinished callback;

        internal void <>m__249(object o)
        {
            if (this.callback != null)
            {
                this.callback(this.<>f__this);
            }
        }

        internal void <>m__24A(object o)
        {
            this.<>f__this.m_newDeckButton.gameObject.SetActive(false);
            if (this.callback != null)
            {
                this.callback(this.<>f__this);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ShowTrayDoors>c__Iterator254 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>show;
        internal List<TraySection>.Enumerator <$s_1426>__0;
        internal DeckTrayDeckListContent <>f__this;
        internal TraySection <traySection>__1;
        internal bool show;

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
                    if (this.show)
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(0.3f);
                    this.$PC = 1;
                    goto Label_00CB;

                case 1:
                    break;

                case 2:
                    this.$PC = -1;
                    goto Label_00C9;

                default:
                    goto Label_00C9;
            }
            this.<$s_1426>__0 = this.<>f__this.m_traySections.GetEnumerator();
            try
            {
                while (this.<$s_1426>__0.MoveNext())
                {
                    this.<traySection>__1 = this.<$s_1426>__0.Current;
                    this.<traySection>__1.ShowDoor(this.show);
                }
            }
            finally
            {
                this.<$s_1426>__0.Dispose();
            }
            this.$current = null;
            this.$PC = 2;
            goto Label_00CB;
        Label_00C9:
            return false;
        Label_00CB:
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
    private sealed class <UpdateAllTraysAnimation>c__Iterator255 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>immediate;
        internal List<TraySection> <$>showTraySections;
        internal List<TraySection>.Enumerator <$s_1433>__0;
        internal DeckTrayDeckListContent <>f__this;
        internal TraySection <traySection>__1;
        internal bool immediate;
        internal List<TraySection> showTraySections;

        [DebuggerHidden]
        public void Dispose()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 1:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_1433>__0.Dispose();
                    }
                    break;
            }
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            bool flag = false;
            switch (num)
            {
                case 0:
                    this.<$s_1433>__0 = this.showTraySections.GetEnumerator();
                    num = 0xfffffffd;
                    break;

                case 1:
                    break;

                default:
                    goto Label_00D1;
            }
            try
            {
                while (this.<$s_1433>__0.MoveNext())
                {
                    this.<traySection>__1 = this.<$s_1433>__0.Current;
                    this.<traySection>__1.ShowDeckBox(this.immediate, null);
                    if (!this.immediate)
                    {
                        this.$current = new WaitForSeconds(0.015f);
                        this.$PC = 1;
                        flag = true;
                        return true;
                    }
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_1433>__0.Dispose();
            }
            this.<>f__this.UpdateNewDeckButton(null);
            this.$PC = -1;
        Label_00D1:
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

    public delegate void BusyWithDeck(bool busy);

    public delegate void DeckCountChanged(int deckCount);
}

