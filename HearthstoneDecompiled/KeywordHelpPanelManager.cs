using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class KeywordHelpPanelManager : MonoBehaviour
{
    private const float DELAY_BEFORE_FADE_IN = 0.4f;
    private const float FADE_IN_TIME = 0.125f;
    private Actor m_actor;
    private Card m_card;
    private Pool<KeywordHelpPanel> m_keywordPanelPool = new Pool<KeywordHelpPanel>();
    public KeywordHelpPanel m_keywordPanelPrefab;
    private List<KeywordHelpPanel> m_keywordPanels = new List<KeywordHelpPanel>();
    private static KeywordHelpPanelManager s_instance;
    private float scaleToUse = ((float) KeywordHelpPanel.GAMEPLAY_SCALE);

    private void Awake()
    {
        s_instance = this;
        this.m_keywordPanelPool.SetCreateItemCallback(new Pool<KeywordHelpPanel>.CreateItemCallback(this.CreateKeywordPanel));
        this.m_keywordPanelPool.SetDestroyItemCallback(new Pool<KeywordHelpPanel>.DestroyItemCallback(this.DestroyKeywordPanel));
        this.m_keywordPanelPool.SetExtensionCount(1);
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().RegisterSceneUnloadedEvent(new SceneMgr.SceneUnloadedCallback(this.OnSceneUnloaded));
        }
    }

    private void CleanTweensOnPanel(KeywordHelpPanel helpPanel)
    {
        iTween.Stop(base.gameObject);
        RenderUtils.SetAlpha(helpPanel.gameObject, 0f, true);
    }

    public KeywordHelpPanel CreateKeywordPanel(int i)
    {
        return UnityEngine.Object.Instantiate<KeywordHelpPanel>(this.m_keywordPanelPrefab);
    }

    private void DestroyKeywordPanel(KeywordHelpPanel panel)
    {
        UnityEngine.Object.Destroy(panel.gameObject);
    }

    private void FadeInPanel(KeywordHelpPanel helpPanel)
    {
        this.CleanTweensOnPanel(helpPanel);
        float num = 0.4f;
        if ((GameState.Get() != null) && GameState.Get().GetGameEntity().IsKeywordHelpDelayOverridden())
        {
            num = 0f;
        }
        object[] args = new object[] { "onupdatetarget", base.gameObject, "onupdate", "OnUberTextFadeUpdate", "time", 0.125f, "delay", num, "to", 1f, "from", 0f };
        iTween.ValueTo(base.gameObject, iTween.Hash(args));
    }

    public static KeywordHelpPanelManager Get()
    {
        return s_instance;
    }

    public Card GetCard()
    {
        return this.m_card;
    }

    private Vector3 GetPanelPosition(KeywordHelpPanel panel)
    {
        Vector3 vector = new Vector3(0f, 0f, 0f);
        KeywordHelpPanel panel2 = null;
        float num = 0f;
        float num2 = 0f;
        for (int i = 0; i < this.m_keywordPanels.Count; i++)
        {
            KeywordHelpPanel panel3 = this.m_keywordPanels[i];
            if (this.m_card.GetEntity().IsHero())
            {
                num = 1.2f;
            }
            else if (this.m_card.GetEntity().GetZone() == TAG_ZONE.PLAY)
            {
                num = 1.05f;
            }
            else
            {
                num = 0.85f;
            }
            if (this.m_actor.GetMeshRenderer() == null)
            {
                return vector;
            }
            num2 = -0.2f * this.m_actor.GetMeshRenderer().bounds.size.z;
            if (panel3 == panel)
            {
                if (i == 0)
                {
                    vector = this.m_actor.transform.position + new Vector3(this.m_actor.GetMeshRenderer().bounds.size.x * num, 0f, this.m_actor.GetMeshRenderer().bounds.extents.z + num2);
                }
                else
                {
                    vector = panel2.transform.position - new Vector3(0f, 0f, (panel2.GetHeight() * 0.35f) + (panel3.GetHeight() * 0.35f));
                }
            }
            panel2 = panel3;
        }
        return vector;
    }

    public Vector3 GetPositionOfTopPanel()
    {
        if (this.m_keywordPanels.Count == 0)
        {
            return new Vector3(0f, 0f, 0f);
        }
        return this.m_keywordPanels[0].transform.position;
    }

    private string GetTextForTag(GAME_TAG tag, string key)
    {
        int spellPower;
        if (tag != GAME_TAG.SPELLPOWER)
        {
            return GameStrings.Get(key);
        }
        if (this.m_card != null)
        {
            spellPower = this.m_card.GetEntity().GetSpellPower();
        }
        else if (((this.m_actor != null) && (this.m_actor.GetEntityDef() != null)) && (this.m_actor.GetEntityDef().GetCardId() == "EX1_563"))
        {
            spellPower = 5;
        }
        else
        {
            spellPower = 1;
        }
        object[] args = new object[] { spellPower };
        return GameStrings.Format(key, args);
    }

    public void HideKeywordHelp()
    {
        GameState state = GameState.Get();
        if (((state != null) && state.GetGameEntity().ShouldShowCrazyKeywordTooltip()) && (TutorialKeywordManager.Get() != null))
        {
            TutorialKeywordManager.Get().HideKeywordHelp();
        }
        foreach (KeywordHelpPanel panel in this.m_keywordPanels)
        {
            if (panel != null)
            {
                this.CleanTweensOnPanel(panel);
                panel.gameObject.SetActive(false);
                this.m_keywordPanelPool.Release(panel);
            }
        }
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnSceneUnloaded(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        foreach (KeywordHelpPanel panel in this.m_keywordPanels)
        {
            UnityEngine.Object.Destroy(panel.gameObject);
        }
        this.m_keywordPanels.Clear();
        this.m_keywordPanelPool.Clear();
        UnityEngine.Object.Destroy(this.m_actor);
        this.m_actor = null;
        UnityEngine.Object.Destroy(this.m_card);
        this.m_card = null;
    }

    private void OnUberTextFadeUpdate(float newValue)
    {
        foreach (KeywordHelpPanel panel in this.m_keywordPanels)
        {
            RenderUtils.SetAlpha(panel.gameObject, newValue, true);
        }
    }

    [DebuggerHidden]
    private IEnumerator PositionPanelsForCM(Actor actor, bool reverse)
    {
        return new <PositionPanelsForCM>c__IteratorA1 { actor = actor, reverse = reverse, <$>actor = actor, <$>reverse = reverse, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator PositionPanelsForForge(GameObject actorObject, int cardChoice = 0)
    {
        return new <PositionPanelsForForge>c__IteratorA2 { actorObject = actorObject, cardChoice = cardChoice, <$>actorObject = actorObject, <$>cardChoice = cardChoice, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator PositionPanelsForGame(GameObject actorObject, bool showOnRight, bool inHand, Vector3? overrideOffset = new Vector3?())
    {
        return new <PositionPanelsForGame>c__Iterator9F { overrideOffset = overrideOffset, showOnRight = showOnRight, actorObject = actorObject, inHand = inHand, <$>overrideOffset = overrideOffset, <$>showOnRight = showOnRight, <$>actorObject = actorObject, <$>inHand = inHand, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator PositionPanelsForHistory(Actor actor, UberText createdByText)
    {
        return new <PositionPanelsForHistory>c__IteratorA0 { createdByText = createdByText, actor = actor, <$>createdByText = createdByText, <$>actor = actor, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator PositionPanelsForMulligan(GameObject actorObject)
    {
        return new <PositionPanelsForMulligan>c__IteratorA4 { actorObject = actorObject, <$>actorObject = actorObject, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator PositionPanelsForPackOpening(GameObject actorObject)
    {
        return new <PositionPanelsForPackOpening>c__IteratorA3 { actorObject = actorObject, <$>actorObject = actorObject, <>f__this = this };
    }

    private void PrepareToUpdateKeywordHelp(Actor actor)
    {
        this.HideKeywordHelp();
        this.m_actor = actor;
        this.m_keywordPanels.Clear();
    }

    private void SetupKeywordPanel(GAME_TAG tag)
    {
        string keywordName = GameStrings.GetKeywordName(tag);
        string textForTag = this.GetTextForTag(tag, GameStrings.GetKeywordTextKey(tag));
        this.SetupKeywordPanel(keywordName, textForTag);
    }

    private void SetupKeywordPanel(string headline, string description)
    {
        KeywordHelpPanel item = this.m_keywordPanelPool.Acquire();
        if (item != null)
        {
            item.Reset();
            item.Initialize(headline, description);
            item.SetScale(this.scaleToUse);
            this.m_keywordPanels.Add(item);
            this.FadeInPanel(item);
        }
    }

    private bool SetupKeywordPanelIfNecessary(EntityBase entityInfo, GAME_TAG tag)
    {
        if (entityInfo.HasTag(tag))
        {
            if ((tag == GAME_TAG.WINDFURY) && (entityInfo.GetTag(tag) > 1))
            {
                return false;
            }
            this.SetupKeywordPanel(tag);
            return true;
        }
        if (entityInfo.HasReferencedTag(tag))
        {
            this.SetupKeywordRefPanel(tag);
            return true;
        }
        return false;
    }

    private void SetupKeywordRefPanel(GAME_TAG tag)
    {
        string keywordName = GameStrings.GetKeywordName(tag);
        string textForTag = this.GetTextForTag(tag, GameStrings.GetRefKeywordTextKey(tag));
        this.SetupKeywordPanel(keywordName, textForTag);
    }

    private void SetUpPanels(EntityBase entityInfo)
    {
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.TAUNT);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.STEALTH);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.DIVINE_SHIELD);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.SPELLPOWER);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.ENRAGED);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.CHARGE);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.BATTLECRY);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.FROZEN);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.FREEZE);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.WINDFURY);
        if (entityInfo.GetZone() != TAG_ZONE.SECRET)
        {
            this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.SECRET);
        }
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.DEATHRATTLE);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.OVERLOAD);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.COMBO);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.SILENCE);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.COUNTER);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.CANT_BE_DAMAGED);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.SPARE_PART);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.INSPIRE);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.TREASURE);
        if (entityInfo.IsHeroPower())
        {
            this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.AI_MUST_PLAY);
        }
    }

    public void ShowKeywordHelp()
    {
        foreach (KeywordHelpPanel panel in this.m_keywordPanels)
        {
            panel.gameObject.SetActive(true);
        }
    }

    public void UpdateKeywordHelp(Card card, Actor actor, bool showOnRight = true, float? overrideScale = new float?(), Vector3? overrideOffset = new Vector3?())
    {
        this.m_card = card;
        this.UpdateKeywordHelp(card.GetEntity(), actor, showOnRight, overrideScale, overrideOffset);
    }

    public void UpdateKeywordHelp(Entity entity, Actor actor, bool showOnRight, float? overrideScale = new float?(), Vector3? overrideOffset = new Vector3?())
    {
        this.m_card = entity.GetCard();
        if (GameState.Get().GetGameEntity().ShouldShowCrazyKeywordTooltip())
        {
            if (TutorialKeywordManager.Get() != null)
            {
                TutorialKeywordManager.Get().UpdateKeywordHelp(entity, actor, showOnRight, overrideScale);
            }
        }
        else
        {
            bool inHand = this.m_card.GetZone() is ZoneHand;
            if (overrideScale.HasValue)
            {
                this.scaleToUse = overrideScale.Value;
            }
            else if (inHand)
            {
                this.scaleToUse = (float) KeywordHelpPanel.HAND_SCALE;
            }
            else
            {
                this.scaleToUse = (float) KeywordHelpPanel.GAMEPLAY_SCALE;
            }
            this.PrepareToUpdateKeywordHelp(actor);
            string[] strArray = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
            if (strArray != null)
            {
                this.SetupKeywordPanel(strArray[0], strArray[1]);
            }
            this.SetUpPanels(entity);
            base.StartCoroutine(this.PositionPanelsForGame(actor.GetMeshRenderer().gameObject, showOnRight, inHand, overrideOffset));
            GameState.Get().GetGameEntity().NotifyOfHelpPanelDisplay(this.m_keywordPanels.Count);
        }
    }

    public void UpdateKeywordHelpForCollectionManager(EntityDef entityDef, Actor actor, bool reverse)
    {
        this.scaleToUse = (float) KeywordHelpPanel.COLLECTION_MANAGER_SCALE;
        this.PrepareToUpdateKeywordHelp(actor);
        this.SetUpPanels(entityDef);
        base.StartCoroutine(this.PositionPanelsForCM(actor, reverse));
    }

    public void UpdateKeywordHelpForDeckHelper(EntityDef entityDef, Actor actor)
    {
        this.scaleToUse = 3.75f;
        this.PrepareToUpdateKeywordHelp(actor);
        this.SetUpPanels(entityDef);
        base.StartCoroutine(this.PositionPanelsForForge(actor.GetMeshRenderer().gameObject, 0));
    }

    public void UpdateKeywordHelpForForge(EntityDef entityDef, Actor actor, int cardChoice = 0)
    {
        this.scaleToUse = (float) KeywordHelpPanel.FORGE_SCALE;
        this.PrepareToUpdateKeywordHelp(actor);
        this.SetUpPanels(entityDef);
        base.StartCoroutine(this.PositionPanelsForForge(actor.GetMeshRenderer().gameObject, cardChoice));
    }

    public void UpdateKeywordHelpForHistoryCard(Entity entity, Actor actor, UberText createdByText)
    {
        this.m_card = entity.GetCard();
        this.scaleToUse = (float) KeywordHelpPanel.HISTORY_SCALE;
        this.PrepareToUpdateKeywordHelp(actor);
        string[] strArray = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
        if (strArray != null)
        {
            this.SetupKeywordPanel(strArray[0], strArray[1]);
        }
        this.SetUpPanels(entity);
        base.StartCoroutine(this.PositionPanelsForHistory(actor, createdByText));
    }

    public void UpdateKeywordHelpForMulliganCard(Entity entity, Actor actor)
    {
        this.m_card = entity.GetCard();
        this.scaleToUse = (float) KeywordHelpPanel.MULLIGAN_SCALE;
        this.PrepareToUpdateKeywordHelp(actor);
        string[] strArray = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
        if (strArray != null)
        {
            this.SetupKeywordPanel(strArray[0], strArray[1]);
        }
        this.SetUpPanels(entity);
        base.StartCoroutine(this.PositionPanelsForMulligan(actor.GetMeshRenderer().gameObject));
    }

    public void UpdateKeywordHelpForPackOpening(EntityDef entityDef, Actor actor)
    {
        this.scaleToUse = 2.75f;
        this.PrepareToUpdateKeywordHelp(actor);
        this.SetUpPanels(entityDef);
        base.StartCoroutine(this.PositionPanelsForPackOpening(actor.GetMeshRenderer().gameObject));
    }

    [CompilerGenerated]
    private sealed class <PositionPanelsForCM>c__IteratorA1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        internal bool <$>reverse;
        internal KeywordHelpPanelManager <>f__this;
        internal GameObject <actorObject>__0;
        internal Vector3 <actorStartAnchor>__3;
        internal RenderToTexture <ghostR2T>__9;
        internal Spell <ghostSpell>__8;
        internal int <i>__6;
        internal int <maxPanelCount>__2;
        internal KeywordHelpPanel <panel>__7;
        internal Vector3 <panelEndAnchor>__5;
        internal Vector3 <panelStartAnchor>__4;
        internal KeywordHelpPanel <prevPanel>__1;
        internal RenderToTexture <r2t>__10;
        internal Actor actor;
        internal bool reverse;

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
                    this.<actorObject>__0 = this.actor.GetMeshRenderer().gameObject;
                    this.<prevPanel>__1 = null;
                    this.<maxPanelCount>__2 = this.<>f__this.m_keywordPanels.Count;
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<maxPanelCount>__2 = Mathf.Min(this.<>f__this.m_keywordPanels.Count, 3);
                    }
                    if (this.reverse)
                    {
                        this.<actorStartAnchor>__3 = new Vector3(1f, 0f, 0f);
                        this.<panelStartAnchor>__4 = Vector3.zero;
                        this.<panelEndAnchor>__5 = new Vector3(0f, 0f, 1f);
                    }
                    else
                    {
                        this.<actorStartAnchor>__3 = new Vector3(1f, 0f, 1f);
                        this.<panelStartAnchor>__4 = new Vector3(0f, 0f, 1f);
                        this.<panelEndAnchor>__5 = Vector3.zero;
                    }
                    this.<i>__6 = 0;
                    while (this.<i>__6 < this.<>f__this.m_keywordPanels.Count)
                    {
                        this.<panel>__7 = this.<>f__this.m_keywordPanels[this.<i>__6];
                        if (this.<i>__6 >= this.<maxPanelCount>__2)
                        {
                            this.<panel>__7.gameObject.SetActive(false);
                            goto Label_02F0;
                        }
                    Label_0174:
                        while (!this.<panel>__7.IsTextRendered())
                        {
                            this.$current = null;
                            this.$PC = 1;
                            return true;
                        }
                        if (this.actor.IsSpellActive(SpellType.GHOSTCARD))
                        {
                            this.<ghostSpell>__8 = this.actor.GetSpell(SpellType.GHOSTCARD);
                            if (this.<ghostSpell>__8 != null)
                            {
                                this.<ghostR2T>__9 = this.<ghostSpell>__8.gameObject.GetComponentInChildren<RenderToTexture>();
                                if (this.<ghostR2T>__9 != null)
                                {
                                    this.<actorObject>__0 = this.<ghostR2T>__9.GetRenderToObject();
                                }
                            }
                        }
                        if (this.<i>__6 == 0)
                        {
                            TransformUtil.SetPoint(this.<panel>__7.gameObject, this.<panelStartAnchor>__4, this.<actorObject>__0, this.<actorStartAnchor>__3, Vector3.zero);
                            if (this.actor.isMissingCard())
                            {
                                this.<r2t>__10 = this.actor.m_missingCardEffect.GetComponent<RenderToTexture>();
                                if (this.<r2t>__10 != null)
                                {
                                    Log.Kyle.Print("Missing card keyword tooltip offset: " + this.<r2t>__10.GetOffscreenPositionOffset().ToString(), new object[0]);
                                    Transform transform = this.<panel>__7.gameObject.transform;
                                    transform.position -= this.<r2t>__10.GetOffscreenPositionOffset();
                                }
                            }
                        }
                        else
                        {
                            TransformUtil.SetPoint(this.<panel>__7.gameObject, this.<panelStartAnchor>__4, this.<prevPanel>__1.gameObject, this.<panelEndAnchor>__5, Vector3.zero);
                        }
                        this.<prevPanel>__1 = this.<panel>__7;
                    Label_02F0:
                        this.<i>__6++;
                    }
                    this.$PC = -1;
                    break;

                case 1:
                    goto Label_0174;
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
    private sealed class <PositionPanelsForForge>c__IteratorA2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>actorObject;
        internal int <$>cardChoice;
        internal KeywordHelpPanelManager <>f__this;
        internal int <i>__1;
        internal KeywordHelpPanel <panel>__2;
        internal KeywordHelpPanel <prevPanel>__0;
        internal GameObject actorObject;
        internal int cardChoice;

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
                    this.<prevPanel>__0 = null;
                    this.<i>__1 = 0;
                    goto Label_01CE;

                case 1:
                    break;

                default:
                    goto Label_01F0;
            }
        Label_0068:
            while (!this.<panel>__2.IsTextRendered())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            if (this.<i>__1 == 0)
            {
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    TransformUtil.SetPoint(this.<panel>__2.gameObject, new Vector3(0f, 0f, 1f), this.actorObject, (this.cardChoice != 3) ? new Vector3(1f, 0f, 1f) : new Vector3(0f, 0f, 1f), (this.cardChoice != 3) ? Vector3.zero : new Vector3(-31f, 0f, 0f));
                }
                else
                {
                    TransformUtil.SetPoint(this.<panel>__2.gameObject, new Vector3(0f, 0f, 1f), this.actorObject, new Vector3(1f, 0f, 1f), Vector3.zero);
                }
            }
            else
            {
                TransformUtil.SetPoint(this.<panel>__2.gameObject, new Vector3(0f, 0f, 1f), this.<prevPanel>__0.gameObject, new Vector3(0f, 0f, 0f), Vector3.zero);
            }
            this.<prevPanel>__0 = this.<panel>__2;
            this.<i>__1++;
        Label_01CE:
            if (this.<i>__1 < this.<>f__this.m_keywordPanels.Count)
            {
                this.<panel>__2 = this.<>f__this.m_keywordPanels[this.<i>__1];
                goto Label_0068;
            }
            this.$PC = -1;
        Label_01F0:
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
    private sealed class <PositionPanelsForGame>c__Iterator9F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>actorObject;
        internal bool <$>inHand;
        internal Vector3? <$>overrideOffset;
        internal bool <$>showOnRight;
        internal KeywordHelpPanelManager <>f__this;
        internal KeywordHelpPanel <curPanel>__2;
        internal int <i>__1;
        internal KeywordHelpPanel <prevPanel>__0;
        internal GameObject actorObject;
        internal bool inHand;
        internal Vector3? overrideOffset;
        internal bool showOnRight;

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
                    this.<prevPanel>__0 = null;
                    this.<i>__1 = 0;
                    goto Label_0405;

                case 1:
                    break;

                default:
                    goto Label_0427;
            }
        Label_0068:
            while (!this.<curPanel>__2.IsTextRendered())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            if (this.<i>__1 == 0)
            {
                if (this.overrideOffset.HasValue)
                {
                    if (this.showOnRight)
                    {
                        TransformUtil.SetPoint(this.<curPanel>__2.gameObject, new Vector3(0f, 0f, 1f), this.actorObject, new Vector3(1f, 0f, 1f), this.overrideOffset.Value);
                    }
                    else
                    {
                        TransformUtil.SetPoint(this.<curPanel>__2.gameObject, new Vector3(1f, 0f, 1f), this.actorObject, new Vector3(0f, 0f, 1f), this.overrideOffset.Value);
                    }
                }
                else if (this.inHand)
                {
                    if (this.showOnRight)
                    {
                        TransformUtil.SetPoint(this.<curPanel>__2.gameObject, new Vector3(0f, 0f, 1f), this.actorObject, new Vector3(1f, 0f, 1f), Vector3.zero);
                    }
                    else
                    {
                        TransformUtil.SetPoint(this.<curPanel>__2.gameObject, new Vector3(1f, 0f, 1f), this.actorObject, new Vector3(0f, 0f, 1f), new Vector3(-0.15f, 0f, 0f));
                    }
                }
                else if (UniversalInputManager.UsePhoneUI != null)
                {
                    if (this.showOnRight)
                    {
                        TransformUtil.SetPoint(this.<curPanel>__2.gameObject, new Vector3(0f, 0f, 1f), this.actorObject, new Vector3(1f, 0f, 1f), new Vector3(1.5f, 0f, 2f));
                    }
                    else
                    {
                        TransformUtil.SetPoint(this.<curPanel>__2.gameObject, new Vector3(1f, 0f, 1f), this.actorObject, new Vector3(0f, 0f, 1f), new Vector3(-1.8f, 0f, 2f));
                    }
                }
                else if (this.showOnRight)
                {
                    TransformUtil.SetPoint(this.<curPanel>__2.gameObject, new Vector3(0f, 0f, 1f), this.actorObject, new Vector3(1f, 0f, 1f), new Vector3((0.5f * this.<>f__this.scaleToUse) + 0.15f, 0f, 0.8f));
                }
                else
                {
                    TransformUtil.SetPoint(this.<curPanel>__2.gameObject, new Vector3(1f, 0f, 1f), this.actorObject, new Vector3(0f, 0f, 1f), new Vector3((-0.78f * this.<>f__this.scaleToUse) - 0.15f, 0f, 0.8f));
                }
            }
            else
            {
                TransformUtil.SetPoint(this.<curPanel>__2.gameObject, new Vector3(0f, 0f, 1f), this.<prevPanel>__0.gameObject, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.17f));
            }
            this.<prevPanel>__0 = this.<curPanel>__2;
            this.<i>__1++;
        Label_0405:
            if (this.<i>__1 < this.<>f__this.m_keywordPanels.Count)
            {
                this.<curPanel>__2 = this.<>f__this.m_keywordPanels[this.<i>__1];
                goto Label_0068;
            }
            this.$PC = -1;
        Label_0427:
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
    private sealed class <PositionPanelsForHistory>c__IteratorA0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        internal UberText <$>createdByText;
        internal KeywordHelpPanelManager <>f__this;
        internal KeywordHelpPanel <curPanel>__5;
        internal GameObject <firstRelativeAnchor>__0;
        internal GameObject <historyKeywordBone>__1;
        internal int <i>__4;
        internal float <newZ>__6;
        internal KeywordHelpPanel <prevPanel>__2;
        internal bool <showHorizontally>__3;
        internal Actor actor;
        internal UberText createdByText;

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
                    if (!this.createdByText.gameObject.activeSelf)
                    {
                        this.<historyKeywordBone>__1 = this.actor.FindBone("HistoryKeywordBone");
                        if (this.<historyKeywordBone>__1 == null)
                        {
                            object[] messageArgs = new object[] { this.actor };
                            Error.AddDevWarning("Missing Bone", "Missing HistoryKeywordBone on {0}", messageArgs);
                            this.$current = null;
                            this.$PC = 1;
                            goto Label_02C5;
                        }
                        break;
                    }
                    this.<firstRelativeAnchor>__0 = this.createdByText.gameObject;
                    goto Label_00B4;

                case 1:
                    break;

                case 2:
                    goto Label_0121;

                default:
                    goto Label_02C3;
            }
            this.<firstRelativeAnchor>__0 = this.<historyKeywordBone>__1;
        Label_00B4:
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.<>f__this.m_keywordPanels.Clear();
            }
            this.<prevPanel>__2 = null;
            this.<showHorizontally>__3 = false;
            this.<i>__4 = 0;
            while (this.<i>__4 < this.<>f__this.m_keywordPanels.Count)
            {
                this.<curPanel>__5 = this.<>f__this.m_keywordPanels[this.<i>__4];
            Label_0121:
                while (!this.<curPanel>__5.IsTextRendered())
                {
                    this.$current = null;
                    this.$PC = 2;
                    goto Label_02C5;
                }
                if (this.<i>__4 == 0)
                {
                    TransformUtil.SetPoint(this.<curPanel>__5.gameObject, new Vector3(0.5f, 0f, 1f), this.<firstRelativeAnchor>__0, new Vector3(0.5f, 0f, 0f));
                }
                else
                {
                    this.<newZ>__6 = (this.<prevPanel>__2.GetHeight() * 0.35f) + (this.<curPanel>__5.GetHeight() * 0.35f);
                    if ((this.<prevPanel>__2.transform.position.z - this.<newZ>__6) < -8.3f)
                    {
                        this.<showHorizontally>__3 = true;
                    }
                    if (this.<showHorizontally>__3)
                    {
                        TransformUtil.SetPoint(this.<curPanel>__5.gameObject, new Vector3(0f, 0f, 1f), this.<prevPanel>__2.gameObject, new Vector3(1f, 0f, 1f), Vector3.zero);
                    }
                    else
                    {
                        TransformUtil.SetPoint(this.<curPanel>__5.gameObject, new Vector3(0.5f, 0f, 1f), this.<prevPanel>__2.gameObject, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0f, 0.06094122f));
                    }
                }
                this.<prevPanel>__2 = this.<curPanel>__5;
                this.<i>__4++;
            }
            this.$PC = -1;
        Label_02C3:
            return false;
        Label_02C5:
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
    private sealed class <PositionPanelsForMulligan>c__IteratorA4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>actorObject;
        internal KeywordHelpPanelManager <>f__this;
        internal KeywordHelpPanel <curPanel>__3;
        internal int <i>__2;
        internal float <newZ>__4;
        internal KeywordHelpPanel <prevPanel>__0;
        internal bool <showHorizontally>__1;
        internal GameObject actorObject;

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
                    this.<prevPanel>__0 = null;
                    this.<showHorizontally>__1 = false;
                    this.<i>__2 = 0;
                    goto Label_0203;

                case 1:
                    break;

                default:
                    goto Label_0225;
            }
        Label_006F:
            while (!this.<curPanel>__3.IsTextRendered())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            if (this.<i>__2 == 0)
            {
                TransformUtil.SetPoint(this.<curPanel>__3.gameObject, new Vector3(0.5f, 0f, 1f), this.actorObject, new Vector3(0.5f, 0f, 0f), new Vector3(-0.112071f, 0f, -0.1244259f));
            }
            else
            {
                this.<newZ>__4 = (this.<prevPanel>__0.GetHeight() * 0.35f) + (this.<curPanel>__3.GetHeight() * 0.35f);
                if ((this.<prevPanel>__0.transform.position.z - this.<newZ>__4) < -8.3f)
                {
                    this.<showHorizontally>__1 = true;
                }
                if (this.<showHorizontally>__1)
                {
                    TransformUtil.SetPoint(this.<curPanel>__3.gameObject, new Vector3(0f, 0f, 1f), this.<prevPanel>__0.gameObject, new Vector3(1f, 0f, 1f), Vector3.zero);
                }
                else
                {
                    TransformUtil.SetPoint(this.<curPanel>__3.gameObject, new Vector3(0.5f, 0f, 1f), this.<prevPanel>__0.gameObject, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0f, 0.1588802f));
                }
            }
            this.<prevPanel>__0 = this.<curPanel>__3;
            this.<i>__2++;
        Label_0203:
            if (this.<i>__2 < this.<>f__this.m_keywordPanels.Count)
            {
                this.<curPanel>__3 = this.<>f__this.m_keywordPanels[this.<i>__2];
                goto Label_006F;
            }
            this.$PC = -1;
        Label_0225:
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
    private sealed class <PositionPanelsForPackOpening>c__IteratorA3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>actorObject;
        internal KeywordHelpPanelManager <>f__this;
        internal int <i>__1;
        internal KeywordHelpPanel <panel>__2;
        internal KeywordHelpPanel <prevPanel>__0;
        internal GameObject actorObject;

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
                    this.<prevPanel>__0 = null;
                    this.<i>__1 = 0;
                    goto Label_0166;

                case 1:
                    break;

                default:
                    goto Label_0188;
            }
        Label_0068:
            while (!this.<panel>__2.IsTextRendered())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            if (this.<i>__1 == 0)
            {
                TransformUtil.SetPoint(this.<panel>__2.gameObject, new Vector3(1f, 0f, 1f), this.actorObject, new Vector3(0f, 0f, 1f), Vector3.zero);
                this.<panel>__2.transform.position -= new Vector3(1.2f, 0f, 0f);
            }
            else
            {
                TransformUtil.SetPoint(this.<panel>__2.gameObject, new Vector3(0f, 0f, 1f), this.<prevPanel>__0.gameObject, new Vector3(0f, 0f, 0f), Vector3.zero);
            }
            this.<prevPanel>__0 = this.<panel>__2;
            this.<i>__1++;
        Label_0166:
            if (this.<i>__1 < this.<>f__this.m_keywordPanels.Count)
            {
                this.<panel>__2 = this.<>f__this.m_keywordPanels[this.<i>__1];
                goto Label_0068;
            }
            this.$PC = -1;
        Label_0188:
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

