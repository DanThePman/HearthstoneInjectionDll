using PegasusGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HistoryManager : MonoBehaviour
{
    [CompilerGenerated]
    private static Spell.StateFinishedCallback <>f__am$cache10;
    private const float BATTLEFIELD_CARD_DISPLAY_TIME = 3f;
    private Vector3[] bigCardPath;
    private HistoryCard currentBigCard;
    private HistoryCard currentlyMousedOverTile;
    public Texture fatigueTexture;
    private const float HERO_POWER_DISPLAY_TIME = 4f;
    private bool historyDisabled;
    private bool m_animatingDesat;
    private bool m_animatingVignette;
    private List<HistoryCard> m_historyTiles;
    public Texture m_hunterSecretTexture;
    public Texture m_mageSecretTexture;
    public Texture m_paladinSecretTexture;
    private List<HistoryEntry> m_queuedEntries;
    public SoundDucker m_SoundDucker;
    private static HistoryManager s_instance;
    private const float SECRET_CARD_DISPLAY_TIME = 4f;
    private float sizeOfBigTile;
    private float SPACE_BETWEEN_TILES = 0.15f;
    private const float SPELL_CARD_DISPLAY_TIME = 4f;

    private void AnimateVignetteIn()
    {
        FullScreenEffects component = Camera.main.GetComponent<FullScreenEffects>();
        this.m_animatingVignette = component.VignettingEnable;
        if (this.m_animatingVignette)
        {
            object[] args = new object[] { 
                "from", component.VignettingIntensity, "to", 0.6f, "time", 0.4f, "easetype", iTween.EaseType.easeInOutQuad, "onupdate", "OnUpdateVignetteVal", "onupdatetarget", base.gameObject, "name", "historyVig", "oncomplete", "OnVignetteInFinished", 
                "oncompletetarget", base.gameObject
             };
            Hashtable hashtable = iTween.Hash(args);
            iTween.StopByName(Camera.main.gameObject, "historyVig");
            iTween.ValueTo(Camera.main.gameObject, hashtable);
        }
        this.m_animatingDesat = component.DesaturationEnabled;
        if (this.m_animatingDesat)
        {
            object[] objArray2 = new object[] { 
                "from", component.Desaturation, "to", 1f, "time", 0.4f, "easetype", iTween.EaseType.easeInOutQuad, "onupdate", "OnUpdateDesatVal", "onupdatetarget", base.gameObject, "name", "historyDesat", "oncomplete", "OnDesatInFinished", 
                "oncompletetarget", base.gameObject
             };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.StopByName(Camera.main.gameObject, "historyDesat");
            iTween.ValueTo(Camera.main.gameObject, hashtable2);
        }
    }

    private void AnimateVignetteOut()
    {
        FullScreenEffects component = Camera.main.GetComponent<FullScreenEffects>();
        this.m_animatingVignette = component.VignettingEnable;
        if (this.m_animatingVignette)
        {
            object[] args = new object[] { 
                "from", component.VignettingIntensity, "to", 0f, "time", 0.4f, "easetype", iTween.EaseType.easeInOutQuad, "onupdate", "OnUpdateVignetteVal", "onupdatetarget", base.gameObject, "name", "historyVig", "oncomplete", "OnVignetteOutFinished", 
                "oncompletetarget", base.gameObject
             };
            Hashtable hashtable = iTween.Hash(args);
            iTween.StopByName(Camera.main.gameObject, "historyVig");
            iTween.ValueTo(Camera.main.gameObject, hashtable);
        }
        this.m_animatingDesat = component.DesaturationEnabled;
        if (this.m_animatingDesat)
        {
            object[] objArray2 = new object[] { 
                "from", component.Desaturation, "to", 0f, "time", 0.4f, "easetype", iTween.EaseType.easeInOutQuad, "onupdate", "OnUpdateDesatVal", "onupdatetarget", base.gameObject, "name", "historyDesat", "oncomplete", "OnDesatOutFinished", 
                "oncompletetarget", base.gameObject
             };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.StopByName(Camera.main.gameObject, "historyDesat");
            iTween.ValueTo(Camera.main.gameObject, hashtable2);
        }
    }

    private void AttackHistoryCardLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        List<HistoryInfo> cards = (List<HistoryInfo>) callbackData;
        HistoryInfo info = cards[0];
        cards.RemoveAt(0);
        HistoryCard component = actorObject.GetComponent<HistoryCard>();
        this.m_historyTiles.Add(component);
        component.SetCardInfo(info.GetDuplicatedEntity(), info.m_bigCardPortraitTexture, info.GetSplatAmount(), info.m_died, null, false, false, info.m_bigCardGoldenMaterial);
        component.LoadAttackTileActor();
        if (info.GetOriginalEntity().GetCardType() == TAG_CARDTYPE.HERO_POWER)
        {
            HeroSkinHeroPower componentInChildren = info.GetCard().GetComponentInChildren<HeroSkinHeroPower>();
            if (componentInChildren != null)
            {
                component.SetHeroPowerFrontTexture(componentInChildren.GetFrontTexture());
            }
        }
        this.LoadHistoryChildren(component, cards);
    }

    private void Awake()
    {
        s_instance = this;
        this.m_queuedEntries = new List<HistoryEntry>();
        base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + 0.15f, base.transform.position.z);
    }

    private void BigCardLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        BigCardEntry entry = (BigCardEntry) callbackData;
        HistoryInfo cardInfo = entry.cardInfo;
        Entity originalEntity = cardInfo.GetOriginalEntity();
        Card card = cardInfo.GetCard();
        if ((originalEntity.GetCardType() == TAG_CARDTYPE.SPELL) || (originalEntity.GetCardType() == TAG_CARDTYPE.HERO_POWER))
        {
            actorObject.transform.position = card.transform.position;
            actorObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
        else
        {
            actorObject.transform.position = this.GetBigCardPosition();
        }
        HistoryCard component = actorObject.GetComponent<HistoryCard>();
        component.SetCardInfo(cardInfo.GetDuplicatedEntity(), cardInfo.m_bigCardPortraitTexture, 0, false, entry.callbackFunction, entry.wasCountered, entry.waitForSecretSpell, cardInfo.m_bigCardGoldenMaterial);
        if (originalEntity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
        {
            HeroSkinHeroPower componentInChildren = card.GetComponentInChildren<HeroSkinHeroPower>();
            if (componentInChildren != null)
            {
                component.SetHeroPowerFrontTexture(componentInChildren.GetFrontTexture());
            }
        }
        component.LoadBigCardActor(true);
    }

    private void CheckForMouseOff()
    {
        if (this.currentlyMousedOverTile != null)
        {
            this.currentlyMousedOverTile.NotifyMousedOut();
            this.currentlyMousedOverTile = null;
            this.m_SoundDucker.StopDucking();
            this.FadeVignetteOut();
        }
    }

    private void ChildLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        HistoryCallbackData data = (HistoryCallbackData) callbackData;
        HistoryCard parentCard = data.parentCard;
        HistoryInfo sourceCard = data.sourceCard;
        HistoryChildCard component = actorObject.GetComponent<HistoryChildCard>();
        component.SetCardInfo(sourceCard.GetDuplicatedEntity(), sourceCard.m_bigCardPortraitTexture, sourceCard.GetSplatAmount(), sourceCard.m_died, sourceCard.m_bigCardGoldenMaterial);
        component.transform.parent = parentCard.transform;
        parentCard.AddHistoryChild(component);
        component.LoadActor();
    }

    [DebuggerHidden]
    private IEnumerator ClearBigCard()
    {
        return new <ClearBigCard>c__Iterator98 { <>f__this = this };
    }

    public void CreateAttackTile(Entity attacker, Entity defender, PowerTaskList taskList)
    {
        if (!this.historyDisabled)
        {
            HistoryEntry item = new HistoryEntry();
            this.m_queuedEntries.Add(item);
            item.SetAttacker(attacker);
            item.SetDefender(defender);
            Entity duplicatedEntity = item.m_lastAttacker.GetDuplicatedEntity();
            Entity entity = item.m_lastDefender.GetDuplicatedEntity();
            int entityId = attacker.GetEntityId();
            int num2 = defender.GetEntityId();
            int num3 = -1;
            List<PowerTask> list = taskList.GetTaskList();
            for (int i = 0; i < list.Count; i++)
            {
                Network.PowerHistory power = list[i].GetPower();
                if (power.Type == Network.PowerType.META_DATA)
                {
                    Network.HistMetaData data = (Network.HistMetaData) power;
                    if ((data.MetaType == HistoryMeta.Type.DAMAGE) && data.Info.Contains(num2))
                    {
                        num3 = i;
                        break;
                    }
                }
            }
            for (int j = 0; j < num3; j++)
            {
                Network.PowerHistory history2 = list[j].GetPower();
                switch (history2.Type)
                {
                    case Network.PowerType.SHOW_ENTITY:
                    {
                        Network.HistShowEntity showEntity = (Network.HistShowEntity) history2;
                        if (entityId == showEntity.Entity.ID)
                        {
                            GameUtils.ApplyShowEntity(duplicatedEntity, showEntity);
                        }
                        if (num2 == showEntity.Entity.ID)
                        {
                            GameUtils.ApplyShowEntity(entity, showEntity);
                        }
                        break;
                    }
                    case Network.PowerType.HIDE_ENTITY:
                    {
                        Network.HistHideEntity hideEntity = (Network.HistHideEntity) history2;
                        if (entityId == hideEntity.Entity)
                        {
                            GameUtils.ApplyHideEntity(duplicatedEntity, hideEntity);
                        }
                        if (num2 == hideEntity.Entity)
                        {
                            GameUtils.ApplyHideEntity(entity, hideEntity);
                        }
                        break;
                    }
                    case Network.PowerType.TAG_CHANGE:
                    {
                        Network.HistTagChange tagChange = (Network.HistTagChange) history2;
                        if (entityId == tagChange.Entity)
                        {
                            GameUtils.ApplyTagChange(duplicatedEntity, tagChange);
                        }
                        if (num2 == tagChange.Entity)
                        {
                            GameUtils.ApplyTagChange(entity, tagChange);
                        }
                        break;
                    }
                }
            }
        }
    }

    public void CreateCardPlayedTile(Entity playedEntity, Entity targetedEntity)
    {
        if (!this.historyDisabled)
        {
            HistoryEntry item = new HistoryEntry();
            this.m_queuedEntries.Add(item);
            item.SetCardPlayed(playedEntity);
            item.SetCardTargeted(targetedEntity);
            if (item.m_lastCardPlayed.GetDuplicatedEntity() == null)
            {
                base.StartCoroutine("WaitForCardLoadedAndDuplicateInfo", item.m_lastCardPlayed);
            }
        }
    }

    public void CreateFatigueTile()
    {
        if (!this.historyDisabled)
        {
            HistoryEntry item = new HistoryEntry();
            this.m_queuedEntries.Add(item);
            item.SetFatigue();
        }
    }

    public void CreatePlayedBigCard(Entity entity, FinishedCallback functionToCall, bool wasCountered)
    {
        if (!GameState.Get().GetGameEntity().ShouldShowBigCard())
        {
            functionToCall();
        }
        else
        {
            base.StopCoroutine("WaitForCardLoadedAndCreateBigCard");
            BigCardEntry entry = new BigCardEntry {
                cardInfo = new HistoryInfo()
            };
            entry.cardInfo.SetOriginalEntity(entity);
            if (entity.IsWeapon())
            {
                entry.cardInfo.m_infoType = HistoryInfoType.WEAPON_PLAYED;
            }
            else
            {
                entry.cardInfo.m_infoType = HistoryInfoType.CARD_PLAYED;
            }
            entry.callbackFunction = functionToCall;
            entry.wasCountered = wasCountered;
            entry.waitForSecretSpell = false;
            base.StartCoroutine("WaitForCardLoadedAndCreateBigCard", entry);
        }
    }

    public void CreateSecretBigCard(Entity entity, FinishedCallback callback)
    {
        this.CreateTriggeredBigCard(entity, callback, true);
    }

    public void CreateTriggeredBigCard(Entity entity, FinishedCallback callback)
    {
        this.CreateTriggeredBigCard(entity, callback, false);
    }

    private void CreateTriggeredBigCard(Entity entity, FinishedCallback callback, bool secret)
    {
        base.StopCoroutine("WaitForCardLoadedAndCreateBigCard");
        BigCardEntry entry = new BigCardEntry {
            cardInfo = new HistoryInfo()
        };
        entry.cardInfo.SetOriginalEntity(entity);
        entry.cardInfo.m_infoType = HistoryInfoType.TRIGGER;
        entry.callbackFunction = callback;
        entry.waitForSecretSpell = secret;
        base.StartCoroutine("WaitForCardLoadedAndCreateBigCard", entry);
    }

    public void CreateTriggerTile(Entity triggeredEntity)
    {
        if (!this.historyDisabled)
        {
            HistoryEntry item = new HistoryEntry();
            this.m_queuedEntries.Add(item);
            item.SetCardTriggered(triggeredEntity);
        }
    }

    private void DestroyBigCard()
    {
        if ((this.currentBigCard != null) && (this.currentBigCard.m_tileActor == null))
        {
            this.currentBigCard.RunCallbackNow();
            if (this.currentBigCard.m_mainCardActor != null)
            {
                if (this.currentBigCard.WasCountered())
                {
                    if (<>f__am$cache10 == null)
                    {
                        <>f__am$cache10 = delegate (Spell s, SpellStateType prevStateType, object d) {
                            if (s.GetActiveState() == SpellStateType.NONE)
                            {
                                HistoryCard card = d as HistoryCard;
                                if (card != null)
                                {
                                    UnityEngine.Object.Destroy(card.gameObject);
                                }
                            }
                        };
                    }
                    Spell.StateFinishedCallback callback = <>f__am$cache10;
                    Spell spell = this.currentBigCard.m_mainCardActor.GetSpell(SpellType.DEATH);
                    if (spell == null)
                    {
                        this.currentBigCard.m_mainCardActor.Hide();
                        UnityEngine.Object.Destroy(this.currentBigCard.gameObject);
                        this.currentBigCard = null;
                    }
                    else
                    {
                        spell.AddStateFinishedCallback(callback, this.currentBigCard);
                        this.currentBigCard = null;
                        spell.Activate();
                    }
                }
                else
                {
                    this.currentBigCard.m_mainCardActor.Hide();
                    UnityEngine.Object.Destroy(this.currentBigCard.gameObject);
                    this.currentBigCard = null;
                }
            }
        }
    }

    private void DestroyHistoryTilesThatFallOffTheEnd()
    {
        if (this.sizeOfBigTile > 0f)
        {
            float num = 0f;
            float z = base.GetComponent<Collider>().bounds.size.z;
            for (int i = 0; i < this.m_historyTiles.Count; i++)
            {
                if (this.m_historyTiles[i].GetHalfSize())
                {
                    num += this.sizeOfBigTile / 2f;
                }
                else
                {
                    num += this.sizeOfBigTile;
                }
            }
            num += this.SPACE_BETWEEN_TILES * (this.m_historyTiles.Count - 1);
            while (num > z)
            {
                if (this.m_historyTiles[0].GetHalfSize())
                {
                    num -= this.sizeOfBigTile / 2f;
                }
                else
                {
                    num -= this.sizeOfBigTile;
                }
                num -= this.SPACE_BETWEEN_TILES;
                UnityEngine.Object.Destroy(this.m_historyTiles[0].gameObject);
                this.m_historyTiles.RemoveAt(0);
            }
        }
    }

    public void DisableHistory()
    {
        this.historyDisabled = true;
    }

    private void FadeVignetteIn()
    {
        foreach (HistoryCard card in this.m_historyTiles)
        {
            if (card.m_tileActor != null)
            {
                SceneUtils.SetLayer(card.m_tileActor.gameObject, GameLayer.IgnoreFullScreenEffects);
            }
        }
        SceneUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
        FullScreenEffects component = Camera.main.GetComponent<FullScreenEffects>();
        component.VignettingEnable = true;
        component.DesaturationEnabled = true;
        this.AnimateVignetteIn();
    }

    private void FadeVignetteOut()
    {
        foreach (HistoryCard card in this.m_historyTiles)
        {
            if (card.m_tileActor != null)
            {
                SceneUtils.SetLayer(card.GetTileCollider().gameObject, GameLayer.Default);
            }
        }
        SceneUtils.SetLayer(base.gameObject, GameLayer.CardRaycast);
        this.AnimateVignetteOut();
    }

    public static HistoryManager Get()
    {
        return s_instance;
    }

    public Vector3[] GetBigCardPath()
    {
        return this.bigCardPath;
    }

    public Vector3 GetBigCardPosition()
    {
        return Board.Get().FindBone("BigCardPosition").position;
    }

    public HistoryCard GetCurrentBigCard()
    {
        return this.currentBigCard;
    }

    private HistoryEntry GetCurrentHistoryEntry()
    {
        if (this.m_queuedEntries.Count != 0)
        {
            for (int i = this.m_queuedEntries.Count - 1; i >= 0; i--)
            {
                if (!this.m_queuedEntries[i].m_complete)
                {
                    return this.m_queuedEntries[i];
                }
            }
        }
        return null;
    }

    public int GetIndexForTile(HistoryCard tile)
    {
        for (int i = 0; i < this.m_historyTiles.Count; i++)
        {
            if (this.m_historyTiles[i] == tile)
            {
                return i;
            }
        }
        UnityEngine.Debug.LogWarning("HistoryManager.GetIndexForTile() - that Tile doesn't exist!");
        return -1;
    }

    public int GetNumHistoryTiles()
    {
        return this.m_historyTiles.Count;
    }

    public Vector3 GetTopTilePosition()
    {
        return new Vector3(base.transform.position.x, base.transform.position.y - 0.15f, base.transform.position.z);
    }

    public void HandleClickOnBigCard(HistoryCard card)
    {
        if (this.currentBigCard == card)
        {
            this.StopTimerAndKillBigCardNow();
        }
    }

    public bool HasBigCard()
    {
        return (this.currentBigCard != null);
    }

    private bool IsDeadInLaterHistoryEntry(Entity entity)
    {
        bool flag = false;
        for (int i = this.m_queuedEntries.Count - 1; i >= 0; i--)
        {
            HistoryEntry entry = this.m_queuedEntries[i];
            if (!entry.m_complete)
            {
                return flag;
            }
            for (int j = 0; j < entry.m_affectedCards.Count; j++)
            {
                HistoryInfo info = entry.m_affectedCards[j];
                if ((info.GetOriginalEntity() == entity) && (info.m_died || (info.GetSplatAmount() >= entity.GetRemainingHP())))
                {
                    flag = true;
                }
            }
        }
        return false;
    }

    private bool IsEntityTheAffectedCard(Entity entity, int index)
    {
        HistoryEntry currentHistoryEntry = this.GetCurrentHistoryEntry();
        return ((currentHistoryEntry.m_affectedCards[index] != null) && (entity == currentHistoryEntry.m_affectedCards[index].GetOriginalEntity()));
    }

    private bool IsEntityTheLastAttacker(Entity entity)
    {
        HistoryEntry currentHistoryEntry = this.GetCurrentHistoryEntry();
        return ((currentHistoryEntry.m_lastAttacker != null) && (entity == currentHistoryEntry.m_lastAttacker.GetOriginalEntity()));
    }

    private bool IsEntityTheLastCardPlayed(Entity entity)
    {
        HistoryEntry currentHistoryEntry = this.GetCurrentHistoryEntry();
        return ((currentHistoryEntry.m_lastCardPlayed != null) && (entity == currentHistoryEntry.m_lastCardPlayed.GetOriginalEntity()));
    }

    private bool IsEntityTheLastCardTargeted(Entity entity)
    {
        HistoryEntry currentHistoryEntry = this.GetCurrentHistoryEntry();
        return ((currentHistoryEntry.m_lastCardTargeted != null) && (entity == currentHistoryEntry.m_lastCardTargeted.GetOriginalEntity()));
    }

    private bool IsEntityTheLastDefender(Entity entity)
    {
        HistoryEntry currentHistoryEntry = this.GetCurrentHistoryEntry();
        return ((currentHistoryEntry.m_lastDefender != null) && (entity == currentHistoryEntry.m_lastDefender.GetOriginalEntity()));
    }

    public void LoadChildren(List<HistoryCallbackData> loadInfo)
    {
        foreach (HistoryCallbackData data in loadInfo)
        {
            AssetLoader.Get().LoadActor("HistoryChildCard", new AssetLoader.GameObjectCallback(this.ChildLoadedCallback), data, false);
        }
    }

    private void LoadHistoryChildren(HistoryCard parentCard, List<HistoryInfo> cards)
    {
        if (cards.Count >= 1)
        {
            List<HistoryCallbackData> callbacks = new List<HistoryCallbackData>();
            for (int i = 0; i < cards.Count; i++)
            {
                HistoryCallbackData item = new HistoryCallbackData {
                    parentCard = parentCard,
                    sourceCard = cards[i]
                };
                callbacks.Add(item);
            }
            parentCard.SetLoadChildrenInfo(callbacks);
        }
    }

    private void LoadNextHistoryEntry()
    {
        if ((this.m_queuedEntries.Count != 0) && this.m_queuedEntries[0].m_complete)
        {
            base.StartCoroutine(this.LoadNextHistoryEntryWhenLoaded());
        }
    }

    [DebuggerHidden]
    private IEnumerator LoadNextHistoryEntryWhenLoaded()
    {
        return new <LoadNextHistoryEntryWhenLoaded>c__Iterator9B { <>f__this = this };
    }

    public void MarkCurrentHistoryEntryAsCompleted()
    {
        if (!this.historyDisabled)
        {
            this.GetCurrentHistoryEntry().m_complete = true;
            this.LoadNextHistoryEntry();
        }
    }

    public void NotifyAboutAdditionalTarget(int entityID)
    {
        if (!this.historyDisabled)
        {
            Entity entity = GameState.Get().GetEntity(entityID);
            if (entity == null)
            {
                base.StartCoroutine(this.WaitForEntityThenNotify(entityID));
            }
            else
            {
                this.NotifyAboutPreDamage(entity);
            }
        }
    }

    public void NotifyAboutArmorChanged(Entity entity, int newArmor)
    {
        if (!this.historyDisabled)
        {
            int num = entity.GetArmor() - newArmor;
            if ((num > 0) && !this.IsEntityTheLastCardPlayed(entity))
            {
                HistoryEntry currentHistoryEntry = this.GetCurrentHistoryEntry();
                if (this.IsEntityTheLastAttacker(entity))
                {
                    currentHistoryEntry.m_lastAttacker.m_armorChangeAmount = num;
                }
                else if (this.IsEntityTheLastDefender(entity))
                {
                    currentHistoryEntry.m_lastDefender.m_armorChangeAmount = num;
                }
                else if (this.IsEntityTheLastCardTargeted(entity))
                {
                    currentHistoryEntry.m_lastCardTargeted.m_armorChangeAmount = num;
                }
                else
                {
                    for (int i = 0; i < currentHistoryEntry.m_affectedCards.Count; i++)
                    {
                        if (this.IsEntityTheAffectedCard(entity, i))
                        {
                            currentHistoryEntry.m_affectedCards[i].m_armorChangeAmount = num;
                            return;
                        }
                    }
                    this.NotifyAboutPreDamage(entity);
                    this.NotifyAboutDamageChanged(entity, newArmor);
                }
            }
        }
    }

    public void NotifyAboutCardDeath(Entity entity)
    {
        if ((!this.historyDisabled && !entity.IsEnchantment()) && !this.IsEntityTheLastCardPlayed(entity))
        {
            HistoryEntry currentHistoryEntry = this.GetCurrentHistoryEntry();
            if (this.IsEntityTheLastAttacker(entity))
            {
                currentHistoryEntry.m_lastAttacker.m_died = true;
            }
            else if (this.IsEntityTheLastDefender(entity))
            {
                currentHistoryEntry.m_lastDefender.m_died = true;
            }
            else if (this.IsEntityTheLastCardTargeted(entity))
            {
                currentHistoryEntry.m_lastCardTargeted.m_died = true;
            }
            else
            {
                for (int i = 0; i < currentHistoryEntry.m_affectedCards.Count; i++)
                {
                    if (this.IsEntityTheAffectedCard(entity, i))
                    {
                        currentHistoryEntry.m_affectedCards[i].m_died = true;
                        return;
                    }
                }
                if (!this.IsDeadInLaterHistoryEntry(entity))
                {
                    this.NotifyAboutPreDamage(entity);
                    this.NotifyAboutCardDeath(entity);
                }
            }
        }
    }

    public void NotifyAboutDamageChanged(Entity entity, int damage)
    {
        if ((entity != null) && !this.historyDisabled)
        {
            HistoryEntry currentHistoryEntry = this.GetCurrentHistoryEntry();
            int num = damage - entity.GetDamage();
            if (this.IsEntityTheLastCardPlayed(entity))
            {
                currentHistoryEntry.m_lastCardPlayed.m_damageChangeAmount = num;
            }
            else if (this.IsEntityTheLastAttacker(entity))
            {
                currentHistoryEntry.m_lastAttacker.m_damageChangeAmount = num;
            }
            else if (this.IsEntityTheLastDefender(entity))
            {
                currentHistoryEntry.m_lastDefender.m_damageChangeAmount = num;
            }
            else if (this.IsEntityTheLastCardTargeted(entity))
            {
                currentHistoryEntry.m_lastCardTargeted.m_damageChangeAmount = num;
            }
            else
            {
                for (int i = 0; i < currentHistoryEntry.m_affectedCards.Count; i++)
                {
                    if (this.IsEntityTheAffectedCard(entity, i))
                    {
                        currentHistoryEntry.m_affectedCards[i].m_damageChangeAmount = num;
                        return;
                    }
                }
                this.NotifyAboutPreDamage(entity);
                this.NotifyAboutDamageChanged(entity, damage);
            }
        }
    }

    public void NotifyAboutPreDamage(Entity entity)
    {
        if ((((!this.historyDisabled && !entity.IsEnchantment()) && !this.IsEntityTheLastAttacker(entity)) && !this.IsEntityTheLastDefender(entity)) && !this.IsEntityTheLastCardTargeted(entity))
        {
            HistoryEntry currentHistoryEntry = this.GetCurrentHistoryEntry();
            if ((currentHistoryEntry.m_lastCardPlayed == null) || (entity != currentHistoryEntry.m_lastCardPlayed.GetOriginalEntity()))
            {
                for (int i = 0; i < currentHistoryEntry.m_affectedCards.Count; i++)
                {
                    if (this.IsEntityTheAffectedCard(entity, i))
                    {
                        return;
                    }
                }
                HistoryInfo item = new HistoryInfo();
                item.SetOriginalEntity(entity);
                currentHistoryEntry.m_affectedCards.Add(item);
            }
        }
    }

    public void NotifyOfInput(float zPosition)
    {
        if (this.m_historyTiles.Count == 0)
        {
            this.CheckForMouseOff();
        }
        else
        {
            float num = 1000f;
            float num2 = -1000f;
            float num3 = 1000f;
            HistoryCard card = null;
            foreach (HistoryCard card2 in this.m_historyTiles)
            {
                if (card2.HasBeenShown())
                {
                    Collider tileCollider = card2.GetTileCollider();
                    if (tileCollider != null)
                    {
                        float num4 = tileCollider.bounds.center.z - tileCollider.bounds.extents.z;
                        float num5 = tileCollider.bounds.center.z + tileCollider.bounds.extents.z;
                        if (num4 < num)
                        {
                            num = num4;
                        }
                        if (num5 > num2)
                        {
                            num2 = num5;
                        }
                        float num6 = Mathf.Abs((float) (zPosition - num4));
                        if (num6 < num3)
                        {
                            num3 = num6;
                            card = card2;
                        }
                        float num7 = Mathf.Abs((float) (zPosition - num5));
                        if (num7 < num3)
                        {
                            num3 = num7;
                            card = card2;
                        }
                    }
                }
            }
            if ((zPosition < num) || (zPosition > num2))
            {
                this.CheckForMouseOff();
            }
            else if (card == null)
            {
                this.CheckForMouseOff();
            }
            else
            {
                this.m_SoundDucker.StartDucking();
                if (card != this.currentlyMousedOverTile)
                {
                    if (this.currentlyMousedOverTile != null)
                    {
                        this.currentlyMousedOverTile.NotifyMousedOut();
                    }
                    else
                    {
                        this.FadeVignetteIn();
                    }
                    this.currentlyMousedOverTile = card;
                    card.NotifyMousedOver();
                }
            }
        }
    }

    public void NotifyOfMouseOff()
    {
        this.CheckForMouseOff();
    }

    public void NotifyOfSecretSpellFinished()
    {
        this.currentBigCard.NotifyOfSecretFinished();
    }

    private void OnDesatInFinished()
    {
        this.m_animatingDesat = false;
    }

    private void OnDesatOutFinished()
    {
        this.m_animatingDesat = false;
        Camera.main.GetComponent<FullScreenEffects>().DesaturationEnabled = false;
        this.OnFullScreenEffectOutFinished();
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnFullScreenEffectOutFinished()
    {
        if (!this.m_animatingDesat && !this.m_animatingVignette)
        {
            Camera.main.GetComponent<FullScreenEffects>().Disable();
            foreach (HistoryCard card in this.m_historyTiles)
            {
                if (card.m_tileActor != null)
                {
                    SceneUtils.SetLayer(card.m_tileActor.gameObject, GameLayer.Default);
                }
            }
        }
    }

    public void OnHistoryTileComplete()
    {
        if (this.m_queuedEntries.Count > 0)
        {
            this.LoadNextHistoryEntry();
        }
    }

    private void OnUpdateDesatVal(float val)
    {
        Camera.main.GetComponent<FullScreenEffects>().Desaturation = val;
    }

    private void OnUpdateVignetteVal(float val)
    {
        Camera.main.GetComponent<FullScreenEffects>().VignettingIntensity = val;
    }

    private void OnVignetteInFinished()
    {
        this.m_animatingVignette = false;
    }

    private void OnVignetteOutFinished()
    {
        this.m_animatingVignette = false;
        Camera.main.GetComponent<FullScreenEffects>().VignettingEnable = false;
        this.OnFullScreenEffectOutFinished();
    }

    public void SetAsideTileAndTryToUpdate(HistoryCard tile)
    {
        Vector3 topTilePosition = this.GetTopTilePosition();
        tile.transform.position = new Vector3(topTilePosition.x - 20f, topTilePosition.y, topTilePosition.z);
        this.UpdateLayout();
    }

    public void SetBigCard(HistoryCard newCard, bool delayTimer)
    {
        this.StopTimerAndKillBigCardNow();
        this.currentBigCard = newCard;
        if (!delayTimer)
        {
            base.StartCoroutine("ClearBigCard");
        }
    }

    public void SetBigTileSize(float size)
    {
        this.sizeOfBigTile = size;
    }

    private void Start()
    {
        this.m_historyTiles = new List<HistoryCard>();
        base.StartCoroutine(this.WaitForBoardLoadedAndSetPaths());
    }

    public void StartBigCardTimer()
    {
        base.StartCoroutine("ClearBigCard");
    }

    private void StopTimerAndKillBigCardNow()
    {
        base.StopCoroutine("ClearBigCard");
        this.DestroyBigCard();
    }

    private void TileHistoryCardLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        List<HistoryInfo> cards = (List<HistoryInfo>) callbackData;
        HistoryInfo sourceCard = cards[0];
        cards.RemoveAt(0);
        HistoryCard component = actorObject.GetComponent<HistoryCard>();
        this.m_historyTiles.Add(component);
        if (sourceCard.m_infoType == HistoryInfoType.FATIGUE)
        {
            component.SetFatigue(this.fatigueTexture);
            component.LoadCorrectTileActor(sourceCard);
            this.LoadHistoryChildren(component, cards);
        }
        else
        {
            Entity duplicatedEntity = sourceCard.GetDuplicatedEntity();
            if ((duplicatedEntity.IsSecret() && duplicatedEntity.IsHidden()) && duplicatedEntity.IsControlledByConcealedPlayer())
            {
                if (duplicatedEntity.GetClass() == TAG_CLASS.PALADIN)
                {
                    sourceCard.m_bigCardPortraitTexture = this.m_paladinSecretTexture;
                }
                else if (duplicatedEntity.GetClass() == TAG_CLASS.HUNTER)
                {
                    sourceCard.m_bigCardPortraitTexture = this.m_hunterSecretTexture;
                }
                else
                {
                    sourceCard.m_bigCardPortraitTexture = this.m_mageSecretTexture;
                }
            }
            if (((duplicatedEntity.GetController() != null) && !duplicatedEntity.GetController().IsFriendlySide()) && duplicatedEntity.IsObfuscated())
            {
                sourceCard.m_bigCardPortraitTexture = this.m_paladinSecretTexture;
            }
            component.SetCardInfo(duplicatedEntity, sourceCard.m_bigCardPortraitTexture, sourceCard.GetSplatAmount(), sourceCard.m_died, null, false, false, sourceCard.m_bigCardGoldenMaterial);
            if (duplicatedEntity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
            {
                HeroSkinHeroPower componentInChildren = sourceCard.GetCard().GetComponentInChildren<HeroSkinHeroPower>();
                if (componentInChildren != null)
                {
                    component.SetHeroPowerFrontTexture(componentInChildren.GetFrontTexture());
                }
            }
            component.AssignMaterials(sourceCard.GetCard().GetCardDef());
            component.LoadCorrectTileActor(sourceCard);
            this.LoadHistoryChildren(component, cards);
        }
    }

    public void UpdateLayout()
    {
        if (!this.UserIsMousedOverAHistoryTile())
        {
            float num = 0f;
            Vector3 topTilePosition = this.GetTopTilePosition();
            for (int i = this.m_historyTiles.Count - 1; i >= 0; i--)
            {
                int num3 = 0;
                if (this.m_historyTiles[i].GetHalfSize())
                {
                    num3 = 1;
                }
                Collider tileCollider = this.m_historyTiles[i].GetTileCollider();
                float num4 = 0f;
                if (tileCollider != null)
                {
                    num4 = tileCollider.bounds.size.z / 2f;
                }
                Vector3 position = new Vector3(topTilePosition.x, topTilePosition.y, (topTilePosition.z - num) + (num3 * num4));
                this.m_historyTiles[i].MarkAsShown();
                iTween.MoveTo(this.m_historyTiles[i].gameObject, position, 1f);
                if (tileCollider != null)
                {
                    num += tileCollider.bounds.size.z + this.SPACE_BETWEEN_TILES;
                }
            }
            this.DestroyHistoryTilesThatFallOffTheEnd();
        }
    }

    private bool UserIsMousedOverAHistoryTile()
    {
        RaycastHit hit;
        if ((UniversalInputManager.Get().GetInputHitInfo(GameLayer.Default.LayerBit(), out hit) && (hit.transform.GetComponentInChildren<HistoryManager>() == null)) && (hit.transform.GetComponentInChildren<HistoryCard>() == null))
        {
            return false;
        }
        float z = hit.point.z;
        float num2 = 1000f;
        float num3 = -1000f;
        foreach (HistoryCard card in this.m_historyTiles)
        {
            if (card.HasBeenShown())
            {
                Collider tileCollider = card.GetTileCollider();
                if (tileCollider != null)
                {
                    float num4 = tileCollider.bounds.center.z - tileCollider.bounds.extents.z;
                    float num5 = tileCollider.bounds.center.z + tileCollider.bounds.extents.z;
                    if (num4 < num2)
                    {
                        num2 = num4;
                    }
                    if (num5 > num3)
                    {
                        num3 = num5;
                    }
                }
            }
        }
        return ((z >= num2) && (z <= num3));
    }

    [DebuggerHidden]
    private IEnumerator WaitForBoardLoadedAndSetPaths()
    {
        return new <WaitForBoardLoadedAndSetPaths>c__Iterator97 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForCardLoadedAndCreateBigCard(BigCardEntry bigCardEntry)
    {
        return new <WaitForCardLoadedAndCreateBigCard>c__Iterator99 { bigCardEntry = bigCardEntry, <$>bigCardEntry = bigCardEntry, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForCardLoadedAndDuplicateInfo(HistoryInfo info)
    {
        return new <WaitForCardLoadedAndDuplicateInfo>c__Iterator9A { info = info, <$>info = info };
    }

    [DebuggerHidden]
    private IEnumerator WaitForEntityThenNotify(int entityID)
    {
        return new <WaitForEntityThenNotify>c__Iterator9C { entityID = entityID, <$>entityID = entityID, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <ClearBigCard>c__Iterator98 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HistoryManager <>f__this;
        internal TAG_CARDTYPE <curCardType>__1;
        internal float <timeToWait>__0;

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
                    this.<timeToWait>__0 = 0f;
                    if (this.<>f__this.currentBigCard.GetEntity() == null)
                    {
                        this.<timeToWait>__0 = 4f;
                        break;
                    }
                    this.<curCardType>__1 = this.<>f__this.currentBigCard.GetEntity().GetCardType();
                    if (this.<curCardType>__1 != TAG_CARDTYPE.SPELL)
                    {
                        if (this.<curCardType>__1 == TAG_CARDTYPE.HERO_POWER)
                        {
                            this.<timeToWait>__0 = 4f + GameState.Get().GetGameEntity().GetAdditionalTimeToWaitForSpells();
                        }
                        else
                        {
                            this.<timeToWait>__0 = 3f;
                        }
                        break;
                    }
                    this.<timeToWait>__0 = 4f + GameState.Get().GetGameEntity().GetAdditionalTimeToWaitForSpells();
                    break;

                case 1:
                    this.<>f__this.DestroyBigCard();
                    this.$PC = -1;
                    goto Label_00FF;

                default:
                    goto Label_00FF;
            }
            this.$current = new WaitForSeconds(this.<timeToWait>__0);
            this.$PC = 1;
            return true;
        Label_00FF:
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
    private sealed class <LoadNextHistoryEntryWhenLoaded>c__Iterator9B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HistoryManager <>f__this;
        internal List<HistoryInfo> <cards>__1;
        internal HistoryManager.HistoryEntry <currentEntry>__0;
        internal HistoryInfo <targetInfo>__2;

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
                    this.<currentEntry>__0 = this.<>f__this.m_queuedEntries[0];
                    this.<>f__this.m_queuedEntries.RemoveAt(0);
                    break;

                case 1:
                    break;

                default:
                    goto Label_0125;
            }
            if (!this.<currentEntry>__0.CanDuplicateAllEntities())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<currentEntry>__0.DuplicateAllEntities();
            this.<cards>__1 = new List<HistoryInfo>();
            this.<cards>__1.Add(this.<currentEntry>__0.GetSourceInfo());
            this.<targetInfo>__2 = this.<currentEntry>__0.GetTargetInfo();
            if (this.<targetInfo>__2 != null)
            {
                this.<cards>__1.Add(this.<targetInfo>__2);
            }
            if (this.<currentEntry>__0.m_affectedCards.Count > 0)
            {
                this.<cards>__1.AddRange(this.<currentEntry>__0.m_affectedCards);
            }
            AssetLoader.Get().LoadActor("HistoryCard", new AssetLoader.GameObjectCallback(this.<>f__this.TileHistoryCardLoadedCallback), this.<cards>__1, false);
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
    private sealed class <WaitForBoardLoadedAndSetPaths>c__Iterator97 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HistoryManager <>f__this;
        internal Transform <bigCardPathPoint>__0;

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
                    if (ZoneMgr.Get() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<bigCardPathPoint>__0 = Board.Get().FindBone("BigCardPathPoint");
                    if (this.<bigCardPathPoint>__0 != null)
                    {
                        this.<>f__this.bigCardPath = new Vector3[3];
                        this.<>f__this.bigCardPath[1] = this.<bigCardPathPoint>__0.position;
                        this.<>f__this.bigCardPath[2] = this.<>f__this.GetBigCardPosition();
                        this.$PC = -1;
                    }
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
    private sealed class <WaitForCardLoadedAndCreateBigCard>c__Iterator99 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HistoryManager.BigCardEntry <$>bigCardEntry;
        internal HistoryManager <>f__this;
        internal HistoryInfo <lastCardInfo>__0;
        internal HistoryManager.BigCardEntry bigCardEntry;

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
                    this.<lastCardInfo>__0 = this.bigCardEntry.cardInfo;
                    break;

                case 1:
                    break;

                default:
                    goto Label_0094;
            }
            if (!this.<lastCardInfo>__0.CanDuplicateEntity())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<lastCardInfo>__0.DuplicateEntity();
            AssetLoader.Get().LoadActor("HistoryCard", new AssetLoader.GameObjectCallback(this.<>f__this.BigCardLoadedCallback), this.bigCardEntry, false);
            this.$PC = -1;
        Label_0094:
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
    private sealed class <WaitForCardLoadedAndDuplicateInfo>c__Iterator9A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HistoryInfo <$>info;
        internal HistoryInfo info;

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
                    if (!this.info.CanDuplicateEntity())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.info.DuplicateEntity();
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
    private sealed class <WaitForEntityThenNotify>c__Iterator9C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>entityID;
        internal HistoryManager <>f__this;
        internal int entityID;

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
                    if (GameState.Get().GetEntity(this.entityID) == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.NotifyAboutPreDamage(GameState.Get().GetEntity(this.entityID));
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

    private class BigCardEntry
    {
        public HistoryManager.FinishedCallback callbackFunction;
        public HistoryInfo cardInfo;
        public bool waitForSecretSpell;
        public bool wasCountered;
    }

    public delegate void FinishedCallback();

    public class HistoryCallbackData
    {
        public HistoryCard parentCard;
        public HistoryInfo sourceCard;
    }

    private class HistoryEntry
    {
        public List<HistoryInfo> m_affectedCards = new List<HistoryInfo>();
        public bool m_complete;
        public HistoryInfo m_fatigueInfo;
        public HistoryInfo m_lastAttacker;
        public HistoryInfo m_lastCardPlayed;
        public HistoryInfo m_lastCardTargeted;
        public HistoryInfo m_lastCardTriggered;
        public HistoryInfo m_lastDefender;

        public bool CanDuplicateAllEntities()
        {
            HistoryInfo sourceInfo = this.GetSourceInfo();
            if (this.ShouldDuplicateEntity(sourceInfo) && !sourceInfo.CanDuplicateEntity())
            {
                return false;
            }
            HistoryInfo targetInfo = this.GetTargetInfo();
            if (this.ShouldDuplicateEntity(targetInfo) && !targetInfo.CanDuplicateEntity())
            {
                return false;
            }
            for (int i = 0; i < this.m_affectedCards.Count; i++)
            {
                HistoryInfo info = this.m_affectedCards[i];
                if (this.ShouldDuplicateEntity(info) && !info.CanDuplicateEntity())
                {
                    return false;
                }
            }
            return true;
        }

        public void DuplicateAllEntities()
        {
            HistoryInfo sourceInfo = this.GetSourceInfo();
            if (this.ShouldDuplicateEntity(sourceInfo))
            {
                sourceInfo.DuplicateEntity();
            }
            HistoryInfo targetInfo = this.GetTargetInfo();
            if (this.ShouldDuplicateEntity(targetInfo))
            {
                targetInfo.DuplicateEntity();
            }
            for (int i = 0; i < this.m_affectedCards.Count; i++)
            {
                HistoryInfo info = this.m_affectedCards[i];
                if (this.ShouldDuplicateEntity(info))
                {
                    info.DuplicateEntity();
                }
            }
        }

        public HistoryInfo GetSourceInfo()
        {
            if (this.m_lastCardPlayed != null)
            {
                return this.m_lastCardPlayed;
            }
            if (this.m_lastAttacker != null)
            {
                return this.m_lastAttacker;
            }
            if (this.m_lastCardTriggered != null)
            {
                return this.m_lastCardTriggered;
            }
            if (this.m_fatigueInfo != null)
            {
                return this.m_fatigueInfo;
            }
            UnityEngine.Debug.LogError("HistoryEntry.GetSourceInfo() - no source info");
            return null;
        }

        public HistoryInfo GetTargetInfo()
        {
            if ((this.m_lastCardPlayed != null) && (this.m_lastCardTargeted != null))
            {
                return this.m_lastCardTargeted;
            }
            if ((this.m_lastAttacker != null) && (this.m_lastDefender != null))
            {
                return this.m_lastDefender;
            }
            return null;
        }

        public void SetAttacker(Entity attacker)
        {
            this.m_lastAttacker = new HistoryInfo();
            this.m_lastAttacker.m_infoType = HistoryInfoType.ATTACK;
            this.m_lastAttacker.SetOriginalEntity(attacker);
        }

        public void SetCardPlayed(Entity entity)
        {
            this.m_lastCardPlayed = new HistoryInfo();
            if (entity.IsWeapon())
            {
                this.m_lastCardPlayed.m_infoType = HistoryInfoType.WEAPON_PLAYED;
            }
            else
            {
                this.m_lastCardPlayed.m_infoType = HistoryInfoType.CARD_PLAYED;
            }
            this.m_lastCardPlayed.SetOriginalEntity(entity);
        }

        public void SetCardTargeted(Entity entity)
        {
            if (entity != null)
            {
                this.m_lastCardTargeted = new HistoryInfo();
                this.m_lastCardTargeted.SetOriginalEntity(entity);
            }
        }

        public void SetCardTriggered(Entity entity)
        {
            if ((!entity.IsGame() && !entity.IsPlayer()) && !entity.IsHero())
            {
                this.m_lastCardTriggered = new HistoryInfo();
                this.m_lastCardTriggered.m_infoType = HistoryInfoType.TRIGGER;
                this.m_lastCardTriggered.SetOriginalEntity(entity);
            }
        }

        public void SetDefender(Entity defender)
        {
            this.m_lastDefender = new HistoryInfo();
            this.m_lastDefender.SetOriginalEntity(defender);
        }

        public void SetFatigue()
        {
            this.m_fatigueInfo = new HistoryInfo();
            this.m_fatigueInfo.m_infoType = HistoryInfoType.FATIGUE;
        }

        public bool ShouldDuplicateEntity(HistoryInfo info)
        {
            if (info == null)
            {
                return false;
            }
            if (info == this.m_fatigueInfo)
            {
                return false;
            }
            return true;
        }
    }
}

