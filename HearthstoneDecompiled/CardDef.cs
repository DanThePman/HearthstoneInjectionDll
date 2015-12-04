using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class CardDef : MonoBehaviour
{
    protected const int LARGE_MINION_COST = 7;
    [CustomEditField(Sections="Portrait")]
    public bool m_AlwaysRenderPremiumPortrait;
    [CustomEditField(Sections="Hero", T=EditType.SOUND_PREFAB)]
    public string m_AnnouncerLinePath;
    [CustomEditField(Sections="Attack")]
    public CardEffectDef m_AttackEffectDef;
    [CustomEditField(Sections="Custom", T=EditType.SPELL)]
    public string m_CustomDeathSpellPath;
    [CustomEditField(Sections="Portrait")]
    public Material_MobileOverride m_CustomDeckPortrait;
    [CustomEditField(Sections="Hero", T=EditType.SPELL)]
    public string m_CustomHeroArmorSpell;
    [CustomEditField(Sections="Hero", T=EditType.TEXTURE)]
    public string m_CustomHeroPhoneManaGem;
    [CustomEditField(Sections="Hero", T=EditType.TEXTURE)]
    public string m_CustomHeroPhoneTray;
    [CustomEditField(Sections="Hero", T=EditType.TEXTURE)]
    public string m_CustomHeroTray;
    [CustomEditField(Sections="Hero")]
    public List<Board.CustomTraySettings> m_CustomHeroTraySettings;
    [CustomEditField(Sections="Custom", T=EditType.SPELL)]
    public string m_CustomKeywordSpellPath;
    [CustomEditField(Sections="Custom", T=EditType.SPELL)]
    public string m_CustomSpawnSpellPath;
    [CustomEditField(Sections="Custom", T=EditType.SPELL)]
    public string m_CustomSummonSpellPath;
    [CustomEditField(Sections="Death")]
    public CardEffectDef m_DeathEffectDef;
    [CustomEditField(Sections="Portrait")]
    public Material m_DeckBoxPortrait;
    [CustomEditField(Sections="Portrait")]
    public Material m_DeckCardBarPortrait;
    [CustomEditField(Sections="Portrait")]
    public Material_MobileOverride m_DeckPickerPortrait;
    [CustomEditField(Sections="Hero")]
    public List<EmoteEntryDef> m_EmoteDefs;
    [CustomEditField(Sections="Portrait")]
    public Material m_EnchantmentPortrait;
    [CustomEditField(Sections="Custom", T=EditType.SPELL)]
    public string m_GoldenCustomDeathSpellPath;
    [CustomEditField(Sections="Custom", T=EditType.SPELL)]
    public string m_GoldenCustomSpawnSpellPath;
    [CustomEditField(Sections="Custom", T=EditType.SPELL)]
    public string m_GoldenCustomSummonSpellPath;
    [CustomEditField(Sections="Portrait")]
    public Material m_HistoryTileFullPortrait;
    [CustomEditField(Sections="Portrait")]
    public Material m_HistoryTileHalfPortrait;
    [CustomEditField(Sections="Lifetime")]
    public CardEffectDef m_LifetimeEffectDef;
    private Material m_LoadedCustomDeckPortrait;
    private Material m_LoadedDeckBoxPortrait;
    private Material m_LoadedDeckCardBarPortrait;
    private Material m_LoadedDeckPickerPortrait;
    private Material m_LoadedEnchantmentPortrait;
    private Material m_LoadedHistoryTileFullPortrait;
    private Material m_LoadedHistoryTileHalfPortrait;
    private Texture m_LoadedPortraitTexture;
    private Material m_LoadedPracticeAIPortrait;
    private Material m_LoadedPremiumPortraitMaterial;
    private Texture m_LoadedPremiumPortraitTexture;
    [CustomEditField(Sections="Play")]
    public CardEffectDef m_PlayEffectDef;
    private CardPortraitQuality m_portraitQuality = CardPortraitQuality.GetUnloaded();
    [CustomEditField(Sections="Portrait", T=EditType.CARD_TEXTURE)]
    public string m_PortraitTexturePath;
    [CustomEditField(Sections="Portrait")]
    public Material m_PracticeAIPortrait;
    [CustomEditField(Sections="Portrait", T=EditType.MATERIAL)]
    public string m_PremiumPortraitMaterialPath;
    [CustomEditField(Sections="Portrait", T=EditType.CARD_TEXTURE)]
    public string m_PremiumPortraitTexturePath;
    [CustomEditField(Sections="Hero", T=EditType.SPELL)]
    public string m_SocketInEffectFriendly;
    [CustomEditField(Sections="Hero", T=EditType.SPELL)]
    public string m_SocketInEffectFriendlyPhone;
    [CustomEditField(Sections="Hero", T=EditType.SPELL)]
    public string m_SocketInEffectOpponent;
    [CustomEditField(Sections="Hero", T=EditType.SPELL)]
    public string m_SocketInEffectOpponentPhone;
    [CustomEditField(Sections="Hero")]
    public bool m_SocketInOverrideHeroAnimation;
    [CustomEditField(Sections="Hero")]
    public bool m_SocketInParentEffectToHero = true;
    [CustomEditField(Sections="SubOption")]
    public List<CardEffectDef> m_SubOptionEffectDefs;
    [CustomEditField(Sections="Misc")]
    public bool m_SuppressDeathrattleDeath;
    [CustomEditField(Sections="Trigger")]
    public List<CardEffectDef> m_TriggerEffectDefs;
    protected const int MEDIUM_MINION_COST = 4;

    public void Awake()
    {
        if (string.IsNullOrEmpty(this.m_PortraitTexturePath))
        {
            this.m_portraitQuality.TextureQuality = 3;
            this.m_portraitQuality.LoadPremium = true;
        }
        else if (string.IsNullOrEmpty(this.m_PremiumPortraitMaterialPath))
        {
            this.m_portraitQuality.LoadPremium = true;
        }
    }

    public virtual string DetermineActorNameForZone(Entity entity, TAG_ZONE zoneTag)
    {
        return ActorNames.GetZoneActor(entity, zoneTag);
    }

    public virtual SpellType DetermineSummonInSpell_HandToPlay(Card card)
    {
        Entity entity = card.GetEntity();
        int cost = entity.GetEntityDef().GetCost();
        TAG_PREMIUM premiumType = entity.GetPremiumType();
        bool flag = entity.GetController().IsFriendlySide();
        if (cost < 7)
        {
            if (cost < 4)
            {
                switch (premiumType)
                {
                    case TAG_PREMIUM.NORMAL:
                        goto Label_00F1;

                    case TAG_PREMIUM.GOLDEN:
                        if (flag)
                        {
                            return SpellType.SUMMON_IN_PREMIUM;
                        }
                        return SpellType.SUMMON_IN_OPPONENT_PREMIUM;
                }
                Debug.LogWarning(string.Format("CardDef.DetermineSummonInSpell_HandToPlay() - unexpected premium type {0}", premiumType));
                goto Label_00F1;
            }
            switch (premiumType)
            {
                case TAG_PREMIUM.NORMAL:
                    goto Label_00A6;

                case TAG_PREMIUM.GOLDEN:
                    if (flag)
                    {
                        return SpellType.SUMMON_IN_MEDIUM_PREMIUM;
                    }
                    return SpellType.SUMMON_IN_OPPONENT_MEDIUM_PREMIUM;
            }
            Debug.LogWarning(string.Format("CardDef.DetermineSummonInSpell_HandToPlay() - unexpected premium type {0}", premiumType));
        }
        else
        {
            switch (premiumType)
            {
                case TAG_PREMIUM.NORMAL:
                    break;

                case TAG_PREMIUM.GOLDEN:
                    if (flag)
                    {
                        return SpellType.SUMMON_IN_LARGE_PREMIUM;
                    }
                    return SpellType.SUMMON_IN_OPPONENT_LARGE_PREMIUM;

                default:
                    Debug.LogWarning(string.Format("CardDef.DetermineSummonInSpell_HandToPlay() - unexpected premium type {0}", premiumType));
                    break;
            }
            if (flag)
            {
                return SpellType.SUMMON_IN_LARGE;
            }
            return SpellType.SUMMON_IN_OPPONENT_LARGE;
        }
    Label_00A6:
        if (flag)
        {
            return SpellType.SUMMON_IN_MEDIUM;
        }
        return SpellType.SUMMON_IN_OPPONENT_MEDIUM;
    Label_00F1:
        if (flag)
        {
            return SpellType.SUMMON_IN;
        }
        return SpellType.SUMMON_IN_OPPONENT;
    }

    public virtual SpellType DetermineSummonOutSpell_HandToPlay(Card card)
    {
        Entity entity = card.GetEntity();
        if (!entity.GetController().IsFriendlySide())
        {
            return SpellType.SUMMON_OUT;
        }
        int cost = entity.GetEntityDef().GetCost();
        TAG_PREMIUM premiumType = entity.GetPremiumType();
        if (cost < 7)
        {
            if (cost < 4)
            {
                switch (premiumType)
                {
                    case TAG_PREMIUM.NORMAL:
                        goto Label_00C4;

                    case TAG_PREMIUM.GOLDEN:
                        return SpellType.SUMMON_OUT_PREMIUM;
                }
                Debug.LogWarning(string.Format("CardDef.DetermineSummonOutSpell_HandToPlay(): unexpected premium type {0}", premiumType));
                goto Label_00C4;
            }
            switch (premiumType)
            {
                case TAG_PREMIUM.NORMAL:
                    goto Label_008D;

                case TAG_PREMIUM.GOLDEN:
                    return SpellType.SUMMON_OUT_PREMIUM;
            }
            Debug.LogWarning(string.Format("CardDef.DetermineSummonOutSpell_HandToPlay(): unexpected premium type {0}", premiumType));
        }
        else
        {
            switch (premiumType)
            {
                case TAG_PREMIUM.NORMAL:
                    break;

                case TAG_PREMIUM.GOLDEN:
                    return SpellType.SUMMON_OUT_PREMIUM;

                default:
                    Debug.LogWarning(string.Format("CardDef.DetermineSummonOutSpell_HandToPlay(): unexpected premium type {0}", premiumType));
                    break;
            }
            return SpellType.SUMMON_OUT_LARGE;
        }
    Label_008D:
        return SpellType.SUMMON_OUT_MEDIUM;
    Label_00C4:
        return SpellType.SUMMON_OUT;
    }

    public Material GetCustomDeckPortrait()
    {
        return this.m_LoadedCustomDeckPortrait;
    }

    public Material GetDeckBoxPortrait()
    {
        return this.m_LoadedDeckBoxPortrait;
    }

    public Material GetDeckCardBarPortrait()
    {
        return this.m_LoadedDeckCardBarPortrait;
    }

    public Material GetDeckPickerPortrait()
    {
        return this.m_LoadedDeckPickerPortrait;
    }

    public Material GetEnchantmentPortrait()
    {
        return this.m_LoadedEnchantmentPortrait;
    }

    public Material GetHistoryTileFullPortrait()
    {
        return this.m_LoadedHistoryTileFullPortrait;
    }

    public Material GetHistoryTileHalfPortrait()
    {
        return this.m_LoadedHistoryTileHalfPortrait;
    }

    public CardPortraitQuality GetPortraitQuality()
    {
        return this.m_portraitQuality;
    }

    public Texture GetPortraitTexture()
    {
        return this.m_LoadedPortraitTexture;
    }

    public Material GetPracticeAIPortrait()
    {
        return this.m_LoadedPracticeAIPortrait;
    }

    public Material GetPremiumPortraitMaterial()
    {
        return this.m_LoadedPremiumPortraitMaterial;
    }

    public bool IsPremiumLoaded()
    {
        return this.m_portraitQuality.LoadPremium;
    }

    public void OnPortraitLoaded(Texture portrait, int quality)
    {
        if (quality <= this.m_portraitQuality.TextureQuality)
        {
            Debug.LogWarning(string.Format("Loaded texture of quality lower or equal to what was was already available ({0} <= {1}), texture={2}", quality, this.m_portraitQuality, portrait));
        }
        else
        {
            this.m_portraitQuality.TextureQuality = quality;
            this.m_LoadedPortraitTexture = portrait;
            if ((this.m_LoadedPremiumPortraitMaterial != null) && string.IsNullOrEmpty(this.m_PremiumPortraitTexturePath))
            {
                this.m_LoadedPremiumPortraitMaterial.mainTexture = portrait;
                this.m_portraitQuality.LoadPremium = true;
            }
            SetTextureIfNotNull(this.m_DeckCardBarPortrait, ref this.m_LoadedDeckCardBarPortrait, portrait);
            SetTextureIfNotNull(this.m_EnchantmentPortrait, ref this.m_LoadedEnchantmentPortrait, portrait);
            SetTextureIfNotNull(this.m_HistoryTileFullPortrait, ref this.m_LoadedHistoryTileFullPortrait, portrait);
            SetTextureIfNotNull(this.m_HistoryTileHalfPortrait, ref this.m_LoadedHistoryTileHalfPortrait, portrait);
            SetTextureIfNotNull((Material) this.m_CustomDeckPortrait, ref this.m_LoadedCustomDeckPortrait, portrait);
            SetTextureIfNotNull((Material) this.m_DeckPickerPortrait, ref this.m_LoadedDeckPickerPortrait, portrait);
            SetTextureIfNotNull(this.m_PracticeAIPortrait, ref this.m_LoadedPracticeAIPortrait, portrait);
            SetTextureIfNotNull(this.m_DeckBoxPortrait, ref this.m_LoadedDeckBoxPortrait, portrait);
        }
    }

    public void OnPremiumMaterialLoaded(Material material, Texture portrait)
    {
        if (this.m_LoadedPremiumPortraitMaterial != null)
        {
            Debug.LogWarning(string.Format("Loaded premium material twice: {0}", material));
        }
        else
        {
            if (material != null)
            {
                this.m_LoadedPremiumPortraitMaterial = UnityEngine.Object.Instantiate<Material>(material);
            }
            this.m_LoadedPremiumPortraitTexture = portrait;
            if (string.IsNullOrEmpty(this.m_PremiumPortraitTexturePath))
            {
                if (this.m_LoadedPortraitTexture != null)
                {
                    if (this.m_LoadedPremiumPortraitMaterial != null)
                    {
                        this.m_LoadedPremiumPortraitMaterial.mainTexture = this.m_LoadedPortraitTexture;
                    }
                    this.m_portraitQuality.LoadPremium = true;
                }
            }
            else if (this.m_LoadedPremiumPortraitTexture != null)
            {
                if (this.m_LoadedPremiumPortraitMaterial != null)
                {
                    this.m_LoadedPremiumPortraitMaterial.mainTexture = this.m_LoadedPremiumPortraitTexture;
                }
                this.m_portraitQuality.LoadPremium = true;
            }
        }
    }

    private static void SetTextureIfNotNull(Material baseMat, ref Material targetMat, Texture tex)
    {
        if (baseMat != null)
        {
            if (targetMat == null)
            {
                targetMat = UnityEngine.Object.Instantiate<Material>(baseMat);
            }
            targetMat.mainTexture = tex;
        }
    }
}

