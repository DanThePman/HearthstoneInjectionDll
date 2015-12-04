using PegasusShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class TavernBrawlDisplay : MonoBehaviour
{
    private readonly string CARD_COUNT_PANEL_CLOSE_ANIM = "TavernBrawl_DecksNumberCoverUp_Close";
    private readonly string CARD_COUNT_PANEL_OPEN_ANIM = "TavernBrawl_DecksNumberCoverUp_Open";
    private static readonly PlatformDependentValue<string> DEFAULT_CHALKBOARD_TEXTURE_NAME_NO_DECK;
    private static readonly PlatformDependentValue<string> DEFAULT_CHALKBOARD_TEXTURE_NAME_WITH_DECK;
    private static readonly PlatformDependentValue<UnityEngine.Vector2> DEFAULT_CHALKBOARD_TEXTURE_OFFSET_NO_DECK;
    private static readonly PlatformDependentValue<UnityEngine.Vector2> DEFAULT_CHALKBOARD_TEXTURE_OFFSET_WITH_DECK;
    [CustomEditField(Sections="Buttons")]
    public UIBButton m_backButton;
    [CustomEditField(Sections="Animating Elements")]
    public Animation m_cardCountPanelAnim;
    private bool m_cardCountPanelAnimOpen;
    [CustomEditField(Sections="Animating Elements")]
    public SlidingTray m_cardListPanel;
    public GameObject m_chalkboard;
    [CustomEditField(Sections="Strings")]
    public UberText m_chalkboardEndInfo;
    [CustomEditField(Sections="Strings")]
    public UberText m_chalkboardHeader;
    [CustomEditField(Sections="Strings")]
    public UberText m_chalkboardInfo;
    public Material m_chestOpenMaterial;
    [CustomEditField(Sections="Buttons")]
    public UIBButton m_createDeckButton;
    [CustomEditField(Sections="Highlights")]
    public HighlightState m_createDeckHighlight;
    private long m_deckBeingEdited;
    public GameObject m_deleteIcon;
    public Color m_disabledTextColor = new Color(0.5f, 0.5f, 0.5f);
    private bool m_doWipeAnimation;
    [CustomEditField(Sections="Buttons")]
    public UIBButton m_editDeckButton;
    [CustomEditField(Sections="Highlights")]
    public HighlightState m_editDeckHighlight;
    public GameObject m_editIcon;
    public UberText m_editText;
    public GameObject m_fullPanel;
    [CustomEditField(Sections="Strings")]
    public UberText m_numWins;
    private Color? m_originalEditIconColor;
    private Color? m_originalEditTextColor;
    public GameObject m_panelWithCreateDeck;
    [CustomEditField(Sections="Buttons")]
    public PlayButton m_playButton;
    [CustomEditField(Sections="Buttons")]
    public PegUIElement m_rewardChest;
    [CustomEditField(Sections="Animating Elements")]
    public GameObject m_rewardContainer;
    [CustomEditField(Sections="Highlights")]
    public HighlightState m_rewardHighlight;
    private GameObject m_rewardObject;
    public PegUIElement m_rewardOffClickCatcher;
    [CustomEditField(Sections="Animating Elements")]
    public GameObject m_rewardsPreview;
    private Vector3 m_rewardsScale;
    [CustomEditField(Sections="Animating Elements")]
    public UberText m_rewardsText;
    [CustomEditField(Sections="Animating Elements")]
    public SlidingTray m_tavernBrawlTray;
    public GameObject m_winsBanner;
    public float m_wipeAnimStartDelay;
    private static TavernBrawlDisplay s_instance;

    static TavernBrawlDisplay()
    {
        PlatformDependentValue<string> value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "TavernBrawl_Chalkboard_Default_NoBorders",
            Phone = "TavernBrawl_Chalkboard_Default_phone"
        };
        DEFAULT_CHALKBOARD_TEXTURE_NAME_NO_DECK = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "TavernBrawl_Chalkboard_Default_Borders",
            Phone = "TavernBrawl_Chalkboard_Default_phone"
        };
        DEFAULT_CHALKBOARD_TEXTURE_NAME_WITH_DECK = value2;
        PlatformDependentValue<UnityEngine.Vector2> value3 = new PlatformDependentValue<UnityEngine.Vector2>(PlatformCategory.Screen) {
            PC = UnityEngine.Vector2.zero,
            Phone = UnityEngine.Vector2.zero
        };
        DEFAULT_CHALKBOARD_TEXTURE_OFFSET_NO_DECK = value3;
        value3 = new PlatformDependentValue<UnityEngine.Vector2>(PlatformCategory.Screen) {
            PC = UnityEngine.Vector2.zero,
            Phone = new UnityEngine.Vector2(0f, -0.389f)
        };
        DEFAULT_CHALKBOARD_TEXTURE_OFFSET_WITH_DECK = value3;
    }

    private void Awake()
    {
        RewardTrigger rewardTrigger;
        s_instance = this;
        base.gameObject.transform.position = Vector3.zero;
        base.gameObject.transform.localScale = Vector3.one;
        TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
        RewardType rewardType = mission.rewardType;
        if ((rewardType != RewardType.REWARD_BOOSTER_PACKS) && (rewardType == RewardType.REWARD_CARD_BACK))
        {
            rewardTrigger = mission.rewardTrigger;
            if ((rewardTrigger != RewardTrigger.REWARD_TRIGGER_WIN_GAME) && (rewardTrigger == RewardTrigger.REWARD_TRIGGER_FINISH_GAME))
            {
                this.m_rewardsText.Text = GameStrings.Get("GLUE_TAVERN_BRAWL_REWARD_DESC_FINISH_CARDBACK");
            }
            else
            {
                this.m_rewardsText.Text = GameStrings.Get("GLUE_TAVERN_BRAWL_REWARD_DESC_CARDBACK");
            }
        }
        else
        {
            rewardTrigger = mission.rewardTrigger;
            if ((rewardTrigger != RewardTrigger.REWARD_TRIGGER_WIN_GAME) && (rewardTrigger == RewardTrigger.REWARD_TRIGGER_FINISH_GAME))
            {
                this.m_rewardsText.Text = GameStrings.Get("GLUE_TAVERN_BRAWL_REWARD_DESC_FINISH");
            }
            else
            {
                this.m_rewardsText.Text = GameStrings.Get("GLUE_TAVERN_BRAWL_REWARD_DESC");
            }
        }
        this.m_rewardsScale = this.m_rewardsPreview.transform.localScale;
        this.m_rewardsPreview.transform.localScale = (Vector3) (Vector3.one * 0.01f);
        if (this.m_editDeckButton != null)
        {
            this.m_editDeckButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.EditOrDeleteDeck));
        }
        if (this.m_createDeckButton != null)
        {
            this.m_createDeckButton.AddEventListener(UIEventType.RELEASE, e => this.CreateDeck());
        }
        if (this.m_rewardOffClickCatcher != null)
        {
            this.m_rewardChest.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.ShowReward));
            this.m_rewardOffClickCatcher.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.HideReward));
        }
        else
        {
            this.m_rewardChest.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.ShowReward));
            this.m_rewardChest.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.HideReward));
        }
        this.m_playButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.StartGame));
        CollectionManager.Get().RegisterDeckCreatedListener(new CollectionManager.DelOnDeckCreated(this.OnDeckCreated));
        CollectionManager.Get().RegisterDeckDeletedListener(new CollectionManager.DelOnDeckDeleted(this.OnDeckDeleted));
        CollectionManager.Get().RegisterDeckContentsListener(new CollectionManager.DelOnDeckContents(this.OnDeckContents));
        FriendChallengeMgr.Get().AddChangedListener(new FriendChallengeMgr.ChangedCallback(this.OnFriendChallengeChanged));
        NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheTavernBrawlRecord), new System.Action(this.NetCache_OnTavernBrawlRecord));
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        this.SetUIForFriendlyChallenge(FriendChallengeMgr.Get().IsChallengeTavernBrawl());
        if (this.m_backButton != null)
        {
            this.m_backButton.AddEventListener(UIEventType.RELEASE, e => this.OnBackButton());
        }
        if ((mission == null) || !mission.canEditDeck)
        {
            Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
        }
    }

    public bool BackFromDeckEdit(bool animate)
    {
        if (!this.IsInDeckEditMode())
        {
            return false;
        }
        if (animate)
        {
            PresenceMgr.Get().SetPrevStatus();
        }
        if (CollectionManagerDisplay.Get().GetViewMode() != CollectionManagerDisplay.ViewMode.CARDS)
        {
            TAG_CLASS pageClass = (TavernBrawlManager.Get().CurrentDeck() != null) ? TavernBrawlManager.Get().CurrentDeck().GetClass() : TAG_CLASS.DRUID;
            CollectionManagerDisplay.Get().m_pageManager.JumpToCollectionClassPage(pageClass);
        }
        this.m_tavernBrawlTray.ToggleTraySlider(true, null, animate);
        this.RefreshStateBasedUI(animate);
        this.m_deckBeingEdited = 0L;
        BnetBar.Get().m_currencyFrame.RefreshContents();
        FriendChallengeMgr.Get().UpdateMyAvailability();
        this.UpdateEditOrCreate();
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_editDeckButton.SetText(GameStrings.Get("GLUE_EDIT"));
            if (this.m_editIcon != null)
            {
                this.m_editIcon.SetActive(true);
            }
            if (this.m_deleteIcon != null)
            {
                this.m_deleteIcon.SetActive(false);
            }
        }
        CollectionDeckTray.Get().ExitEditDeckModeForTavernBrawl();
        return true;
    }

    private void CreateDeck()
    {
        Enum[] args = new Enum[] { PresenceStatus.TAVERN_BRAWL_DECKEDITOR };
        PresenceMgr.Get().SetStatus(args);
        CollectionManagerDisplay.Get().EnterSelectNewDeckHeroMode();
    }

    private void EditOrDeleteDeck(UIEvent e)
    {
        if (this.IsInDeckEditMode())
        {
            this.OnDeleteButtonPressed();
        }
        else
        {
            Enum[] args = new Enum[] { PresenceStatus.TAVERN_BRAWL_DECKEDITOR };
            PresenceMgr.Get().SetStatus(args);
            if (this.SwitchToEditDeckMode(TavernBrawlManager.Get().CurrentDeck()))
            {
            }
        }
    }

    public void EnablePlayButton()
    {
        TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
        if ((mission == null) || mission.canCreateDeck)
        {
            this.ValidateDeck();
        }
        else
        {
            this.m_playButton.Enable();
        }
    }

    public static TavernBrawlDisplay Get()
    {
        return s_instance;
    }

    private void HandleGameStartupFailure()
    {
        if (!TavernBrawlManager.Get().SelectHeroBeforeMission())
        {
            this.EnablePlayButton();
        }
    }

    private void HideReward(UIEvent e)
    {
        iTween.Stop(this.m_rewardsPreview);
        object[] args = new object[] { "scale", (Vector3) (Vector3.one * 0.01f), "time", 0.15f, "oncomplete", o => this.m_rewardsPreview.SetActive(false) };
        iTween.ScaleTo(this.m_rewardsPreview, iTween.Hash(args));
    }

    public bool IsInDeckEditMode()
    {
        return (this.m_deckBeingEdited > 0L);
    }

    public static bool IsTavernBrawlEditing()
    {
        return (IsTavernBrawlOpen() && s_instance.IsInDeckEditMode());
    }

    public static bool IsTavernBrawlOpen()
    {
        if (SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL)
        {
            return false;
        }
        if (s_instance == null)
        {
            return false;
        }
        return true;
    }

    public static bool IsTavernBrawlViewing()
    {
        return (IsTavernBrawlOpen() && !s_instance.IsInDeckEditMode());
    }

    private void NetCache_OnTavernBrawlRecord()
    {
        this.m_numWins.Text = TavernBrawlManager.Get().GamesWon().ToString();
        if (TavernBrawlManager.Get().RewardProgress() > 0)
        {
            this.m_rewardChest.GetComponent<Renderer>().material = this.m_chestOpenMaterial;
            this.m_rewardHighlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
            this.m_rewardChest.SetEnabled(false);
        }
    }

    private void OnBackButton()
    {
        Navigation.GoBack();
    }

    private void OnDeckContents(long deckID)
    {
        CollectionDeck deck = TavernBrawlManager.Get().CurrentDeck();
        if (((deck != null) && (deckID == deck.ID)) && IsTavernBrawlOpen())
        {
            this.ValidateDeck();
        }
    }

    private void OnDeckCreated(long deckID)
    {
        CollectionDeck deck = TavernBrawlManager.Get().CurrentDeck();
        if ((deck != null) && (deckID == deck.ID))
        {
            this.SwitchToEditDeckMode(deck);
        }
    }

    private void OnDeckDeleted(long deckID)
    {
        if ((deckID == this.m_deckBeingEdited) && IsTavernBrawlOpen())
        {
            base.StartCoroutine(this.WaitThenCreateDeck());
        }
    }

    private void OnDeleteButtonConfirmationResponse(AlertPopup.Response response, object userData)
    {
        if (response != AlertPopup.Response.CANCEL)
        {
            CollectionDeckTray.Get().DeleteEditingDeck(true);
            if (CollectionManagerDisplay.Get() != null)
            {
                CollectionManagerDisplay.Get().OnDoneEditingDeck();
            }
        }
    }

    private void OnDeleteButtonPressed()
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_HEADER"),
            m_text = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_DESC"),
            m_showAlertIcon = false,
            m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
            m_responseCallback = new AlertPopup.ResponseCallback(this.OnDeleteButtonConfirmationResponse)
        };
        DialogManager.Get().ShowPopup(info);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        switch (eventData.m_state)
        {
            case FindGameState.CLIENT_CANCELED:
            case FindGameState.CLIENT_ERROR:
            case FindGameState.BNET_ERROR:
            case FindGameState.SERVER_GAME_CANCELED:
                this.HandleGameStartupFailure();
                break;

            case FindGameState.SERVER_GAME_STARTED:
                FriendChallengeMgr.Get().RemoveChangedListener(new FriendChallengeMgr.ChangedCallback(this.OnFriendChallengeChanged));
                break;
        }
        return false;
    }

    private void OnFriendChallengeChanged(FriendChallengeEvent challengeEvent, BnetPlayer player, object userData)
    {
        if ((challengeEvent == FriendChallengeEvent.OPPONENT_ACCEPTED_CHALLENGE) || (challengeEvent == FriendChallengeEvent.I_ACCEPTED_CHALLENGE))
        {
            this.SetUIForFriendlyChallenge(true);
        }
        else if (challengeEvent == FriendChallengeEvent.SELECTED_DECK)
        {
            if ((player != BnetPresenceMgr.Get().GetMyPlayer()) && FriendChallengeMgr.Get().DidISelectDeck())
            {
                FriendlyChallengeHelper.Get().HideFriendChallengeWaitingForOpponentDialog();
                FriendlyChallengeHelper.Get().WaitForFriendChallengeToStart();
            }
        }
        else if (((challengeEvent == FriendChallengeEvent.I_RESCINDED_CHALLENGE) || (challengeEvent == FriendChallengeEvent.OPPONENT_DECLINED_CHALLENGE)) || (challengeEvent == FriendChallengeEvent.OPPONENT_RESCINDED_CHALLENGE))
        {
            this.SetUIForFriendlyChallenge(false);
        }
        else if ((challengeEvent == FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE) || (challengeEvent == FriendChallengeEvent.OPPONENT_REMOVED_FROM_FRIENDS))
        {
            this.SetUIForFriendlyChallenge(false);
            FriendlyChallengeHelper.Get().StopWaitingForFriendChallenge();
        }
    }

    private void OnFriendChallengeWaitingForOpponentDialogResponse(AlertPopup.Response response, object userData)
    {
        if (response == AlertPopup.Response.CANCEL)
        {
            FriendlyChallengeHelper.Get().StopWaitingForFriendChallenge();
            if (!TavernBrawlManager.Get().SelectHeroBeforeMission())
            {
                this.EnablePlayButton();
            }
        }
    }

    private bool OnNavigateBack()
    {
        this.m_tavernBrawlTray.m_animateBounce = false;
        this.m_tavernBrawlTray.ShowTray();
        SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
        return true;
    }

    public void RefreshDataBasedUI(float animDelay = 0f)
    {
        this.RefreshTavernBrawlInfo(animDelay);
        this.NetCache_OnTavernBrawlRecord();
    }

    private void RefreshStateBasedUI(bool animate)
    {
        this.UpdateDeckPanels(animate);
        this.ValidateDeck();
    }

    private void RefreshTavernBrawlInfo(float animDelay)
    {
        this.UpdateEditOrCreate();
        TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
        if ((mission == null) || (mission.missionId < 0))
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLUE_TAVERN_BRAWL_HAS_ENDED_HEADER"),
                m_text = GameStrings.Get("GLUE_TAVERN_BRAWL_HAS_ENDED_TEXT"),
                m_responseDisplay = AlertPopup.ResponseDisplay.OK,
                m_responseCallback = new AlertPopup.ResponseCallback(this.RefreshTavernBrawlInfo_ConfirmEnded),
                m_offset = new Vector3(0f, 104f, 0f)
            };
            DialogManager.Get().ShowPopup(info);
        }
        else
        {
            DbfRecord record = GameDbf.Scenario.GetRecord(mission.missionId);
            this.m_chalkboardHeader.Text = record.GetLocString("NAME");
            this.m_chalkboardInfo.Text = record.GetLocString("DESCRIPTION");
            base.CancelInvoke("UpdateTimeText");
            base.InvokeRepeating("UpdateTimeText", 0.1f, 0.1f);
            this.UpdateTimeText();
            if (((this.m_chalkboard != null) && (this.m_chalkboard.GetComponent<MeshRenderer>() != null)) && (this.m_chalkboard.GetComponent<MeshRenderer>().material != null))
            {
                Material material = this.m_chalkboard.GetComponent<MeshRenderer>().material;
                string name = record.GetString("TB_TEXTURE");
                UnityEngine.Vector2 zero = UnityEngine.Vector2.zero;
                if (PlatformSettings.Screen == ScreenCategory.Phone)
                {
                    name = record.GetString("TB_TEXTURE_PHONE");
                    zero.y = record.GetFloat("TB_TEXTURE_PHONE_OFFSET_Y");
                }
                Texture texture = (name != null) ? AssetLoader.Get().LoadTexture(name, false) : null;
                if (texture == null)
                {
                    bool canCreateDeck = TavernBrawlManager.Get().CurrentMission().canCreateDeck;
                    name = !canCreateDeck ? ((string) DEFAULT_CHALKBOARD_TEXTURE_NAME_NO_DECK) : ((string) DEFAULT_CHALKBOARD_TEXTURE_NAME_WITH_DECK);
                    zero = !canCreateDeck ? ((UnityEngine.Vector2) DEFAULT_CHALKBOARD_TEXTURE_OFFSET_NO_DECK) : ((UnityEngine.Vector2) DEFAULT_CHALKBOARD_TEXTURE_OFFSET_WITH_DECK);
                    texture = AssetLoader.Get().LoadTexture(name, false);
                }
                if (texture != null)
                {
                    material.SetTexture("_TopTex", texture);
                    material.SetTextureOffset("_MainTex", zero);
                }
                base.StartCoroutine(this.WaitThenPlayWipeAnim(!this.m_doWipeAnimation ? 0f : animDelay));
            }
        }
    }

    private void RefreshTavernBrawlInfo_ConfirmEnded(AlertPopup.Response response, object userData)
    {
        if (s_instance != null)
        {
            Navigation.Clear();
            this.OnNavigateBack();
        }
    }

    private void SetUIForFriendlyChallenge(bool isTavernBrawlChallenge)
    {
        string key = "GLUE_BRAWL";
        if (TavernBrawlManager.Get().SelectHeroBeforeMission())
        {
            key = "GLUE_CHOOSE";
        }
        else if (isTavernBrawlChallenge)
        {
            key = "GLUE_BRAWL_FRIEND";
        }
        this.m_playButton.SetText(GameStrings.Get(key));
        this.m_rewardChest.gameObject.SetActive(!isTavernBrawlChallenge);
        this.m_winsBanner.SetActive(!isTavernBrawlChallenge);
        if (this.m_editDeckButton != null)
        {
            if (!this.m_originalEditTextColor.HasValue)
            {
                this.m_originalEditTextColor = new Color?(this.m_editText.TextColor);
            }
            if (isTavernBrawlChallenge)
            {
                this.m_editText.TextColor = this.m_disabledTextColor;
                this.m_editDeckButton.SetEnabled(false);
            }
            else
            {
                this.m_editText.TextColor = this.m_originalEditTextColor.Value;
                this.m_editDeckButton.SetEnabled(true);
            }
            if (this.m_editIcon != null)
            {
                if (!this.m_originalEditIconColor.HasValue)
                {
                    this.m_originalEditIconColor = new Color?(this.m_editIcon.GetComponent<Renderer>().material.color);
                }
                if (isTavernBrawlChallenge)
                {
                    this.m_editIcon.GetComponent<Renderer>().material.color = this.m_disabledTextColor;
                }
                else
                {
                    this.m_editIcon.GetComponent<Renderer>().material.color = this.m_originalEditIconColor.Value;
                }
            }
        }
    }

    private void ShowReward(UIEvent e)
    {
        if (TavernBrawlManager.Get().CurrentMission() != null)
        {
            RewardType rewardType = TavernBrawlManager.Get().CurrentMission().rewardType;
            if (rewardType != RewardType.REWARD_BOOSTER_PACKS)
            {
                if (rewardType != RewardType.REWARD_CARD_BACK)
                {
                    object[] objArray4 = new object[] { TavernBrawlManager.Get().CurrentMission().rewardType };
                    UnityEngine.Debug.LogErrorFormat("Tavern Brawl reward type currently not supported! Add type {0} to TaverBrawlDisplay.ShowReward().", objArray4);
                    return;
                }
                if (this.m_rewardObject == null)
                {
                    int cardBackIdx = (int) TavernBrawlManager.Get().CurrentMission().RewardData1;
                    CardBackManager.LoadCardBackData data = CardBackManager.Get().LoadCardBackByIndex(cardBackIdx, false, "Card_Hidden");
                    if (data == null)
                    {
                        object[] objArray3 = new object[] { cardBackIdx };
                        UnityEngine.Debug.LogErrorFormat("TavernBrawlDisplay.ShowReward() - Could not load cardback ID {0}!", objArray3);
                        return;
                    }
                    this.m_rewardObject = data.m_GameObject;
                    GameUtils.SetParent(this.m_rewardObject, this.m_rewardContainer, false);
                    this.m_rewardObject.transform.localScale = (Vector3) (Vector3.one * 5.92f);
                }
            }
            else if (this.m_rewardObject == null)
            {
                int id = (int) TavernBrawlManager.Get().CurrentMission().RewardData1;
                DbfRecord record = GameDbf.Booster.GetRecord(id);
                if (record == null)
                {
                    object[] objArray1 = new object[] { id };
                    UnityEngine.Debug.LogErrorFormat("TavernBrawlDisplay.ShowReward() - no record found for booster {0}!", objArray1);
                    return;
                }
                string assetName = record.GetAssetName("PACK_OPENING_PREFAB");
                if (string.IsNullOrEmpty(assetName))
                {
                    object[] objArray2 = new object[] { id };
                    UnityEngine.Debug.LogErrorFormat("TavernBrawlDisplay.ShowReward() - no prefab found for booster {0}!", objArray2);
                    return;
                }
                GameObject obj2 = AssetLoader.Get().LoadActor(assetName, false, false);
                if (obj2 == null)
                {
                    UnityEngine.Debug.LogError(string.Format("TavernBrawlDisplay.ShowReward() - failed to load prefab {0} for booster {1}!", assetName, id));
                    return;
                }
                this.m_rewardObject = obj2;
                UnopenedPack component = obj2.GetComponent<UnopenedPack>();
                if (component == null)
                {
                    UnityEngine.Debug.LogError(string.Format("TavernBrawlDisplay.ShowReward() - No UnopenedPack script found on prefab {0} for booster {1}!", assetName, id));
                    return;
                }
                GameUtils.SetParent(this.m_rewardObject, this.m_rewardContainer, false);
                component.AddBooster();
            }
            this.m_rewardsPreview.SetActive(true);
            iTween.Stop(this.m_rewardsPreview);
            object[] args = new object[] { "scale", this.m_rewardsScale, "time", 0.15f };
            iTween.ScaleTo(this.m_rewardsPreview, iTween.Hash(args));
        }
    }

    private void Start()
    {
        this.m_tavernBrawlTray.ToggleTraySlider(true, null, false);
        Enum[] status = PresenceMgr.Get().GetStatus();
        if (((status == null) || (status.Length <= 0)) || (((PresenceStatus) status[0]) != PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING))
        {
            Enum[] args = new Enum[] { PresenceStatus.TAVERN_BRAWL_SCREEN };
            PresenceMgr.Get().SetStatus(args);
        }
        this.RefreshStateBasedUI(false);
        this.RefreshDataBasedUI(this.m_wipeAnimStartDelay);
        MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_TavernBrawl);
        if (TavernBrawlManager.Get().CurrentMission() != null)
        {
            int @int = Options.Get().GetInt(Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD);
            if (@int == 0)
            {
                this.m_doWipeAnimation = true;
                if (!NotificationManager.Get().HasSoundPlayedThisSession("VO_INNKEEPER_TAVERNBRAWL_WELCOME1_27"))
                {
                    NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_TAVERNBRAWL_WELCOME1_27"), "VO_INNKEEPER_TAVERNBRAWL_WELCOME1_27", 0f, null);
                    NotificationManager.Get().ForceAddSoundToPlayedList("VO_INNKEEPER_TAVERNBRAWL_WELCOME1_27");
                }
            }
            else if (@int < TavernBrawlManager.Get().CurrentMission().seasonId)
            {
                this.m_doWipeAnimation = true;
                int val = Options.Get().GetInt(Option.TIMES_SEEN_TAVERNBRAWL_CRAZY_RULES_QUOTE);
                if (!NotificationManager.Get().HasSoundPlayedThisSession("VO_INNKEEPER_TAVERNBRAWL_DESC2_30") && (val < 3))
                {
                    NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_TAVERNBRAWL_DESC2_30"), "VO_INNKEEPER_TAVERNBRAWL_DESC2_30", 0f, null);
                    NotificationManager.Get().ForceAddSoundToPlayedList("VO_INNKEEPER_TAVERNBRAWL_DESC2_30");
                    val++;
                    Options.Get().SetInt(Option.TIMES_SEEN_TAVERNBRAWL_CRAZY_RULES_QUOTE, val);
                }
            }
            if (@int != TavernBrawlManager.Get().CurrentMission().seasonId)
            {
                Options.Get().SetInt(Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD, TavernBrawlManager.Get().CurrentMission().seasonId);
            }
        }
        if (TavernBrawlManager.Get().RewardProgress() == 0)
        {
            this.m_rewardHighlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        }
        base.StartCoroutine(this.UpdateQuestsWhenReady());
    }

    private void StartGame(UIEvent e)
    {
        TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
        if (mission == null)
        {
            this.RefreshDataBasedUI(0f);
        }
        else
        {
            if (TavernBrawlManager.Get().SelectHeroBeforeMission())
            {
                if (HeroPickerDisplay.Get() != null)
                {
                    Log.JMac.PrintWarning("Attempting to load HeroPickerDisplay a second time!", new object[0]);
                    return;
                }
                AssetLoader.Get().LoadActor("HeroPicker", false, false);
            }
            else if (mission.canCreateDeck)
            {
                if (!TavernBrawlManager.Get().HasValidDeck())
                {
                    UnityEngine.Debug.LogError("Attempting to start a Tavern Brawl game without having a valid deck!");
                    return;
                }
                CollectionDeck deck = TavernBrawlManager.Get().CurrentDeck();
                if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
                {
                    FriendChallengeMgr.Get().SelectDeck(deck.ID);
                    FriendlyChallengeHelper.Get().StartChallengeOrWaitForOpponent("GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_OPPONENT_WAITING_READY", new AlertPopup.ResponseCallback(this.OnFriendChallengeWaitingForOpponentDialogResponse));
                }
                else
                {
                    TavernBrawlManager.Get().StartGame(deck.ID);
                }
            }
            else if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
            {
                FriendChallengeMgr.Get().SkipDeckSelection();
                FriendlyChallengeHelper.Get().StartChallengeOrWaitForOpponent("GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_OPPONENT_WAITING_READY", new AlertPopup.ResponseCallback(this.OnFriendChallengeWaitingForOpponentDialogResponse));
            }
            else
            {
                TavernBrawlManager.Get().StartGame(0L);
            }
            this.m_playButton.SetEnabled(false);
        }
    }

    private bool SwitchToEditDeckMode(CollectionDeck deck)
    {
        if ((CollectionManagerDisplay.Get() == null) || (deck == null))
        {
            return false;
        }
        this.m_tavernBrawlTray.HideTray();
        this.UpdateDeckPanels(true, true);
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_editDeckButton.gameObject.SetActive(TavernBrawlManager.Get().CurrentMission().canEditDeck);
            this.m_editDeckButton.SetText(GameStrings.Get("GLUE_COLLECTION_DECK_DELETE"));
            if (this.m_editIcon != null)
            {
                this.m_editIcon.SetActive(false);
            }
            if (this.m_deleteIcon != null)
            {
                this.m_deleteIcon.SetActive(true);
            }
            this.m_editDeckHighlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
        }
        this.m_deckBeingEdited = deck.ID;
        BnetBar.Get().m_currencyFrame.RefreshContents();
        CollectionDeckTray.Get().EnterEditDeckModeForTavernBrawl();
        FriendChallengeMgr.Get().UpdateMyAvailability();
        return true;
    }

    public void Unload()
    {
        CollectionManager.Get().RemoveDeckCreatedListener(new CollectionManager.DelOnDeckCreated(this.OnDeckCreated));
        CollectionManager.Get().RemoveDeckDeletedListener(new CollectionManager.DelOnDeckDeleted(this.OnDeckDeleted));
        CollectionManager.Get().RemoveDeckContentsListener(new CollectionManager.DelOnDeckContents(this.OnDeckContents));
        FriendChallengeMgr.Get().RemoveChangedListener(new FriendChallengeMgr.ChangedCallback(this.OnFriendChallengeChanged));
        NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheTavernBrawlRecord), new System.Action(this.NetCache_OnTavernBrawlRecord));
        GameMgr.Get().UnregisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        if ((FriendChallengeMgr.Get().IsChallengeTavernBrawl() && !SceneMgr.Get().IsInGame()) && !SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FRIENDLY))
        {
            FriendChallengeMgr.Get().CancelChallenge();
        }
        if (this.IsInDeckEditMode())
        {
            Navigation.Pop();
        }
    }

    private void UpdateDeckPanels(bool animate = true)
    {
        TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
        this.UpdateDeckPanels(((mission != null) && mission.canCreateDeck) && TavernBrawlManager.Get().HasCreatedDeck(), animate);
    }

    private void UpdateDeckPanels(bool hasDeck, bool animate)
    {
        if (this.m_cardListPanel != null)
        {
            bool show = !hasDeck;
            if (animate && !show)
            {
                this.m_createDeckButton.gameObject.SetActive(false);
                this.m_createDeckHighlight.gameObject.SetActive(false);
            }
            else if (show)
            {
                this.m_createDeckButton.gameObject.SetActive(true);
                this.m_createDeckHighlight.gameObject.SetActive(true);
            }
            this.m_cardListPanel.ToggleTraySlider(show, null, animate);
        }
        if ((this.m_cardCountPanelAnim != null) && (this.m_cardCountPanelAnimOpen != hasDeck))
        {
            this.m_cardCountPanelAnim.Play(!hasDeck ? this.CARD_COUNT_PANEL_CLOSE_ANIM : this.CARD_COUNT_PANEL_OPEN_ANIM);
            this.m_cardCountPanelAnimOpen = hasDeck;
        }
    }

    private void UpdateEditOrCreate()
    {
        TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
        bool flag = (mission != null) && mission.canCreateDeck;
        bool flag2 = (mission != null) && mission.canEditDeck;
        bool flag3 = TavernBrawlManager.Get().HasCreatedDeck();
        bool flag4 = flag && !flag3;
        bool flag5 = (flag2 && flag) && flag3;
        if (this.m_editDeckButton != null)
        {
            this.m_editDeckButton.gameObject.SetActive(flag5);
            if (this.m_editIcon != null)
            {
                this.m_editIcon.SetActive(true);
            }
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            if (this.m_createDeckButton != null)
            {
                this.m_createDeckButton.gameObject.SetActive(flag4);
            }
        }
        else
        {
            if (this.m_panelWithCreateDeck != null)
            {
                this.m_panelWithCreateDeck.SetActive(flag4);
            }
            if (this.m_fullPanel != null)
            {
                this.m_fullPanel.SetActive(!flag4);
            }
        }
        if (this.m_createDeckHighlight != null)
        {
            if (!this.m_createDeckHighlight.gameObject.activeInHierarchy && flag4)
            {
                UnityEngine.Debug.LogWarning("Attempting to activate m_createDeckHighlight, but it is inactive! This will not behave correctly!");
            }
            this.m_createDeckHighlight.ChangeState(!flag4 ? ActorStateType.HIGHLIGHT_OFF : ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateQuestsWhenReady()
    {
        return new <UpdateQuestsWhenReady>c__Iterator1E4();
    }

    private void UpdateTimeText()
    {
        int seconds = (TavernBrawlManager.Get().CurrentMission() != null) ? TavernBrawlManager.Get().CurrentTavernBrawlSeasonEnd : -1;
        if (seconds < 0)
        {
            base.CancelInvoke("UpdateTimeText");
        }
        else
        {
            TimeUtils.ElapsedStringSet stringSet = new TimeUtils.ElapsedStringSet {
                m_seconds = "GLUE_TAVERN_BRAWL_LABEL_ENDING_SECONDS",
                m_minutes = "GLUE_TAVERN_BRAWL_LABEL_ENDING_MINUTES",
                m_hours = "GLUE_TAVERN_BRAWL_LABEL_ENDING_HOURS",
                m_days = "GLUE_TAVERN_BRAWL_LABEL_ENDING_DAYS",
                m_weeks = "GLUE_TAVERN_BRAWL_LABEL_ENDING_WEEKS",
                m_monthAgo = "GLUE_TAVERN_BRAWL_LABEL_ENDING_OVER_1_MONTH"
            };
            string elapsedTimeString = TimeUtils.GetElapsedTimeString(seconds, stringSet);
            this.m_chalkboardEndInfo.Text = elapsedTimeString;
        }
    }

    public void ValidateDeck()
    {
        TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
        if (mission == null)
        {
            this.m_playButton.Disable();
        }
        else if (mission.canCreateDeck)
        {
            if (TavernBrawlManager.Get().HasValidDeck())
            {
                this.m_playButton.Enable();
                this.m_editDeckHighlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
            }
            else
            {
                this.m_playButton.Disable();
                this.m_editDeckHighlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitThenCreateDeck()
    {
        return new <WaitThenCreateDeck>c__Iterator1E6 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenPlayWipeAnim(float waitTime)
    {
        return new <WaitThenPlayWipeAnim>c__Iterator1E5 { waitTime = waitTime, <$>waitTime = waitTime, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <UpdateQuestsWhenReady>c__Iterator1E4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
                    if (!AchieveManager.Get().IsReady())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (AchieveManager.Get().HasActiveQuests(true))
                    {
                        WelcomeQuests.Show(false, null, false);
                    }
                    else
                    {
                        GameToastMgr.Get().UpdateQuestProgressToasts();
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

    [CompilerGenerated]
    private sealed class <WaitThenCreateDeck>c__Iterator1E6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TavernBrawlDisplay <>f__this;

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
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    goto Label_0079;

                case 1:
                    this.<>f__this.CreateDeck();
                    this.$current = new WaitForSeconds(0.4f);
                    this.$PC = 2;
                    goto Label_0079;

                case 2:
                    this.<>f__this.BackFromDeckEdit(false);
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0079:
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
    private sealed class <WaitThenPlayWipeAnim>c__Iterator1E5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>waitTime;
        internal TavernBrawlDisplay <>f__this;
        internal PlayMakerFSM <fsm>__0;
        internal float waitTime;

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
                    this.$current = new WaitForSeconds(this.waitTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (((this.<>f__this.m_chalkboard != null) && TavernBrawlManager.Get().IsTavernBrawlActive) && TavernBrawlManager.Get().IsScenarioDataReady)
                    {
                        this.<fsm>__0 = this.<>f__this.m_chalkboard.GetComponent<PlayMakerFSM>();
                        this.<fsm>__0.SendEvent(!this.<>f__this.m_doWipeAnimation ? "QuickShow" : "Wipe");
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
}

