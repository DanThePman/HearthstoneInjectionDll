using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class CardBackManager : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<CardBackData> <>f__am$cacheB;
    [CompilerGenerated]
    private static Comparison<OwnedCardBack> <>f__am$cacheC;
    private Map<int, CardBackData> m_cardBackData;
    private bool m_DefaultCardBackChangeListenerRegistered;
    private CardBack m_FriendlyCardBack;
    private string m_FriendlyCardBackName = string.Empty;
    private bool m_isFriendlyLoading;
    private bool m_isOpponentLoading;
    private Map<string, CardBack> m_LoadedCardBacks;
    private CardBack m_OpponentCardBack;
    private string m_OpponentCardBackName = string.Empty;
    private bool m_ResetDefaultRegistered;
    private static CardBackManager s_instance;

    public void AddNewCardBack(int cardBackID)
    {
        NetCache.NetCacheCardBacks netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>();
        if (netObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CardBackManager.AddNewCardBack({0}): trying to access NetCacheCardBacks before it's been loaded", cardBackID));
        }
        else
        {
            netObject.CardBacks.Add(cardBackID);
        }
    }

    private void Awake()
    {
        s_instance = this;
        this.InitCardBackData();
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
    }

    public static CardBackManager Get()
    {
        return s_instance;
    }

    public CardBack GetCardBack(Actor actor)
    {
        if (this.IsActorFriendly(actor))
        {
            return this.m_FriendlyCardBack;
        }
        return this.m_OpponentCardBack;
    }

    public HashSet<int> GetCardBackIds(bool all = true)
    {
        HashSet<int> set = new HashSet<int>();
        if (!all)
        {
            return this.GetCardBacksOwned();
        }
        foreach (KeyValuePair<int, CardBackData> pair in this.m_cardBackData)
        {
            if (pair.Value.Enabled)
            {
                set.Add(pair.Key);
            }
        }
        return set;
    }

    public string GetCardBackName(int cardBackId)
    {
        CardBackData data;
        if (this.m_cardBackData.TryGetValue(cardBackId, out data))
        {
            return data.Name;
        }
        return null;
    }

    public HashSet<int> GetCardBacksOwned()
    {
        NetCache.NetCacheCardBacks netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>();
        if (netObject == null)
        {
            UnityEngine.Debug.LogWarning("CardBackManager.GetCardBacksOwned(): trying to access NetCacheCardBacks before it's been loaded");
            return null;
        }
        return netObject.CardBacks;
    }

    public int GetDeckCardBackID(long deck)
    {
        <GetDeckCardBackID>c__AnonStorey2BE storeybe = new <GetDeckCardBackID>c__AnonStorey2BE {
            deck = deck
        };
        NetCache.DeckHeader header = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>().Decks.Find(new Predicate<NetCache.DeckHeader>(storeybe.<>m__77));
        if (header == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CardBackManager.GetDeckCardBackID() could not find deck with ID {0}", storeybe.deck));
            return 0;
        }
        return header.CardBack;
    }

    public int GetDefaultCardBackID()
    {
        NetCache.NetCacheCardBacks netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>();
        if (netObject == null)
        {
            UnityEngine.Debug.LogWarning("CardBackManager.GetDefaultCardBackID(): trying to access NetCacheCardBacks before it's been loaded");
            return 0;
        }
        return netObject.DefaultCardBack;
    }

    public List<CardBackData> GetEnabledCardBacks()
    {
        if (<>f__am$cacheB == null)
        {
            <>f__am$cacheB = obj => obj.Enabled;
        }
        return this.m_cardBackData.Values.ToList<CardBackData>().FindAll(<>f__am$cacheB);
    }

    public CardBack GetFriendlyCardBack()
    {
        return this.m_FriendlyCardBack;
    }

    public int GetNumCardBacksOwned()
    {
        NetCache.NetCacheCardBacks netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>();
        if (netObject == null)
        {
            UnityEngine.Debug.LogWarning("CardBackManager.GetNumCardBacksOwned(): trying to access NetCacheCardBacks before it's been loaded");
            return 0;
        }
        return netObject.CardBacks.Count;
    }

    public int GetNumEnabledCardBacks()
    {
        return this.GetEnabledCardBacks().Count;
    }

    public CardBack GetOpponentCardBack()
    {
        return this.m_OpponentCardBack;
    }

    public List<OwnedCardBack> GetOrderedEnabledCardBacks(bool checkOwned)
    {
        List<CardBackData> enabledCardBacks = this.GetEnabledCardBacks();
        List<OwnedCardBack> list2 = new List<OwnedCardBack>();
        foreach (CardBackData data in enabledCardBacks)
        {
            bool flag = this.IsCardBackOwned(data.ID);
            if (!checkOwned || flag)
            {
                DbfRecord record = GameDbf.CardBack.GetRecord(data.ID);
                long @long = -1L;
                if (record.GetString("SOURCE") == "season")
                {
                    @long = record.GetLong("DATA1");
                }
                OwnedCardBack item = new OwnedCardBack {
                    m_cardBackId = data.ID,
                    m_owned = flag,
                    m_sortOrder = record.GetInt("SORT_ORDER"),
                    m_sortCategory = record.GetInt("SORT_CATEGORY"),
                    m_seasonId = @long
                };
                list2.Add(item);
            }
        }
        if (<>f__am$cacheC == null)
        {
            <>f__am$cacheC = delegate (OwnedCardBack lhs, OwnedCardBack rhs) {
                if (lhs.m_owned != rhs.m_owned)
                {
                    return !lhs.m_owned ? 1 : -1;
                }
                if (lhs.m_sortCategory != rhs.m_sortCategory)
                {
                    return (lhs.m_sortCategory >= rhs.m_sortCategory) ? 1 : -1;
                }
                if (lhs.m_sortOrder != rhs.m_sortOrder)
                {
                    return (lhs.m_sortOrder >= rhs.m_sortOrder) ? 1 : -1;
                }
                if (lhs.m_seasonId != rhs.m_seasonId)
                {
                    return (lhs.m_seasonId <= rhs.m_seasonId) ? 1 : -1;
                }
                return Mathf.Clamp(lhs.m_cardBackId - rhs.m_cardBackId, -1, 1);
            };
        }
        list2.Sort(<>f__am$cacheC);
        return list2;
    }

    private int GetValidCardBackID(int cardBackID)
    {
        if (!this.m_cardBackData.ContainsKey(cardBackID))
        {
            object[] args = new object[] { cardBackID };
            Log.CardbackMgr.Print("No cardback for {0}, use 0 instead", args);
            return 0;
        }
        return cardBackID;
    }

    [DebuggerHidden]
    private IEnumerator HiddenActorCardBackLoadedSetup(LoadCardBackData data)
    {
        return new <HiddenActorCardBackLoadedSetup>c__Iterator25 { data = data, <$>data = data, <>f__this = this };
    }

    public void InitCardBackData()
    {
        List<CardBackData> list = new List<CardBackData>();
        foreach (DbfRecord record in GameDbf.CardBack.GetRecords())
        {
            list.Add(new CardBackData(record.GetId(), EnumUtils.GetEnum<CardBackData.CardBackSource>(record.GetString("SOURCE")), record.GetLong("DATA1"), record.GetLocString("NAME"), record.GetBool("ENABLED"), record.GetAssetName("PREFAB_NAME")));
        }
        this.m_cardBackData = new Map<int, CardBackData>();
        foreach (CardBackData data in list)
        {
            this.m_cardBackData[data.ID] = data;
        }
        this.m_LoadedCardBacks = new Map<string, CardBack>();
    }

    private void InitCardBacks()
    {
    }

    public bool IsActorFriendly(Actor actor)
    {
        if (actor == null)
        {
            Log.Kyle.Print("CardBack IsActorFriendly: actor is null!", new object[0]);
            return true;
        }
        Entity entity = actor.GetEntity();
        if (entity != null)
        {
            Player controller = entity.GetController();
            if ((controller != null) && (controller.GetSide() == Player.Side.OPPOSING))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsCardBackEnabled(int cardBackID)
    {
        CardBackData data;
        if (!this.m_cardBackData.TryGetValue(cardBackID, out data))
        {
            return false;
        }
        return data.Enabled;
    }

    public bool IsCardBackOwned(int cardBackID)
    {
        NetCache.NetCacheCardBacks netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>();
        if (netObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CardBackManager.IsCardBackOwned({0}): trying to access NetCacheCardBacks before it's been loaded", cardBackID));
            return false;
        }
        return netObject.CardBacks.Contains(cardBackID);
    }

    private void LoadCardBack(string cardBackPath, bool friendlySide)
    {
        if (this.m_LoadedCardBacks.ContainsKey(cardBackPath))
        {
            if (this.m_LoadedCardBacks[cardBackPath] != null)
            {
                if (friendlySide)
                {
                    this.m_FriendlyCardBack = this.m_LoadedCardBacks[cardBackPath];
                }
                else
                {
                    this.m_OpponentCardBack = this.m_LoadedCardBacks[cardBackPath];
                }
                return;
            }
            this.m_LoadedCardBacks.Remove(cardBackPath);
        }
        if (friendlySide)
        {
            if (this.m_FriendlyCardBackName == cardBackPath)
            {
                return;
            }
            this.m_isFriendlyLoading = true;
            this.m_FriendlyCardBackName = cardBackPath;
        }
        else
        {
            if (this.m_OpponentCardBackName == cardBackPath)
            {
                return;
            }
            this.m_isOpponentLoading = true;
            this.m_OpponentCardBackName = cardBackPath;
        }
        LoadCardBackData callbackData = new LoadCardBackData {
            m_FriendlySide = friendlySide,
            m_Path = cardBackPath
        };
        AssetLoader.Get().LoadCardBack(cardBackPath, new AssetLoader.GameObjectCallback(this.OnCardBackLoaded), callbackData, false);
    }

    public bool LoadCardBackByIndex(int cardBackIdx, LoadCardBackData.LoadCardBackCallback callback, string actorName = "Card_Hidden")
    {
        return this.LoadCardBackByIndex(cardBackIdx, callback, false, actorName);
    }

    public LoadCardBackData LoadCardBackByIndex(int cardBackIdx, bool unlit = false, string actorName = "Card_Hidden")
    {
        if (!this.m_cardBackData.ContainsKey(cardBackIdx))
        {
            object[] args = new object[] { cardBackIdx };
            Log.CardbackMgr.Print("CardBackManager.LoadCardBackByIndex() - wrong cardBackIdx {0}", args);
            return null;
        }
        LoadCardBackData data = new LoadCardBackData {
            m_CardBackIndex = cardBackIdx,
            m_Unlit = unlit,
            m_Name = this.m_cardBackData[cardBackIdx].Name,
            m_GameObject = AssetLoader.Get().LoadActor(actorName, false, false)
        };
        if (data.m_GameObject == null)
        {
            object[] objArray2 = new object[] { actorName };
            Log.CardbackMgr.Print("CardBackManager.LoadCardBackByIndex() - failed to load Actor {0}", objArray2);
            return null;
        }
        string prefabName = this.m_cardBackData[cardBackIdx].PrefabName;
        GameObject obj2 = AssetLoader.Get().LoadCardBack(prefabName, true, false);
        if (obj2 == null)
        {
            object[] objArray3 = new object[] { prefabName };
            Log.CardbackMgr.Print("CardBackManager.LoadCardBackByIndex() - failed to load CardBack {0}", objArray3);
            return null;
        }
        CardBack componentInChildren = obj2.GetComponentInChildren<CardBack>();
        if (componentInChildren == null)
        {
            UnityEngine.Debug.LogWarning("CardBackManager.LoadCardBackByIndex() - cardback=null");
            return null;
        }
        data.m_CardBack = componentInChildren;
        Actor component = data.m_GameObject.GetComponent<Actor>();
        this.SetCardBack(component.m_cardMesh, data.m_CardBack, data.m_Unlit);
        component.SetCardbackUpdateIgnore(true);
        data.m_CardBack.gameObject.transform.parent = data.m_GameObject.transform;
        return data;
    }

    public bool LoadCardBackByIndex(int cardBackIdx, LoadCardBackData.LoadCardBackCallback callback, bool unlit, string actorName = "Card_Hidden")
    {
        if (!this.m_cardBackData.ContainsKey(cardBackIdx))
        {
            object[] args = new object[] { cardBackIdx };
            Log.CardbackMgr.Print("CardBackManager.LoadCardBackByIndex() - wrong cardBackIdx {0}", args);
            return false;
        }
        LoadCardBackData callbackData = new LoadCardBackData {
            m_CardBackIndex = cardBackIdx,
            m_Callback = callback,
            m_Unlit = unlit,
            m_Name = this.m_cardBackData[cardBackIdx].Name
        };
        AssetLoader.Get().LoadActor(actorName, new AssetLoader.GameObjectCallback(this.OnHiddenActorLoaded), callbackData, false);
        return true;
    }

    private void OnCardBackLoaded(string name, GameObject go, object callbackData)
    {
        LoadCardBackData data = callbackData as LoadCardBackData;
        go.transform.parent = base.transform;
        go.transform.position = new Vector3(1000f, -1000f, -1000f);
        CardBack component = go.GetComponent<CardBack>();
        if (component == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CardBackManager OnCardBackLoaded(): Failed to find CardBack component: {0}", data.m_Path));
        }
        else if (component.m_CardBackMesh == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CardBackManager OnCardBackLoaded(): cardBack.m_CardBackMesh in null! - {0}", data.m_Path));
        }
        else if (component.m_CardBackMaterial == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CardBackManager OnCardBackLoaded(): cardBack.m_CardBackMaterial in null! - {0}", data.m_Path));
        }
        else if (component.m_CardBackTexture == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CardBackManager OnCardBackLoaded(): cardBack.m_CardBackTexture in null! - {0}", data.m_Path));
        }
        else
        {
            this.m_LoadedCardBacks[data.m_Path] = component;
            if (data.m_FriendlySide)
            {
                this.m_isFriendlyLoading = false;
                if (component == null)
                {
                    UnityEngine.Debug.LogError(string.Format("CardBackManager OnCardBackLoaded(): Failed to find CardBack component for: {0}", name));
                }
                this.m_FriendlyCardBack = component;
            }
            else
            {
                this.m_isOpponentLoading = false;
                if (component == null)
                {
                    UnityEngine.Debug.LogError(string.Format("CardBackManager OnCardBackLoaded(): Failed to find CardBack component for: {0}", name));
                }
                this.m_OpponentCardBack = component;
            }
        }
    }

    private void OnCheatOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        Log.Kyle.Print("Cheat Option Change Called", new object[0]);
        int @int = Options.Get().GetInt(option, 0);
        if (this.m_cardBackData.ContainsKey(@int))
        {
            bool friendlySide = true;
            if (option == Option.CARD_BACK2)
            {
                friendlySide = false;
            }
            this.LoadCardBack(this.m_cardBackData[@int].PrefabName, friendlySide);
            this.UpdateAllCardBacks();
        }
    }

    private void OnDefaultCardBackChanged(int defaultCardBackID, object userData)
    {
        int validCardBackID = this.GetValidCardBackID(defaultCardBackID);
        bool flag = false;
        if ((GameMgr.Get() != null) && GameMgr.Get().IsSpectator())
        {
            flag = true;
        }
        if (!flag)
        {
            this.LoadCardBack(this.m_cardBackData[validCardBackID].PrefabName, true);
            this.UpdateAllCardBacks();
        }
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        s_instance = null;
    }

    private void OnHiddenActorCardBackLoaded(string name, GameObject go, object userData)
    {
        CardBack componentInChildren = go.GetComponentInChildren<CardBack>();
        if (componentInChildren == null)
        {
            UnityEngine.Debug.LogWarning("CardBackManager OnHiddenActorCardBackLoaded() cardback=null");
        }
        else
        {
            LoadCardBackData data = (LoadCardBackData) userData;
            data.m_CardBack = componentInChildren;
            base.StartCoroutine(this.HiddenActorCardBackLoadedSetup(data));
        }
    }

    private void OnHiddenActorLoaded(string name, GameObject go, object userData)
    {
        LoadCardBackData data = (LoadCardBackData) userData;
        int cardBackIndex = data.m_CardBackIndex;
        string prefabName = this.m_cardBackData[cardBackIndex].PrefabName;
        data.m_GameObject = go;
        AssetLoader.Get().LoadCardBack(prefabName, new AssetLoader.GameObjectCallback(this.OnHiddenActorCardBackLoaded), userData, false);
    }

    private void OnSceneChangeResetDefaultCardBack(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY)
        {
            this.LoadCardBack(this.m_cardBackData[this.GetDefaultCardBackID()].PrefabName, true);
        }
    }

    [DebuggerHidden]
    private IEnumerator RegisterForDefaultCardBackChangesWhenPossible()
    {
        return new <RegisterForDefaultCardBackChangesWhenPossible>c__Iterator22 { <>f__this = this };
    }

    private void SetCardBack(GameObject go, CardBack cardBack)
    {
        this.SetCardBack(go, cardBack, false);
    }

    private void SetCardBack(GameObject go, bool friendlySide)
    {
        CardBack friendlyCardBack = this.m_FriendlyCardBack;
        if (!friendlySide)
        {
            friendlyCardBack = this.m_OpponentCardBack;
        }
        if (friendlyCardBack == null)
        {
            UnityEngine.Debug.LogWarning("CardBackManager SetCardBack() cardBack=null");
        }
        else
        {
            this.SetCardBack(go, friendlyCardBack);
        }
    }

    private void SetCardBack(GameObject go, CardBack cardBack, bool unlit)
    {
        if (cardBack == null)
        {
            UnityEngine.Debug.LogWarning("CardBackManager SetCardBack() cardback=null");
        }
        else if (go == null)
        {
            UnityEngine.Debug.LogWarning("CardBackManager SetCardBack() go=null");
        }
        else
        {
            Mesh cardBackMesh = cardBack.m_CardBackMesh;
            if (cardBackMesh != null)
            {
                MeshFilter component = go.GetComponent<MeshFilter>();
                if (component != null)
                {
                    component.mesh = cardBackMesh;
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("CardBackManager SetCardBack() mesh=null");
            }
            float num = 0f;
            if (!unlit && (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY))
            {
                num = 1f;
            }
            Material cardBackMaterial = cardBack.m_CardBackMaterial;
            Material original = cardBack.m_CardBackMaterial1;
            if (cardBackMaterial != null)
            {
                int num2 = 1;
                if (original != null)
                {
                    num2 = 2;
                }
                Material[] materialArray = new Material[num2];
                materialArray[0] = UnityEngine.Object.Instantiate<Material>(cardBackMaterial);
                if (original != null)
                {
                    materialArray[1] = UnityEngine.Object.Instantiate<Material>(original);
                }
                float num3 = UnityEngine.Random.Range((float) 0f, (float) 1f);
                foreach (Material material3 in materialArray)
                {
                    if (material3 != null)
                    {
                        if (material3.HasProperty("_Seed") && (material3.GetFloat("_Seed") == 0f))
                        {
                            material3.SetFloat("_Seed", num3);
                        }
                        if (material3.HasProperty("_LightingBlend"))
                        {
                            material3.SetFloat("_LightingBlend", num);
                        }
                    }
                }
                go.GetComponent<Renderer>().materials = materialArray;
            }
            else
            {
                UnityEngine.Debug.LogWarning("CardBackManager SetCardBack() material=null");
            }
            Actor actor = SceneUtils.FindComponentInThisOrParents<Actor>(go);
            if (actor != null)
            {
                actor.UpdateMissingCardArt();
            }
        }
    }

    public void SetCardBackTexture(Renderer renderer, int matIdx, bool friendlySide)
    {
        if (friendlySide && this.m_isFriendlyLoading)
        {
            base.StartCoroutine(this.SetTextureWhenLoaded(renderer, matIdx, friendlySide));
        }
        else if (!friendlySide && this.m_isOpponentLoading)
        {
            base.StartCoroutine(this.SetTextureWhenLoaded(renderer, matIdx, friendlySide));
        }
        else
        {
            this.SetTexture(renderer, matIdx, friendlySide);
        }
    }

    [DebuggerHidden]
    private IEnumerator SetCardBackWhenLoaded(GameObject go, bool friendlySide)
    {
        return new <SetCardBackWhenLoaded>c__Iterator1F { friendlySide = friendlySide, go = go, <$>friendlySide = friendlySide, <$>go = go, <>f__this = this };
    }

    private void SetDeckCardBack(GameObject go, bool friendlySide)
    {
        if (friendlySide)
        {
            if (this.m_FriendlyCardBack == null)
            {
                return;
            }
        }
        else if (this.m_OpponentCardBack == null)
        {
            return;
        }
        Texture cardBackTexture = this.m_FriendlyCardBack.m_CardBackTexture;
        if (!friendlySide)
        {
            cardBackTexture = this.m_OpponentCardBack.m_CardBackTexture;
        }
        if (cardBackTexture == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CardBackManager SetDeckCardBack(): texture is null!", new object[0]));
        }
        else
        {
            foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
            {
                renderer.material.mainTexture = cardBackTexture;
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator SetDeckCardBackWhenLoaded(GameObject go, bool friendlySide)
    {
        return new <SetDeckCardBackWhenLoaded>c__Iterator21 { friendlySide = friendlySide, go = go, <$>friendlySide = friendlySide, <$>go = go, <>f__this = this };
    }

    private void SetDragEffects(GameObject go, bool friendlySide)
    {
        CardBackDragEffect componentInChildren = go.GetComponentInChildren<CardBackDragEffect>();
        if (componentInChildren != null)
        {
            CardBack friendlyCardBack = this.m_FriendlyCardBack;
            if (friendlyCardBack != null)
            {
                if (!friendlySide)
                {
                    friendlyCardBack = this.m_OpponentCardBack;
                }
                if (componentInChildren.m_EffectsRoot != null)
                {
                    UnityEngine.Object.Destroy(componentInChildren.m_EffectsRoot);
                }
                if (friendlyCardBack.m_DragEffect != null)
                {
                    GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(friendlyCardBack.m_DragEffect);
                    componentInChildren.m_EffectsRoot = obj2;
                    obj2.transform.parent = componentInChildren.gameObject.transform;
                    obj2.transform.localPosition = Vector3.zero;
                    obj2.transform.localRotation = Quaternion.identity;
                    obj2.transform.localScale = Vector3.one;
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator SetDragEffectsWhenLoaded(GameObject go, bool friendlySide)
    {
        return new <SetDragEffectsWhenLoaded>c__Iterator20 { friendlySide = friendlySide, go = go, <$>friendlySide = friendlySide, <$>go = go, <>f__this = this };
    }

    public void SetGameCardBackIDs(int friendlyCardBackID, int opponentCardBackID)
    {
        int validCardBackID = this.GetValidCardBackID(friendlyCardBackID);
        this.LoadCardBack(this.m_cardBackData[validCardBackID].PrefabName, true);
        int num2 = this.GetValidCardBackID(opponentCardBackID);
        this.LoadCardBack(this.m_cardBackData[num2].PrefabName, false);
        this.UpdateAllCardBacks();
    }

    private void SetTexture(Renderer renderer, int matIdx, bool friendlySide)
    {
        Texture cardBackTexture = this.m_FriendlyCardBack.m_CardBackTexture;
        if (!friendlySide)
        {
            cardBackTexture = this.m_OpponentCardBack.m_CardBackTexture;
        }
        if (cardBackTexture == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CardBackManager SetTexture(): texture is null!   obj: {0}  friendly: {1}", renderer.gameObject.name, friendlySide));
        }
        else
        {
            renderer.materials[matIdx].mainTexture = cardBackTexture;
        }
    }

    [DebuggerHidden]
    private IEnumerator SetTextureWhenLoaded(Renderer renderer, int matIdx, bool friendlySide)
    {
        return new <SetTextureWhenLoaded>c__Iterator1E { friendlySide = friendlySide, renderer = renderer, matIdx = matIdx, <$>friendlySide = friendlySide, <$>renderer = renderer, <$>matIdx = matIdx, <>f__this = this };
    }

    private void Start()
    {
        Options.Get().RegisterChangedListener(Option.CARD_BACK, new Options.ChangedCallback(this.OnCheatOptionChanged));
        Options.Get().RegisterChangedListener(Option.CARD_BACK2, new Options.ChangedCallback(this.OnCheatOptionChanged));
        if (!this.m_ResetDefaultRegistered)
        {
            SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneChangeResetDefaultCardBack));
            this.m_ResetDefaultRegistered = true;
        }
        this.InitCardBacks();
        base.StartCoroutine(this.RegisterForDefaultCardBackChangesWhenPossible());
        base.StartCoroutine(this.UpdateDefaultCardBackWhenReady());
    }

    private void Update()
    {
    }

    public void UpdateAllCardBacks()
    {
        base.StartCoroutine(this.UpdateAllCardBacksImpl());
    }

    [DebuggerHidden]
    private IEnumerator UpdateAllCardBacksImpl()
    {
        return new <UpdateAllCardBacksImpl>c__Iterator24 { <>f__this = this };
    }

    public void UpdateCardBack(Actor actor, CardBack cardBack)
    {
        if (((actor.gameObject != null) && (actor.m_cardMesh != null)) && (cardBack != null))
        {
            this.SetCardBack(actor.m_cardMesh, cardBack);
        }
    }

    public void UpdateCardBack(GameObject go, bool friendlySide)
    {
        if (go != null)
        {
            if (friendlySide && this.m_isFriendlyLoading)
            {
                base.StartCoroutine(this.SetCardBackWhenLoaded(go, friendlySide));
            }
            else if (!friendlySide && this.m_isOpponentLoading)
            {
                base.StartCoroutine(this.SetCardBackWhenLoaded(go, friendlySide));
            }
            else
            {
                this.SetCardBack(go, friendlySide);
            }
        }
    }

    public void UpdateCardBackWithInternalCardBack(Actor actor)
    {
        if ((actor.gameObject != null) && (actor.m_cardMesh != null))
        {
            CardBack componentInChildren = actor.gameObject.GetComponentInChildren<CardBack>();
            if (componentInChildren != null)
            {
                this.SetCardBack(actor.m_cardMesh, componentInChildren);
            }
        }
    }

    public void UpdateDeck(GameObject go, bool friendlySide)
    {
        if (go != null)
        {
            if (friendlySide && this.m_isFriendlyLoading)
            {
                base.StartCoroutine(this.SetDeckCardBackWhenLoaded(go, friendlySide));
            }
            else if (!friendlySide && this.m_isOpponentLoading)
            {
                base.StartCoroutine(this.SetDeckCardBackWhenLoaded(go, friendlySide));
            }
            else
            {
                this.SetDeckCardBack(go, friendlySide);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateDefaultCardBackWhenReady()
    {
        return new <UpdateDefaultCardBackWhenReady>c__Iterator23 { <>f__this = this };
    }

    public void UpdateDragEffect(GameObject go, bool friendlySide)
    {
        if (go != null)
        {
            if (friendlySide && this.m_isFriendlyLoading)
            {
                base.StartCoroutine(this.SetDragEffectsWhenLoaded(go, friendlySide));
            }
            else if (!friendlySide && this.m_isOpponentLoading)
            {
                base.StartCoroutine(this.SetDragEffectsWhenLoaded(go, friendlySide));
            }
            else
            {
                this.SetDragEffects(go, friendlySide);
            }
        }
    }

    private void WillReset()
    {
        this.InitCardBackData();
    }

    [CompilerGenerated]
    private sealed class <GetDeckCardBackID>c__AnonStorey2BE
    {
        internal long deck;

        internal bool <>m__77(NetCache.DeckHeader obj)
        {
            return (obj.ID == this.deck);
        }
    }

    [CompilerGenerated]
    private sealed class <HiddenActorCardBackLoadedSetup>c__Iterator25 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardBackManager.LoadCardBackData <$>data;
        internal CardBackManager <>f__this;
        internal Actor <actor>__0;
        internal CardBackManager.LoadCardBackData data;

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
                    this.$current = null;
                    this.$PC = 1;
                    goto Label_00DB;

                case 1:
                    this.$current = null;
                    this.$PC = 2;
                    goto Label_00DB;

                case 2:
                    this.<actor>__0 = this.data.m_GameObject.GetComponent<Actor>();
                    this.<>f__this.SetCardBack(this.<actor>__0.m_cardMesh, this.data.m_CardBack, this.data.m_Unlit);
                    this.data.m_CardBack.gameObject.transform.parent = this.data.m_GameObject.transform;
                    this.data.m_Callback(this.data);
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00DB:
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
    private sealed class <RegisterForDefaultCardBackChangesWhenPossible>c__Iterator22 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardBackManager <>f__this;

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
                    if (!this.<>f__this.m_DefaultCardBackChangeListenerRegistered)
                    {
                        break;
                    }
                    goto Label_0085;

                case 1:
                    break;

                default:
                    goto Label_0085;
            }
            while (CollectionManager.Get() == null)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.m_DefaultCardBackChangeListenerRegistered = CollectionManager.Get().RegisterDefaultCardbackChangedListener(new CollectionManager.DefaultCardbackChangedCallback(this.<>f__this.OnDefaultCardBackChanged));
            this.$PC = -1;
        Label_0085:
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
    private sealed class <SetCardBackWhenLoaded>c__Iterator1F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>friendlySide;
        internal GameObject <$>go;
        internal CardBackManager <>f__this;
        internal bool friendlySide;
        internal GameObject go;

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
                    if (!this.friendlySide)
                    {
                        goto Label_0075;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0075;

                default:
                    goto Label_00A8;
            }
            if (this.<>f__this.m_isFriendlyLoading)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_00AA;
            }
            goto Label_0085;
        Label_0075:
            while (this.<>f__this.m_isOpponentLoading)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00AA;
            }
        Label_0085:
            this.<>f__this.SetCardBack(this.go, this.friendlySide);
            goto Label_00A8;
            this.$PC = -1;
        Label_00A8:
            return false;
        Label_00AA:
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
    private sealed class <SetDeckCardBackWhenLoaded>c__Iterator21 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>friendlySide;
        internal GameObject <$>go;
        internal CardBackManager <>f__this;
        internal bool friendlySide;
        internal GameObject go;

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
                    if (!this.friendlySide)
                    {
                        goto Label_0075;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0075;

                default:
                    goto Label_00A8;
            }
            if (this.<>f__this.m_isFriendlyLoading)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_00AA;
            }
            goto Label_0085;
        Label_0075:
            while (this.<>f__this.m_isOpponentLoading)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00AA;
            }
        Label_0085:
            this.<>f__this.SetDeckCardBack(this.go, this.friendlySide);
            goto Label_00A8;
            this.$PC = -1;
        Label_00A8:
            return false;
        Label_00AA:
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
    private sealed class <SetDragEffectsWhenLoaded>c__Iterator20 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>friendlySide;
        internal GameObject <$>go;
        internal CardBackManager <>f__this;
        internal bool friendlySide;
        internal GameObject go;

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
                    if (!this.friendlySide)
                    {
                        goto Label_0075;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0075;

                default:
                    goto Label_00A8;
            }
            if (this.<>f__this.m_isFriendlyLoading)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_00AA;
            }
            goto Label_0085;
        Label_0075:
            while (this.<>f__this.m_isOpponentLoading)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00AA;
            }
        Label_0085:
            this.<>f__this.SetDragEffects(this.go, this.friendlySide);
            goto Label_00A8;
            this.$PC = -1;
        Label_00A8:
            return false;
        Label_00AA:
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
    private sealed class <SetTextureWhenLoaded>c__Iterator1E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>friendlySide;
        internal int <$>matIdx;
        internal Renderer <$>renderer;
        internal CardBackManager <>f__this;
        internal bool friendlySide;
        internal int matIdx;
        internal Renderer renderer;

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
                    if (!this.friendlySide)
                    {
                        goto Label_0075;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0075;

                default:
                    goto Label_00AE;
            }
            if (this.<>f__this.m_isFriendlyLoading)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_00B0;
            }
            goto Label_0085;
        Label_0075:
            while (this.<>f__this.m_isOpponentLoading)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00B0;
            }
        Label_0085:
            this.<>f__this.SetTexture(this.renderer, this.matIdx, this.friendlySide);
            goto Label_00AE;
            this.$PC = -1;
        Label_00AE:
            return false;
        Label_00B0:
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
    private sealed class <UpdateAllCardBacksImpl>c__Iterator24 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor[] <$s_184>__1;
        internal int <$s_185>__2;
        internal CardBackDisplay[] <$s_186>__5;
        internal int <$s_187>__6;
        internal CardBackDragEffect[] <$s_188>__9;
        internal int <$s_189>__10;
        internal CardBackDeckDisplay[] <$s_190>__13;
        internal int <$s_191>__14;
        internal CardBackManager <>f__this;
        internal Actor <actor>__3;
        internal Actor[] <actors>__0;
        internal CardBackDisplay <cbd>__7;
        internal CardBackDisplay[] <cbDisplays>__4;
        internal CardBackDeckDisplay <deck>__15;
        internal CardBackDeckDisplay[] <deckDisplays>__12;
        internal CardBackDragEffect[] <dragFx>__8;
        internal CardBackDragEffect <fx>__11;

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
                    if (this.<>f__this.m_isFriendlyLoading || this.<>f__this.m_isOpponentLoading)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<actors>__0 = UnityEngine.Object.FindObjectsOfType(typeof(Actor)) as Actor[];
                    this.<$s_184>__1 = this.<actors>__0;
                    this.<$s_185>__2 = 0;
                    while (this.<$s_185>__2 < this.<$s_184>__1.Length)
                    {
                        this.<actor>__3 = this.<$s_184>__1[this.<$s_185>__2];
                        this.<actor>__3.UpdateCardBack();
                        this.<$s_185>__2++;
                    }
                    this.<cbDisplays>__4 = UnityEngine.Object.FindObjectsOfType(typeof(CardBackDisplay)) as CardBackDisplay[];
                    this.<$s_186>__5 = this.<cbDisplays>__4;
                    this.<$s_187>__6 = 0;
                    while (this.<$s_187>__6 < this.<$s_186>__5.Length)
                    {
                        this.<cbd>__7 = this.<$s_186>__5[this.<$s_187>__6];
                        this.<cbd>__7.UpdateCardBack();
                        this.<$s_187>__6++;
                    }
                    this.<dragFx>__8 = UnityEngine.Object.FindObjectsOfType(typeof(CardBackDragEffect)) as CardBackDragEffect[];
                    this.<$s_188>__9 = this.<dragFx>__8;
                    this.<$s_189>__10 = 0;
                    while (this.<$s_189>__10 < this.<$s_188>__9.Length)
                    {
                        this.<fx>__11 = this.<$s_188>__9[this.<$s_189>__10];
                        this.<fx>__11.SetEffect();
                        this.<$s_189>__10++;
                    }
                    this.<deckDisplays>__12 = UnityEngine.Object.FindObjectsOfType(typeof(CardBackDeckDisplay)) as CardBackDeckDisplay[];
                    this.<$s_190>__13 = this.<deckDisplays>__12;
                    this.<$s_191>__14 = 0;
                    while (this.<$s_191>__14 < this.<$s_190>__13.Length)
                    {
                        this.<deck>__15 = this.<$s_190>__13[this.<$s_191>__14];
                        this.<deck>__15.UpdateDeckCardBacks();
                        this.<$s_191>__14++;
                    }
                    break;

                default:
                    break;
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
    private sealed class <UpdateDefaultCardBackWhenReady>c__Iterator23 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardBackManager <>f__this;
        internal NetCache.NetCacheCardBacks <netCacheCardBacks>__0;

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
                    this.<netCacheCardBacks>__0 = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>();
                    if (this.<netCacheCardBacks>__0 == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (!this.<>f__this.m_cardBackData.ContainsKey(this.<netCacheCardBacks>__0.DefaultCardBack))
                    {
                        object[] args = new object[] { this.<netCacheCardBacks>__0.DefaultCardBack };
                        Log.CardbackMgr.Print("No cardback for {0}, set default to 0", args);
                        this.<netCacheCardBacks>__0.DefaultCardBack = 0;
                    }
                    this.<>f__this.OnDefaultCardBackChanged(this.<netCacheCardBacks>__0.DefaultCardBack, null);
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

    public class LoadCardBackData
    {
        public LoadCardBackCallback m_Callback;
        public CardBack m_CardBack;
        public int m_CardBackIndex;
        public bool m_FriendlySide;
        public GameObject m_GameObject;
        public string m_Name;
        public string m_Path;
        public bool m_Unlit;

        public delegate void LoadCardBackCallback(CardBackManager.LoadCardBackData cardBackData);
    }

    public class OwnedCardBack
    {
        public int m_cardBackId;
        public bool m_owned;
        public long m_seasonId = -1L;
        public int m_sortCategory;
        public int m_sortOrder;
    }
}

