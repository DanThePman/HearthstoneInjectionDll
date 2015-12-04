using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class DefLoader
{
    private Map<string, CardDef> m_cachedCardDefs = new Map<string, CardDef>();
    private Map<string, EntityDef> m_entityDefCache = new Map<string, EntityDef>();
    private bool m_loadedEntityDefs;
    private static DefLoader s_instance;

    public void Clear()
    {
        this.ClearEntityDefs();
        this.ClearCardDefs();
    }

    public void ClearCardDef(string cardID)
    {
        if (this.m_cachedCardDefs.ContainsKey(cardID))
        {
            CardDef def = this.m_cachedCardDefs[cardID];
            this.m_cachedCardDefs.Remove(cardID);
            UnityEngine.Object.Destroy(def.gameObject);
        }
    }

    public void ClearCardDefs()
    {
        this.m_cachedCardDefs.Clear();
    }

    public void ClearEntityDefs()
    {
        this.m_entityDefCache.Clear();
        this.m_loadedEntityDefs = false;
    }

    public static DefLoader Get()
    {
        if (s_instance == null)
        {
            s_instance = new DefLoader();
            if (ApplicationMgr.Get() != null)
            {
                ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
            }
        }
        return s_instance;
    }

    public Map<string, EntityDef> GetAllEntityDefs()
    {
        return this.m_entityDefCache;
    }

    public CardDef GetCardDef(string cardId, CardPortraitQuality quality = null)
    {
        CardDef def;
        if (quality == null)
        {
            quality = CardPortraitQuality.GetDefault();
        }
        if (true)
        {
            quality.TextureQuality = 3;
        }
        CardDef component = null;
        if (this.m_cachedCardDefs.TryGetValue(cardId, out def))
        {
            component = def;
            if (CardPortraitQuality.GetFromDef(def) >= quality)
            {
                return def;
            }
        }
        if (component == null)
        {
            GameObject brokenCardPrefab = AssetLoader.Get().LoadCardPrefab(cardId, true, false);
            if (brokenCardPrefab == null)
            {
                brokenCardPrefab = GameUtils.GetBrokenCardPrefab();
            }
            component = brokenCardPrefab.GetComponent<CardDef>();
            if (component == null)
            {
                Debug.LogError(string.Format("LoadCardDef: Could not find card def for {0}", cardId));
                return null;
            }
            this.m_cachedCardDefs.Add(cardId, component);
        }
        this.UpdateCardAssets(component, quality);
        return component;
    }

    public EntityDef GetEntityDef(int dbId)
    {
        string cardId = GameUtils.TranslateDbIdToCardId(dbId);
        if (cardId == null)
        {
            Debug.LogError(string.Format("DefLoader.GetEntityDef() - dbId {0} does not map to a cardId", dbId));
            return null;
        }
        return this.GetEntityDef(cardId);
    }

    public EntityDef GetEntityDef(string cardId)
    {
        EntityDef def = null;
        this.m_entityDefCache.TryGetValue(cardId, out def);
        return def;
    }

    public FullDef GetFullDef(string cardId, CardPortraitQuality quality = null)
    {
        EntityDef entityDef = this.GetEntityDef(cardId);
        CardDef cardDef = this.GetCardDef(cardId, quality);
        FullDef def3 = new FullDef();
        def3.SetEntityDef(entityDef);
        def3.SetCardDef(cardDef);
        return def3;
    }

    private static string GetTextureName(string path, int quality)
    {
        switch (quality)
        {
            case 1:
            case 2:
            {
                int length = path.LastIndexOf('/');
                string str = path.Substring(0, length);
                string str2 = path.Substring(length + 1);
                return string.Format("{0}/LowResPortrait/{1}", str, str2);
            }
            case 3:
                return path;
        }
        Debug.LogError("Invalid texture quality value.");
        return string.Empty;
    }

    public bool HasCardDef(GameObject go)
    {
        CardDef def = SceneUtils.FindComponentInThisOrParents<CardDef>(go);
        if (def == null)
        {
            return false;
        }
        return this.m_cachedCardDefs.ContainsValue(def);
    }

    public bool HasDef(GameObject go)
    {
        return this.HasCardDef(go);
    }

    public bool HasLoadedEntityDefs()
    {
        return this.m_loadedEntityDefs;
    }

    public void Initialize()
    {
        this.LoadAllEntityDefs();
    }

    public void LoadAllEntityDefs()
    {
        int errors = 0;
        List<string> allCardIds = GameUtils.GetAllCardIds();
        this.m_entityDefCache = AssetLoader.Get().LoadBatchCardXmls(allCardIds, out errors);
        this.m_loadedEntityDefs = true;
    }

    public void LoadCardDef(string cardId, LoadDefCallback<CardDef> callback, object userData = null, CardPortraitQuality quality = null)
    {
        CardDef cardDef = this.GetCardDef(cardId, quality);
        callback(cardId, cardDef, userData);
    }

    public void LoadFullDef(string cardId, LoadDefCallback<FullDef> callback)
    {
        this.LoadFullDef(cardId, callback, null);
    }

    public void LoadFullDef(string cardId, LoadDefCallback<FullDef> callback, object userData)
    {
        callback(cardId, this.GetFullDef(cardId, null), userData);
    }

    private void UpdateCardAssets(CardDef cardDef, CardPortraitQuality quality)
    {
        CardPortraitQuality portraitQuality = cardDef.GetPortraitQuality();
        if ((quality > portraitQuality) && !string.IsNullOrEmpty(cardDef.m_PortraitTexturePath))
        {
            if (portraitQuality.TextureQuality < quality.TextureQuality)
            {
                string textureName = GetTextureName(cardDef.m_PortraitTexturePath, quality.TextureQuality);
                Texture portrait = AssetLoader.Get().LoadCardTexture(textureName, false);
                if (portrait == null)
                {
                    object[] messageArgs = new object[] { cardDef.m_PortraitTexturePath, cardDef };
                    Error.AddDevFatal("DefLoader.UpdateCardTextures() - Failed to load {0} for card {1}", messageArgs);
                    return;
                }
                cardDef.OnPortraitLoaded(portrait, quality.TextureQuality);
            }
            if (((quality.LoadPremium && !portraitQuality.LoadPremium) || cardDef.m_AlwaysRenderPremiumPortrait) && !string.IsNullOrEmpty(cardDef.m_PremiumPortraitMaterialPath))
            {
                Material material = AssetLoader.Get().LoadPremiumMaterial(cardDef.m_PremiumPortraitMaterialPath, false);
                Texture texture2 = null;
                if (material == null)
                {
                    object[] objArray2 = new object[] { cardDef.m_PremiumPortraitMaterialPath, cardDef };
                    Error.AddDevFatal("DefLoader.UpdateCardTextures() - Failed to load {0} for card {1}", objArray2);
                }
                else
                {
                    if (!string.IsNullOrEmpty(cardDef.m_PremiumPortraitTexturePath))
                    {
                        texture2 = AssetLoader.Get().LoadCardTexture(cardDef.m_PremiumPortraitTexturePath, false);
                        if (texture2 == null)
                        {
                            object[] objArray3 = new object[] { cardDef.m_PremiumPortraitTexturePath, cardDef };
                            Error.AddDevFatal("DefLoader.UpdateCardTextures() - Failed to load {0} for card {1}", objArray3);
                            return;
                        }
                    }
                    cardDef.OnPremiumMaterialLoaded(material, texture2);
                }
            }
        }
    }

    private void WillReset()
    {
        this.ClearEntityDefs();
    }

    public delegate void LoadDefCallback<T>(string cardId, T def, object userData);
}

