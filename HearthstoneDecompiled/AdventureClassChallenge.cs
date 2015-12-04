using PegasusShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureClassChallenge : MonoBehaviour
{
    [CompilerGenerated]
    private static Comparison<DbfRecord> <>f__am$cache1E;
    private const float CHALLENGE_BUTTON_OFFSET = 4.3f;
    private readonly float[] EMPTY_SLOT_UV_OFFSET = new float[] { 0f, 0.223f, 0.377f, 0.535f, 0.69f, 0.85f };
    [CustomEditField(Sections="Basic UI")]
    public UIBButton m_BackButton;
    [CustomEditField(Sections="Class Challenge Buttons")]
    public GameObject m_ChallengeButtonContainer;
    [CustomEditField(Sections="Class Challenge Buttons")]
    public float m_ChallengeButtonHeight;
    [CustomEditField(Sections="Class Challenge Buttons")]
    public UIBScrollable m_ChallengeButtonScroller;
    [CustomEditField(Sections="Text")]
    public UberText m_ChallengeDescription;
    [CustomEditField(Sections="Text")]
    public UberText m_ChallengeTitle;
    [CustomEditField(Sections="Reward UI")]
    public AdventureClassChallengeChestButton m_ChestButton;
    [CustomEditField(Sections="Reward UI")]
    public GameObject m_ChestButtonCover;
    [CustomEditField(Sections="Class Challenge Buttons")]
    public GameObject m_ClassChallengeButtonPrefab;
    [CustomEditField(Sections="Class Challenge Buttons")]
    public Vector3 m_ClassChallengeButtonSpacing;
    private List<ClassChallengeData> m_ClassChallenges = new List<ClassChallengeData>();
    [CustomEditField(Sections="Class Challenge Buttons")]
    public GameObject m_EmptyChallengeButtonSlot;
    private bool m_gameDenied;
    private GameObject m_LeftHero;
    [CustomEditField(Sections="Hero Portraits")]
    public GameObject m_LeftHeroContainer;
    [CustomEditField(Sections="Hero Portraits")]
    public UberText m_LeftHeroName;
    [CustomEditField(Sections="DBF Stuff")]
    public UberText m_ModeName;
    [CustomEditField(Sections="Basic UI")]
    public PlayButton m_PlayButton;
    [CustomEditField(Sections="Reward UI")]
    public Transform m_RewardBone;
    private GameObject m_RightHero;
    [CustomEditField(Sections="Hero Portraits")]
    public GameObject m_RightHeroContainer;
    [CustomEditField(Sections="Hero Portraits")]
    public UberText m_RightHeroName;
    private Map<int, int> m_ScenarioChallengeLookup = new Map<int, int>();
    private AdventureClassChallengeButton m_SelectedButton;
    private int m_SelectedScenario;
    private int m_UVoffset;
    [CustomEditField(Sections="Versus Text")]
    public Color m_VersusTextColor;
    [CustomEditField(Sections="Versus Text")]
    public GameObject m_VersusTextContainer;
    [CustomEditField(Sections="Versus Text", T=EditType.GAME_OBJECT)]
    public string m_VersusTextPrefab;
    private const int VISIBLE_SLOT_COUNT = 10;

    private void Awake()
    {
        this.m_BackButton.AddEventListener(UIEventType.RELEASE, e => this.BackButton());
        this.m_PlayButton.AddEventListener(UIEventType.RELEASE, e => this.Play());
        this.m_EmptyChallengeButtonSlot.SetActive(false);
        AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(this.m_VersusTextPrefab), new AssetLoader.GameObjectCallback(this.OnVersusLettersLoaded), null, false);
    }

    private void BackButton()
    {
        Navigation.GoBack();
    }

    private int BossCreateParamsSortComparison(ClassChallengeData data1, ClassChallengeData data2)
    {
        return GameUtils.MissionSortComparison(data1.scenarioRecord, data2.scenarioRecord);
    }

    private void ButtonPressed(UIEvent e)
    {
        if ((this.m_ChallengeButtonScroller == null) || !this.m_ChallengeButtonScroller.IsTouchDragging())
        {
            AdventureClassChallengeButton element = (AdventureClassChallengeButton) e.GetElement();
            this.m_SelectedButton.Deselect();
            this.SetSelectedButton(element);
            element.Select(true);
            this.m_SelectedScenario = element.m_ScenarioID;
            this.m_SelectedButton = element;
            this.GetRewardCardForSelectedScenario();
        }
    }

    [DebuggerHidden]
    private IEnumerator CreateChallengeButtons()
    {
        return new <CreateChallengeButtons>c__Iterator1 { <>f__this = this };
    }

    private void GetRewardCardForSelectedScenario()
    {
        if (this.m_RewardBone != null)
        {
            this.m_ChestButton.m_IsRewardLoading = true;
            List<CardRewardData> immediateCardRewardsForDefeatingScenario = AdventureProgressMgr.Get().GetImmediateCardRewardsForDefeatingScenario(this.m_SelectedScenario);
            if ((immediateCardRewardsForDefeatingScenario != null) && (immediateCardRewardsForDefeatingScenario.Count > 0))
            {
                immediateCardRewardsForDefeatingScenario[0].LoadRewardObject(new Reward.DelOnRewardLoaded(this.RewardCardLoaded));
            }
        }
    }

    private void InitAdventureChallenges()
    {
        List<DbfRecord> list = new List<DbfRecord>(GameDbf.Scenario.GetRecords());
        if (<>f__am$cache1E == null)
        {
            <>f__am$cache1E = delegate (DbfRecord a, DbfRecord b) {
                int @int = a.GetInt("SORT_ORDER");
                int num2 = b.GetInt("SORT_ORDER");
                if (@int < num2)
                {
                    return -1;
                }
                if (@int > num2)
                {
                    return 1;
                }
                return 0;
            };
        }
        list.Sort(<>f__am$cache1E);
        foreach (DbfRecord record in list)
        {
            if ((record.GetInt("ADVENTURE_ID") == AdventureConfig.Get().GetSelectedAdventure()) && (record.GetInt("MODE_ID") == 4))
            {
                int dbId = record.GetInt("PLAYER1_HERO_CARD_ID");
                int num2 = record.GetInt("CLIENT_PLAYER2_HERO_CARD_ID");
                if (num2 == 0)
                {
                    num2 = record.GetInt("PLAYER2_HERO_CARD_ID");
                }
                ClassChallengeData item = new ClassChallengeData {
                    scenarioRecord = record,
                    heroID0 = GameUtils.TranslateDbIdToCardId(dbId),
                    heroID1 = GameUtils.TranslateDbIdToCardId(num2),
                    unlocked = AdventureProgressMgr.Get().CanPlayScenario(record.GetId())
                };
                if (AdventureProgressMgr.Get().HasDefeatedScenario(record.GetId()))
                {
                    item.defeated = true;
                }
                else
                {
                    item.defeated = false;
                }
                item.name = record.GetLocString("SHORT_NAME");
                item.title = record.GetLocString("NAME");
                item.description = record.GetLocString("DESCRIPTION");
                item.completedDescription = record.GetLocString("COMPLETED_DESCRIPTION");
                item.opponentName = record.GetLocString("OPPONENT_NAME");
                this.m_ScenarioChallengeLookup.Add(record.GetId(), this.m_ClassChallenges.Count);
                this.m_ClassChallenges.Add(item);
            }
        }
    }

    private void InitModeName()
    {
        int selectedAdventure = (int) AdventureConfig.Get().GetSelectedAdventure();
        int modeId = 4;
        string locString = GameUtils.GetAdventureDataRecord(selectedAdventure, modeId).GetLocString((UniversalInputManager.UsePhoneUI == null) ? "NAME" : "SHORT_NAME");
        this.m_ModeName.Text = locString;
    }

    private void LoadButtonPortrait(AdventureClassChallengeButton button, string heroID)
    {
        DefLoader.Get().LoadFullDef(heroID, new DefLoader.LoadDefCallback<FullDef>(this.OnButtonFullDefLoaded), button);
    }

    private void LoadHero(int heroNum, string heroID)
    {
        HeroLoadData userData = new HeroLoadData {
            heroNum = heroNum,
            heroID = heroID
        };
        DefLoader.Get().LoadFullDef(heroID, new DefLoader.LoadDefCallback<FullDef>(this.OnHeroFullDefLoaded), userData);
    }

    private void OnActorLoaded(string name, GameObject actorObject, object userData)
    {
        HeroLoadData data = (HeroLoadData) userData;
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("AdventureClassChallenge.OnActorLoaded() - FAILED to load actor \"{0}\"", name));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("AdventureClassChallenge.OnActorLoaded() - ERROR actor \"{0}\" has no Actor component", name));
            }
            else
            {
                component.TurnOffCollider();
                component.SetUnlit();
                UnityEngine.Object.Destroy(component.m_healthObject);
                UnityEngine.Object.Destroy(component.m_attackObject);
                component.SetEntityDef(data.fulldef.GetEntityDef());
                component.SetCardDef(data.fulldef.GetCardDef());
                component.SetCardFlair(new CardFlair(TAG_PREMIUM.NORMAL));
                component.UpdateAllComponents();
                GameObject leftHeroContainer = this.m_LeftHeroContainer;
                if (data.heroNum == 0)
                {
                    UnityEngine.Object.Destroy(this.m_LeftHero);
                    this.m_LeftHero = actorObject;
                    this.m_LeftHeroName.Text = data.fulldef.GetEntityDef().GetName();
                }
                else
                {
                    UnityEngine.Object.Destroy(this.m_RightHero);
                    this.m_RightHero = actorObject;
                    leftHeroContainer = this.m_RightHeroContainer;
                }
                GameUtils.SetParent(component, leftHeroContainer, false);
                component.transform.localRotation = Quaternion.identity;
                component.transform.localScale = Vector3.one;
                component.GetAttackObject().Hide();
                component.Show();
            }
        }
    }

    private void OnButtonFullDefLoaded(string cardId, FullDef fullDef, object userData)
    {
        AdventureClassChallengeButton button = (AdventureClassChallengeButton) userData;
        CardDef cardDef = fullDef.GetCardDef();
        Material practiceAIPortrait = cardDef.GetPracticeAIPortrait();
        if (practiceAIPortrait != null)
        {
            practiceAIPortrait.mainTexture = cardDef.GetPortraitTexture();
            button.SetPortraitMaterial(practiceAIPortrait);
        }
    }

    private void OnDestroy()
    {
        GameMgr.Get().UnregisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        if (eventData.m_state == FindGameState.INVALID)
        {
            this.m_PlayButton.Enable();
        }
        return false;
    }

    private void OnHeroFullDefLoaded(string cardId, FullDef fullDef, object userData)
    {
        if (fullDef == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("AdventureClassChallenge.OnHeroFullDefLoaded() - FAILED to load \"{0}\"", cardId));
        }
        else
        {
            HeroLoadData callbackData = (HeroLoadData) userData;
            callbackData.fulldef = fullDef;
            AssetLoader.Get().LoadActor("Card_Play_Hero", new AssetLoader.GameObjectCallback(this.OnActorLoaded), callbackData, false);
        }
    }

    private static bool OnNavigateBack()
    {
        AdventureConfig.Get().ChangeToLastSubScene(true);
        return true;
    }

    private void OnVersusLettersLoaded(string name, GameObject go, object userData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("AdventureClassChallenge.OnVersusLettersLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            GameUtils.SetParent(go, this.m_VersusTextContainer, false);
            go.GetComponentInChildren<VS>().ActivateShadow(true);
            go.transform.localRotation = Quaternion.identity;
            go.transform.Rotate(new Vector3(0f, 180f, 0f));
            go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            Component[] componentsInChildren = go.GetComponentsInChildren(typeof(Renderer));
            for (int i = 0; i < (componentsInChildren.Length - 1); i++)
            {
                Renderer renderer = (Renderer) componentsInChildren[i];
                renderer.material.SetColor("_Color", this.m_VersusTextColor);
            }
        }
    }

    private void Play()
    {
        this.m_PlayButton.Disable();
        GameMgr.Get().FindGame(GameType.GT_VS_AI, this.m_SelectedScenario, 0L, 0L);
    }

    private void RewardCardLoaded(Reward reward, object callbackData)
    {
        if (reward == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("AdventureClassChallenge.RewardCardLoaded() - FAILED to load reward \"{0}\"", base.name));
        }
        else if (reward.gameObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("AdventureClassChallenge.RewardCardLoaded() - Reward GameObject is null \"{0}\"", base.name));
        }
        else
        {
            reward.gameObject.transform.parent = this.m_ChestButton.transform;
            CardReward component = reward.GetComponent<CardReward>();
            if (this.m_ChestButton.m_RewardCard != null)
            {
                UnityEngine.Object.Destroy(this.m_ChestButton.m_RewardCard);
            }
            this.m_ChestButton.m_RewardCard = component.m_nonHeroCardsRoot;
            GameUtils.SetParent(component.m_nonHeroCardsRoot, (Component) this.m_RewardBone, false);
            component.m_nonHeroCardsRoot.SetActive(false);
            UnityEngine.Object.Destroy(component.gameObject);
            this.m_ChestButton.m_IsRewardLoading = false;
        }
    }

    private void SetScenario(int scenarioID)
    {
        ClassChallengeData data = this.m_ClassChallenges[this.m_ScenarioChallengeLookup[scenarioID]];
        this.LoadHero(0, data.heroID0);
        this.LoadHero(1, data.heroID1);
        this.m_RightHeroName.Text = data.opponentName;
        this.m_ChallengeTitle.Text = data.title;
        if (data.defeated)
        {
            this.m_ChallengeDescription.Text = data.completedDescription;
        }
        else
        {
            this.m_ChallengeDescription.Text = data.description;
        }
        if (UniversalInputManager.UsePhoneUI == null)
        {
            if (this.m_ClassChallenges[this.m_ScenarioChallengeLookup[scenarioID]].defeated)
            {
                this.m_ChestButton.gameObject.SetActive(false);
                this.m_ChestButtonCover.SetActive(true);
            }
            else
            {
                this.m_ChestButton.gameObject.SetActive(true);
                this.m_ChestButtonCover.SetActive(false);
            }
        }
    }

    private void SetSelectedButton(AdventureClassChallengeButton button)
    {
        int scenarioID = button.m_ScenarioID;
        AdventureConfig.Get().SetMission((ScenarioDbId) scenarioID, true);
        this.SetScenario(scenarioID);
    }

    private void Start()
    {
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        this.InitModeName();
        this.InitAdventureChallenges();
        Navigation.PushUnique(new Navigation.NavigateBackHandler(AdventureClassChallenge.OnNavigateBack));
        base.StartCoroutine(this.CreateChallengeButtons());
    }

    [CompilerGenerated]
    private sealed class <CreateChallengeButtons>c__Iterator1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<AdventureClassChallenge.ClassChallengeData>.Enumerator <$s_32>__2;
        internal AdventureClassChallenge <>f__this;
        internal GameObject <button>__4;
        internal int <buttonCount>__0;
        internal AdventureClassChallenge.ClassChallengeData <cdata>__3;
        internal AdventureClassChallengeButton <challengeButton>__5;
        internal GameObject <emptySlot>__8;
        internal int <emptySlotCount>__6;
        internal int <lastSelectedScenario>__1;
        internal Renderer <renderer>__9;
        internal int <s>__7;

        internal float <>m__A()
        {
            return (this.<>f__this.m_ChallengeButtonHeight * this.<>f__this.m_ClassChallenges.Count);
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
                    this.<buttonCount>__0 = 0;
                    this.<lastSelectedScenario>__1 = (int) AdventureConfig.Get().GetLastSelectedMission();
                    this.<$s_32>__2 = this.<>f__this.m_ClassChallenges.GetEnumerator();
                    try
                    {
                        while (this.<$s_32>__2.MoveNext())
                        {
                            this.<cdata>__3 = this.<$s_32>__2.Current;
                            if (this.<cdata>__3.unlocked)
                            {
                                this.<button>__4 = (GameObject) GameUtils.Instantiate(this.<>f__this.m_ClassChallengeButtonPrefab, this.<>f__this.m_ChallengeButtonContainer, false);
                                this.<button>__4.transform.localPosition = this.<>f__this.m_ClassChallengeButtonSpacing * this.<buttonCount>__0;
                                this.<challengeButton>__5 = this.<button>__4.GetComponent<AdventureClassChallengeButton>();
                                this.<challengeButton>__5.m_Text.Text = this.<cdata>__3.name;
                                this.<challengeButton>__5.m_ScenarioID = this.<cdata>__3.scenarioRecord.GetId();
                                this.<challengeButton>__5.m_Chest.SetActive(!this.<cdata>__3.defeated);
                                this.<challengeButton>__5.m_Checkmark.SetActive(this.<cdata>__3.defeated);
                                this.<challengeButton>__5.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.ButtonPressed));
                                this.<>f__this.LoadButtonPortrait(this.<challengeButton>__5, this.<cdata>__3.heroID1);
                                if ((this.<lastSelectedScenario>__1 == this.<challengeButton>__5.m_ScenarioID) || (this.<>f__this.m_SelectedButton == null))
                                {
                                    this.<>f__this.m_SelectedButton = this.<challengeButton>__5;
                                    this.<>f__this.m_SelectedScenario = this.<cdata>__3.scenarioRecord.GetId();
                                }
                                this.<buttonCount>__0++;
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_32>__2.Dispose();
                    }
                    this.<emptySlotCount>__6 = 10 - this.<buttonCount>__0;
                    if (this.<emptySlotCount>__6 <= 0)
                    {
                        UnityEngine.Debug.LogError(string.Format("Adventure Class Challenge tray UI doesn't support scrolling yet. More than {0} buttons where added.", 10));
                        break;
                    }
                    this.<s>__7 = 0;
                    while (this.<s>__7 < this.<emptySlotCount>__6)
                    {
                        this.<emptySlot>__8 = (GameObject) GameUtils.Instantiate(this.<>f__this.m_EmptyChallengeButtonSlot, this.<>f__this.m_ChallengeButtonContainer, false);
                        this.<emptySlot>__8.transform.localPosition = this.<>f__this.m_ClassChallengeButtonSpacing * (this.<buttonCount>__0 + this.<s>__7);
                        this.<emptySlot>__8.transform.localRotation = Quaternion.identity;
                        this.<emptySlot>__8.SetActive(true);
                        this.<renderer>__9 = this.<emptySlot>__8.GetComponentInChildren<Renderer>();
                        this.<renderer>__9.material.mainTextureOffset = new UnityEngine.Vector2(0f, this.<>f__this.EMPTY_SLOT_UV_OFFSET[this.<>f__this.m_UVoffset]);
                        this.<>f__this.m_UVoffset++;
                        if (this.<>f__this.m_UVoffset > 5)
                        {
                            this.<>f__this.m_UVoffset = 0;
                        }
                        this.<s>__7++;
                    }
                    this.$current = null;
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.<>f__this.m_SelectedButton != null)
                    {
                        this.<>f__this.SetSelectedButton(this.<>f__this.m_SelectedButton);
                        this.<>f__this.m_SelectedButton.Select(false);
                        this.<>f__this.GetRewardCardForSelectedScenario();
                        this.<>f__this.m_PlayButton.Enable();
                        if (this.<>f__this.m_ChallengeButtonScroller != null)
                        {
                            this.<>f__this.m_ChallengeButtonScroller.SetScrollHeightCallback(new UIBScrollable.ScrollHeightCallback(this.<>m__A), false, false);
                        }
                        this.<>f__this.GetComponent<AdventureSubScene>().SetIsLoaded(true);
                        this.$PC = -1;
                        break;
                    }
                    UnityEngine.Debug.LogWarning("AdventureClassChallenge.m_SelectedButton is null!");
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

    private class ClassChallengeData
    {
        public string completedDescription;
        public bool defeated;
        public string description;
        public string heroID0;
        public string heroID1;
        public string name;
        public string opponentName;
        public DbfRecord scenarioRecord;
        public string title;
        public bool unlocked;
    }

    private class HeroLoadData
    {
        public FullDef fulldef;
        public string heroID;
        public int heroNum;
    }
}

