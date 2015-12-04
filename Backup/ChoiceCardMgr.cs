using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class ChoiceCardMgr : MonoBehaviour
{
    [CompilerGenerated]
    private static Spell.StateFinishedCallback <>f__am$cacheB;
    [CompilerGenerated]
    private static Action<object> <>f__am$cacheC;
    private static readonly Vector3 INVISIBLE_SCALE = new Vector3(0.0001f, 0.0001f, 0.0001f);
    private Banner m_choiceBanner;
    private NormalButton m_choiceButton;
    public ChoiceData m_ChoiceData = new ChoiceData();
    public ChoiceEffectData m_ChoiceEffectData = new ChoiceEffectData();
    private Map<int, ChoiceState> m_choiceStateMap = new Map<int, ChoiceState>();
    public CommonData m_CommonData = new CommonData();
    private bool m_friendlyChoicesShown;
    public SubOptionData m_SubOptionData = new SubOptionData();
    private SubOptionState m_subOptionState;
    private static ChoiceCardMgr s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    private void CancelChoices()
    {
        this.HideChoiceUi();
        foreach (ChoiceState state in this.m_choiceStateMap.Values)
        {
            for (int i = 0; i < state.m_cards.Count; i++)
            {
                state.m_cards[i].HideCard();
            }
        }
        this.m_choiceStateMap.Clear();
    }

    public void CancelSubOptions()
    {
        if (this.HasSubOption())
        {
            Spell playSpell = this.m_subOptionState.m_parentCard.GetPlaySpell(true);
            if (playSpell != null)
            {
                SpellStateType activeState = playSpell.GetActiveState();
                if ((activeState != SpellStateType.NONE) && (activeState != SpellStateType.CANCEL))
                {
                    playSpell.ActivateState(SpellStateType.CANCEL);
                }
            }
            Entity entity = this.m_subOptionState.m_parentCard.GetEntity();
            if (entity.IsHeroPower())
            {
                entity.SetTagAndHandleChange<int>(GAME_TAG.EXHAUSTED, 0);
            }
            this.HideSubOptions(null);
        }
    }

    private void ChoiceButton_OnPress(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("UI_MouseClick_01");
    }

    private void ChoiceButton_OnRelease(UIEvent e)
    {
        int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
        ChoiceState state = this.m_choiceStateMap[friendlyPlayerId];
        if (this.m_friendlyChoicesShown)
        {
            this.m_choiceButton.SetText(GameStrings.Get("GLOBAL_SHOW"));
            this.HideChoiceCards(state);
            this.m_friendlyChoicesShown = false;
        }
        else
        {
            this.m_choiceButton.SetText(GameStrings.Get("GLOBAL_HIDE"));
            this.ShowChoiceCards(state, true, this.m_ChoiceEffectData.m_AlwaysPlayChoiceEffects);
            this.m_friendlyChoicesShown = true;
        }
    }

    public void ClearSubOptions()
    {
        this.m_subOptionState = null;
    }

    private void DoCommonHideChoicesWork(int playerId)
    {
        if (playerId == GameState.Get().GetFriendlyPlayerId())
        {
            this.HideChoiceUi();
        }
        this.m_choiceStateMap.Remove(playerId);
    }

    public static ChoiceCardMgr Get()
    {
        return s_instance;
    }

    public NormalButton GetChoiceButton()
    {
        return this.m_choiceButton;
    }

    private Spell GetChoiceEffectForCard(Entity sourceEntity, Card card)
    {
        if (sourceEntity.HasReferencedTag(GAME_TAG.TREASURE))
        {
            return this.m_ChoiceEffectData.m_DiscoverCardEffect;
        }
        return null;
    }

    public List<Card> GetFriendlyCards()
    {
        ChoiceState state;
        if (this.m_subOptionState != null)
        {
            return this.m_subOptionState.m_cards;
        }
        int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
        if (this.m_choiceStateMap.TryGetValue(friendlyPlayerId, out state))
        {
            return state.m_cards;
        }
        return null;
    }

    public PowerTaskList GetFriendlyPreChoiceTaskList()
    {
        int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
        return this.GetPreChoiceTaskList(friendlyPlayerId);
    }

    public PowerTaskList GetPreChoiceTaskList(int playerId)
    {
        ChoiceState state;
        if (this.m_choiceStateMap.TryGetValue(playerId, out state))
        {
            return state.m_preTaskList;
        }
        return null;
    }

    public Card GetSubOptionParentCard()
    {
        return ((this.m_subOptionState != null) ? this.m_subOptionState.m_parentCard : null);
    }

    public bool HasChoices()
    {
        return (this.m_choiceStateMap.Count > 0);
    }

    public bool HasChoices(int playerId)
    {
        return this.m_choiceStateMap.ContainsKey(playerId);
    }

    public bool HasFriendlyChoices()
    {
        int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
        return this.HasChoices(friendlyPlayerId);
    }

    public bool HasSubOption()
    {
        return (this.m_subOptionState != null);
    }

    private void HideChoiceBanner()
    {
        if (this.m_choiceBanner != null)
        {
            UnityEngine.Object.Destroy(this.m_choiceBanner.gameObject);
        }
    }

    private void HideChoiceButton()
    {
        if (this.m_choiceButton != null)
        {
            UnityEngine.Object.Destroy(this.m_choiceButton.gameObject);
        }
    }

    private void HideChoiceCard(Card card)
    {
        if (<>f__am$cacheC == null)
        {
            <>f__am$cacheC = userData => ((Card) userData).HideCard();
        }
        Action<object> action = <>f__am$cacheC;
        iTween.Stop(card.gameObject);
        object[] args = new object[] { "scale", INVISIBLE_SCALE, "time", this.m_ChoiceData.m_CardHideTime, "oncomplete", action, "oncompleteparams", card, "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ScaleTo(card.gameObject, hashtable);
    }

    private void HideChoiceCards(ChoiceState state)
    {
        for (int i = 0; i < state.m_cards.Count; i++)
        {
            Card card = state.m_cards[i];
            this.HideChoiceCard(card);
        }
    }

    private void HideChoicesFromInput(int playerId, ChoiceState state, List<Entity> chosenEntities)
    {
        for (int i = 0; i < state.m_cards.Count; i++)
        {
            Card card = state.m_cards[i];
            Entity item = card.GetEntity();
            if (!chosenEntities.Contains(item))
            {
                card.HideCard();
            }
        }
        this.DoCommonHideChoicesWork(playerId);
    }

    private void HideChoicesFromPacket(int playerId, ChoiceState state, Network.EntitiesChosen chosen)
    {
        for (int i = 0; i < state.m_cards.Count; i++)
        {
            Card card = state.m_cards[i];
            if (!this.WasCardChosen(card, chosen.Entities))
            {
                card.HideCard();
            }
        }
        this.DoCommonHideChoicesWork(playerId);
        GameState.Get().OnEntitiesChosenProcessed(chosen);
    }

    private void HideChoiceUi()
    {
        this.HideChoiceBanner();
        this.HideChoiceButton();
    }

    private void HideSubOptions(Entity chosenEntity = null)
    {
        for (int i = 0; i < this.m_subOptionState.m_cards.Count; i++)
        {
            Card card = this.m_subOptionState.m_cards[i];
            if (card.GetEntity() != chosenEntity)
            {
                card.HideCard();
            }
        }
    }

    private bool IsCardActorReady(Card card)
    {
        if (!card.IsActorReady())
        {
            return false;
        }
        return true;
    }

    private bool IsCardReady(Card card)
    {
        if (card.GetCardDef() == null)
        {
            return false;
        }
        return true;
    }

    private bool IsChoiceCardReady(Card card)
    {
        Entity entity = card.GetEntity();
        if (!this.IsEntityReady(entity))
        {
            return false;
        }
        if (!this.IsCardReady(card))
        {
            return false;
        }
        if (!this.IsCardActorReady(card))
        {
            return false;
        }
        return true;
    }

    private bool IsEntityReady(Entity entity)
    {
        if (entity.GetZone() != TAG_ZONE.SETASIDE)
        {
            return false;
        }
        if (entity.IsBusy())
        {
            return false;
        }
        return true;
    }

    public bool IsFriendlyShown()
    {
        if (this.m_subOptionState != null)
        {
            return true;
        }
        int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
        return this.m_choiceStateMap.ContainsKey(friendlyPlayerId);
    }

    public bool IsFriendlyWaitingToShowChoices()
    {
        int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
        return this.IsWaitingToShowChoices(friendlyPlayerId);
    }

    public bool IsShown()
    {
        return ((this.m_subOptionState != null) || (this.m_choiceStateMap.Count > 0));
    }

    public bool IsWaitingToShowChoices(int playerId)
    {
        ChoiceState state;
        return (this.m_choiceStateMap.TryGetValue(playerId, out state) && state.m_waitingToShow);
    }

    public bool IsWaitingToShowSubOptions()
    {
        if (this.HasSubOption() && this.m_subOptionState.m_parentCard.GetEntity().IsMinion())
        {
            ZonePlay battlefieldZone = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone();
            if (this.m_subOptionState.m_parentCard.GetZone() != battlefieldZone)
            {
                return true;
            }
        }
        return false;
    }

    [DebuggerHidden]
    private IEnumerator LoadChoiceCardActors(Entity entity, Card card)
    {
        return new <LoadChoiceCardActors>c__Iterator87 { entity = entity, card = card, <$>entity = entity, <$>card = card, <>f__this = this };
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private bool OnEntitiesChosenReceived(Network.EntitiesChosen chosen, Network.EntityChoices choices, object userData)
    {
        if (choices.ChoiceType != CHOICE_TYPE.GENERAL)
        {
            return false;
        }
        base.StartCoroutine(this.WaitThenHideChoices(chosen, choices));
        return true;
    }

    private void OnEntityChoicesReceived(Network.EntityChoices choices, PowerTaskList preChoiceTaskList, object userData)
    {
        if (choices.ChoiceType == CHOICE_TYPE.GENERAL)
        {
            base.StartCoroutine(this.WaitThenShowChoices(choices, preChoiceTaskList));
        }
    }

    private void OnGameOver(object userData)
    {
        base.StopAllCoroutines();
        this.CancelSubOptions();
        this.CancelChoices();
    }

    public void OnSendChoices(Network.EntityChoices choicePacket, List<Entity> chosenEntities)
    {
        if (choicePacket.ChoiceType == CHOICE_TYPE.GENERAL)
        {
            ChoiceState state;
            int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
            if (!this.m_choiceStateMap.TryGetValue(friendlyPlayerId, out state))
            {
                object[] messageArgs = new object[] { friendlyPlayerId };
                Error.AddDevFatal("ChoiceCardMgr.OnSendChoices() - there is no ChoiceState for friendly player {0}", messageArgs);
            }
            this.HideChoicesFromInput(friendlyPlayerId, state, chosenEntities);
        }
    }

    public void OnSubOptionClicked(Entity chosenEntity)
    {
        if (this.HasSubOption())
        {
            this.HideSubOptions(chosenEntity);
        }
    }

    private void PlayChoiceEffects(ChoiceState state, bool friendly)
    {
        if (friendly)
        {
            GameState state2 = GameState.Get();
            Entity sourceEntity = state2.GetEntity(state2.GetFriendlyEntityChoices().Source);
            if (sourceEntity != null)
            {
                foreach (Card card in state.m_cards)
                {
                    Spell choiceEffectForCard = this.GetChoiceEffectForCard(sourceEntity, card);
                    if (choiceEffectForCard != null)
                    {
                        Spell spell2 = UnityEngine.Object.Instantiate<Spell>(choiceEffectForCard);
                        TransformUtil.AttachAndPreserveLocalTransform(spell2.transform, card.GetActor().transform);
                        if (<>f__am$cacheB == null)
                        {
                            <>f__am$cacheB = delegate (Spell spell, SpellStateType prevStateType, object userData) {
                                if (spell.GetActiveState() == SpellStateType.NONE)
                                {
                                    UnityEngine.Object.Destroy(spell.gameObject);
                                }
                            };
                        }
                        spell2.AddStateFinishedCallback(<>f__am$cacheB);
                        spell2.Activate();
                    }
                }
            }
        }
    }

    private void ShowChoiceBanner(List<Card> cards)
    {
        this.HideChoiceBanner();
        Network.EntityChoices friendlyEntityChoices = GameState.Get().GetFriendlyEntityChoices();
        Transform transform = Board.Get().FindBone(this.m_ChoiceData.m_BannerBoneName);
        this.m_choiceBanner = (Banner) UnityEngine.Object.Instantiate(this.m_ChoiceData.m_BannerPrefab, transform.position, transform.rotation);
        string headline = GameState.Get().GetGameEntity().CustomChoiceBannerText();
        if (headline == null)
        {
            if (friendlyEntityChoices.CountMax == 1)
            {
                headline = GameStrings.Get("GAMEPLAY_CHOOSE_ONE");
                foreach (Card card in cards)
                {
                    if ((null != card) && card.GetEntity().IsHeroPower())
                    {
                        headline = GameStrings.Get("GAMEPLAY_CHOOSE_ONE_HERO_POWER");
                        break;
                    }
                }
            }
            else
            {
                headline = string.Format("[PH] Choose {0} to {1}", friendlyEntityChoices.CountMin, friendlyEntityChoices.CountMax);
            }
        }
        this.m_choiceBanner.SetText(headline);
        Vector3 localScale = this.m_choiceBanner.transform.localScale;
        this.m_choiceBanner.transform.localScale = INVISIBLE_SCALE;
        object[] args = new object[] { "scale", localScale, "time", this.m_ChoiceData.m_UiShowTime };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ScaleTo(this.m_choiceBanner.gameObject, hashtable);
    }

    private void ShowChoiceButton()
    {
        this.HideChoiceButton();
        string name = FileUtils.GameAssetPathToName(this.m_ChoiceData.m_ButtonPrefab);
        this.m_choiceButton = AssetLoader.Get().LoadActor(name, false, false).GetComponent<NormalButton>();
        this.m_choiceButton.GetButtonUberText().TextAlpha = 1f;
        string buttonBoneName = this.m_ChoiceData.m_ButtonBoneName;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            buttonBoneName = buttonBoneName + "_phone";
        }
        Transform transform = Board.Get().FindBone(buttonBoneName);
        TransformUtil.CopyWorld((Component) this.m_choiceButton, (Component) transform);
        this.m_friendlyChoicesShown = true;
        this.m_choiceButton.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.ChoiceButton_OnPress));
        this.m_choiceButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ChoiceButton_OnRelease));
        this.m_choiceButton.SetText(GameStrings.Get("GLOBAL_HIDE"));
        this.m_choiceButton.m_button.GetComponent<Spell>().ActivateState(SpellStateType.BIRTH);
    }

    private void ShowChoiceCards(ChoiceState state, bool friendly, bool playEffects)
    {
        string name = !friendly ? this.m_ChoiceData.m_OpponentBoneName : this.m_ChoiceData.m_FriendlyBoneName;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            name = name + "_phone";
        }
        int count = state.m_cards.Count;
        Transform transform = Board.Get().FindBone(name);
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        Vector3 localScale = transform.localScale;
        if ((UniversalInputManager.UsePhoneUI != null) && (count > this.m_CommonData.m_PhoneMaxCardsBeforeAdjusting))
        {
            localScale.x *= this.m_CommonData.m_PhoneMaxCardScale;
            localScale.z *= this.m_CommonData.m_PhoneMaxCardScale;
        }
        for (int i = 0; i < count; i++)
        {
            Card card = state.m_cards[i];
            card.ShowCard();
            card.transform.localScale = INVISIBLE_SCALE;
            iTween.Stop(card.gameObject);
            iTween.RotateTo(card.gameObject, eulerAngles, this.m_ChoiceData.m_CardShowTime);
            iTween.ScaleTo(card.gameObject, localScale, this.m_ChoiceData.m_CardShowTime);
        }
        if (playEffects)
        {
            this.PlayChoiceEffects(state, friendly);
        }
    }

    private void ShowChoices(ChoiceState state, bool friendly)
    {
        List<Card> cards = state.m_cards;
        if (friendly)
        {
            this.ShowChoiceUi(cards);
        }
        int count = cards.Count;
        string name = !friendly ? this.m_ChoiceData.m_OpponentBoneName : this.m_ChoiceData.m_FriendlyBoneName;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            name = name + "_phone";
        }
        Vector3 position = Board.Get().FindBone(name).position;
        float horizontalPadding = this.m_ChoiceData.m_HorizontalPadding;
        if ((UniversalInputManager.UsePhoneUI != null) && (count > this.m_CommonData.m_PhoneMaxCardsBeforeAdjusting))
        {
            horizontalPadding = this.m_ChoiceData.m_PhoneMaxHorizontalPadding;
        }
        float num3 = !friendly ? this.m_CommonData.m_OpponentCardWidth : this.m_CommonData.m_FriendlyCardWidth;
        float num4 = 0.5f * num3;
        float num5 = (num3 * count) + (horizontalPadding * (count - 1));
        float num6 = 0.5f * num5;
        float num7 = (position.x - num6) + num4;
        for (int i = 0; i < count; i++)
        {
            Card card = cards[i];
            Vector3 vector2 = new Vector3 {
                x = num7,
                y = position.y,
                z = position.z
            };
            card.transform.position = vector2;
            num7 += num3 + horizontalPadding;
        }
        this.ShowChoiceCards(state, friendly, true);
    }

    private void ShowChoiceUi(List<Card> cards)
    {
        this.ShowChoiceBanner(cards);
        this.ShowChoiceButton();
    }

    private void ShowSubOptions()
    {
        GameState state = GameState.Get();
        Card parentCard = this.m_subOptionState.m_parentCard;
        Entity entity = this.m_subOptionState.m_parentCard.GetEntity();
        string boneName = this.m_SubOptionData.m_BoneName;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            boneName = boneName + "_phone";
        }
        Transform transform = Board.Get().FindBone(boneName);
        float friendlyCardWidth = this.m_CommonData.m_FriendlyCardWidth;
        float x = transform.position.x;
        ZonePlay battlefieldZone = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone();
        List<int> subCardIDs = entity.GetSubCardIDs();
        if (entity.IsMinion() && (UniversalInputManager.UsePhoneUI == null))
        {
            int zonePosition = parentCard.GetZonePosition();
            x = battlefieldZone.GetCardPosition(parentCard).x;
            if (zonePosition > 5)
            {
                friendlyCardWidth += this.m_SubOptionData.m_AdjacentCardXOffset;
                x -= ((this.m_CommonData.m_FriendlyCardWidth * 1.5f) + this.m_SubOptionData.m_AdjacentCardXOffset) + this.m_SubOptionData.m_MinionParentXOffset;
            }
            else if ((zonePosition == 1) && (battlefieldZone.GetCards().Count > 6))
            {
                friendlyCardWidth += this.m_SubOptionData.m_AdjacentCardXOffset;
                x += (this.m_CommonData.m_FriendlyCardWidth / 2f) + this.m_SubOptionData.m_MinionParentXOffset;
            }
            else
            {
                friendlyCardWidth += this.m_SubOptionData.m_MinionParentXOffset * 2f;
                x -= (this.m_CommonData.m_FriendlyCardWidth / 2f) + this.m_SubOptionData.m_MinionParentXOffset;
            }
        }
        else
        {
            int count = subCardIDs.Count;
            friendlyCardWidth += (count <= this.m_CommonData.m_PhoneMaxCardsBeforeAdjusting) ? this.m_SubOptionData.m_AdjacentCardXOffset : this.m_SubOptionData.m_PhoneMaxAdjacentCardXOffset;
            x -= (friendlyCardWidth / 2f) * (count - 1);
        }
        for (int i = 0; i < subCardIDs.Count; i++)
        {
            int id = subCardIDs[i];
            Card card = state.GetEntity(id).GetCard();
            if (card != null)
            {
                this.m_subOptionState.m_cards.Add(card);
                card.ForceLoadHandActor();
                card.transform.position = parentCard.transform.position;
                card.transform.localScale = INVISIBLE_SCALE;
                Vector3 position = new Vector3 {
                    x = x + (i * friendlyCardWidth),
                    y = transform.position.y,
                    z = transform.position.z
                };
                iTween.MoveTo(card.gameObject, position, this.m_SubOptionData.m_CardShowTime);
                Vector3 localScale = transform.localScale;
                if ((UniversalInputManager.UsePhoneUI != null) && (subCardIDs.Count > this.m_CommonData.m_PhoneMaxCardsBeforeAdjusting))
                {
                    localScale.x *= this.m_CommonData.m_PhoneMaxCardScale;
                    localScale.z *= this.m_CommonData.m_PhoneMaxCardScale;
                }
                iTween.ScaleTo(card.gameObject, localScale, this.m_SubOptionData.m_CardShowTime);
            }
        }
    }

    public void ShowSubOptions(Card parentCard)
    {
        this.m_subOptionState = new SubOptionState();
        this.m_subOptionState.m_parentCard = parentCard;
        base.StartCoroutine(this.WaitThenShowSubOptions());
    }

    private void Start()
    {
        GameState.Get().RegisterEntityChoicesReceivedListener(new GameState.EntityChoicesReceivedCallback(this.OnEntityChoicesReceived));
        GameState.Get().RegisterEntitiesChosenReceivedListener(new GameState.EntitiesChosenReceivedCallback(this.OnEntitiesChosenReceived));
        GameState.Get().RegisterGameOverListener(new GameState.GameOverCallback(this.OnGameOver), null);
    }

    [DebuggerHidden]
    private IEnumerator WaitThenHideChoices(Network.EntitiesChosen chosen, Network.EntityChoices choices)
    {
        return new <WaitThenHideChoices>c__Iterator88 { chosen = chosen, <$>chosen = chosen, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenShowChoices(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
    {
        return new <WaitThenShowChoices>c__Iterator86 { choices = choices, preChoiceTaskList = preChoiceTaskList, <$>choices = choices, <$>preChoiceTaskList = preChoiceTaskList, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenShowSubOptions()
    {
        return new <WaitThenShowSubOptions>c__Iterator89 { <>f__this = this };
    }

    private bool WasCardChosen(Card card, List<int> chosenEntityIds)
    {
        <WasCardChosen>c__AnonStorey2F2 storeyf = new <WasCardChosen>c__AnonStorey2F2();
        storeyf.entityId = card.GetEntity().GetEntityId();
        return (chosenEntityIds.FindIndex(new Predicate<int>(storeyf.<>m__EF)) >= 0);
    }

    [CompilerGenerated]
    private sealed class <LoadChoiceCardActors>c__Iterator87 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>card;
        internal Entity <$>entity;
        internal ChoiceCardMgr <>f__this;
        internal Card card;
        internal Entity entity;

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
                    if (!this.<>f__this.IsEntityReady(this.entity))
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_00A0;
                    }
                    this.card.HideCard();
                    break;

                case 2:
                    break;

                default:
                    goto Label_009E;
            }
            while (!this.<>f__this.IsCardReady(this.card))
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00A0;
            }
            this.card.ForceLoadHandActor();
            this.$PC = -1;
        Label_009E:
            return false;
        Label_00A0:
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
    private sealed class <WaitThenHideChoices>c__Iterator88 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Network.EntitiesChosen <$>chosen;
        internal ChoiceCardMgr <>f__this;
        internal int <playerId>__0;
        internal ChoiceCardMgr.ChoiceState <state>__1;
        internal Network.EntitiesChosen chosen;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            object[] objArray2;
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                {
                    this.<playerId>__0 = this.chosen.PlayerId;
                    this.<state>__1 = this.<>f__this.m_choiceStateMap[this.<playerId>__0];
                    if (!this.<state>__1.m_waitingToShow)
                    {
                        goto Label_00D9;
                    }
                    object[] args = new object[] { this.chosen.ID };
                    Log.Power.Print("ChoiceCardMgr.WaitThenHideChoices() - id={0} WAIT for EntityChoice", args);
                    break;
                }
                case 1:
                    break;

                case 2:
                    goto Label_00D9;

                default:
                    goto Label_0125;
            }
            if (this.<state>__1.m_waitingToShow)
            {
                this.$current = null;
                this.$PC = 1;
            }
            else
            {
                this.$current = new WaitForSeconds(this.<>f__this.m_ChoiceData.m_MinShowTime);
                this.$PC = 2;
            }
            return true;
        Label_00D9:
            objArray2 = new object[] { this.chosen.ID };
            Log.Power.Print("ChoiceCardMgr.WaitThenHideChoices() - id={0} BEGIN", objArray2);
            this.<>f__this.HideChoicesFromPacket(this.<playerId>__0, this.<state>__1, this.chosen);
            this.$PC = -1;
        Label_0125:
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
    private sealed class <WaitThenShowChoices>c__Iterator86 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Network.EntityChoices <$>choices;
        internal PowerTaskList <$>preChoiceTaskList;
        internal ChoiceCardMgr <>f__this;
        internal Card <card>__6;
        internal Card <card>__8;
        internal Entity <entity>__5;
        internal int <entityId>__4;
        internal bool <friendly>__10;
        internal int <friendlyPlayerId>__9;
        internal int <i>__3;
        internal int <i>__7;
        internal int <playerId>__0;
        internal PowerProcessor <powerProcessor>__2;
        internal ChoiceCardMgr.ChoiceState <state>__1;
        internal Network.EntityChoices choices;
        internal PowerTaskList preChoiceTaskList;

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
                    this.<playerId>__0 = this.choices.PlayerId;
                    this.<state>__1 = new ChoiceCardMgr.ChoiceState();
                    this.<>f__this.m_choiceStateMap.Add(this.<playerId>__0, this.<state>__1);
                    this.<state>__1.m_waitingToShow = true;
                    this.<powerProcessor>__2 = GameState.Get().GetPowerProcessor();
                    if (this.<powerProcessor>__2.HasTaskList(this.<state>__1.m_preTaskList))
                    {
                        object[] objArray1 = new object[] { this.choices.ID, this.preChoiceTaskList.GetId() };
                        Log.Power.Print("ChoiceCardMgr.WaitThenShowChoices() - id={0} WAIT for taskList {1}", objArray1);
                    }
                    this.<state>__1.m_preTaskList = this.preChoiceTaskList;
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_026D;

                case 3:
                    goto Label_02F3;

                default:
                    goto Label_032C;
            }
            while (this.<powerProcessor>__2.HasTaskList(this.<state>__1.m_preTaskList))
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_032E;
            }
            object[] args = new object[] { this.choices.ID };
            Log.Power.Print("ChoiceCardMgr.WaitThenShowChoices() - id={0} BEGIN", args);
            this.<i>__3 = 0;
            while (this.<i>__3 < this.choices.Entities.Count)
            {
                this.<entityId>__4 = this.choices.Entities[this.<i>__3];
                this.<entity>__5 = GameState.Get().GetEntity(this.<entityId>__4);
                this.<card>__6 = this.<entity>__5.GetCard();
                if (this.<card>__6 == null)
                {
                    object[] messageArgs = new object[] { this.<entity>__5, this.<i>__3 };
                    Error.AddDevFatal("ChoiceCardMgr.WaitThenShowChoices() - Entity {0} (option {1}) has no Card", messageArgs);
                }
                else
                {
                    this.<state>__1.m_cards.Add(this.<card>__6);
                    this.<>f__this.StartCoroutine(this.<>f__this.LoadChoiceCardActors(this.<entity>__5, this.<card>__6));
                }
                this.<i>__3++;
            }
            this.<i>__7 = 0;
            while (this.<i>__7 < this.<state>__1.m_cards.Count)
            {
                this.<card>__8 = this.<state>__1.m_cards[this.<i>__7];
            Label_026D:
                while (!this.<>f__this.IsChoiceCardReady(this.<card>__8))
                {
                    this.$current = null;
                    this.$PC = 2;
                    goto Label_032E;
                }
                this.<i>__7++;
            }
            this.<friendlyPlayerId>__9 = GameState.Get().GetFriendlyPlayerId();
            this.<friendly>__10 = this.<playerId>__0 == this.<friendlyPlayerId>__9;
            if (!this.<friendly>__10)
            {
                goto Label_0302;
            }
        Label_02F3:
            while (GameState.Get().IsTurnStartManagerBlockingInput())
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_032E;
            }
        Label_0302:
            this.<state>__1.m_waitingToShow = false;
            this.<>f__this.ShowChoices(this.<state>__1, this.<friendly>__10);
            this.$PC = -1;
        Label_032C:
            return false;
        Label_032E:
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
    private sealed class <WaitThenShowSubOptions>c__Iterator89 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ChoiceCardMgr <>f__this;
        internal int <currentPacketId>__0;
        internal Network.Options <options>__1;

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
                    this.<currentPacketId>__0 = GameState.Get().GetOptionsPacket().ID;
                    break;

                case 1:
                    if (this.<>f__this.m_subOptionState != null)
                    {
                        if (GameMgr.Get().IsSpectator())
                        {
                            this.<options>__1 = GameState.Get().GetOptionsPacket();
                            if ((this.<options>__1 == null) || (this.<options>__1.ID != this.<currentPacketId>__0))
                            {
                                InputManager.Get().DropSubOptionParentCard();
                                goto Label_00D4;
                            }
                        }
                        break;
                    }
                    goto Label_00D4;

                default:
                    goto Label_00D4;
            }
            if (this.<>f__this.IsWaitingToShowSubOptions())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.ShowSubOptions();
            this.$PC = -1;
        Label_00D4:
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
    private sealed class <WasCardChosen>c__AnonStorey2F2
    {
        internal int entityId;

        internal bool <>m__EF(int currEntityId)
        {
            return (this.entityId == currEntityId);
        }
    }

    [Serializable]
    public class ChoiceData
    {
        public string m_BannerBoneName = "ChoiceBanner";
        public Banner m_BannerPrefab;
        public string m_ButtonBoneName = "ChoiceButton";
        [CustomEditField(T=EditType.GAME_OBJECT)]
        public string m_ButtonPrefab;
        public float m_CardHideTime = 0.2f;
        public float m_CardShowTime = 0.2f;
        public string m_FriendlyBoneName = "FriendlyChoice";
        public float m_HorizontalPadding = 0.75f;
        public float m_MinShowTime = 1f;
        public string m_OpponentBoneName = "OpponentChoice";
        public float m_PhoneMaxHorizontalPadding = 0.1f;
        public float m_UiShowTime = 0.5f;
    }

    [Serializable]
    public class ChoiceEffectData
    {
        public bool m_AlwaysPlayChoiceEffects;
        public Spell m_DiscoverCardEffect;
    }

    private class ChoiceState
    {
        public List<Card> m_cards = new List<Card>();
        public PowerTaskList m_preTaskList;
        public bool m_waitingToShow;
    }

    [Serializable]
    public class CommonData
    {
        public float m_FriendlyCardWidth = 2.85f;
        public float m_OpponentCardWidth = 1.5f;
        public int m_PhoneMaxCardsBeforeAdjusting = 3;
        public float m_PhoneMaxCardScale = 0.85f;
    }

    [Serializable]
    public class SubOptionData
    {
        public float m_AdjacentCardXOffset = 0.75f;
        public string m_BoneName = "SubOption";
        public float m_CardShowTime = 0.2f;
        public float m_MinionParentXOffset = 0.9f;
        public float m_PhoneMaxAdjacentCardXOffset = 0.1f;
    }

    private class SubOptionState
    {
        public List<Card> m_cards = new List<Card>();
        public Card m_parentCard;
    }
}

