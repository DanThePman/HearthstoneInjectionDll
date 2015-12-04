using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class Actor : MonoBehaviour
{
    protected readonly Color CLASS_COLOR_DRUID = new Color(0.42f, 0.29f, 0.14f);
    protected readonly Color CLASS_COLOR_GENERIC = new Color(0.8f, 0.8f, 0.8f);
    protected readonly Color CLASS_COLOR_HUNTER = new Color(0.26f, 0.54f, 0.18f);
    protected readonly Color CLASS_COLOR_MAGE = new Color(0.44f, 0.48f, 0.69f);
    protected readonly Color CLASS_COLOR_PALADIN = new Color(0.71f, 0.49f, 0.2f);
    protected readonly Color CLASS_COLOR_PRIEST = new Color(0.65f, 0.65f, 0.65f);
    protected readonly Color CLASS_COLOR_ROGUE = new Color(0.72f, 0.68f, 0.76f);
    protected readonly Color CLASS_COLOR_SHAMAN = new Color(0f, 0.32f, 0.71f);
    protected readonly Color CLASS_COLOR_WARLOCK = new Color(0.33f, 0.2f, 0.4f);
    protected readonly Color CLASS_COLOR_WARRIOR = new Color(0.43f, 0.14f, 0.14f);
    protected bool forceIdleState;
    protected readonly Color GEM_COLOR_COMMON = new Color(0.549f, 0.549f, 0.549f);
    protected readonly Color GEM_COLOR_EPIC = new Color(0.596f, 0.1568f, 0.7333f);
    protected readonly Color GEM_COLOR_LEGENDARY = new Color(1f, 0.5333f, 0f);
    protected readonly Color GEM_COLOR_RARE = new Color(0.1529f, 0.498f, 1f);
    protected readonly Vector2 GEM_TEXTURE_OFFSET_COMMON = new Vector2(0f, 0f);
    protected readonly Vector2 GEM_TEXTURE_OFFSET_EPIC = new Vector2(0f, 0.5f);
    protected readonly Vector2 GEM_TEXTURE_OFFSET_LEGENDARY = new Vector2(0.5f, 0.5f);
    protected readonly Vector2 GEM_TEXTURE_OFFSET_RARE = new Vector2(0.5f, 0f);
    protected bool isPortraitMaterialDirty;
    protected ActorStateType m_actorState = ActorStateType.CARD_IDLE;
    protected ActorStateMgr m_actorStateMgr;
    private ActorStateType m_actualState;
    protected ArmorSpell m_armorSpell;
    public GameObject m_armorSpellBone;
    protected bool m_armorSpellLoading;
    public GameObject m_attackObject;
    public UberText m_attackTextMesh;
    protected GameObject m_bones;
    protected Card m_card;
    public int m_cardBackMatIdx = -1;
    protected Player.Side? m_cardBackSideOverride;
    protected CardDef m_cardDef;
    protected CardFlair m_cardFlair;
    public int m_cardFrontMatIdx = -1;
    public GameObject m_cardMesh;
    public GameObject m_cardTypeAnchorObject;
    public GameObject m_classIconObject;
    public UberText m_costTextMesh;
    public GameObject m_descriptionMesh;
    public GameObject m_descriptionTrimMesh;
    protected bool m_DisablePremiumPortrait;
    public GameObject m_eliteObject;
    protected Entity m_entity;
    protected EntityDef m_entityDef;
    protected bool m_ghostCard;
    public GameObject m_ghostCardGameObject;
    public GameObject m_glints;
    public GameObject m_healthObject;
    public UberText m_healthTextMesh;
    public GameObject m_heroSpotLight;
    protected GameObject m_hiddenCardStandIn;
    private bool m_hideActorState;
    protected bool m_ignoreUpdateCardback;
    protected Material m_initialPortraitMaterial;
    protected int m_legacyCardColorMaterialIndex = -1;
    protected int m_legacyPortraitMaterialIndex = -1;
    protected List<Material> m_lightBlendMaterials;
    protected List<UberText> m_lightBlendUberText;
    protected Map<SpellType, Spell> m_localSpellTable;
    public GameObject m_manaObject;
    protected bool m_materialEffectsSeeded;
    protected MeshRenderer m_meshRenderer;
    protected bool m_missingcard;
    public GameObject m_missingCardEffect;
    public GameObject m_nameBannerMesh;
    public UberText m_nameTextMesh;
    public int m_portraitFrameMatIdx = -1;
    public int m_portraitMatIdx = -1;
    public GameObject m_portraitMesh;
    protected Texture m_portraitTextureOverride;
    public UberText m_powersTextMesh;
    public int m_premiumRibbon = -1;
    protected ProjectedShadow m_projectedShadow;
    public GameObject m_racePlateMesh;
    public GameObject m_racePlateObject;
    public UberText m_raceTextMesh;
    public GameObject m_rarityFrameMesh;
    public GameObject m_rarityGemMesh;
    protected GameObject m_rootObject;
    public UberText m_secretText;
    protected bool m_shadowform;
    protected SpellTable m_sharedSpellTable;
    protected bool m_shown = true;
    protected SpellTable m_spellTable;
    [CustomEditField(T=EditType.ACTOR)]
    public string m_spellTablePrefab;
    protected bool m_useSharedSpellTable;
    protected const string WATERMARK_BRM = "BRMIcon";
    protected const string WATERMARK_EXPERT1 = "Set1_Icon";
    protected const string WATERMARK_FP1 = "NaxxIcon";
    protected const string WATERMARK_GVG = "GvGIcon";
    protected const string WATERMARK_LOE = "LOEIcon";
    protected const string WATERMARK_TGT = "TGTIcon";

    [DebuggerHidden]
    private IEnumerator ActivateArmorSpell(SpellStateType stateType, bool armorShouldBeOn)
    {
        return new <ActivateArmorSpell>c__Iterator30 { stateType = stateType, armorShouldBeOn = armorShouldBeOn, <$>stateType = stateType, <$>armorShouldBeOn = armorShouldBeOn, <>f__this = this };
    }

    public Spell ActivateSpell(SpellType spellType)
    {
        Spell spell = this.GetSpell(spellType);
        if (spell == null)
        {
            return null;
        }
        spell.ActivateState(SpellStateType.BIRTH);
        return spell;
    }

    public Spell ActivateTaunt()
    {
        this.DeactivateTaunt();
        if (this.GetEntity().IsStealthed() && !Options.Get().GetBool(Option.HAS_SEEN_STEALTH_TAUNTER, false))
        {
            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_STEALTH_TAUNT3_22"), "VO_INNKEEPER_STEALTH_TAUNT3_22", 0f, null);
            Options.Get().SetBool(Option.HAS_SEEN_STEALTH_TAUNTER, true);
        }
        if (this.GetCardFlair().Premium == TAG_PREMIUM.GOLDEN)
        {
            if (this.GetEntity().IsStealthed())
            {
                return this.ActivateSpell(SpellType.TAUNT_PREMIUM_STEALTH);
            }
            return this.ActivateSpell(SpellType.TAUNT_PREMIUM);
        }
        if (this.GetEntity().IsStealthed())
        {
            return this.ActivateSpell(SpellType.TAUNT_STEALTH);
        }
        return this.ActivateSpell(SpellType.TAUNT);
    }

    private void AssignBones()
    {
        this.m_bones = SceneUtils.FindChildBySubstring(base.gameObject, "Bones");
    }

    private void AssignMaterials(MeshRenderer meshRenderer)
    {
        for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
        {
            Material sharedMaterial = RenderUtils.GetSharedMaterial((Renderer) meshRenderer, i);
            if (sharedMaterial != null)
            {
                if (sharedMaterial.name.LastIndexOf("Portrait", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    this.m_legacyPortraitMaterialIndex = i;
                }
                else if (sharedMaterial.name.IndexOf("Card_Inhand_Ability_Warlock", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    this.m_legacyCardColorMaterialIndex = i;
                }
                else if (sharedMaterial.name.IndexOf("Card_Inhand_Warlock", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    this.m_legacyCardColorMaterialIndex = i;
                }
                else if (sharedMaterial.name.IndexOf("Card_Inhand_Weapon_Warlock", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    this.m_legacyCardColorMaterialIndex = i;
                }
            }
        }
    }

    private void AssignMeshRenderers()
    {
        foreach (MeshRenderer renderer in base.gameObject.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer.gameObject.name.Equals("Mesh", StringComparison.OrdinalIgnoreCase))
            {
                this.m_meshRenderer = renderer;
                foreach (MeshRenderer renderer2 in renderer.gameObject.GetComponentsInChildren<MeshRenderer>(true))
                {
                    this.AssignMaterials(renderer2);
                }
                break;
            }
        }
    }

    private void AssignRootObject()
    {
        this.m_rootObject = SceneUtils.FindChildBySubstring(base.gameObject, "RootObject");
    }

    private void AssignSpells()
    {
        this.m_spellTable = base.gameObject.GetComponentInChildren<SpellTable>();
        this.m_actorStateMgr = base.gameObject.GetComponentInChildren<ActorStateMgr>();
        if (this.m_spellTable == null)
        {
            if (!string.IsNullOrEmpty(this.m_spellTablePrefab))
            {
                SpellCache cache = SpellCache.Get();
                if (cache != null)
                {
                    SpellTable spellTable = cache.GetSpellTable(this.m_spellTablePrefab);
                    if (spellTable != null)
                    {
                        this.m_useSharedSpellTable = true;
                        this.m_sharedSpellTable = spellTable;
                        this.m_localSpellTable = new Map<SpellType, Spell>();
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("failed to load spell table: " + this.m_spellTablePrefab);
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("Null spell cache: " + this.m_spellTablePrefab);
                }
            }
        }
        else if (this.m_actorStateMgr != null)
        {
            foreach (SpellTableEntry entry in this.m_spellTable.m_Table)
            {
                if (entry.m_Spell != null)
                {
                    entry.m_Spell.AddStateStartedCallback(new Spell.StateStartedCallback(this.OnSpellStateStarted));
                }
            }
        }
    }

    public virtual void Awake()
    {
        this.AssignRootObject();
        this.AssignBones();
        this.AssignMeshRenderers();
        this.AssignSpells();
    }

    public virtual Actor Clone()
    {
        GameObject obj2 = (GameObject) UnityEngine.Object.Instantiate(base.gameObject, base.transform.position, base.transform.rotation);
        Actor component = obj2.GetComponent<Actor>();
        component.SetEntity(this.m_entity);
        component.SetEntityDef(this.m_entityDef);
        component.SetCard(this.m_card);
        component.SetCardFlair(this.m_cardFlair);
        obj2.transform.localScale = base.gameObject.transform.localScale;
        obj2.transform.position = base.gameObject.transform.position;
        component.SetActorState(this.m_actorState);
        if (this.m_shown)
        {
            component.ShowImpl(false);
            return component;
        }
        component.HideImpl(false);
        return component;
    }

    public void ContactShadow(bool visible)
    {
        string tag = "FakeShadow";
        string str2 = "FakeShadowUnique";
        GameObject obj2 = SceneUtils.FindChildByTag(base.gameObject, tag);
        GameObject obj3 = SceneUtils.FindChildByTag(base.gameObject, str2);
        if (visible)
        {
            if (this.IsElite())
            {
                if (obj2 != null)
                {
                    obj2.GetComponent<Renderer>().enabled = false;
                }
                if (obj3 != null)
                {
                    obj3.GetComponent<Renderer>().enabled = true;
                }
            }
            else
            {
                if (obj2 != null)
                {
                    obj2.GetComponent<Renderer>().enabled = true;
                }
                if (obj3 != null)
                {
                    obj3.GetComponent<Renderer>().enabled = false;
                }
            }
        }
        else
        {
            if (obj2 != null)
            {
                obj2.GetComponent<Renderer>().enabled = false;
            }
            if (obj3 != null)
            {
                obj3.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    private Material CreateReplacementMaterial(Material oldMaterial, Material newMaterialPrefab)
    {
        Material material = UnityEngine.Object.Instantiate<Material>(newMaterialPrefab);
        material.mainTexture = oldMaterial.mainTexture;
        return material;
    }

    public void DeactivateAllPreDeathSpells()
    {
        IEnumerator enumerator = Enum.GetValues(typeof(SpellType)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                SpellType current = (SpellType) ((int) enumerator.Current);
                if ((this.IsSpellActive(current) && (current != SpellType.DEATH)) && ((current != SpellType.DEATHRATTLE_DEATH) && (current != SpellType.DAMAGE)))
                {
                    this.DeactivateSpell(current);
                }
            }
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable == null)
            {
            }
            disposable.Dispose();
        }
    }

    public void DeactivateAllSpells()
    {
        if (this.m_useSharedSpellTable)
        {
            foreach (Spell spell in this.m_localSpellTable.Values)
            {
                if (spell.IsActive())
                {
                    spell.ActivateState(SpellStateType.DEATH);
                }
            }
        }
        else if (this.m_spellTable != null)
        {
            foreach (SpellTableEntry entry in this.m_spellTable.m_Table)
            {
                Spell spell2 = entry.m_Spell;
                if ((spell2 != null) && spell2.IsActive())
                {
                    spell2.ActivateState(SpellStateType.DEATH);
                }
            }
        }
    }

    public void DeactivateSpell(SpellType spellType)
    {
        Spell spellIfLoaded = this.GetSpellIfLoaded(spellType);
        if (spellIfLoaded != null)
        {
            spellIfLoaded.ActivateState(SpellStateType.DEATH);
        }
    }

    public void DeactivateTaunt()
    {
        if (this.IsSpellActive(SpellType.TAUNT))
        {
            this.DeactivateSpell(SpellType.TAUNT);
        }
        if (this.IsSpellActive(SpellType.TAUNT_PREMIUM))
        {
            this.DeactivateSpell(SpellType.TAUNT_PREMIUM);
        }
        if (this.IsSpellActive(SpellType.TAUNT_PREMIUM_STEALTH))
        {
            this.DeactivateSpell(SpellType.TAUNT_PREMIUM_STEALTH);
        }
        if (this.IsSpellActive(SpellType.TAUNT_STEALTH))
        {
            this.DeactivateSpell(SpellType.TAUNT_STEALTH);
        }
    }

    public void Destroy()
    {
        if (this.m_localSpellTable != null)
        {
            foreach (Spell spell in this.m_localSpellTable.Values)
            {
                spell.Deactivate();
            }
        }
        if (this.m_spellTable != null)
        {
            foreach (SpellTableEntry entry in this.m_spellTable.m_Table)
            {
                if (entry.m_Spell != null)
                {
                    entry.m_Spell.Deactivate();
                }
            }
        }
        UnityEngine.Object.Destroy(base.gameObject);
    }

    public void DestroySpell(SpellType spellType)
    {
        if (this.m_useSharedSpellTable)
        {
            Spell spell;
            if (this.m_localSpellTable.TryGetValue(spellType, out spell))
            {
                UnityEngine.Object.Destroy(spell.gameObject);
                this.m_localSpellTable.Remove(spellType);
            }
        }
        else
        {
            UnityEngine.Debug.LogError(string.Format("Actor.DestroySpell() - FAILED to destroy {0} because the Actor is not using a shared spell table.", spellType));
        }
    }

    public void DisableMissingCardEffect()
    {
        this.m_missingcard = false;
        if (this.m_missingCardEffect != null)
        {
            RenderToTexture component = this.m_missingCardEffect.GetComponent<RenderToTexture>();
            if (component != null)
            {
                component.enabled = false;
            }
            this.MaterialShaderAnimation(true);
        }
    }

    private void DisableTextMesh(UberText mesh)
    {
        if (mesh != null)
        {
            mesh.gameObject.SetActive(false);
        }
    }

    public GameObject FindBone(string boneName)
    {
        if (this.m_bones == null)
        {
            return null;
        }
        return SceneUtils.FindChildBySubstring(this.m_bones, boneName);
    }

    public ActorStateMgr GetActorStateMgr()
    {
        return this.m_actorStateMgr;
    }

    public ActorStateType GetActorStateType()
    {
        return ((this.m_actorStateMgr != null) ? this.m_actorStateMgr.GetActiveStateType() : ActorStateType.NONE);
    }

    public GemObject GetAttackObject()
    {
        if (this.m_attackObject == null)
        {
            return null;
        }
        return this.m_attackObject.GetComponent<GemObject>();
    }

    public UberText GetAttackText()
    {
        return this.m_attackTextMesh;
    }

    public GameObject GetAttackTextObject()
    {
        if (this.m_attackTextMesh == null)
        {
            return null;
        }
        return this.m_attackTextMesh.gameObject;
    }

    public GameObject GetBones()
    {
        return this.m_bones;
    }

    public Card GetCard()
    {
        return this.m_card;
    }

    public Player.Side GetCardBackSide()
    {
        if (this.m_cardBackSideOverride.HasValue)
        {
            return this.m_cardBackSideOverride.Value;
        }
        if (this.m_entity == null)
        {
            return Player.Side.FRIENDLY;
        }
        Player controller = this.m_entity.GetController();
        if (controller == null)
        {
            return Player.Side.FRIENDLY;
        }
        return controller.GetSide();
    }

    public Player.Side? GetCardBackSideOverride()
    {
        return this.m_cardBackSideOverride;
    }

    public bool GetCardbackUpdateIgnore()
    {
        return this.m_ignoreUpdateCardback;
    }

    public CardDef GetCardDef()
    {
        return this.m_cardDef;
    }

    public CardFlair GetCardFlair()
    {
        return this.m_cardFlair;
    }

    public TAG_CARD_SET GetCardSet()
    {
        if ((this.m_entityDef == null) && (this.m_entity == null))
        {
            return TAG_CARD_SET.NONE;
        }
        if (this.m_entityDef != null)
        {
            return this.m_entityDef.GetCardSet();
        }
        return this.m_entity.GetCardSet();
    }

    public GameObject GetCardTypeBannerAnchor()
    {
        if (this.m_cardTypeAnchorObject == null)
        {
            return base.gameObject;
        }
        return this.m_cardTypeAnchorObject;
    }

    public Collider GetCollider()
    {
        if (this.GetMeshRenderer() == null)
        {
            return null;
        }
        return this.GetMeshRenderer().gameObject.GetComponent<Collider>();
    }

    public UberText GetCostText()
    {
        if (this.m_costTextMesh == null)
        {
            return null;
        }
        return this.m_costTextMesh;
    }

    public GameObject GetCostTextObject()
    {
        if (this.m_costTextMesh == null)
        {
            return null;
        }
        return this.m_costTextMesh.gameObject;
    }

    public Entity GetEntity()
    {
        return this.m_entity;
    }

    public EntityDef GetEntityDef()
    {
        return this.m_entityDef;
    }

    public GemObject GetHealthObject()
    {
        if (this.m_healthObject == null)
        {
            return null;
        }
        return this.m_healthObject.GetComponent<GemObject>();
    }

    public UberText GetHealthText()
    {
        return this.m_healthTextMesh;
    }

    public GameObject GetHealthTextObject()
    {
        if (this.m_healthTextMesh == null)
        {
            return null;
        }
        return this.m_healthTextMesh.gameObject;
    }

    public Light GetHeroSpotlight()
    {
        if (this.m_heroSpotLight == null)
        {
            return null;
        }
        return this.m_heroSpotLight.GetComponent<Light>();
    }

    public GameObject GetHiddenStandIn()
    {
        return this.m_hiddenCardStandIn;
    }

    public HistoryCard GetHistoryCard()
    {
        if (base.transform.parent == null)
        {
            return null;
        }
        return base.transform.parent.gameObject.GetComponent<HistoryCard>();
    }

    public HistoryChildCard GetHistoryChildCard()
    {
        if (base.transform.parent == null)
        {
            return null;
        }
        return base.transform.parent.gameObject.GetComponent<HistoryChildCard>();
    }

    public Spell GetLoadedSpell(SpellType spellType)
    {
        Spell spell = null;
        if (this.m_localSpellTable != null)
        {
            this.m_localSpellTable.TryGetValue(spellType, out spell);
        }
        if (spell == null)
        {
            spell = this.LoadSpell(spellType);
        }
        return spell;
    }

    public MeshRenderer GetMeshRenderer()
    {
        return this.m_meshRenderer;
    }

    public UberText GetNameText()
    {
        return this.m_nameTextMesh;
    }

    protected virtual Material GetPortraitMaterial()
    {
        if (((this.m_portraitMesh != null) && (0 <= this.m_portraitMatIdx)) && (this.m_portraitMatIdx < this.m_portraitMesh.GetComponent<Renderer>().materials.Length))
        {
            return this.m_portraitMesh.GetComponent<Renderer>().materials[this.m_portraitMatIdx];
        }
        if (this.m_legacyPortraitMaterialIndex >= 0)
        {
            return this.m_meshRenderer.materials[this.m_legacyPortraitMaterialIndex];
        }
        return null;
    }

    public GameObject GetPortraitMesh()
    {
        return this.m_portraitMesh;
    }

    public Texture GetPortraitTexture()
    {
        Material portraitMaterial = this.GetPortraitMaterial();
        if (portraitMaterial == null)
        {
            return null;
        }
        return portraitMaterial.mainTexture;
    }

    public UberText GetPowersText()
    {
        return this.m_powersTextMesh;
    }

    public UberText GetRaceText()
    {
        return this.m_raceTextMesh;
    }

    public TAG_RARITY GetRarity()
    {
        if (this.m_entityDef != null)
        {
            return this.m_entityDef.GetRarity();
        }
        if (this.m_entity != null)
        {
            return this.m_entity.GetRarity();
        }
        return TAG_RARITY.FREE;
    }

    private bool GetRarityTextureOffset(out Vector2 offset, out Color tint)
    {
        offset = this.GEM_TEXTURE_OFFSET_COMMON;
        tint = this.GEM_COLOR_COMMON;
        if ((this.m_entityDef != null) || (this.m_entity != null))
        {
            TAG_CARD_SET cardSet;
            if (this.m_entityDef != null)
            {
                cardSet = this.m_entityDef.GetCardSet();
            }
            else
            {
                cardSet = this.m_entity.GetCardSet();
            }
            switch (cardSet)
            {
                case TAG_CARD_SET.CORE:
                    return false;

                case TAG_CARD_SET.MISSIONS:
                    return false;

                default:
                    switch (this.GetRarity())
                    {
                        case TAG_RARITY.COMMON:
                            offset = this.GEM_TEXTURE_OFFSET_COMMON;
                            tint = this.GEM_COLOR_COMMON;
                            goto Label_010A;

                        case TAG_RARITY.RARE:
                            offset = this.GEM_TEXTURE_OFFSET_RARE;
                            tint = this.GEM_COLOR_RARE;
                            goto Label_010A;

                        case TAG_RARITY.EPIC:
                            offset = this.GEM_TEXTURE_OFFSET_EPIC;
                            tint = this.GEM_COLOR_EPIC;
                            goto Label_010A;

                        case TAG_RARITY.LEGENDARY:
                            offset = this.GEM_TEXTURE_OFFSET_LEGENDARY;
                            tint = this.GEM_COLOR_LEGENDARY;
                            goto Label_010A;
                    }
                    break;
            }
        }
        return false;
    Label_010A:
        return true;
    }

    public GameObject GetRootObject()
    {
        return this.m_rootObject;
    }

    public UberText GetSecretText()
    {
        return this.m_secretText;
    }

    public Spell GetSpell(SpellType spellType)
    {
        Spell spell = null;
        if (this.m_useSharedSpellTable)
        {
            return this.GetLoadedSpell(spellType);
        }
        if (this.m_spellTable != null)
        {
            this.m_spellTable.FindSpell(spellType, out spell);
        }
        return spell;
    }

    private Spell GetSpellIfLoaded(SpellType spellType)
    {
        Spell result = null;
        if (this.m_useSharedSpellTable)
        {
            this.GetSpellIfLoaded(spellType, out result);
            return result;
        }
        if (this.m_spellTable != null)
        {
            this.m_spellTable.FindSpell(spellType, out result);
        }
        return result;
    }

    public bool GetSpellIfLoaded(SpellType spellType, out Spell result)
    {
        if ((this.m_localSpellTable == null) || !this.m_localSpellTable.ContainsKey(spellType))
        {
            result = null;
            return false;
        }
        result = this.m_localSpellTable[spellType];
        return (result != null);
    }

    public SpellTable GetSpellTable()
    {
        return this.m_spellTable;
    }

    public void GhostCardEffect(bool enabled = true)
    {
        if (this.m_ghostCard != enabled)
        {
            this.m_ghostCard = enabled;
            this.UpdateAllComponents();
        }
    }

    public void Hide()
    {
        if (this.m_shown)
        {
            this.m_shown = false;
            this.HideImpl(false);
        }
    }

    public void Hide(bool ignoreSpells)
    {
        if (this.m_shown)
        {
            this.m_shown = false;
            this.HideImpl(ignoreSpells);
        }
    }

    public void HideActorState()
    {
    }

    public void HideAllText()
    {
        this.ToggleTextVisibility(false);
    }

    public void HideArmorSpell()
    {
        this.m_armorSpell.gameObject.SetActive(false);
    }

    protected virtual void HideImpl(bool ignoreSpells)
    {
        if (this.m_rootObject != null)
        {
            this.m_rootObject.SetActive(false);
        }
        if (this.m_armorSpell != null)
        {
            this.m_armorSpell.Hide();
        }
        if (this.m_actorStateMgr != null)
        {
            this.m_actorStateMgr.HideStateMgr();
        }
        if (this.m_projectedShadow != null)
        {
            this.m_projectedShadow.enabled = false;
        }
        if (this.m_ghostCardGameObject != null)
        {
            this.m_ghostCardGameObject.SetActive(false);
        }
        if (!ignoreSpells)
        {
            this.HideSpellTable();
        }
        if (this.m_missingCardEffect != null)
        {
            this.UpdateMissingCardArt();
        }
        HighlightState componentInChildren = base.GetComponentInChildren<HighlightState>();
        if (componentInChildren != null)
        {
            componentInChildren.Hide();
        }
    }

    public void HideSpellTable()
    {
        if (this.m_localSpellTable != null)
        {
            foreach (Spell spell in this.m_localSpellTable.Values)
            {
                if (spell.GetSpellType() != SpellType.NONE)
                {
                    spell.Hide();
                }
            }
        }
        if (this.m_spellTable != null)
        {
            this.m_spellTable.Hide();
        }
    }

    public void Init()
    {
        if (this.m_portraitMesh != null)
        {
            this.m_initialPortraitMaterial = RenderUtils.GetMaterial(this.m_portraitMesh, this.m_portraitMatIdx);
        }
        else if (this.m_legacyPortraitMaterialIndex >= 0)
        {
            this.m_initialPortraitMaterial = RenderUtils.GetMaterial((Renderer) this.m_meshRenderer, this.m_legacyPortraitMaterialIndex);
        }
        if (this.m_rootObject != null)
        {
            TransformUtil.Identity(this.m_rootObject.transform);
        }
        if (this.m_actorStateMgr != null)
        {
            this.m_actorStateMgr.ChangeState(this.m_actorState);
        }
        this.m_projectedShadow = base.GetComponent<ProjectedShadow>();
        if (this.m_shown)
        {
            this.ShowImpl(false);
        }
        else
        {
            this.HideImpl(false);
        }
        this.m_DisablePremiumPortrait = GraphicsManager.Get().isVeryLowQualityDevice();
    }

    public bool IsElite()
    {
        if (this.m_entityDef != null)
        {
            return this.m_entityDef.IsElite();
        }
        return ((this.m_entity != null) && this.m_entity.IsElite());
    }

    public bool isGhostCard()
    {
        return (this.m_ghostCard && ((bool) this.m_ghostCardGameObject));
    }

    public bool isMissingCard()
    {
        if (this.m_missingCardEffect == null)
        {
            return false;
        }
        RenderToTexture component = this.m_missingCardEffect.GetComponent<RenderToTexture>();
        if (component == null)
        {
            return false;
        }
        return component.enabled;
    }

    public bool IsShown()
    {
        return this.m_shown;
    }

    public bool IsSpellActive(SpellType spellType)
    {
        Spell spellIfLoaded = this.GetSpellIfLoaded(spellType);
        if (spellIfLoaded == null)
        {
            return false;
        }
        return spellIfLoaded.IsActive();
    }

    public bool IsSpellDyingOrNone(SpellType spellType)
    {
        Spell spellIfLoaded = this.GetSpellIfLoaded(spellType);
        return ((spellIfLoaded == null) || ((spellIfLoaded.GetActiveState() == SpellStateType.DEATH) || (spellIfLoaded.GetActiveState() == SpellStateType.NONE)));
    }

    private void LoadArmorSpell()
    {
        if (this.m_armorSpellBone != null)
        {
            this.m_armorSpellLoading = true;
            string name = "Assets/Game/Spells/Armor/Hero_Armor";
            if ((this.GetCardDef() != null) && (this.GetCardDef().m_CustomHeroArmorSpell != string.Empty))
            {
                name = this.GetCardDef().m_CustomHeroArmorSpell;
            }
            AssetLoader.Get().LoadSpell(name, new AssetLoader.GameObjectCallback(this.OnArmorSpellLoaded), null, false);
        }
    }

    public Spell LoadSpell(SpellType spellType)
    {
        if (this.m_sharedSpellTable == null)
        {
            return null;
        }
        Spell spell = this.m_sharedSpellTable.GetSpell(spellType);
        if (spell == null)
        {
            return null;
        }
        this.m_localSpellTable.Add(spellType, spell);
        Transform child = spell.gameObject.transform;
        Transform transform = base.gameObject.transform;
        TransformUtil.AttachAndPreserveLocalTransform(child, transform);
        child.localScale.Scale(this.m_sharedSpellTable.gameObject.transform.localScale);
        SceneUtils.SetLayer(spell.gameObject, (GameLayer) base.gameObject.layer);
        spell.OnLoad();
        if (this.m_actorStateMgr != null)
        {
            spell.AddStateStartedCallback(new Spell.StateStartedCallback(this.OnSpellStateStarted));
        }
        return spell;
    }

    public void MaterialShaderAnimation(bool animationEnabled)
    {
        float num = 0f;
        if (animationEnabled)
        {
            num = 1f;
        }
        foreach (Renderer renderer in base.GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if ((material != null) && material.HasProperty("_TimeScale"))
                {
                    material.SetFloat("_TimeScale", num);
                }
            }
        }
    }

    public bool MissingCardEffect()
    {
        if ((this.m_missingCardEffect != null) && (this.m_missingCardEffect.GetComponent<RenderToTexture>() != null))
        {
            this.m_missingcard = true;
            this.UpdateAllComponents();
            return true;
        }
        return false;
    }

    private void OnArmorSpellLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0} - Actor.OnArmorSpellLoaded() - failed to load Hero_Armor spell! m_armorSpell GameObject = null!", name));
        }
        else
        {
            this.m_armorSpellLoading = false;
            this.m_armorSpell = go.GetComponent<ArmorSpell>();
            if (this.m_armorSpell == null)
            {
                UnityEngine.Debug.LogError(string.Format("{0} - Actor.OnArmorSpellLoaded() - failed to load Hero_Armor spell! m_armorSpell Spell = null!", name));
            }
            else
            {
                go.transform.parent = this.m_armorSpellBone.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
        }
    }

    private void OnEnable()
    {
        if (this.isPortraitMaterialDirty)
        {
            this.UpdateAllComponents();
        }
    }

    private void OnSpellStateStarted(Spell spell, SpellStateType prevStateType, object userData)
    {
        spell.AddStateStartedCallback(new Spell.StateStartedCallback(this.OnSpellStateStarted));
        this.m_actorStateMgr.RefreshStateMgr();
        if (this.m_projectedShadow != null)
        {
            this.m_projectedShadow.UpdateContactShadow();
        }
    }

    private void OnWatermarkLoaded(string name, UnityEngine.Object tex, object callbackData)
    {
        if (this.m_descriptionMesh != null)
        {
            Texture texture = (Texture) tex;
            this.m_descriptionMesh.GetComponent<Renderer>().material.SetTexture("_SecondTex", texture);
        }
    }

    public void OverrideAllMeshMaterials(Material material)
    {
        if (this.m_rootObject != null)
        {
            this.RecursivelyReplaceMaterialsList(this.m_rootObject.transform, material);
        }
    }

    public void OverrideNameText(UberText newText)
    {
        if (this.m_nameTextMesh != null)
        {
            this.m_nameTextMesh.gameObject.SetActive(false);
        }
        this.m_nameTextMesh = newText;
        this.UpdateNameText();
        if (this.m_shown && (newText != null))
        {
            newText.gameObject.SetActive(true);
        }
    }

    private void RecursivelyReplaceMaterialsList(Transform transformToRecurse, Material newMaterialPrefab)
    {
        bool flag = true;
        if (transformToRecurse.GetComponent<MaterialReplacementExclude>() != null)
        {
            flag = false;
        }
        else if (transformToRecurse.GetComponent<UberText>() != null)
        {
            flag = false;
        }
        else if (transformToRecurse.GetComponent<Renderer>() == null)
        {
            flag = false;
        }
        if (flag)
        {
            this.ReplaceMaterialsList(transformToRecurse.GetComponent<Renderer>(), newMaterialPrefab);
        }
        IEnumerator enumerator = transformToRecurse.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Transform current = (Transform) enumerator.Current;
                this.RecursivelyReplaceMaterialsList(current, newMaterialPrefab);
            }
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable == null)
            {
            }
            disposable.Dispose();
        }
        this.m_lightBlendMaterials = null;
        this.m_lightBlendUberText = null;
    }

    public void ReleasePortrait()
    {
        this.SetPortraitMaterial(null);
        this.SetPortraitTexture(null);
        this.SetPortraitTextureOverride(null);
        this.m_lightBlendMaterials = null;
        this.m_lightBlendUberText = null;
    }

    private void ReplaceMaterialsList(Renderer renderer, Material newMaterialPrefab)
    {
        Material[] materialArray = new Material[renderer.materials.Length];
        for (int i = 0; i < renderer.materials.Length; i++)
        {
            Material oldMaterial = renderer.materials[i];
            materialArray[i] = this.CreateReplacementMaterial(oldMaterial, newMaterialPrefab);
        }
        renderer.materials = materialArray;
        this.m_lightBlendMaterials = null;
        this.m_lightBlendUberText = null;
        if (renderer == this.m_meshRenderer)
        {
            this.UpdatePortraitTexture();
        }
    }

    public void SeedMaterialEffects()
    {
        if (!this.m_materialEffectsSeeded)
        {
            this.m_materialEffectsSeeded = true;
            Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
            float num = UnityEngine.Random.Range((float) 0f, (float) 2f);
            foreach (Renderer renderer in componentsInChildren)
            {
                if (renderer.sharedMaterials.Length == 1)
                {
                    if (renderer.material.HasProperty("_Seed") && (renderer.material.GetFloat("_Seed") == 0f))
                    {
                        renderer.material.SetFloat("_Seed", num);
                    }
                }
                else
                {
                    Material[] materials = renderer.materials;
                    if ((materials != null) && (materials.Length != 0))
                    {
                        foreach (Material material in materials)
                        {
                            if ((material != null) && (material.HasProperty("_Seed") && (material.GetFloat("_Seed") == 0f)))
                            {
                                material.SetFloat("_Seed", num);
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetActorState(ActorStateType stateType)
    {
        this.m_actorState = stateType;
        if (this.m_actorStateMgr != null)
        {
            if (this.forceIdleState)
            {
                this.m_actorState = ActorStateType.CARD_IDLE;
            }
            else if ((GameState.Get() != null) && GameState.Get().IsMulliganBlockingPowers())
            {
                this.m_actorState = ActorStateType.CARD_IDLE;
            }
            this.m_actorStateMgr.ChangeState(this.m_actorState);
        }
    }

    public void SetCard(Card card)
    {
        if (this.m_card != card)
        {
            if (card == null)
            {
                this.m_card = null;
                base.transform.parent = null;
            }
            else
            {
                this.m_card = card;
                base.transform.parent = card.transform;
                TransformUtil.Identity(base.transform);
                if (this.m_rootObject != null)
                {
                    TransformUtil.Identity(this.m_rootObject.transform);
                }
            }
        }
    }

    public void SetCardBackSideOverride(Player.Side? sideOverride)
    {
        this.m_cardBackSideOverride = sideOverride;
    }

    public void SetCardbackUpdateIgnore(bool ignoreUpdate)
    {
        this.m_ignoreUpdateCardback = ignoreUpdate;
    }

    public void SetCardDef(CardDef cardDef)
    {
        if (this.m_cardDef != cardDef)
        {
            this.m_cardDef = cardDef;
            this.LoadArmorSpell();
        }
    }

    public virtual void SetCardFlair(CardFlair cardFlair)
    {
        this.m_cardFlair = cardFlair;
    }

    public void SetDisablePremiumPortrait(bool disable)
    {
        this.m_DisablePremiumPortrait = disable;
    }

    public void SetEntity(Entity entity)
    {
        this.m_entity = entity;
        if (this.m_entity != null)
        {
            this.SetCardFlair(this.m_entity.GetCardFlair());
        }
    }

    public void SetEntityDef(EntityDef entityDef)
    {
        this.m_entityDef = entityDef;
    }

    public void SetHiddenStandIn(GameObject standIn)
    {
        this.m_hiddenCardStandIn = standIn;
    }

    public void SetHistoryCard(HistoryCard card)
    {
        if (card == null)
        {
            base.transform.parent = null;
        }
        else
        {
            base.transform.parent = card.transform;
            TransformUtil.Identity(base.transform);
            if (this.m_rootObject != null)
            {
                TransformUtil.Identity(this.m_rootObject.transform);
            }
            this.m_entity = card.GetEntity();
            this.UpdateTextComponents(this.m_entity);
            this.UpdateMeshComponents();
            if (((this.m_cardFlair != null) && (this.m_cardFlair.Premium == TAG_PREMIUM.GOLDEN)) && (card.GetBigCardPotraitGoldenMaterial() != null))
            {
                Material bigCardPotraitGoldenMaterial = card.GetBigCardPotraitGoldenMaterial();
                this.SetPortraitMaterial(bigCardPotraitGoldenMaterial);
            }
            else
            {
                Texture bigCardPortraitTexture = card.GetBigCardPortraitTexture();
                this.SetPortraitTextureOverride(bigCardPortraitTexture);
            }
            if (this.m_spellTable != null)
            {
                foreach (SpellTableEntry entry in this.m_spellTable.m_Table)
                {
                    Spell spell = entry.m_Spell;
                    if (spell != null)
                    {
                        spell.m_BlockServerEvents = false;
                    }
                }
            }
        }
    }

    public void SetHistoryChildCard(HistoryChildCard card)
    {
        if (card == null)
        {
            base.transform.parent = null;
        }
        else
        {
            base.transform.parent = card.transform;
            TransformUtil.Identity(base.transform);
            if (this.m_rootObject != null)
            {
                TransformUtil.Identity(this.m_rootObject.transform);
            }
            this.m_entity = card.GetEntity();
            this.UpdateTextComponents(card.GetEntity());
            this.UpdateMeshComponents();
            if ((this.m_cardFlair.Premium == TAG_PREMIUM.GOLDEN) && (card.GetBigCardPotraitGoldenMaterial() != null))
            {
                Material bigCardPotraitGoldenMaterial = card.GetBigCardPotraitGoldenMaterial();
                this.SetPortraitMaterial(bigCardPotraitGoldenMaterial);
            }
            else
            {
                Texture bigCardPortraitTexture = card.GetBigCardPortraitTexture();
                this.SetPortraitTextureOverride(bigCardPortraitTexture);
            }
            if (this.m_spellTable != null)
            {
                foreach (SpellTableEntry entry in this.m_spellTable.m_Table)
                {
                    Spell spell = entry.m_Spell;
                    if (spell != null)
                    {
                        spell.m_BlockServerEvents = false;
                    }
                }
            }
        }
    }

    public void SetLightBlend(float blendValue)
    {
        if (this.m_lightBlendMaterials == null)
        {
            this.m_lightBlendMaterials = new List<Material>();
            foreach (Renderer renderer in base.GetComponentsInChildren<Renderer>())
            {
                foreach (Material material in renderer.materials)
                {
                    if ((material != null) && material.HasProperty("_LightingBlend"))
                    {
                        this.m_lightBlendMaterials.Add(material);
                    }
                }
            }
            this.m_lightBlendUberText = new List<UberText>();
            foreach (UberText text in base.GetComponentsInChildren<UberText>())
            {
                this.m_lightBlendUberText.Add(text);
            }
        }
        foreach (Material material2 in this.m_lightBlendMaterials)
        {
            if ((material2 != null) && material2.HasProperty("_LightingBlend"))
            {
                material2.SetFloat("_LightingBlend", blendValue);
            }
        }
        foreach (UberText text2 in this.m_lightBlendUberText)
        {
            if (text2 != null)
            {
                text2.AmbientLightBlend = blendValue;
            }
        }
    }

    private void SetLightingBlend(float value)
    {
        foreach (Renderer renderer in base.GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material material in renderer.materials)
            {
                if ((material != null) && material.HasProperty("_LightingBlend"))
                {
                    material.SetFloat("_LightingBlend", value);
                }
            }
        }
        foreach (UberText text in base.GetComponentsInChildren<UberText>(true))
        {
            text.AmbientLightBlend = value;
        }
    }

    public void SetLit()
    {
        this.SetLightingBlend(1f);
    }

    public void SetMissingCardMaterial(Material missingCardMat)
    {
        if ((this.m_missingCardEffect != null) && (missingCardMat != null))
        {
            RenderToTexture component = this.m_missingCardEffect.GetComponent<RenderToTexture>();
            if ((component != null) && this.m_rootObject.activeSelf)
            {
                component.m_Material = missingCardMat;
                this.MaterialShaderAnimation(false);
                if (component.enabled)
                {
                    component.Render();
                }
            }
        }
    }

    public void SetPortraitMaterial(Material material)
    {
        if (material != null)
        {
            if ((this.m_portraitMesh != null) && (this.m_portraitMatIdx > -1))
            {
                Material material2 = RenderUtils.GetMaterial(this.m_portraitMesh, this.m_portraitMatIdx);
                if ((material2.mainTexture != material.mainTexture) || (material2.shader != material.shader))
                {
                    if (material == null)
                    {
                        RenderUtils.SetMaterial(this.m_portraitMesh, this.m_portraitMatIdx, this.m_initialPortraitMaterial);
                    }
                    else
                    {
                        RenderUtils.SetMaterial(this.m_portraitMesh, this.m_portraitMatIdx, material);
                    }
                    if (this.m_entity != null)
                    {
                        float num = 0f;
                        if (this.m_entity.GetZone() == TAG_ZONE.PLAY)
                        {
                            num = 1f;
                        }
                        foreach (Material material3 in this.m_portraitMesh.GetComponent<Renderer>().materials)
                        {
                            if (material3.HasProperty("_LightingBlend"))
                            {
                                material3.SetFloat("_LightingBlend", num);
                            }
                            if (material3.HasProperty("_Seed") && (material3.GetFloat("_Seed") == 0f))
                            {
                                material3.SetFloat("_Seed", UnityEngine.Random.Range((float) 0f, (float) 2f));
                            }
                        }
                    }
                }
            }
            else if ((this.m_legacyPortraitMaterialIndex >= 0) && (RenderUtils.GetMaterial((Renderer) this.m_meshRenderer, this.m_legacyPortraitMaterialIndex) != material))
            {
                RenderUtils.SetMaterial((Renderer) this.m_meshRenderer, this.m_legacyPortraitMaterialIndex, material);
            }
        }
    }

    public void SetPortraitTexture(Texture texture)
    {
        TAG_PREMIUM premiumType = CardFlair.GetPremiumType(this.m_cardFlair);
        if ((((this.m_cardDef == null) || this.m_DisablePremiumPortrait) || ((premiumType != TAG_PREMIUM.GOLDEN) && !this.m_cardDef.m_AlwaysRenderPremiumPortrait)) || (this.m_cardDef.GetPremiumPortraitMaterial() == null))
        {
            Material portraitMaterial = this.GetPortraitMaterial();
            if (portraitMaterial != null)
            {
                portraitMaterial.mainTexture = texture;
            }
        }
    }

    public void SetPortraitTextureOverride(Texture portrait)
    {
        this.m_portraitTextureOverride = portrait;
        this.UpdatePortraitTexture();
    }

    public void SetPremium(TAG_PREMIUM premium)
    {
        CardFlair cardFlair = new CardFlair(premium);
        this.SetCardFlair(cardFlair);
    }

    public void SetShadowform(bool shadowform)
    {
        this.m_shadowform = shadowform;
    }

    public void SetUnlit()
    {
        this.SetLightingBlend(0f);
    }

    public void Show()
    {
        if (!this.m_shown)
        {
            this.m_shown = true;
            this.ShowImpl(false);
        }
    }

    public void Show(bool ignoreSpells)
    {
        if (!this.m_shown)
        {
            this.m_shown = true;
            this.ShowImpl(ignoreSpells);
        }
    }

    public void ShowActorState()
    {
    }

    public void ShowAllText()
    {
        this.ToggleTextVisibility(true);
    }

    protected virtual void ShowImpl(bool ignoreSpells)
    {
        if (this.m_rootObject != null)
        {
            this.m_rootObject.SetActive(true);
        }
        this.ShowAllText();
        this.UpdateAllComponents();
        if (this.m_projectedShadow != null)
        {
            this.m_projectedShadow.enabled = true;
        }
        if (this.m_actorStateMgr != null)
        {
            this.m_actorStateMgr.ShowStateMgr();
        }
        if (!ignoreSpells)
        {
            this.ShowSpellTable();
        }
        if (this.m_ghostCardGameObject != null)
        {
            this.m_ghostCardGameObject.SetActive(true);
        }
        HighlightState componentInChildren = base.GetComponentInChildren<HighlightState>();
        if (componentInChildren != null)
        {
            componentInChildren.Show();
        }
    }

    public void ShowSpellTable()
    {
        if (this.m_localSpellTable != null)
        {
            foreach (Spell spell in this.m_localSpellTable.Values)
            {
                spell.Show();
            }
        }
        if (this.m_spellTable != null)
        {
            this.m_spellTable.Show();
        }
    }

    private void Start()
    {
        this.Init();
    }

    public void ToggleCollider(bool enabled)
    {
        MeshRenderer meshRenderer = this.GetMeshRenderer();
        if ((meshRenderer != null) && (meshRenderer.gameObject.GetComponent<Collider>() != null))
        {
            meshRenderer.gameObject.GetComponent<Collider>().enabled = enabled;
        }
    }

    public void ToggleForceIdle(bool bOn)
    {
        this.forceIdleState = bOn;
    }

    private void ToggleTextVisibility(bool bOn)
    {
        if (this.m_healthTextMesh != null)
        {
            this.m_healthTextMesh.gameObject.SetActive(bOn);
        }
        if (this.m_attackTextMesh != null)
        {
            this.m_attackTextMesh.gameObject.SetActive(bOn);
        }
        if (this.m_nameTextMesh != null)
        {
            this.m_nameTextMesh.gameObject.SetActive(bOn);
            if (this.m_nameTextMesh.RenderOnObject != null)
            {
                this.m_nameTextMesh.RenderOnObject.GetComponent<Renderer>().enabled = bOn;
            }
        }
        if (this.m_powersTextMesh != null)
        {
            this.m_powersTextMesh.gameObject.SetActive(bOn);
        }
        if (this.m_costTextMesh != null)
        {
            this.m_costTextMesh.gameObject.SetActive(bOn);
        }
        if (this.m_raceTextMesh != null)
        {
            this.m_raceTextMesh.gameObject.SetActive(bOn);
        }
        if (this.m_secretText != null)
        {
            this.m_secretText.gameObject.SetActive(bOn);
        }
    }

    public void TurnOffCollider()
    {
        this.ToggleCollider(false);
    }

    public void TurnOnCollider()
    {
        this.ToggleCollider(true);
    }

    public void UpdateAllComponents()
    {
        this.UpdateTextComponents();
        this.UpdateMaterials();
        this.UpdateTextures();
        this.UpdateCardBack();
        this.UpdateMeshComponents();
        this.UpdateRootObjectSpellComponents();
        this.UpdateMissingCardArt();
        this.UpdateGhostCardEffect();
    }

    private void UpdateArmorSpell()
    {
        if (this.m_armorSpell.gameObject.activeInHierarchy)
        {
            int armor = this.m_entity.GetArmor();
            int num2 = this.m_armorSpell.GetArmor();
            this.m_armorSpell.SetArmor(armor);
            if (armor > 0)
            {
                bool flag = this.m_armorSpell.IsShown();
                if (!flag)
                {
                    this.m_armorSpell.Show();
                }
                if (num2 <= 0)
                {
                    base.StartCoroutine(this.ActivateArmorSpell(SpellStateType.BIRTH, true));
                }
                else if (num2 > armor)
                {
                    base.StartCoroutine(this.ActivateArmorSpell(SpellStateType.ACTION, true));
                }
                else if (num2 < armor)
                {
                    base.StartCoroutine(this.ActivateArmorSpell(SpellStateType.CANCEL, true));
                }
                else if (!flag)
                {
                    base.StartCoroutine(this.ActivateArmorSpell(SpellStateType.IDLE, true));
                }
            }
            else if (num2 > 0)
            {
                base.StartCoroutine(this.ActivateArmorSpell(SpellStateType.DEATH, false));
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateArmorSpellWhenLoaded()
    {
        return new <UpdateArmorSpellWhenLoaded>c__Iterator2F { <>f__this = this };
    }

    public void UpdateCardBack()
    {
        if (!this.m_ignoreUpdateCardback)
        {
            CardBackManager manager = CardBackManager.Get();
            if (manager != null)
            {
                bool friendlySide = this.GetCardBackSide() == Player.Side.FRIENDLY;
                this.UpdateCardBackDisplay(friendlySide);
                this.UpdateCardBackDragEffect();
                if ((this.m_cardMesh != null) && (this.m_cardBackMatIdx >= 0))
                {
                    manager.SetCardBackTexture(this.m_cardMesh.GetComponent<Renderer>(), this.m_cardBackMatIdx, friendlySide);
                }
            }
        }
    }

    private void UpdateCardBackDisplay(bool friendlySide)
    {
        CardBackDisplay componentInChildren = base.GetComponentInChildren<CardBackDisplay>();
        if (componentInChildren != null)
        {
            componentInChildren.SetCardBack(friendlySide);
        }
    }

    private void UpdateCardBackDragEffect()
    {
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
        {
            CardBackDragEffect componentInChildren = base.GetComponentInChildren<CardBackDragEffect>();
            if (componentInChildren != null)
            {
                componentInChildren.SetEffect();
            }
        }
    }

    public void UpdateCardColor()
    {
        if (((this.m_legacyPortraitMaterialIndex >= 0) || (this.m_cardMesh != null)) && ((this.GetEntityDef() != null) || (this.GetEntity() != null)))
        {
            TAG_CARDTYPE cardType;
            TAG_CLASS tag_class;
            CardColorSwitcher.CardColorType type;
            TAG_PREMIUM premiumType = CardFlair.GetPremiumType(this.m_cardFlair);
            if (this.m_entityDef != null)
            {
                cardType = this.m_entityDef.GetCardType();
                tag_class = this.m_entityDef.GetClass();
            }
            else
            {
                cardType = this.m_entity.GetCardType();
                tag_class = this.m_entity.GetClass();
            }
            Color magenta = Color.magenta;
            switch (tag_class)
            {
                case TAG_CLASS.DRUID:
                    type = CardColorSwitcher.CardColorType.TYPE_DRUID;
                    magenta = this.CLASS_COLOR_DRUID;
                    break;

                case TAG_CLASS.HUNTER:
                    type = CardColorSwitcher.CardColorType.TYPE_HUNTER;
                    magenta = this.CLASS_COLOR_HUNTER;
                    break;

                case TAG_CLASS.MAGE:
                    type = CardColorSwitcher.CardColorType.TYPE_MAGE;
                    magenta = this.CLASS_COLOR_MAGE;
                    break;

                case TAG_CLASS.PALADIN:
                    type = CardColorSwitcher.CardColorType.TYPE_PALADIN;
                    magenta = this.CLASS_COLOR_PALADIN;
                    break;

                case TAG_CLASS.PRIEST:
                    type = CardColorSwitcher.CardColorType.TYPE_PRIEST;
                    magenta = this.CLASS_COLOR_PRIEST;
                    break;

                case TAG_CLASS.ROGUE:
                    type = CardColorSwitcher.CardColorType.TYPE_ROGUE;
                    magenta = this.CLASS_COLOR_ROGUE;
                    break;

                case TAG_CLASS.SHAMAN:
                    type = CardColorSwitcher.CardColorType.TYPE_SHAMAN;
                    magenta = this.CLASS_COLOR_SHAMAN;
                    break;

                case TAG_CLASS.WARLOCK:
                    type = CardColorSwitcher.CardColorType.TYPE_WARLOCK;
                    magenta = this.CLASS_COLOR_WARLOCK;
                    break;

                case TAG_CLASS.WARRIOR:
                    type = CardColorSwitcher.CardColorType.TYPE_WARRIOR;
                    magenta = this.CLASS_COLOR_WARRIOR;
                    break;

                case TAG_CLASS.DREAM:
                    type = CardColorSwitcher.CardColorType.TYPE_HUNTER;
                    magenta = this.CLASS_COLOR_HUNTER;
                    break;

                default:
                    type = CardColorSwitcher.CardColorType.TYPE_GENERIC;
                    magenta = this.CLASS_COLOR_GENERIC;
                    break;
            }
            switch (cardType)
            {
                case TAG_CARDTYPE.HERO:
                    if (this.m_entity != null)
                    {
                        if (!this.m_entity.IsControlledByFriendlySidePlayer() && this.m_entity.IsHistoryDupe())
                        {
                            Transform transform = this.GetRootObject().transform.FindChild("History_Hero_Banner");
                            if (transform != null)
                            {
                                transform.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.005f, -0.505f);
                            }
                        }
                        break;
                    }
                    break;

                case TAG_CARDTYPE.MINION:
                    switch (premiumType)
                    {
                        case TAG_PREMIUM.NORMAL:
                            if (CardColorSwitcher.Get() != null)
                            {
                                Texture minionTexture = null;
                                if (CardColorSwitcher.Get() != null)
                                {
                                    minionTexture = CardColorSwitcher.Get().GetMinionTexture(type);
                                }
                                if (this.m_cardMesh != null)
                                {
                                    if (this.m_cardFrontMatIdx > -1)
                                    {
                                        this.m_cardMesh.GetComponent<Renderer>().materials[this.m_cardFrontMatIdx].mainTexture = minionTexture;
                                    }
                                }
                                else if (this.m_legacyCardColorMaterialIndex >= 0)
                                {
                                    this.m_meshRenderer.GetComponent<Renderer>().materials[this.m_legacyCardColorMaterialIndex].mainTexture = minionTexture;
                                }
                            }
                            return;

                        case TAG_PREMIUM.GOLDEN:
                            if ((this.m_premiumRibbon >= 0) && (this.m_cardMesh != null))
                            {
                                Material material2 = this.m_cardMesh.GetComponent<Renderer>().materials[this.m_premiumRibbon];
                                material2.color = magenta;
                            }
                            return;
                    }
                    UnityEngine.Debug.LogWarning(string.Format("Actor.UpdateCardColor(): unexpected premium type {0}", premiumType));
                    break;

                case TAG_CARDTYPE.SPELL:
                    switch (premiumType)
                    {
                        case TAG_PREMIUM.NORMAL:
                            if (CardColorSwitcher.Get() != null)
                            {
                                Texture spellTexture = null;
                                if (CardColorSwitcher.Get() != null)
                                {
                                    spellTexture = CardColorSwitcher.Get().GetSpellTexture(type);
                                }
                                if (this.m_cardMesh != null)
                                {
                                    if (this.m_cardFrontMatIdx > -1)
                                    {
                                        this.m_cardMesh.GetComponent<Renderer>().materials[this.m_cardFrontMatIdx].mainTexture = spellTexture;
                                    }
                                    if ((this.m_portraitMesh != null) && (this.m_portraitFrameMatIdx > -1))
                                    {
                                        this.m_portraitMesh.GetComponent<Renderer>().materials[this.m_portraitFrameMatIdx].mainTexture = spellTexture;
                                    }
                                }
                                else if ((this.m_legacyCardColorMaterialIndex >= 0) && (this.m_meshRenderer != null))
                                {
                                    this.m_meshRenderer.materials[this.m_legacyCardColorMaterialIndex].mainTexture = spellTexture;
                                }
                            }
                            return;

                        case TAG_PREMIUM.GOLDEN:
                            if ((this.m_premiumRibbon >= 0) && (this.m_cardMesh != null))
                            {
                                Material material = this.m_cardMesh.GetComponent<Renderer>().materials[this.m_premiumRibbon];
                                material.color = magenta;
                            }
                            return;
                    }
                    UnityEngine.Debug.LogWarning(string.Format("Actor.UpdateCardColor(): unexpected premium type {0}", premiumType));
                    break;

                case TAG_CARDTYPE.WEAPON:
                    switch (premiumType)
                    {
                        case TAG_PREMIUM.NORMAL:
                            if (this.m_descriptionTrimMesh != null)
                            {
                                this.m_descriptionTrimMesh.GetComponent<Renderer>().material.SetColor("_Color", magenta);
                            }
                            return;

                        case TAG_PREMIUM.GOLDEN:
                            if ((this.m_premiumRibbon >= 0) && (this.m_cardMesh != null))
                            {
                                Material material3 = this.m_cardMesh.GetComponent<Renderer>().materials[this.m_premiumRibbon];
                                material3.color = magenta;
                            }
                            return;
                    }
                    break;
            }
        }
    }

    private void UpdateEliteComponent()
    {
        if (this.m_eliteObject != null)
        {
            bool enable = this.IsElite();
            SceneUtils.EnableRenderers(this.m_eliteObject, enable, true);
        }
    }

    private void UpdateGhostCardEffect()
    {
        if (this.m_ghostCardGameObject != null)
        {
            GhostCard component = this.m_ghostCardGameObject.GetComponent<GhostCard>();
            if (component != null)
            {
                if (this.m_ghostCard)
                {
                    component.RenderGhostCard();
                }
                else
                {
                    component.DisableGhost();
                }
            }
        }
    }

    public void UpdateMaterials()
    {
        this.m_lightBlendMaterials = null;
        this.m_lightBlendUberText = null;
        if (base.gameObject.activeInHierarchy)
        {
            base.StartCoroutine(this.UpdatePortraitMaterials());
        }
        else
        {
            this.isPortraitMaterialDirty = true;
        }
    }

    public void UpdateMeshComponents()
    {
        this.UpdateRarityComponent();
        this.UpdateWatermark();
        this.UpdateEliteComponent();
        this.UpdatePremiumComponents();
        this.UpdateCardColor();
    }

    public void UpdateMissingCardArt()
    {
        if (this.m_missingcard && (this.m_missingCardEffect != null))
        {
            RenderToTexture component = this.m_missingCardEffect.GetComponent<RenderToTexture>();
            if (component != null)
            {
                if (this.m_rootObject.activeSelf)
                {
                    this.MaterialShaderAnimation(false);
                    component.enabled = true;
                    component.Show(true);
                }
                else
                {
                    component.Hide();
                }
            }
        }
    }

    private void UpdateNameText()
    {
        string name;
        if (this.m_nameTextMesh == null)
        {
            return;
        }
        bool flag = false;
        if (this.m_entityDef != null)
        {
            name = this.m_entityDef.GetName();
        }
        else
        {
            flag = (this.m_entity.IsSecret() && this.m_entity.IsHidden()) && this.m_entity.IsControlledByConcealedPlayer();
            name = this.m_entity.GetName();
        }
        if (flag)
        {
            if (GameState.Get().GetGameEntity().ShouldUseSecretClassNames())
            {
                switch (this.m_entity.GetClass())
                {
                    case TAG_CLASS.HUNTER:
                        name = GameStrings.Get("GAMEPLAY_SECRET_NAME_HUNTER");
                        goto Label_00FC;

                    case TAG_CLASS.MAGE:
                        name = GameStrings.Get("GAMEPLAY_SECRET_NAME_MAGE");
                        goto Label_00FC;

                    case TAG_CLASS.PALADIN:
                        name = GameStrings.Get("GAMEPLAY_SECRET_NAME_PALADIN");
                        goto Label_00FC;
                }
                name = GameStrings.Get("GAMEPLAY_SECRET_NAME");
            }
            else
            {
                name = GameStrings.Get("GAMEPLAY_SECRET_NAME");
            }
        }
    Label_00FC:
        this.UpdateText(this.m_nameTextMesh, name);
    }

    private void UpdateNumberText(UberText textMesh, string newText)
    {
        this.UpdateNumberText(textMesh, newText, false);
    }

    private void UpdateNumberText(UberText textMesh, string newText, bool shouldHide)
    {
        GemObject obj2 = SceneUtils.FindComponentInThisOrParents<GemObject>(textMesh.gameObject);
        if (obj2 != null)
        {
            if (!obj2.IsNumberHidden())
            {
                if (shouldHide)
                {
                    textMesh.gameObject.SetActive(false);
                    if ((this.GetHistoryCard() != null) || (this.GetHistoryChildCard() != null))
                    {
                        obj2.Hide();
                    }
                    else
                    {
                        obj2.ScaleToZero();
                    }
                }
                else if (textMesh.Text != newText)
                {
                    obj2.Jiggle();
                }
            }
            else if (!shouldHide)
            {
                textMesh.gameObject.SetActive(true);
                obj2.SetToZeroThenEnlarge();
            }
            obj2.Initialize();
            obj2.SetHideNumberFlag(shouldHide);
        }
        textMesh.Text = newText;
    }

    [DebuggerHidden]
    private IEnumerator UpdatePortraitMaterials()
    {
        return new <UpdatePortraitMaterials>c__Iterator2E { <>f__this = this };
    }

    public void UpdatePortraitTexture()
    {
        if (this.m_portraitTextureOverride != null)
        {
            this.SetPortraitTexture(this.m_portraitTextureOverride);
        }
        else if (this.m_cardDef != null)
        {
            this.SetPortraitTexture(this.m_cardDef.GetPortraitTexture());
        }
    }

    public void UpdatePowersText()
    {
        if (this.m_powersTextMesh != null)
        {
            string cardTextInHand;
            if (this.m_entityDef != null)
            {
                cardTextInHand = this.m_entityDef.GetCardTextInHand();
            }
            else
            {
                if ((this.m_entity.IsSecret() && this.m_entity.IsHidden()) && this.m_entity.IsControlledByConcealedPlayer())
                {
                    cardTextInHand = GameStrings.Get("GAMEPLAY_SECRET_DESC");
                }
                else if (this.m_entity.IsHistoryDupe())
                {
                    cardTextInHand = this.m_entity.GetCardTextInHistory();
                }
                else
                {
                    cardTextInHand = this.m_entity.GetCardTextInHand();
                }
                if ((GameState.Get() != null) && (GameState.Get().GetGameEntity() != null))
                {
                    cardTextInHand = GameState.Get().GetGameEntity().UpdateCardText(this.m_card, this, cardTextInHand);
                }
            }
            this.UpdateText(this.m_powersTextMesh, cardTextInHand);
        }
    }

    private void UpdatePremiumComponents()
    {
        if ((CardFlair.GetPremiumType(this.m_cardFlair) != TAG_PREMIUM.NORMAL) && (this.m_glints != null))
        {
            this.m_glints.SetActive(true);
            foreach (Renderer renderer in this.m_glints.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }
        }
    }

    private void UpdateRace(string raceText)
    {
        if (this.m_racePlateObject != null)
        {
            bool flag = !string.IsNullOrEmpty(raceText);
            foreach (MeshRenderer renderer in this.m_racePlateObject.GetComponents<MeshRenderer>())
            {
                renderer.enabled = flag;
            }
            if (flag)
            {
                if (this.m_descriptionMesh != null)
                {
                    this.m_descriptionMesh.GetComponent<Renderer>().material.SetTextureOffset("_SecondTex", new Vector2(-0.04f, 0f));
                }
            }
            else if (this.m_descriptionMesh != null)
            {
                this.m_descriptionMesh.GetComponent<Renderer>().material.SetTextureOffset("_SecondTex", new Vector2(-0.04f, 0.07f));
            }
            if (this.m_raceTextMesh != null)
            {
                this.m_raceTextMesh.Text = raceText;
            }
        }
    }

    private void UpdateRarityComponent()
    {
        if (this.m_rarityGemMesh != null)
        {
            Vector2 vector;
            Color color;
            bool rarityTextureOffset = this.GetRarityTextureOffset(out vector, out color);
            SceneUtils.EnableRenderers(this.m_rarityGemMesh, rarityTextureOffset, true);
            if (this.m_rarityFrameMesh != null)
            {
                SceneUtils.EnableRenderers(this.m_rarityFrameMesh, rarityTextureOffset, true);
            }
            if (rarityTextureOffset)
            {
                this.m_rarityGemMesh.GetComponent<Renderer>().material.mainTextureOffset = vector;
                this.m_rarityGemMesh.GetComponent<Renderer>().material.SetColor("_tint", color);
            }
        }
    }

    private void UpdateRootObjectSpellComponents()
    {
        if (this.m_entity != null)
        {
            if (this.m_armorSpellLoading)
            {
                base.StartCoroutine(this.UpdateArmorSpellWhenLoaded());
            }
            if (this.m_armorSpell != null)
            {
                this.UpdateArmorSpell();
            }
        }
    }

    private void UpdateSecretText()
    {
        if (this.m_secretText != null)
        {
            string text = "?";
            if ((UniversalInputManager.UsePhoneUI != null) && (this.m_entity != null))
            {
                TransformUtil.SetLocalPosZ(this.m_secretText, -0.01f);
                Player controller = this.m_entity.GetController();
                if (controller != null)
                {
                    ZoneSecret secretZone = controller.GetSecretZone();
                    if (secretZone != null)
                    {
                        int cardCount = secretZone.GetCardCount();
                        if (cardCount > 1)
                        {
                            text = string.Format("{0}", cardCount);
                            TransformUtil.SetLocalPosZ(this.m_secretText, -0.03f);
                        }
                    }
                }
                Transform transform = this.m_secretText.transform.parent.FindChild("Secret_mesh");
                if ((transform != null) && (transform.gameObject != null))
                {
                    SphereCollider component = transform.gameObject.GetComponent<SphereCollider>();
                    if (component != null)
                    {
                        component.radius = 0.5f;
                    }
                }
            }
            this.UpdateText(this.m_secretText, text);
        }
    }

    private void UpdateText(UberText uberTextMesh, string text)
    {
        if (uberTextMesh != null)
        {
            uberTextMesh.Text = text;
        }
    }

    private void UpdateTextColor(UberText originalMesh, int baseNumber, int currentNumber)
    {
        this.UpdateTextColor(originalMesh, baseNumber, currentNumber, false);
    }

    private void UpdateTextColor(UberText uberTextMesh, int baseNumber, int currentNumber, bool higherIsBetter)
    {
        if (((baseNumber > currentNumber) && higherIsBetter) || ((baseNumber < currentNumber) && !higherIsBetter))
        {
            uberTextMesh.TextColor = Color.green;
        }
        else if (((baseNumber < currentNumber) && higherIsBetter) || ((baseNumber > currentNumber) && !higherIsBetter))
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                uberTextMesh.TextColor = new Color(1f, 0.1960784f, 0.1960784f);
            }
            else
            {
                uberTextMesh.TextColor = Color.red;
            }
        }
        else if (baseNumber == currentNumber)
        {
            uberTextMesh.TextColor = Color.white;
        }
    }

    private void UpdateTextColorToGreenOrWhite(UberText uberTextMesh, int baseNumber, int currentNumber)
    {
        if (baseNumber < currentNumber)
        {
            uberTextMesh.TextColor = Color.green;
        }
        else
        {
            uberTextMesh.TextColor = Color.white;
        }
    }

    public virtual void UpdateTextComponents()
    {
        if (this.m_entityDef != null)
        {
            this.UpdateTextComponentsDef(this.m_entityDef);
        }
        else
        {
            this.UpdateTextComponents(this.m_entity);
        }
    }

    public virtual void UpdateTextComponents(Entity entity)
    {
        if (entity != null)
        {
            if (this.m_costTextMesh != null)
            {
                if (this.m_entity.HasTag(GAME_TAG.HIDE_COST))
                {
                    this.UpdateNumberText(this.m_costTextMesh, string.Empty, true);
                }
                else
                {
                    if ((this.m_entity.IsSecret() && this.m_entity.IsHidden()) && this.m_entity.IsControlledByConcealedPlayer())
                    {
                        this.m_costTextMesh.TextColor = Color.white;
                    }
                    else
                    {
                        this.UpdateTextColor(this.m_costTextMesh, entity.GetOriginalCost(), entity.GetCost(), true);
                    }
                    if (this.m_entity.HasTriggerVisual() && this.m_entity.IsHeroPower())
                    {
                        this.UpdateNumberText(this.m_costTextMesh, string.Empty, true);
                    }
                    else
                    {
                        this.UpdateNumberText(this.m_costTextMesh, Convert.ToString(entity.GetCost()));
                    }
                }
            }
            if (this.m_attackTextMesh != null)
            {
                if (entity.IsHero())
                {
                    int aTK = entity.GetATK();
                    if (aTK == 0)
                    {
                        this.UpdateNumberText(this.m_attackTextMesh, string.Empty, true);
                    }
                    else
                    {
                        this.UpdateNumberText(this.m_attackTextMesh, Convert.ToString(aTK));
                    }
                }
                else
                {
                    this.UpdateTextColorToGreenOrWhite(this.m_attackTextMesh, entity.GetOriginalATK(), entity.GetATK());
                    this.UpdateNumberText(this.m_attackTextMesh, Convert.ToString(entity.GetATK()));
                }
            }
            if ((this.m_healthTextMesh != null) && (!entity.IsHero() || (entity.GetZone() != TAG_ZONE.GRAVEYARD)))
            {
                int durability;
                int originalDurability;
                if (entity.IsWeapon())
                {
                    durability = entity.GetDurability();
                    originalDurability = entity.GetOriginalDurability();
                }
                else
                {
                    durability = entity.GetHealth();
                    originalDurability = entity.GetOriginalHealth();
                }
                int currentNumber = durability - entity.GetDamage();
                if (entity.GetDamage() > 0)
                {
                    this.UpdateTextColor(this.m_healthTextMesh, durability, currentNumber);
                }
                else if (durability > originalDurability)
                {
                    this.UpdateTextColor(this.m_healthTextMesh, originalDurability, currentNumber);
                }
                else
                {
                    this.UpdateTextColor(this.m_healthTextMesh, currentNumber, currentNumber);
                }
                this.UpdateNumberText(this.m_healthTextMesh, Convert.ToString(currentNumber));
            }
            this.UpdateNameText();
            this.UpdatePowersText();
            this.UpdateRace(entity.GetRaceText());
            this.UpdateSecretText();
        }
    }

    public virtual void UpdateTextComponentsDef(EntityDef entityDef)
    {
        if (entityDef != null)
        {
            if (this.m_costTextMesh != null)
            {
                if (entityDef.HasTag(GAME_TAG.HIDE_COST))
                {
                    this.m_costTextMesh.Text = string.Empty;
                }
                else if (entityDef.HasTriggerVisual() && entityDef.IsHeroPower())
                {
                    this.m_costTextMesh.Text = string.Empty;
                }
                else
                {
                    this.m_costTextMesh.Text = Convert.ToString(entityDef.GetTag(GAME_TAG.COST));
                }
            }
            int tag = entityDef.GetTag(GAME_TAG.ATK);
            if (entityDef.IsHero())
            {
                if (tag == 0)
                {
                    if ((this.m_attackObject != null) && this.m_attackObject.activeSelf)
                    {
                        this.m_attackObject.SetActive(false);
                    }
                    if (this.m_attackTextMesh != null)
                    {
                        this.m_attackTextMesh.Text = string.Empty;
                    }
                }
                else
                {
                    if ((this.m_attackObject != null) && !this.m_attackObject.activeSelf)
                    {
                        this.m_attackObject.SetActive(true);
                    }
                    if (this.m_attackTextMesh != null)
                    {
                        this.m_attackTextMesh.Text = Convert.ToString(tag);
                    }
                }
            }
            else if (this.m_attackTextMesh != null)
            {
                this.m_attackTextMesh.Text = Convert.ToString(tag);
            }
            if (this.m_healthTextMesh != null)
            {
                if (entityDef.IsWeapon())
                {
                    this.m_healthTextMesh.Text = Convert.ToString(entityDef.GetTag(GAME_TAG.DURABILITY));
                }
                else
                {
                    this.m_healthTextMesh.Text = Convert.ToString(entityDef.GetTag(GAME_TAG.HEALTH));
                }
            }
            this.UpdateNameText();
            this.UpdatePowersText();
            this.UpdateRace(entityDef.GetRaceText());
            this.UpdateSecretText();
        }
    }

    private void UpdateTextures()
    {
        this.UpdatePortraitTexture();
    }

    private void UpdateWatermark()
    {
        if ((this.m_entityDef != null) || (this.m_entity != null))
        {
            TAG_CARD_SET cardSet = this.GetCardSet();
            if ((this.m_descriptionMesh != null) && this.m_descriptionMesh.GetComponent<Renderer>().material.HasProperty("_SecondTint"))
            {
                float num = 0f;
                string textureName = "Set1_Icon";
                TAG_CARD_SET tag_card_set2 = cardSet;
                switch (tag_card_set2)
                {
                    case TAG_CARD_SET.FP1:
                        textureName = "NaxxIcon";
                        num = 0.7734375f;
                        break;

                    case TAG_CARD_SET.PE1:
                        textureName = "GvGIcon";
                        num = 0.7734375f;
                        break;

                    case TAG_CARD_SET.BRM:
                        textureName = "BRMIcon";
                        num = 0.7734375f;
                        break;

                    case TAG_CARD_SET.TGT:
                        textureName = "TGTIcon";
                        num = 0.7734375f;
                        break;

                    case TAG_CARD_SET.LOE:
                        textureName = "LOEIcon";
                        num = 0.7734375f;
                        break;

                    default:
                        if (tag_card_set2 == TAG_CARD_SET.EXPERT1)
                        {
                            num = 0.7734375f;
                        }
                        else
                        {
                            num = 0f;
                        }
                        break;
                }
                AssetLoader.Get().LoadTexture(textureName, new AssetLoader.ObjectCallback(this.OnWatermarkLoaded), null, false);
                Color color = this.m_descriptionMesh.GetComponent<Renderer>().material.GetColor("_SecondTint");
                color.a = num;
                this.m_descriptionMesh.GetComponent<Renderer>().material.SetColor("_SecondTint", color);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ActivateArmorSpell>c__Iterator30 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>armorShouldBeOn;
        internal SpellStateType <$>stateType;
        internal Actor <>f__this;
        internal int <armor>__0;
        internal bool armorShouldBeOn;
        internal SpellStateType stateType;

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
                    if (this.<>f__this.m_armorSpell.GetActiveState() != SpellStateType.NONE)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (this.<>f__this.m_armorSpell.GetActiveState() != this.stateType)
                    {
                        this.<armor>__0 = this.<>f__this.m_entity.GetArmor();
                        if ((!this.armorShouldBeOn || (this.<armor>__0 > 0)) && (this.armorShouldBeOn || (this.<armor>__0 <= 0)))
                        {
                            this.<>f__this.m_armorSpell.ActivateState(this.stateType);
                            this.$PC = -1;
                        }
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
    private sealed class <UpdateArmorSpellWhenLoaded>c__Iterator2F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <>f__this;

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
                    if (this.<>f__this.m_armorSpellLoading)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.UpdateArmorSpell();
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
    private sealed class <UpdatePortraitMaterials>c__Iterator2E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <>f__this;
        internal TAG_PREMIUM <premiumType>__0;

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
                    this.<>f__this.isPortraitMaterialDirty = false;
                    if (!this.<>f__this.m_shadowform && (this.<>f__this.m_cardDef != null))
                    {
                        this.<premiumType>__0 = CardFlair.GetPremiumType(this.<>f__this.m_cardFlair);
                        if (this.<>f__this.m_DisablePremiumPortrait || ((this.<premiumType>__0 != TAG_PREMIUM.GOLDEN) && !this.<>f__this.m_cardDef.m_AlwaysRenderPremiumPortrait))
                        {
                            this.<>f__this.SetPortraitMaterial(this.<>f__this.m_initialPortraitMaterial);
                            goto Label_014E;
                        }
                        if (!this.<>f__this.m_cardDef.IsPremiumLoaded())
                        {
                            this.$current = null;
                            this.$PC = 1;
                            return true;
                        }
                        break;
                    }
                    goto Label_0155;

                case 1:
                    break;

                default:
                    goto Label_0155;
            }
            if (this.<>f__this.m_cardDef.GetPremiumPortraitMaterial() != null)
            {
                this.<>f__this.SetPortraitMaterial(this.<>f__this.m_cardDef.GetPremiumPortraitMaterial());
            }
            else if (this.<>f__this.m_initialPortraitMaterial != null)
            {
                this.<>f__this.SetPortraitMaterial(this.<>f__this.m_initialPortraitMaterial);
            }
        Label_014E:
            this.$PC = -1;
        Label_0155:
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

