using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class ArenaTrayDisplay : MonoBehaviour
{
    public List<ArenaKeyVisualData> m_ArenaKeyVisualData;
    public GameObject m_BehindTheDoors;
    public GameObject m_InstructionText;
    private bool m_isTheKeyIdleEffectsLoading;
    public UberText m_LossesUberText;
    public GameObject m_Paper;
    public GameObject m_PaperMain;
    public Material m_PlainPaperMaterial;
    public int m_Rank;
    private RewardBoxesDisplay m_RewardBoxes;
    public GameObject m_RewardBoxesBone;
    public GameObject m_RewardDoorPlates;
    public Material m_RewardPaperMaterial;
    public PlayMakerFSM m_RewardPlaymaker;
    public GameObject m_TheKeyGlowHoleMesh;
    public GameObject m_TheKeyGlowPlane;
    private GameObject m_TheKeyIdleEffects;
    [CustomEditField(Sections="Keys")]
    public GameObject m_TheKeyMesh;
    public GameObject m_TheKeyOldSelectionGlow;
    private GameObject m_TheKeyParticleSystems;
    public GameObject m_TheKeySelectionGlow;
    public float m_TheKeyTransitionDelay = 0.5f;
    public float m_TheKeyTransitionFadeInTime = 1.5f;
    public float m_TheKeyTransitionFadeOutTime = 2f;
    public ParticleSystem m_TheKeyTransitionParticles;
    public string m_TheKeyTransitionSound = "arena_key_transition";
    [CustomEditField(Sections="Reward Panel")]
    public UberText m_WinCountUberText;
    public UberText m_WinsUberText;
    public GameObject m_Xmark1;
    public GameObject m_Xmark2;
    public GameObject m_Xmark3;
    public List<GameObject> m_XmarkBox;
    public GameObject m_XmarksRoot;
    private static ArenaTrayDisplay s_Instance;

    public void ActivateKey()
    {
        <ActivateKey>c__AnonStorey2ED storeyed = new <ActivateKey>c__AnonStorey2ED();
        SceneUtils.EnableColliders(this.m_TheKeyMesh, true);
        this.m_TheKeySelectionGlow.GetComponent<Renderer>().enabled = true;
        Color color = this.m_TheKeySelectionGlow.GetComponent<Renderer>().sharedMaterial.color;
        color.a = 0f;
        this.m_TheKeySelectionGlow.GetComponent<Renderer>().sharedMaterial.color = color;
        this.m_TheKeySelectionGlow.GetComponent<Renderer>().sharedMaterial.SetFloat("_FxIntensity", 1f);
        object[] args = new object[] { "alpha", 0.8f, "time", 2f, "easetype", iTween.EaseType.easeInOutBack };
        iTween.FadeTo(this.m_TheKeySelectionGlow, iTween.Hash(args));
        storeyed.KeyGlowMat = this.m_TheKeySelectionGlow.GetComponent<Renderer>().material;
        storeyed.KeyGlowMat.SetFloat("_FxIntensity", 0f);
        Action<object> action = new Action<object>(storeyed.<>m__E0);
        object[] objArray2 = new object[] { "time", 2f, "from", 0f, "to", 1f, "easetype", iTween.EaseType.easeInOutBack, "onupdate", action, "onupdatetarget", this.m_TheKeySelectionGlow };
        Hashtable hashtable = iTween.Hash(objArray2);
        iTween.ValueTo(this.m_TheKeySelectionGlow, hashtable);
        PegUIElement component = this.m_TheKeyMesh.GetComponent<PegUIElement>();
        if (component == null)
        {
            UnityEngine.Debug.LogWarning("ArenaTrayDisplay: PegUIElement missing on the Key!");
        }
        else
        {
            component.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.OpenRewardBox));
            Navigation.Push(new Navigation.NavigateBackHandler(Navigation.BlockBackingOut));
        }
    }

    [DebuggerHidden]
    private IEnumerator AnimateKeyTransition(int rank)
    {
        return new <AnimateKeyTransition>c__Iterator62 { rank = rank, <$>rank = rank, <>f__this = this };
    }

    public void AnimateRewards()
    {
        AssetLoader.GameObjectCallback callback = delegate (string name, GameObject go, object callbackData) {
            this.m_RewardBoxes = go.GetComponent<RewardBoxesDisplay>();
            this.m_RewardBoxes.SetRewards(DraftManager.Get().GetRewards());
            this.m_RewardBoxes.RegisterDoneCallback(new System.Action(this.OnRewardBoxesDone));
            TransformUtil.AttachAndPreserveLocalTransform(this.m_RewardBoxes.transform, this.m_RewardBoxesBone.transform);
            this.m_RewardBoxes.AnimateRewards();
        };
        AssetLoader.Get().LoadGameObject("RewardBoxes", callback, null, false);
    }

    private void Awake()
    {
        s_Instance = this;
    }

    public static ArenaTrayDisplay Get()
    {
        return s_Instance;
    }

    private void HidePaper()
    {
        this.m_Paper.SetActive(false);
    }

    public void KeyFXCancel()
    {
        if (this.m_TheKeyIdleEffects != null)
        {
            PlayMakerFSM componentInChildren = this.m_TheKeyIdleEffects.GetComponentInChildren<PlayMakerFSM>();
            if (componentInChildren != null)
            {
                componentInChildren.SendEvent("Cancel");
            }
        }
    }

    private void OnDestroy()
    {
    }

    private void OnDisable()
    {
    }

    private void OnEnable()
    {
    }

    private void OnIdleEffectsLoaded(string name, GameObject go, object callbackData)
    {
        this.m_isTheKeyIdleEffectsLoading = false;
        if (this.m_TheKeyIdleEffects != null)
        {
            UnityEngine.Object.Destroy(this.m_TheKeyIdleEffects);
        }
        this.m_TheKeyIdleEffects = go;
        go.SetActive(true);
        go.transform.parent = this.m_TheKeyMesh.transform;
        go.transform.localPosition = Vector3.zero;
    }

    private void OnRewardBoxesDone()
    {
        if ((this != null) && (base.gameObject != null))
        {
            DraftManager manager = DraftManager.Get();
            if (manager.GetDraftDeck() == null)
            {
                Log.Rachelle.Print("bug 8052, null exception", new object[0]);
            }
            else
            {
                Network.AckDraftRewards(manager.GetDraftDeck().ID, manager.GetSlot());
            }
            DraftDisplay.Get().OnOpenRewardsComplete();
        }
    }

    private void OpenRewardBox()
    {
        if (this.m_RewardPlaymaker == null)
        {
            UnityEngine.Debug.LogWarning("ArenaTrayDisplay: Missing Playmaker FSM!");
        }
        else
        {
            if (this.m_XmarksRoot != null)
            {
                this.m_XmarksRoot.SetActive(false);
            }
            if (this.m_TheKeySelectionGlow != null)
            {
                this.m_TheKeySelectionGlow.SetActive(false);
            }
            this.m_WinsUberText.gameObject.SetActive(false);
            this.m_LossesUberText.gameObject.SetActive(false);
            SceneUtils.EnableColliders(this.m_TheKeyMesh, false);
            SceneUtils.SetLayer(this.m_TheKeyMesh.transform.parent.gameObject, GameLayer.Default);
            if (this.m_TheKeyIdleEffects != null)
            {
                PlayMakerFSM componentInChildren = this.m_TheKeyIdleEffects.GetComponentInChildren<PlayMakerFSM>();
                if (componentInChildren != null)
                {
                    componentInChildren.SendEvent("Death");
                }
            }
            if (this.m_BehindTheDoors == null)
            {
                UnityEngine.Debug.LogWarning("ArenaTrayDisplay: m_BehindTheDoors is null!");
            }
            else
            {
                this.m_BehindTheDoors.SetActive(true);
                this.m_RewardPlaymaker.SendEvent("Birth");
            }
        }
    }

    private void OpenRewardBox(UIEvent e)
    {
        this.OpenRewardBox();
    }

    public void ShowOpenedRewards()
    {
    }

    private void ShowPlainPaper()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_Paper.SetActive(true);
            this.m_Paper.GetComponent<Renderer>().sharedMaterial = this.m_PlainPaperMaterial;
        }
        else
        {
            this.m_Paper.SetActive(false);
            this.m_PaperMain.SetActive(true);
        }
        this.m_XmarksRoot.SetActive(false);
        this.m_WinsUberText.Hide();
        this.m_LossesUberText.Hide();
    }

    public void ShowPlainPaperBackground()
    {
        this.ShowPlainPaper();
        if (this.m_InstructionText != null)
        {
            this.m_InstructionText.SetActive(true);
        }
        if ((this.m_RewardDoorPlates != null) && this.m_RewardDoorPlates.activeSelf)
        {
            this.m_RewardDoorPlates.SetActive(false);
        }
    }

    private void ShowRewardPaper()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_Paper.SetActive(true);
            this.m_Paper.GetComponent<Renderer>().sharedMaterial = this.m_RewardPaperMaterial;
        }
        else
        {
            this.m_Paper.SetActive(true);
            this.m_PaperMain.SetActive(false);
        }
        this.m_XmarksRoot.SetActive(true);
        this.m_WinsUberText.Show();
        this.m_LossesUberText.Show();
    }

    public void ShowRewardsOpenAtStart()
    {
        if (this.m_RewardPlaymaker == null)
        {
            UnityEngine.Debug.LogWarning("ArenaTrayDisplay: Missing Playmaker FSM!");
        }
        else
        {
            this.HidePaper();
            if (this.m_InstructionText != null)
            {
                this.m_InstructionText.SetActive(false);
            }
            if (this.m_WinCountUberText != null)
            {
                this.m_WinCountUberText.gameObject.SetActive(false);
            }
            if (this.m_WinsUberText != null)
            {
                this.m_WinsUberText.gameObject.SetActive(false);
            }
            if (this.m_LossesUberText != null)
            {
                this.m_LossesUberText.gameObject.SetActive(false);
            }
            if (this.m_XmarksRoot != null)
            {
                this.m_XmarksRoot.SetActive(false);
            }
            if (this.m_TheKeySelectionGlow != null)
            {
                this.m_TheKeySelectionGlow.SetActive(false);
            }
            this.m_WinsUberText.gameObject.SetActive(false);
            this.m_LossesUberText.gameObject.SetActive(false);
            this.m_TheKeyMesh.gameObject.SetActive(false);
            if (this.m_BehindTheDoors == null)
            {
                UnityEngine.Debug.LogWarning("ArenaTrayDisplay: m_BehindTheDoors is null!");
            }
            else
            {
                this.m_BehindTheDoors.SetActive(true);
                if (DraftManager.Get() == null)
                {
                    UnityEngine.Debug.LogError("ArenaTrayDisplay: DraftManager.Get() == null!");
                }
                else
                {
                    AssetLoader.GameObjectCallback callback = delegate (string name, GameObject go, object callbackData) {
                        this.m_RewardBoxes = go.GetComponent<RewardBoxesDisplay>();
                        this.m_RewardBoxes.SetRewards(DraftManager.Get().GetRewards());
                        this.m_RewardBoxes.RegisterDoneCallback(new System.Action(this.OnRewardBoxesDone));
                        TransformUtil.AttachAndPreserveLocalTransform(this.m_RewardBoxes.transform, this.m_RewardBoxesBone.transform);
                        this.m_RewardBoxes.DebugLogRewards();
                        this.m_RewardBoxes.ShowAlreadyOpenedRewards();
                    };
                    AssetLoader.Get().LoadGameObject("RewardBoxes", callback, null, false);
                    this.m_RewardPlaymaker.gameObject.SetActive(true);
                    this.m_RewardPlaymaker.SendEvent("Death");
                    if (this.m_TheKeyMesh.GetComponent<PegUIElement>() == null)
                    {
                        UnityEngine.Debug.LogWarning("ArenaTrayDisplay: PegUIElement missing on the Key!");
                    }
                }
            }
        }
    }

    private void Start()
    {
        if ((this.m_WinsUberText == null) || (this.m_LossesUberText == null))
        {
            UnityEngine.Debug.LogWarning("ArenaTrayDisplay: m_WinsUberText or m_LossesUberText is null!");
        }
        else
        {
            this.m_WinsUberText.Text = GameStrings.Get("GLUE_DRAFT_WINS_LABEL");
            this.m_LossesUberText.Text = GameStrings.Get("GLUE_DRAFT_LOSSES_LABEL");
            if (this.m_BehindTheDoors == null)
            {
                UnityEngine.Debug.LogWarning("ArenaTrayDisplay: m_BehindTheDoors is null!");
            }
            else
            {
                this.m_BehindTheDoors.SetActive(false);
                if (this.m_RewardDoorPlates == null)
                {
                    UnityEngine.Debug.LogWarning("ArenaTrayDisplay: m_RewardDoorPlates is null!");
                }
                else
                {
                    this.m_RewardDoorPlates.SetActive(false);
                    SceneUtils.EnableColliders(this.m_TheKeyMesh, false);
                }
            }
        }
    }

    private void UpdateKeyArt(int rank)
    {
        if (this.m_TheKeyMesh == null)
        {
            UnityEngine.Debug.LogWarning("ArenaTrayDisplay: key mesh missing!");
        }
        else
        {
            this.ShowRewardPaper();
            ArenaKeyVisualData data = this.m_ArenaKeyVisualData[rank];
            if (data.m_Mesh != null)
            {
                MeshFilter component = this.m_TheKeyMesh.GetComponent<MeshFilter>();
                if (component != null)
                {
                    component.mesh = UnityEngine.Object.Instantiate<Mesh>(data.m_Mesh);
                }
            }
            if (data.m_Material != null)
            {
                this.m_TheKeyMesh.GetComponent<Renderer>().sharedMaterial = UnityEngine.Object.Instantiate<Material>(data.m_Material);
            }
            if (data.m_IdleEffectsPrefabName != string.Empty)
            {
                this.m_isTheKeyIdleEffectsLoading = true;
                AssetLoader.Get().LoadGameObject(data.m_IdleEffectsPrefabName, new AssetLoader.GameObjectCallback(this.OnIdleEffectsLoaded), null, false);
            }
            if (data.m_ParticlePrefab != null)
            {
                GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(data.m_ParticlePrefab);
                Transform transform = obj2.transform.FindChild("FX_Motes");
                if (transform != null)
                {
                    GameObject gameObject = transform.gameObject;
                    gameObject.transform.parent = this.m_TheKeyMesh.transform;
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localRotation = Quaternion.identity;
                    this.m_RewardPlaymaker.FsmVariables.GetFsmGameObject("FX_Motes").Value = gameObject;
                }
                Transform transform2 = obj2.transform.FindChild("FX_Motes_glow");
                if (transform2 != null)
                {
                    GameObject obj4 = transform2.gameObject;
                    obj4.transform.parent = this.m_TheKeyMesh.transform;
                    obj4.transform.localPosition = Vector3.zero;
                    obj4.transform.localRotation = Quaternion.identity;
                    this.m_RewardPlaymaker.FsmVariables.GetFsmGameObject("FX_Motes_glow").Value = obj4;
                }
                Transform transform3 = obj2.transform.FindChild("FX_Motes_trail");
                if (transform3 != null)
                {
                    GameObject obj5 = transform3.gameObject;
                    obj5.transform.parent = this.m_TheKeyMesh.transform;
                    obj5.transform.localPosition = Vector3.zero;
                    obj5.transform.localRotation = Quaternion.identity;
                    this.m_RewardPlaymaker.FsmVariables.GetFsmGameObject("FX_Motes_trail").Value = obj5;
                }
            }
            if ((this.m_TheKeyGlowPlane != null) && (data.m_EffectGlowTexture != null))
            {
                this.m_TheKeyGlowPlane.GetComponent<Renderer>().material.mainTexture = data.m_EffectGlowTexture;
            }
            if (data.m_KeyHoleGlowMesh != null)
            {
                MeshFilter filter2 = this.m_TheKeyGlowHoleMesh.GetComponent<MeshFilter>();
                if (filter2 != null)
                {
                    filter2.mesh = UnityEngine.Object.Instantiate<Mesh>(data.m_KeyHoleGlowMesh);
                }
            }
            if ((this.m_TheKeySelectionGlow != null) && (data.m_SelectionGlowTexture != null))
            {
                this.m_TheKeySelectionGlow.GetComponent<Renderer>().material.mainTexture = data.m_SelectionGlowTexture;
            }
            SceneUtils.SetLayer(this.m_TheKeyMesh.transform.parent.gameObject, GameLayer.Default);
        }
    }

    public void UpdateTray()
    {
        this.UpdateTray(true);
    }

    public void UpdateTray(bool showNewKey)
    {
        this.ShowPlainPaper();
        if (this.m_InstructionText != null)
        {
            this.m_InstructionText.SetActive(false);
        }
        if ((this.m_RewardDoorPlates != null) && !this.m_RewardDoorPlates.activeSelf)
        {
            this.m_RewardDoorPlates.SetActive(true);
        }
        int rank = 0;
        int losses = 0;
        bool flag = false;
        DraftManager manager = DraftManager.Get();
        if (manager == null)
        {
            UnityEngine.Debug.LogError("ArenaTrayDisplay: DraftManager.Get() == null!");
        }
        else
        {
            rank = manager.GetWins();
            losses = manager.GetLosses();
            if (((SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY) && GameMgr.Get().WasArena()) && manager.GetIsNewKey())
            {
                flag = true;
            }
            this.m_WinCountUberText.Text = rank.ToString();
            if (losses > 0)
            {
                this.m_Xmark1.GetComponent<Renderer>().enabled = true;
            }
            else
            {
                this.m_Xmark1.GetComponent<Renderer>().enabled = false;
            }
            if (losses > 1)
            {
                this.m_Xmark2.GetComponent<Renderer>().enabled = true;
            }
            else
            {
                this.m_Xmark2.GetComponent<Renderer>().enabled = false;
            }
            if (losses > 2)
            {
                this.m_Xmark3.GetComponent<Renderer>().enabled = true;
            }
            else
            {
                this.m_Xmark3.GetComponent<Renderer>().enabled = false;
            }
            this.UpdateXBoxes();
            if ((flag && (rank > 0)) && showNewKey)
            {
                this.UpdateKeyArt(rank - 1);
                base.StartCoroutine(this.AnimateKeyTransition(rank));
            }
            else
            {
                this.UpdateKeyArt(rank);
            }
        }
    }

    private void UpdateXBoxes()
    {
        if (DemoMgr.Get().ArenaIs1WinMode())
        {
            this.m_XmarkBox[0].SetActive(true);
            this.m_XmarkBox[1].SetActive(false);
            this.m_XmarkBox[2].SetActive(false);
        }
    }

    [CompilerGenerated]
    private sealed class <ActivateKey>c__AnonStorey2ED
    {
        internal Material KeyGlowMat;

        internal void <>m__E0(object amount)
        {
            this.KeyGlowMat.SetFloat("_FxIntensity", (float) amount);
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateKeyTransition>c__Iterator62 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>rank;
        internal ArenaTrayDisplay <>f__this;
        internal ArenaTrayDisplay.ArenaKeyVisualData <keyData>__2;
        internal Hashtable <keyGlowArgs>__8;
        internal Material <KeyGlowMat>__6;
        internal Action<object> <keyGlowUpdate>__7;
        internal ArenaTrayDisplay.ArenaKeyVisualData <prevKeyData>__1;
        internal Hashtable <prevKeyGlowArgs>__5;
        internal Material <prevKeyGlowMat>__3;
        internal Action<object> <prevKeyGlowUpdate>__4;
        internal int <prevRank>__0;
        internal int rank;

        internal void <>m__E3(object amount)
        {
            this.<prevKeyGlowMat>__3.SetFloat("_FxIntensity", (float) amount);
        }

        internal void <>m__E4(object amount)
        {
            this.<KeyGlowMat>__6.SetFloat("_FxIntensity", (float) amount);
        }

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
                    this.$current = new WaitForSeconds(this.<>f__this.m_TheKeyTransitionDelay);
                    this.$PC = 1;
                    goto Label_0442;

                case 1:
                case 2:
                    if (this.<>f__this.m_isTheKeyIdleEffectsLoading)
                    {
                        this.$current = null;
                        this.$PC = 2;
                    }
                    else
                    {
                        this.<prevRank>__0 = this.rank - 1;
                        this.<prevKeyData>__1 = this.<>f__this.m_ArenaKeyVisualData[this.<prevRank>__0];
                        this.<keyData>__2 = this.<>f__this.m_ArenaKeyVisualData[this.rank];
                        if ((this.<>f__this.m_TheKeyOldSelectionGlow != null) && (this.<prevKeyData>__1.m_EffectGlowTexture != null))
                        {
                            this.<>f__this.m_TheKeyOldSelectionGlow.GetComponent<Renderer>().material.mainTexture = this.<prevKeyData>__1.m_SelectionGlowTexture;
                        }
                        this.<>f__this.m_TheKeyOldSelectionGlow.GetComponent<Renderer>().enabled = true;
                        this.<prevKeyGlowMat>__3 = this.<>f__this.m_TheKeyOldSelectionGlow.GetComponent<Renderer>().material;
                        this.<prevKeyGlowMat>__3.SetFloat("_FxIntensity", 0f);
                        this.<prevKeyGlowUpdate>__4 = new Action<object>(this.<>m__E3);
                        object[] args = new object[] { "time", this.<>f__this.m_TheKeyTransitionFadeInTime, "from", 0f, "to", 1.5f, "easetype", iTween.EaseType.easeInCubic, "onupdate", this.<prevKeyGlowUpdate>__4, "onupdatetarget", this.<>f__this.m_TheKeyOldSelectionGlow };
                        this.<prevKeyGlowArgs>__5 = iTween.Hash(args);
                        iTween.ValueTo(this.<>f__this.m_TheKeyOldSelectionGlow, this.<prevKeyGlowArgs>__5);
                        if (this.<>f__this.m_TheKeyTransitionSound != string.Empty)
                        {
                            SoundManager.Get().LoadAndPlay(this.<>f__this.m_TheKeyTransitionSound);
                        }
                        this.$current = new WaitForSeconds(this.<>f__this.m_TheKeyTransitionFadeInTime);
                        this.$PC = 3;
                    }
                    goto Label_0442;

                case 3:
                {
                    this.<>f__this.m_TheKeyTransitionParticles.Play();
                    this.<>f__this.UpdateKeyArt(this.rank);
                    this.<>f__this.m_TheKeyOldSelectionGlow.GetComponent<Renderer>().enabled = false;
                    if ((this.<>f__this.m_TheKeySelectionGlow != null) && (this.<keyData>__2.m_EffectGlowTexture != null))
                    {
                        this.<>f__this.m_TheKeySelectionGlow.GetComponent<Renderer>().material.mainTexture = this.<keyData>__2.m_SelectionGlowTexture;
                    }
                    this.<>f__this.m_TheKeySelectionGlow.GetComponent<Renderer>().enabled = true;
                    this.<prevKeyGlowMat>__3.SetFloat("_FxIntensity", 0f);
                    this.<KeyGlowMat>__6 = this.<>f__this.m_TheKeySelectionGlow.GetComponent<Renderer>().material;
                    this.<KeyGlowMat>__6.SetFloat("_FxIntensity", 1.5f);
                    this.<keyGlowUpdate>__7 = new Action<object>(this.<>m__E4);
                    object[] objArray2 = new object[] { "time", this.<>f__this.m_TheKeyTransitionFadeOutTime, "from", 1.5f, "to", 0f, "easetype", iTween.EaseType.easeOutCubic, "onupdate", this.<keyGlowUpdate>__7, "onupdatetarget", this.<>f__this.m_TheKeySelectionGlow };
                    this.<keyGlowArgs>__8 = iTween.Hash(objArray2);
                    iTween.ValueTo(this.<>f__this.m_TheKeySelectionGlow, this.<keyGlowArgs>__8);
                    this.$current = new WaitForSeconds(this.<>f__this.m_TheKeyTransitionFadeOutTime);
                    this.$PC = 4;
                    goto Label_0442;
                }
                case 4:
                    this.<>f__this.m_TheKeySelectionGlow.GetComponent<Renderer>().enabled = false;
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0442:
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
    public class ArenaKeyVisualData
    {
        public Texture m_EffectGlowTexture;
        public string m_IdleEffectsPrefabName;
        public Mesh m_KeyHoleGlowMesh;
        public Material m_Material;
        public Mesh m_Mesh;
        public GameObject m_ParticlePrefab;
        public Texture m_SelectionGlowTexture;
    }
}

