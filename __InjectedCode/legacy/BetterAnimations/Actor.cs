#region " Imports "
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#endregion 

#region " Referenced assemblies "
// - mscorlib v2.0.5.0
// - Assembly-CSharp-firstpass v0.0.0.0
// - System.Core v2.0.5.0
// - UnityEngine v0.0.0.0
// - PlayMaker v1.6.0.0
// - System v2.0.5.0
// - ICSharpCode.SharpZipLib v0.86.0.518
// - IFacebook v0.0.0.0
// - System.Xml v2.0.5.0
// - UnityEngine.UI v1.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class Actor
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    Spell ActivateSpell(SpellType spellType)
    {
        if (spellType.ToString().Contains("BURST"))
            goto normal;
        if (spellType != SpellType.Zzz && spellType != SpellType.CHARGE)
            goto normal;

        if (spellType == SpellType.TAUNT)
            spellType = SpellType.TAUNT_PREMIUM;
        else if (spellType == SpellType.TAUNT_STEALTH)
            spellType = SpellType.TAUNT_PREMIUM_STEALTH;

        spellType = SpellType.SUMMON_IN_LARGE_PREMIUM;

        normal:
        Spell spell = this.GetSpell(spellType);
        if (spell == null)
        {
            return null;
        }
        spell.ActivateState(SpellStateType.BIRTH);
        return spell;
    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    Actor()
    {
    }

    void Awake()
    {
    }

    void OnEnable()
    {
    }

    void Start()
    {
    }

    void Init()
    {
    }

    void Destroy()
    {
    }

    Actor Clone()
    {
        return default(Actor);
    }

    Card GetCard()
    {
        return default(Card);
    }

    void SetCard(Card card)
    {
    }

    CardDef GetCardDef()
    {
        return default(CardDef);
    }

    void SetCardDef(CardDef cardDef)
    {
    }

    Entity GetEntity()
    {
        return default(Entity);
    }

    void SetEntity(Entity entity)
    {
    }

    EntityDef GetEntityDef()
    {
        return default(EntityDef);
    }

    void SetEntityDef(EntityDef entityDef)
    {
    }

    CardFlair GetCardFlair()
    {
        return default(CardFlair);
    }

    void SetCardFlair(CardFlair cardFlair)
    {
    }

    void SetPremium(TAG_PREMIUM premium)
    {
    }

    TAG_CARD_SET GetCardSet()
    {
        return default(TAG_CARD_SET);
    }

    ActorStateType GetActorStateType()
    {
        return default(ActorStateType);
    }

    void ShowActorState()
    {
    }

    void HideActorState()
    {
    }

    void SetActorState(ActorStateType stateType)
    {
    }

    void ToggleForceIdle(bool bOn)
    {
    }

    void TurnOffCollider()
    {
    }

    void TurnOnCollider()
    {
    }

    void ToggleCollider(bool enabled)
    {
    }

    TAG_RARITY GetRarity()
    {
        return default(TAG_RARITY);
    }

    bool IsElite()
    {
        return default(bool);
    }

    void SetHiddenStandIn(UnityEngine.GameObject standIn)
    {
    }

    UnityEngine.GameObject GetHiddenStandIn()
    {
        return default(UnityEngine.GameObject);
    }

    void SetShadowform(bool shadowform)
    {
    }

    void SetDisablePremiumPortrait(bool disable)
    {
    }

    bool IsShown()
    {
        return default(bool);
    }

    void Show()
    {
    }

    void Show(bool ignoreSpells)
    {
    }

    void ShowSpellTable()
    {
    }

    void Hide()
    {
    }

    void Hide(bool ignoreSpells)
    {
    }

    void HideSpellTable()
    {
    }

    void ShowImpl(bool ignoreSpells)
    {
    }

    void HideImpl(bool ignoreSpells)
    {
    }

    ActorStateMgr GetActorStateMgr()
    {
        return default(ActorStateMgr);
    }

    UnityEngine.Collider GetCollider()
    {
        return default(UnityEngine.Collider);
    }

    UnityEngine.GameObject GetRootObject()
    {
        return default(UnityEngine.GameObject);
    }

    UnityEngine.MeshRenderer GetMeshRenderer()
    {
        return default(UnityEngine.MeshRenderer);
    }

    UnityEngine.GameObject GetBones()
    {
        return default(UnityEngine.GameObject);
    }

    UberText GetPowersText()
    {
        return default(UberText);
    }

    UberText GetRaceText()
    {
        return default(UberText);
    }

    UberText GetNameText()
    {
        return default(UberText);
    }

    UnityEngine.Light GetHeroSpotlight()
    {
        return default(UnityEngine.Light);
    }

    UnityEngine.GameObject FindBone(string boneName)
    {
        return default(UnityEngine.GameObject);
    }

    UnityEngine.GameObject GetCardTypeBannerAnchor()
    {
        return default(UnityEngine.GameObject);
    }

    UberText GetAttackText()
    {
        return default(UberText);
    }

    UnityEngine.GameObject GetAttackTextObject()
    {
        return default(UnityEngine.GameObject);
    }

    GemObject GetAttackObject()
    {
        return default(GemObject);
    }

    GemObject GetHealthObject()
    {
        return default(GemObject);
    }

    UberText GetHealthText()
    {
        return default(UberText);
    }

    UnityEngine.GameObject GetHealthTextObject()
    {
        return default(UnityEngine.GameObject);
    }

    UberText GetCostText()
    {
        return default(UberText);
    }

    UnityEngine.GameObject GetCostTextObject()
    {
        return default(UnityEngine.GameObject);
    }

    UberText GetSecretText()
    {
        return default(UberText);
    }

    void UpdateAllComponents()
    {
    }

    bool MissingCardEffect()
    {
        return default(bool);
    }

    void DisableMissingCardEffect()
    {
    }

    void UpdateMissingCardArt()
    {
    }

    void SetMissingCardMaterial(UnityEngine.Material missingCardMat)
    {
    }

    bool isMissingCard()
    {
        return default(bool);
    }

    void GhostCardEffect(bool enabled)
    {
    }

    void UpdateGhostCardEffect()
    {
    }

    bool isGhostCard()
    {
        return default(bool);
    }

    void UpdateMaterials()
    {
    }

    void OverrideAllMeshMaterials(UnityEngine.Material material)
    {
    }

    void SetUnlit()
    {
    }

    void SetLit()
    {
    }

    void SetLightingBlend(float @value)
    {
    }

    void SetLightBlend(float blendValue)
    {
    }

    void ReleasePortrait()
    {
    }

    void RecursivelyReplaceMaterialsList(UnityEngine.Transform transformToRecurse, UnityEngine.Material newMaterialPrefab)
    {
    }

    void ReplaceMaterialsList(UnityEngine.Renderer renderer, UnityEngine.Material newMaterialPrefab)
    {
    }

    UnityEngine.Material CreateReplacementMaterial(UnityEngine.Material oldMaterial, UnityEngine.Material newMaterialPrefab)
    {
        return default(UnityEngine.Material);
    }

    void SeedMaterialEffects()
    {
    }

    void MaterialShaderAnimation(bool animationEnabled)
    {
    }

    bool GetCardbackUpdateIgnore()
    {
        return default(bool);
    }

    void SetCardbackUpdateIgnore(bool ignoreUpdate)
    {
    }

    void UpdateCardBack()
    {
    }

    void UpdateCardBackDragEffect()
    {
    }

    void UpdateCardBackDisplay(bool friendlySide)
    {
    }

    void UpdateTextures()
    {
    }

    void UpdatePortraitTexture()
    {
    }

    void SetPortraitTexture(UnityEngine.Texture texture)
    {
    }

    void SetPortraitTextureOverride(UnityEngine.Texture portrait)
    {
    }

    UnityEngine.Texture GetPortraitTexture()
    {
        return default(UnityEngine.Texture);
    }

    System.Collections.IEnumerator UpdatePortraitMaterials()
    {
        return default(System.Collections.IEnumerator);
    }

    void SetPortraitMaterial(UnityEngine.Material material)
    {
    }

    UnityEngine.GameObject GetPortraitMesh()
    {
        return default(UnityEngine.GameObject);
    }

    UnityEngine.Material GetPortraitMaterial()
    {
        return default(UnityEngine.Material);
    }

    void UpdateTextComponents()
    {
    }

    void UpdateTextComponentsDef(EntityDef entityDef)
    {
    }

    void UpdateTextComponents(Entity entity)
    {
    }

    void UpdatePowersText()
    {
    }

    void UpdateNumberText(UberText textMesh, string newText)
    {
    }

    void UpdateNumberText(UberText textMesh, string newText, bool shouldHide)
    {
    }

    void UpdateNameText()
    {
    }

    void UpdateSecretText()
    {
    }

    void UpdateText(UberText uberTextMesh, string text)
    {
    }

    void UpdateTextColor(UberText originalMesh, int baseNumber, int currentNumber)
    {
    }

    void UpdateTextColor(UberText uberTextMesh, int baseNumber, int currentNumber, bool higherIsBetter)
    {
    }

    void UpdateTextColorToGreenOrWhite(UberText uberTextMesh, int baseNumber, int currentNumber)
    {
    }

    void DisableTextMesh(UberText mesh)
    {
    }

    void OverrideNameText(UberText newText)
    {
    }

    void HideAllText()
    {
    }

    void ShowAllText()
    {
    }

    void ToggleTextVisibility(bool bOn)
    {
    }

    void ContactShadow(bool visible)
    {
    }

    void UpdateMeshComponents()
    {
    }

    void UpdateRarityComponent()
    {
    }

    bool GetRarityTextureOffset(ref UnityEngine.Vector2 offset, ref UnityEngine.Color tint)
    {
        return default(bool);
    }

    void UpdateWatermark()
    {
    }

    void OnWatermarkLoaded(string name, UnityEngine.Object tex, object callbackData)
    {
    }

    void UpdateEliteComponent()
    {
    }

    void UpdatePremiumComponents()
    {
    }

    void UpdateRace(string raceText)
    {
    }

    void UpdateCardColor()
    {
    }

    HistoryCard GetHistoryCard()
    {
        return default(HistoryCard);
    }

    HistoryChildCard GetHistoryChildCard()
    {
        return default(HistoryChildCard);
    }

    void SetHistoryCard(HistoryCard card)
    {
    }

    void SetHistoryChildCard(HistoryChildCard card)
    {
    }

    SpellTable GetSpellTable()
    {
        return default(SpellTable);
    }

    Spell LoadSpell(SpellType spellType)
    {
        return default(Spell);
    }

    Spell GetLoadedSpell(SpellType spellType)
    {
        return default(Spell);
    }

    bool GetSpellIfLoaded(SpellType spellType, ref Spell result)
    {
        return default(bool);
    }

    Spell ActivateTaunt()
    {
        return default(Spell);
    }

    void DeactivateTaunt()
    {
    }

    Spell GetSpell(SpellType spellType)
    {
        return default(Spell);
    }

    Spell GetSpellIfLoaded(SpellType spellType)
    {
        return default(Spell);
    }

    bool IsSpellActive(SpellType spellType)
    {
        return default(bool);
    }

    bool IsSpellDyingOrNone(SpellType spellType)
    {
        return default(bool);
    }

    void DeactivateSpell(SpellType spellType)
    {
    }

    void DeactivateAllSpells()
    {
    }

    void DeactivateAllPreDeathSpells()
    {
    }

    void DestroySpell(SpellType spellType)
    {
    }

    void HideArmorSpell()
    {
    }

    void UpdateRootObjectSpellComponents()
    {
    }

    System.Collections.IEnumerator UpdateArmorSpellWhenLoaded()
    {
        return default(System.Collections.IEnumerator);
    }

    void UpdateArmorSpell()
    {
    }

    System.Collections.IEnumerator ActivateArmorSpell(SpellStateType stateType, bool armorShouldBeOn)
    {
        return default(System.Collections.IEnumerator);
    }

    void OnSpellStateStarted(Spell spell, SpellStateType prevStateType, object userData)
    {
    }

    void AssignRootObject()
    {
    }

    void AssignBones()
    {
    }

    void AssignMeshRenderers()
    {
    }

    void AssignMaterials(UnityEngine.MeshRenderer meshRenderer)
    {
    }

    void AssignSpells()
    {
    }

    void LoadArmorSpell()
    {
    }

    void OnArmorSpellLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static string WATERMARK_EXPERT1;
    static string WATERMARK_FP1;
    static string WATERMARK_GVG;
    static string WATERMARK_BRM;
    static string WATERMARK_TGT;
    static string WATERMARK_LOE;
    UnityEngine.Vector2 GEM_TEXTURE_OFFSET_RARE;
    UnityEngine.Vector2 GEM_TEXTURE_OFFSET_EPIC;
    UnityEngine.Vector2 GEM_TEXTURE_OFFSET_LEGENDARY;
    UnityEngine.Vector2 GEM_TEXTURE_OFFSET_COMMON;
    UnityEngine.Color GEM_COLOR_RARE;
    UnityEngine.Color GEM_COLOR_EPIC;
    UnityEngine.Color GEM_COLOR_LEGENDARY;
    UnityEngine.Color GEM_COLOR_COMMON;
    UnityEngine.Color CLASS_COLOR_GENERIC;
    UnityEngine.Color CLASS_COLOR_WARLOCK;
    UnityEngine.Color CLASS_COLOR_ROGUE;
    UnityEngine.Color CLASS_COLOR_DRUID;
    UnityEngine.Color CLASS_COLOR_SHAMAN;
    UnityEngine.Color CLASS_COLOR_HUNTER;
    UnityEngine.Color CLASS_COLOR_MAGE;
    UnityEngine.Color CLASS_COLOR_PALADIN;
    UnityEngine.Color CLASS_COLOR_PRIEST;
    UnityEngine.Color CLASS_COLOR_WARRIOR;
    UnityEngine.GameObject m_cardMesh;
    int m_cardFrontMatIdx;
    int m_cardBackMatIdx;
    int m_premiumRibbon;
    UnityEngine.GameObject m_portraitMesh;
    int m_portraitFrameMatIdx;
    int m_portraitMatIdx;
    UnityEngine.GameObject m_nameBannerMesh;
    UnityEngine.GameObject m_descriptionMesh;
    UnityEngine.GameObject m_descriptionTrimMesh;
    UnityEngine.GameObject m_rarityFrameMesh;
    UnityEngine.GameObject m_rarityGemMesh;
    UnityEngine.GameObject m_racePlateMesh;
    UnityEngine.GameObject m_attackObject;
    UnityEngine.GameObject m_healthObject;
    UnityEngine.GameObject m_manaObject;
    UnityEngine.GameObject m_racePlateObject;
    UnityEngine.GameObject m_cardTypeAnchorObject;
    UnityEngine.GameObject m_eliteObject;
    UnityEngine.GameObject m_classIconObject;
    UnityEngine.GameObject m_heroSpotLight;
    UnityEngine.GameObject m_glints;
    UnityEngine.GameObject m_armorSpellBone;
    UberText m_costTextMesh;
    UberText m_attackTextMesh;
    UberText m_healthTextMesh;
    UberText m_nameTextMesh;
    UberText m_powersTextMesh;
    UberText m_raceTextMesh;
    UberText m_secretText;
    UnityEngine.GameObject m_missingCardEffect;
    UnityEngine.GameObject m_ghostCardGameObject;
    string m_spellTablePrefab;
    Card m_card;
    Entity m_entity;
    CardDef m_cardDef;
    EntityDef m_entityDef;
    CardFlair m_cardFlair;
    ProjectedShadow m_projectedShadow;
    bool m_shown;
    ActorStateMgr m_actorStateMgr;
    ActorStateType m_actorState;
    bool forceIdleState;
    UnityEngine.GameObject m_rootObject;
    UnityEngine.GameObject m_bones;
    UnityEngine.MeshRenderer m_meshRenderer;
    int m_legacyPortraitMaterialIndex;
    int m_legacyCardColorMaterialIndex;
    UnityEngine.Material m_initialPortraitMaterial;
    System.Collections.Generic.List<UnityEngine.Material> m_lightBlendMaterials;
    System.Collections.Generic.List<UberText> m_lightBlendUberText;
    SpellTable m_sharedSpellTable;
    bool m_useSharedSpellTable;
    Map<SpellType, Spell> m_localSpellTable;
    SpellTable m_spellTable;
    ArmorSpell m_armorSpell;
    UnityEngine.GameObject m_hiddenCardStandIn;
    bool m_shadowform;
    bool m_ghostCard;
    bool m_missingcard;
    bool m_armorSpellLoading;
    bool m_materialEffectsSeeded;
    bool m_ignoreUpdateCardback;
    bool isPortraitMaterialDirty;
    bool m_DisablePremiumPortrait;
    UnityEngine.Texture m_portraitTextureOverride;
    ActorStateType m_actualState;
    bool m_hideActorState;
    #endregion

}
