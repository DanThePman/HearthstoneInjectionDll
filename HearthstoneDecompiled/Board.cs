using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class Board : MonoBehaviour
{
    [CompilerGenerated]
    private static Action<object> <>f__am$cache1B;
    [CompilerGenerated]
    private static Action<object> <>f__am$cache1C;
    private const string GOLDEN_HERO_TRAY_FRIENDLY = "HeroTray_Golden_Friendly";
    private const string GOLDEN_HERO_TRAY_OPPONENT = "HeroTray_Golden_Opponent";
    public Color m_AmbientColor = Color.white;
    private int m_boardDbId;
    public MusicPlaylistType m_BoardMusic = MusicPlaylistType.InGame_Default;
    public Transform m_BoneParent;
    public Transform m_ColliderParent;
    public GameObject m_CombinedPlaySurface;
    public Color m_DeckColor = Color.white;
    public Light m_DirectionalLight;
    public float m_DirectionalLightIntensity = 0.275f;
    public Color m_EndTurnButtonColor = Color.white;
    public GameObject m_FriendlyHeroPhoneTray;
    public GameObject m_FriendlyHeroTray;
    private Spell m_FriendlyTraySpellEffect;
    public Texture m_GemManaPhoneTexture;
    public Color m_GoldenHeroTrayColor = Color.white;
    public Color m_HistoryTileColor = Color.white;
    public GameObject m_MouseClickDustEffect;
    public GameObject m_OpponentHeroPhoneTray;
    public GameObject m_OpponentHeroTray;
    private Spell m_OpponentTraySpellEffect;
    private bool m_raisedLights;
    public Color m_ShadowColor = new Color(0.098f, 0.098f, 0.235f, 0.45f);
    public List<BoardSpecialEvents> m_SpecialEvents;
    public GameObject m_SplitPlaySurface;
    private Color m_TrayTint = Color.white;
    private readonly Color MULLIGAN_AMBIENT_LIGHT_COLOR = new Color(0.1607843f, 0.1921569f, 0.282353f, 1f);
    private const float MULLIGAN_LIGHT_INTENSITY = 0f;
    private static Board s_instance;

    private void Awake()
    {
        s_instance = this;
        if (LoadingScreen.Get() != null)
        {
            LoadingScreen.Get().NotifyMainSceneObjectAwoke(base.gameObject);
        }
        if (this.m_FriendlyHeroTray == null)
        {
            UnityEngine.Debug.LogError("Friendly Hero Tray is not assigned!");
        }
        if (this.m_OpponentHeroTray == null)
        {
            UnityEngine.Debug.LogError("Opponent Hero Tray is not assigned!");
        }
    }

    public void CombinedSurface()
    {
        if ((this.m_CombinedPlaySurface != null) && (this.m_SplitPlaySurface != null))
        {
            this.m_CombinedPlaySurface.SetActive(true);
            this.m_SplitPlaySurface.SetActive(false);
        }
    }

    public void DimTheLights()
    {
        this.DimTheLights(5f);
    }

    public void DimTheLights(float speed)
    {
        if (this.m_raisedLights)
        {
            float num = 3f / speed;
            if (<>f__am$cache1C == null)
            {
                <>f__am$cache1C = amount => RenderSettings.ambientLight = (Color) amount;
            }
            Action<object> action = <>f__am$cache1C;
            object[] args = new object[] { "from", RenderSettings.ambientLight, "to", this.MULLIGAN_AMBIENT_LIGHT_COLOR, "time", num, "easeType", iTween.EaseType.easeInOutQuad, "onupdate", action, "onupdatetarget", base.gameObject };
            Hashtable hashtable = iTween.Hash(args);
            iTween.ValueTo(base.gameObject, hashtable);
            Action<object> action2 = amount => this.m_DirectionalLight.intensity = (float) amount;
            object[] objArray2 = new object[] { "from", this.m_DirectionalLight.intensity, "to", 0f, "time", num, "easeType", iTween.EaseType.easeInOutQuad, "onupdate", action2, "onupdatetarget", base.gameObject };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.ValueTo(base.gameObject, hashtable2);
            this.m_raisedLights = false;
        }
    }

    public Transform FindBone(string name)
    {
        if (this.m_BoneParent != null)
        {
            Transform transform = this.m_BoneParent.Find(name);
            if (transform != null)
            {
                return transform;
            }
        }
        return BoardStandardGame.Get().FindBone(name);
    }

    public Collider FindCollider(string name)
    {
        if (this.m_ColliderParent != null)
        {
            Transform transform = this.m_ColliderParent.Find(name);
            if (transform != null)
            {
                return ((transform != null) ? transform.GetComponent<Collider>() : null);
            }
        }
        return BoardStandardGame.Get().FindCollider(name);
    }

    public static Board Get()
    {
        return s_instance;
    }

    public Spell GetFriendlyTraySpell()
    {
        return this.m_FriendlyTraySpellEffect;
    }

    public GameObject GetMouseClickDustEffectPrefab()
    {
        return this.m_MouseClickDustEffect;
    }

    public Spell GetOpponentTraySpell()
    {
        return this.m_OpponentTraySpellEffect;
    }

    [DebuggerHidden]
    private IEnumerator GoldenHeroes()
    {
        return new <GoldenHeroes>c__Iterator236 { <>f__this = this };
    }

    private void LoadBoardSpecialEvent(BoardSpecialEvents boardSpecialEvent)
    {
        object[] args = new object[] { boardSpecialEvent.Prefab };
        Log.Kyle.Print("Loading Board Special Event Prefab: {0}", args);
        string name = FileUtils.GameAssetPathToName(boardSpecialEvent.Prefab);
        if (AssetLoader.Get().LoadGameObject(name, true, false) == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("Failed to load special board event: {0}", boardSpecialEvent.Prefab));
        }
        this.m_AmbientColor = boardSpecialEvent.AmbientColorOverride;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnHeroPhoneTrayTextureLoaded(string path, UnityEngine.Object obj, object callbackData)
    {
        if (obj == null)
        {
            UnityEngine.Debug.LogError("Board.OnHeroTrayTextureLoaded() loaded texture is null!");
        }
        else
        {
            Texture texture = (Texture) obj;
            Player.Side side = (Player.Side) ((int) callbackData);
            if (side == Player.Side.FRIENDLY)
            {
                if (this.m_FriendlyHeroPhoneTray == null)
                {
                    UnityEngine.Debug.LogWarning("Friendly Hero Phone Tray Object on Board is null!");
                }
                else
                {
                    Material material = this.m_FriendlyHeroPhoneTray.GetComponentInChildren<MeshRenderer>().material;
                    material.mainTexture = texture;
                    material.color = this.m_TrayTint;
                }
            }
            else if (this.m_OpponentHeroPhoneTray == null)
            {
                UnityEngine.Debug.LogWarning("Opponent Hero Phone Tray Object on Board is null!");
            }
            else
            {
                Material material2 = this.m_OpponentHeroPhoneTray.GetComponentInChildren<MeshRenderer>().material;
                material2.mainTexture = texture;
                material2.color = this.m_TrayTint;
            }
        }
    }

    private void OnHeroSkinManaGemTextureLoaded(string path, UnityEngine.Object obj, object callbackData)
    {
        if (obj == null)
        {
            UnityEngine.Debug.LogError("OnHeroSkinManaGemTextureLoaded() loaded texture is null!");
        }
        else
        {
            Texture texture = (Texture) obj;
            ManaCrystalMgr.Get().SetFriendlyManaGemTexture(texture);
            ManaCrystalMgr.Get().SetFriendlyManaGemTint(this.m_TrayTint);
        }
    }

    private void OnHeroTrayEffectLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError("Board.OnHeroTrayEffectLoaded() Hero tray effect is null!");
        }
        else
        {
            Spell component = go.GetComponent<Spell>();
            if (component == null)
            {
                UnityEngine.Debug.LogError("Board.OnHeroTrayEffectLoaded() Hero tray effect: could not find spell component!");
            }
            else
            {
                Player.Side side = (Player.Side) ((int) callbackData);
                if (side == Player.Side.FRIENDLY)
                {
                    go.transform.parent = base.transform;
                    go.transform.position = this.FindBone("CustomSocketIn_Friendly").position;
                    this.m_FriendlyTraySpellEffect = component;
                }
                else
                {
                    go.transform.parent = base.transform;
                    go.transform.position = this.FindBone("CustomSocketIn_Opposing").position;
                    this.m_OpponentTraySpellEffect = component;
                }
            }
        }
    }

    private void OnHeroTrayTextureLoaded(string path, UnityEngine.Object obj, object callbackData)
    {
        if (obj == null)
        {
            UnityEngine.Debug.LogError("Board.OnHeroTrayTextureLoaded() loaded texture is null!");
        }
        else
        {
            Texture texture = (Texture) obj;
            Player.Side side = (Player.Side) ((int) callbackData);
            if (side == Player.Side.FRIENDLY)
            {
                Material material = this.m_FriendlyHeroTray.GetComponentInChildren<MeshRenderer>().material;
                material.mainTexture = texture;
                material.color = this.m_TrayTint;
            }
            else
            {
                Material material2 = this.m_OpponentHeroTray.GetComponentInChildren<MeshRenderer>().material;
                material2.mainTexture = texture;
                material2.color = this.m_TrayTint;
            }
        }
    }

    [ContextMenu("RaiseTheLights")]
    public void RaiseTheLights()
    {
        this.RaiseTheLights(1f);
    }

    public void RaiseTheLights(float speed)
    {
        if (!this.m_raisedLights)
        {
            float num = 3f / speed;
            if (<>f__am$cache1B == null)
            {
                <>f__am$cache1B = amount => RenderSettings.ambientLight = (Color) amount;
            }
            Action<object> action = <>f__am$cache1B;
            object[] args = new object[] { "from", RenderSettings.ambientLight, "to", this.m_AmbientColor, "time", num, "easeType", iTween.EaseType.easeInOutQuad, "onupdate", action, "onupdatetarget", base.gameObject };
            Hashtable hashtable = iTween.Hash(args);
            iTween.ValueTo(base.gameObject, hashtable);
            Action<object> action2 = amount => this.m_DirectionalLight.intensity = (float) amount;
            object[] objArray2 = new object[] { "from", this.m_DirectionalLight.intensity, "to", this.m_DirectionalLightIntensity, "time", num, "easeType", iTween.EaseType.easeInOutQuad, "onupdate", action2, "onupdatetarget", base.gameObject };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.ValueTo(base.gameObject, hashtable2);
            this.m_raisedLights = true;
        }
    }

    public void RaiseTheLightsQuickly()
    {
        this.RaiseTheLights(5f);
    }

    public void ResetAmbientColor()
    {
        RenderSettings.ambientLight = this.m_AmbientColor;
    }

    public void SetBoardDbId(int id)
    {
        this.m_boardDbId = id;
        object[] args = new object[] { id };
        Log.Kyle.Print("Board DB ID: {0}", args);
    }

    private void ShowFriendlyHeroTray(string name, GameObject go, object callbackData)
    {
        go.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.FRIENDLY).transform.position;
        go.SetActive(true);
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = this.m_GoldenHeroTrayColor;
        }
        UnityEngine.Object.Destroy(this.m_FriendlyHeroTray);
        this.m_FriendlyHeroTray = go;
        base.StartCoroutine(this.UpdateHeroTray(Player.Side.FRIENDLY, true));
    }

    private void ShowOpponentHeroTray(string name, GameObject go, object callbackData)
    {
        go.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING).transform.position;
        go.SetActive(true);
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = this.m_GoldenHeroTrayColor;
        }
        this.m_OpponentHeroTray.SetActive(false);
        UnityEngine.Object.Destroy(this.m_OpponentHeroTray);
        this.m_OpponentHeroTray = go;
        base.StartCoroutine(this.UpdateHeroTray(Player.Side.OPPOSING, true));
    }

    public void SplitSurface()
    {
        if ((this.m_CombinedPlaySurface != null) && (this.m_SplitPlaySurface != null))
        {
            this.m_CombinedPlaySurface.SetActive(false);
            this.m_SplitPlaySurface.SetActive(true);
        }
    }

    private void Start()
    {
        RenderSettings.ambientLight = this.MULLIGAN_AMBIENT_LIGHT_COLOR;
        this.m_DirectionalLight.intensity = 0f;
        ProjectedShadow.SetShadowColor(this.m_ShadowColor);
        if (base.GetComponent<Animation>() != null)
        {
            base.GetComponent<Animation>()[base.GetComponent<Animation>().clip.name].normalizedTime = 0.25f;
            base.GetComponent<Animation>()[base.GetComponent<Animation>().clip.name].speed = -3f;
            base.GetComponent<Animation>().Play(base.GetComponent<Animation>().clip.name);
        }
        base.StartCoroutine(this.GoldenHeroes());
        foreach (BoardSpecialEvents events in this.m_SpecialEvents)
        {
            if (SpecialEventManager.Get().IsEventActive(events.EventType, false))
            {
                object[] args = new object[] { events.EventType };
                Log.Kyle.Print("Board Special Event: ", args);
                this.LoadBoardSpecialEvent(events);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateHeroTray(Player.Side side, bool isGolden)
    {
        return new <UpdateHeroTray>c__Iterator237 { side = side, <$>side = side, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <GoldenHeroes>c__Iterator236 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Board <>f__this;
        internal Entity <friendlyEntityDef>__7;
        internal Card <friendlyHeroCard>__5;
        internal bool <friendlyHeroIsGolden>__0;
        internal Player <friendlyPlayer>__3;
        internal GameState <gameState>__2;
        internal Entity <opposingEntityDef>__8;
        internal Card <opposingHeroCard>__6;
        internal bool <opposingHeroIsGolden>__1;
        internal Player <opposingPlayer>__4;

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
                    this.<friendlyHeroIsGolden>__0 = false;
                    this.<opposingHeroIsGolden>__1 = false;
                    this.<gameState>__2 = GameState.Get();
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_00B6;

                case 3:
                    goto Label_010C;

                case 4:
                    goto Label_0157;

                case 5:
                    goto Label_0180;

                case 6:
                    goto Label_01B9;

                default:
                    goto Label_02DC;
            }
            if (this.<gameState>__2 == null)
            {
                this.<gameState>__2 = GameState.Get();
                this.$current = null;
                this.$PC = 1;
                goto Label_02DE;
            }
            this.<friendlyPlayer>__3 = this.<gameState>__2.GetFriendlySidePlayer();
        Label_00B6:
            while (this.<friendlyPlayer>__3 == null)
            {
                this.<friendlyPlayer>__3 = this.<gameState>__2.GetFriendlySidePlayer();
                this.$current = null;
                this.$PC = 2;
                goto Label_02DE;
            }
            this.<opposingPlayer>__4 = this.<gameState>__2.GetOpposingSidePlayer();
            this.<friendlyHeroCard>__5 = this.<friendlyPlayer>__3.GetHeroCard();
        Label_010C:
            while (this.<friendlyHeroCard>__5 == null)
            {
                this.<friendlyHeroCard>__5 = this.<friendlyPlayer>__3.GetHeroCard();
                this.$current = null;
                this.$PC = 3;
                goto Label_02DE;
            }
            this.<opposingHeroCard>__6 = this.<opposingPlayer>__4.GetHeroCard();
        Label_0157:
            while (this.<opposingHeroCard>__6 == null)
            {
                this.<opposingHeroCard>__6 = this.<opposingPlayer>__4.GetHeroCard();
                this.$current = null;
                this.$PC = 4;
                goto Label_02DE;
            }
        Label_0180:
            while (this.<friendlyHeroCard>__5.GetEntity() == null)
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_02DE;
            }
            this.<friendlyEntityDef>__7 = this.<friendlyHeroCard>__5.GetEntity();
        Label_01B9:
            while (this.<opposingHeroCard>__6.GetEntity() == null)
            {
                this.$current = null;
                this.$PC = 6;
                goto Label_02DE;
            }
            this.<opposingEntityDef>__8 = this.<opposingHeroCard>__6.GetEntity();
            if ((this.<friendlyHeroCard>__5.GetCardFlair().Premium == TAG_PREMIUM.GOLDEN) && (this.<friendlyEntityDef>__7.GetCardSet() != TAG_CARD_SET.HERO_SKINS))
            {
                this.<friendlyHeroIsGolden>__0 = true;
            }
            if ((this.<opposingHeroCard>__6.GetCardFlair().Premium == TAG_PREMIUM.GOLDEN) && (this.<opposingEntityDef>__8.GetCardSet() != TAG_CARD_SET.HERO_SKINS))
            {
                this.<opposingHeroIsGolden>__1 = true;
            }
            if (this.<friendlyHeroIsGolden>__0)
            {
                AssetLoader.Get().LoadGameObject("HeroTray_Golden_Friendly", new AssetLoader.GameObjectCallback(this.<>f__this.ShowFriendlyHeroTray), null, false);
            }
            else
            {
                this.<>f__this.StartCoroutine(this.<>f__this.UpdateHeroTray(Player.Side.FRIENDLY, false));
            }
            if (this.<opposingHeroIsGolden>__1)
            {
                AssetLoader.Get().LoadGameObject("HeroTray_Golden_Opponent", new AssetLoader.GameObjectCallback(this.<>f__this.ShowOpponentHeroTray), null, false);
            }
            else
            {
                this.<>f__this.StartCoroutine(this.<>f__this.UpdateHeroTray(Player.Side.OPPOSING, false));
                goto Label_02DC;
                this.$PC = -1;
            }
        Label_02DC:
            return false;
        Label_02DE:
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
    private sealed class <UpdateHeroTray>c__Iterator237 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Player.Side <$>side;
        internal Map<int, Player>.ValueCollection.Enumerator <$s_1355>__2;
        internal Board <>f__this;
        internal CardDef <cardDef>__6;
        internal Entity <hero>__4;
        internal Card <heroCard>__5;
        internal int <i>__7;
        internal Player <p>__0;
        internal Player <player>__3;
        internal Map<int, Player> <playerMap>__1;
        internal Player.Side side;

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
                    if (GameState.Get().GetPlayerMap().Count == 0)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_03D6;
                    }
                    this.<p>__0 = null;
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_0130;

                case 4:
                    goto Label_0169;

                case 5:
                    goto Label_0191;

                case 6:
                    goto Label_01D0;

                case 7:
                    goto Label_0343;

                default:
                    goto Label_03D4;
            }
            while (this.<p>__0 == null)
            {
                this.<playerMap>__1 = GameState.Get().GetPlayerMap();
                this.<$s_1355>__2 = this.<playerMap>__1.Values.GetEnumerator();
                try
                {
                    while (this.<$s_1355>__2.MoveNext())
                    {
                        this.<player>__3 = this.<$s_1355>__2.Current;
                        if (this.<player>__3.GetSide() == this.side)
                        {
                            this.<p>__0 = this.<player>__3;
                            goto Label_00FA;
                        }
                    }
                }
                finally
                {
                    this.<$s_1355>__2.Dispose();
                }
            Label_00FA:
                this.$current = null;
                this.$PC = 2;
                goto Label_03D6;
            }
        Label_0130:
            while (this.<p>__0.GetHero() == null)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_03D6;
            }
            this.<hero>__4 = this.<p>__0.GetHero();
        Label_0169:
            while (this.<hero>__4.IsLoadingAssets())
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_03D6;
            }
        Label_0191:
            while (this.<hero>__4.GetCard() == null)
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_03D6;
            }
            this.<heroCard>__5 = this.<hero>__4.GetCard();
        Label_01D0:
            while (this.<heroCard>__5.GetCardDef() == null)
            {
                this.$current = null;
                this.$PC = 6;
                goto Label_03D6;
            }
            this.<cardDef>__6 = this.<heroCard>__5.GetCardDef();
            this.<i>__7 = 0;
            while (this.<i>__7 < this.<cardDef>__6.m_CustomHeroTraySettings.Count)
            {
                if (this.<>f__this.m_boardDbId == this.<cardDef>__6.m_CustomHeroTraySettings[this.<i>__7].m_Board)
                {
                    this.<>f__this.m_TrayTint = this.<cardDef>__6.m_CustomHeroTraySettings[this.<i>__7].m_Tint;
                }
                this.<i>__7++;
            }
            if (!string.IsNullOrEmpty(this.<cardDef>__6.m_CustomHeroTray))
            {
                AssetLoader.Get().LoadTexture(this.<cardDef>__6.m_CustomHeroTray, new AssetLoader.ObjectCallback(this.<>f__this.OnHeroTrayTextureLoaded), this.side, false);
            }
            if ((UniversalInputManager.UsePhoneUI != null) && !string.IsNullOrEmpty(this.<cardDef>__6.m_CustomHeroPhoneTray))
            {
                AssetLoader.Get().LoadTexture(this.<cardDef>__6.m_CustomHeroPhoneTray, new AssetLoader.ObjectCallback(this.<>f__this.OnHeroPhoneTrayTextureLoaded), this.side, false);
            }
            if (UniversalInputManager.UsePhoneUI == null)
            {
                goto Label_03CD;
            }
        Label_0343:
            while (ManaCrystalMgr.Get() == null)
            {
                this.$current = null;
                this.$PC = 7;
                goto Label_03D6;
            }
            if (this.side == Player.Side.FRIENDLY)
            {
                if (!string.IsNullOrEmpty(this.<cardDef>__6.m_CustomHeroPhoneManaGem))
                {
                    AssetLoader.Get().LoadTexture(this.<cardDef>__6.m_CustomHeroPhoneManaGem, new AssetLoader.ObjectCallback(this.<>f__this.OnHeroSkinManaGemTextureLoaded), null, false);
                }
                else if (this.<>f__this.m_GemManaPhoneTexture != null)
                {
                    ManaCrystalMgr.Get().SetFriendlyManaGemTexture(this.<>f__this.m_GemManaPhoneTexture);
                }
            }
        Label_03CD:
            this.$PC = -1;
        Label_03D4:
            return false;
        Label_03D6:
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

    [Serializable]
    public class BoardSpecialEvents
    {
        public Color AmbientColorOverride = Color.white;
        public SpecialEventType EventType;
        [CustomEditField(T=EditType.GAME_OBJECT)]
        public string Prefab;
    }

    [Serializable]
    public class CustomTraySettings
    {
        public BoardDdId m_Board;
        public Color m_Tint = Color.white;
    }
}

