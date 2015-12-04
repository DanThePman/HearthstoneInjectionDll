using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardReward : Reward
{
    private static readonly Map<TAG_CARDTYPE, Vector3> CARD_SCALE;
    private List<Actor> m_actors = new List<Actor>();
    public CardRewardCount m_cardCount;
    public GameObject m_cardParent;
    public GameObject m_duplicateCardParent;
    private CardSoundSpell m_emote;
    private GameObject m_goToRotate;
    public GameObject m_heroCardRoot;
    public GameObject m_nonHeroCardsRoot;

    static CardReward()
    {
        Map<TAG_CARDTYPE, Vector3> map = new Map<TAG_CARDTYPE, Vector3>();
        map.Add(TAG_CARDTYPE.SPELL, new Vector3(1f, 1f, 1f));
        map.Add(TAG_CARDTYPE.MINION, new Vector3(1f, 1f, 1f));
        map.Add(TAG_CARDTYPE.WEAPON, new Vector3(1f, 0.5f, 1f));
        CARD_SCALE = map;
    }

    private void FinishSettingUpActor(Actor actor, CardDef cardDef)
    {
        CardRewardData data = base.Data as CardRewardData;
        actor.SetCardDef(cardDef);
        actor.SetCardFlair(new CardFlair(data.Premium));
        actor.UpdateAllComponents();
    }

    protected override void HideReward()
    {
        base.HideReward();
        base.m_root.SetActive(false);
    }

    protected override void InitData()
    {
        base.SetData(new CardRewardData(), false);
    }

    private void InitRewardText()
    {
        CardRewardData data = base.Data as CardRewardData;
        EntityDef entityDef = DefLoader.Get().GetEntityDef(data.CardID);
        if (!entityDef.IsHero())
        {
            string headline = GameStrings.Get("GLOBAL_REWARD_CARD_HEADLINE");
            string details = string.Empty;
            string source = string.Empty;
            TAG_CARD_SET cardSet = entityDef.GetCardSet();
            TAG_CLASS tag = entityDef.GetClass();
            string className = GameStrings.GetClassName(tag);
            if (GameMgr.Get().IsTutorial())
            {
                details = GameUtils.GetCurrentTutorialCardRewardDetails();
            }
            else if (cardSet == TAG_CARD_SET.CORE)
            {
                int num = 20;
                int basicCardsIOwn = CollectionManager.Get().GetBasicCardsIOwn(tag);
                if (data.Premium == TAG_PREMIUM.GOLDEN)
                {
                    details = string.Empty;
                }
                else
                {
                    if (num == basicCardsIOwn)
                    {
                        data.InnKeeperLine = CardRewardData.InnKeeperTrigger.CORE_CLASS_SET_COMPLETE;
                    }
                    else if (basicCardsIOwn == 4)
                    {
                        data.InnKeeperLine = CardRewardData.InnKeeperTrigger.SECOND_REWARD_EVER;
                    }
                    object[] args = new object[] { basicCardsIOwn, num, className };
                    details = GameStrings.Format("GLOBAL_REWARD_CORE_CARD_DETAILS", args);
                }
            }
            if (base.Data.Origin == NetCache.ProfileNotice.NoticeOrigin.LEVEL_UP)
            {
                TAG_CLASS originData = (TAG_CLASS) ((int) base.Data.OriginData);
                NetCache.HeroLevel heroLevel = GameUtils.GetHeroLevel(originData);
                object[] objArray2 = new object[] { heroLevel.CurrentLevel.Level.ToString(), GameStrings.GetClassName(originData) };
                source = GameStrings.Format("GLOBAL_REWARD_CARD_LEVEL_UP", objArray2);
            }
            else
            {
                source = string.Empty;
            }
            base.SetRewardText(headline, details, source);
        }
    }

    public void MakeActorsUnlit()
    {
        foreach (Actor actor in this.m_actors)
        {
            actor.SetUnlit();
        }
    }

    private void OnActorLoaded(string name, GameObject go, object callbackData)
    {
        EntityDef entityDef = (EntityDef) callbackData;
        Actor component = go.GetComponent<Actor>();
        this.StartSettingUpNonHeroActor(component, entityDef, this.m_cardParent.transform);
        CardRewardData data = base.Data as CardRewardData;
        this.m_cardCount.SetCount(data.Count);
        if (data.Count > 1)
        {
            Actor actor = UnityEngine.Object.Instantiate<Actor>(component);
            this.StartSettingUpNonHeroActor(actor, entityDef, this.m_duplicateCardParent.transform);
        }
        DefLoader.Get().LoadCardDef(entityDef.GetCardId(), new DefLoader.LoadDefCallback<CardDef>(this.OnCardDefLoaded), entityDef, new CardPortraitQuality(3, true));
    }

    private void OnCardDefLoaded(string cardID, CardDef cardDef, object callbackData)
    {
        if (DefLoader.Get().GetEntityDef(cardID) == null)
        {
            Debug.LogWarning(string.Format("OnCardDefLoaded() - entityDef for CardID {0} is null", cardID));
        }
        else
        {
            foreach (Actor actor in this.m_actors)
            {
                this.FinishSettingUpActor(actor, cardDef);
            }
            foreach (EmoteEntryDef def2 in cardDef.m_EmoteDefs)
            {
                if (def2.m_emoteType == EmoteType.START)
                {
                    AssetLoader.Get().LoadSpell(def2.m_emoteSoundSpellPath, new AssetLoader.GameObjectCallback(this.OnStartEmoteLoaded), null, false);
                }
            }
            base.SetReady(true);
        }
    }

    protected override void OnDataSet(bool updateVisuals)
    {
        if (updateVisuals)
        {
            CardRewardData data = base.Data as CardRewardData;
            if (data == null)
            {
                Debug.LogWarning(string.Format("CardReward.SetData() - data {0} is not CardRewardData", base.Data));
            }
            else if (string.IsNullOrEmpty(data.CardID))
            {
                Debug.LogWarning(string.Format("CardReward.SetData() - data {0} has invalid cardID", data));
            }
            else
            {
                base.SetReady(false);
                EntityDef entityDef = DefLoader.Get().GetEntityDef(data.CardID);
                if (entityDef.IsHero())
                {
                    AssetLoader.Get().LoadActor("Card_Play_Hero", new AssetLoader.GameObjectCallback(this.OnHeroActorLoaded), entityDef, false);
                    this.m_goToRotate = this.m_heroCardRoot;
                    this.m_cardCount.Hide();
                    if (data.Premium == TAG_PREMIUM.GOLDEN)
                    {
                        this.SetUpGoldenHeroAchieves();
                    }
                    else
                    {
                        this.SetupHeroAchieves();
                    }
                }
                else
                {
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.m_cardCount.Hide();
                    }
                    string handActor = ActorNames.GetHandActor(entityDef, data.Premium);
                    AssetLoader.Get().LoadActor(handActor, new AssetLoader.GameObjectCallback(this.OnActorLoaded), entityDef, false);
                    this.m_goToRotate = this.m_nonHeroCardsRoot;
                }
            }
        }
    }

    private void OnHeroActorLoaded(string name, GameObject go, object callbackData)
    {
        EntityDef entityDef = (EntityDef) callbackData;
        Actor component = go.GetComponent<Actor>();
        component.SetEntityDef(entityDef);
        component.transform.parent = this.m_heroCardRoot.transform;
        component.transform.localScale = Vector3.one;
        component.transform.localPosition = Vector3.zero;
        component.transform.localRotation = Quaternion.identity;
        component.TurnOffCollider();
        component.m_healthObject.SetActive(false);
        SceneUtils.SetLayer(component.gameObject, GameLayer.IgnoreFullScreenEffects);
        this.m_actors.Add(component);
        DefLoader.Get().LoadCardDef(entityDef.GetCardId(), new DefLoader.LoadDefCallback<CardDef>(this.OnCardDefLoaded), new CardPortraitQuality(3, true), null);
    }

    private void OnStartEmoteLoaded(string name, GameObject go, object callbackData)
    {
        if (go != null)
        {
            CardSoundSpell component = go.GetComponent<CardSoundSpell>();
            if (component != null)
            {
                this.m_emote = component;
            }
        }
    }

    private void PlayHeroEmote()
    {
        if (this.m_emote != null)
        {
            this.m_emote.Reactivate();
        }
    }

    private void SetUpGoldenHeroAchieves()
    {
        string headline = GameStrings.Get("GLOBAL_REWARD_GOLDEN_HERO_HEADLINE");
        base.SetRewardText(headline, string.Empty, string.Empty);
    }

    private void SetupHeroAchieves()
    {
        List<Achievement> achievesInGroup = AchieveManager.Get().GetAchievesInGroup(Achievement.Group.UNLOCK_HERO);
        List<Achievement> list2 = AchieveManager.Get().GetAchievesInGroup(Achievement.Group.UNLOCK_HERO, true);
        int count = achievesInGroup.Count;
        int num2 = list2.Count;
        CardRewardData data = base.Data as CardRewardData;
        string className = GameStrings.GetClassName(DefLoader.Get().GetEntityDef(data.CardID).GetClass());
        object[] args = new object[] { className };
        string headline = GameStrings.Format("GLOBAL_REWARD_HERO_HEADLINE", args);
        object[] objArray2 = new object[] { num2, count };
        string details = GameStrings.Format("GLOBAL_REWARD_HERO_DETAILS", objArray2);
        object[] objArray3 = new object[] { className };
        string source = GameStrings.Format("GLOBAL_REWARD_HERO_SOURCE", objArray3);
        base.SetRewardText(headline, details, source);
    }

    protected override void ShowReward(bool updateCacheValues)
    {
        CardRewardData cardReward = base.Data as CardRewardData;
        if (!cardReward.IsDummyReward && updateCacheValues)
        {
            CollectionManager.Get().AddCardReward(cardReward, true);
        }
        this.InitRewardText();
        base.m_root.SetActive(true);
        this.m_goToRotate.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
        object[] args = new object[] { "amount", new Vector3(0f, 0f, 540f), "time", 1.5f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self };
        Hashtable hashtable = iTween.Hash(args);
        iTween.RotateAdd(this.m_goToRotate.gameObject, hashtable);
        SoundManager.Get().LoadAndPlay("game_end_reward");
        this.PlayHeroEmote();
    }

    private void StartSettingUpNonHeroActor(Actor actor, EntityDef entityDef, Transform parentTransform)
    {
        actor.SetEntityDef(entityDef);
        actor.transform.parent = parentTransform;
        actor.transform.localScale = CARD_SCALE[entityDef.GetCardType()];
        actor.transform.localPosition = Vector3.zero;
        actor.transform.localRotation = Quaternion.identity;
        actor.TurnOffCollider();
        if (base.Data.Origin != NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT)
        {
            SceneUtils.SetLayer(actor.gameObject, GameLayer.IgnoreFullScreenEffects);
        }
        this.m_actors.Add(actor);
    }
}

