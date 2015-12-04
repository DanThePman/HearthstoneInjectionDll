using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HistoryCard : HistoryItem
{
    private const float ABILITY_CARD_ANIMATE_TO_BIG_CARD_AREA_TIME = 1f;
    private const float BIG_CARD_SCALE = 1.03f;
    private const string CREATED_BY_BONE_NAME = "HistoryCreatedByBone";
    private bool finishedCallbackHasRun;
    private HistoryManager.FinishedCallback finishedLoadingCallback;
    private readonly Color FRIENDLY_COLOR = new Color(0.6509f, 0.6705f, 0.9843f, 1f);
    private bool halfSize;
    private bool hasBeenShown;
    private List<HistoryChildCard> historyChildren;
    public UberText m_createdByText;
    private Material m_fullHistory;
    private bool m_gameEntityMousedOver;
    private Material m_halfHistory;
    private bool m_haveDisplayedCreator;
    private Texture m_HeroPowerFrontTex;
    private List<HistoryManager.HistoryCallbackData> m_loadChildrenInfo;
    private int m_numChildrenToLoad;
    public Actor m_tileActor;
    private const float MAX_WIDTH_OF_CHILDREN = 5f;
    private const float MOUSE_OVER_HEIGHT_OFFSET = 7.524521f;
    private PlatformDependentValue<float> MOUSE_OVER_SCALE;
    private PlatformDependentValue<float> MOUSE_OVER_X_OFFSET;
    private const float MOUSE_OVER_Z_OFFSET_BOTTOM = 0.1681719f;
    private const float MOUSE_OVER_Z_OFFSET_PHONE = -4.75f;
    private const float MOUSE_OVER_Z_OFFSET_SECRET_PHONE = -4.3f;
    private const float MOUSE_OVER_Z_OFFSET_TOP = -1.404475f;
    private bool mousedOver;
    private readonly Color OPPONENT_COLOR = new Color(0.7137f, 0.2f, 0.1333f, 1f);
    private bool secretFinished;
    private Actor seperator;
    private bool seperatorStartedLoading;
    private const float TIME_TO_WAIT_BEFORE_RUNNING_SPELL_EFFECTS = 1f;
    private bool waitingForSecretToFinish;
    private bool wasCountered;
    private PlatformDependentValue<float> X_SIZE_OF_MOUSE_OVER_CHILD;

    public HistoryCard()
    {
        PlatformDependentValue<float> value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 4.326718f,
            Tablet = 4.7f,
            Phone = 5.4f
        };
        this.MOUSE_OVER_X_OFFSET = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 1f,
            Tablet = 1.35f,
            Phone = 1.35f
        };
        this.MOUSE_OVER_SCALE = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 2.5f,
            Tablet = 2.5f,
            Phone = 2.5f
        };
        this.X_SIZE_OF_MOUSE_OVER_CHILD = value2;
    }

    public void AddHistoryChild(HistoryChildCard childCard)
    {
        this.historyChildren.Add(childCard);
        if ((this.seperator == null) && !this.seperatorStartedLoading)
        {
            this.LoadArrow();
        }
    }

    public void AssignMaterials(CardDef cardDef)
    {
        this.m_fullHistory = cardDef.GetHistoryTileFullPortrait();
        this.m_halfHistory = cardDef.GetHistoryTileHalfPortrait();
    }

    private void Awake()
    {
        this.historyChildren = new List<HistoryChildCard>();
    }

    private bool ChildrenLoaded()
    {
        if (this.historyChildren.Count < this.m_numChildrenToLoad)
        {
            return false;
        }
        for (int i = 0; i < this.historyChildren.Count; i++)
        {
            if (this.historyChildren[i].m_mainCardActor == null)
            {
                return false;
            }
        }
        return true;
    }

    public bool GetHalfSize()
    {
        return this.halfSize;
    }

    private Vector3 GetHistoryTokenScale()
    {
        return HistoryManager.Get().transform.localScale;
    }

    public Collider GetTileCollider()
    {
        if (this.m_tileActor == null)
        {
            return null;
        }
        if (this.m_tileActor.GetMeshRenderer() == null)
        {
            return null;
        }
        Transform transform = this.m_tileActor.GetMeshRenderer().transform.FindChild("Collider");
        if (transform == null)
        {
            return null;
        }
        return transform.GetComponent<Collider>();
    }

    private float GetZOffsetForThisTilesMouseOverCard()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            if (base.m_entity.IsSecret() && base.m_entity.IsHidden())
            {
                return -4.3f;
            }
            return -4.75f;
        }
        float num = Mathf.Abs((float) -1.572647f);
        HistoryManager manager = HistoryManager.Get();
        float num2 = num / ((float) manager.GetNumHistoryTiles());
        int num3 = (manager.GetNumHistoryTiles() - manager.GetIndexForTile(this)) - 1;
        return (-1.404475f + (num2 * num3));
    }

    public bool HasBeenShown()
    {
        return this.hasBeenShown;
    }

    private void InitDisplayedCreator()
    {
        if (base.m_entity != null)
        {
            Entity displayedCreator = base.m_entity.GetDisplayedCreator();
            if (displayedCreator != null)
            {
                GameObject relative = base.m_mainCardActor.FindBone("HistoryCreatedByBone");
                if (relative == null)
                {
                    object[] messageArgs = new object[] { "HistoryCreatedByBone", base.m_mainCardActor };
                    Error.AddDevWarning("Missing Bone", "Missing {0} on {1}", messageArgs);
                }
                else
                {
                    string name = displayedCreator.GetName();
                    object[] args = new object[] { name };
                    this.m_createdByText.Text = GameStrings.Format("GAMEPLAY_HISTORY_CREATED_BY", args);
                    this.m_createdByText.transform.parent = base.m_mainCardActor.GetRootObject().transform;
                    this.m_createdByText.gameObject.SetActive(true);
                    TransformUtil.SetPoint(this.m_createdByText, new Vector3(0.5f, 0f, 1f), relative, new Vector3(0.5f, 0f, 0f));
                    this.m_createdByText.gameObject.SetActive(false);
                    this.m_haveDisplayedCreator = true;
                }
            }
        }
    }

    private void LoadArrow()
    {
        this.seperatorStartedLoading = true;
        AssetLoader.Get().LoadActor("History_Arrow", new AssetLoader.GameObjectCallback(this.LoadSeparatorCallback), null, false);
    }

    public void LoadAttackTileActor()
    {
        this.halfSize = true;
        AssetLoader.Get().LoadActor("HistoryTile_Attack", new AssetLoader.GameObjectCallback(this.LoadTileActorCallback), null, false);
        this.LoadCrossedSwords();
    }

    public void LoadBigCardActor(bool andShowBigCard)
    {
        if (base.isFatigue)
        {
            AssetLoader.Get().LoadActor("Card_Hand_Fatigue", new AssetLoader.GameObjectCallback(this.LoadBigCardActorCallback), false, false);
        }
        else
        {
            string historyActor = ActorNames.GetHistoryActor(base.m_entity);
            AssetLoader.Get().LoadActor(historyActor, new AssetLoader.GameObjectCallback(this.LoadBigCardActorCallback), andShowBigCard, false);
        }
    }

    private void LoadBigCardActorCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("HistoryCard.LoadBigCardActorCallback() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("HistoryCard.LoadBigCardActorCallback() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                bool flag = (bool) callbackData;
                base.m_mainCardActor = component;
                if (base.isFatigue)
                {
                    base.m_mainCardActor.GetPowersText().Text = GameStrings.Get("GAMEPLAY_FATIGUE_HISTORY_TEXT");
                }
                else
                {
                    base.m_mainCardActor.SetCardFlair(base.m_entity.GetCardFlair());
                }
                base.m_mainCardActor.SetHistoryCard(this);
                if (!flag)
                {
                    SceneUtils.SetLayer(base.m_mainCardActor, GameLayer.Tooltip);
                }
                this.InitDisplayedCreator();
                HeroSkinHeroPower componentInChildren = base.gameObject.GetComponentInChildren<HeroSkinHeroPower>();
                if (componentInChildren != null)
                {
                    componentInChildren.SetFrontTexture(this.m_HeroPowerFrontTex);
                }
                if (flag)
                {
                    HistoryManager.Get().SetBigCard(this, this.waitingForSecretToFinish);
                    base.StartCoroutine(this.WaitForSecretIfNecessary());
                    base.m_mainCardActor.UpdateAllComponents();
                }
                else
                {
                    base.m_mainCardActor.Hide();
                    base.StartCoroutine(this.PositionWhenChildrenReady());
                }
            }
        }
    }

    public void LoadCorrectTileActor(HistoryInfo sourceCard)
    {
        switch (sourceCard.m_infoType)
        {
            case HistoryInfoType.NONE:
            case HistoryInfoType.WEAPON_PLAYED:
            case HistoryInfoType.CARD_PLAYED:
            case HistoryInfoType.FATIGUE:
                this.LoadTileActor();
                break;

            case HistoryInfoType.ATTACK:
                this.LoadAttackTileActor();
                break;

            case HistoryInfoType.TRIGGER:
                this.LoadTriggerTileActor();
                break;

            case HistoryInfoType.WEAPON_BREAK:
                this.LoadWeaponBreakActor();
                break;
        }
    }

    private void LoadCrossedSwords()
    {
        this.seperatorStartedLoading = true;
        AssetLoader.Get().LoadActor("History_Swords", new AssetLoader.GameObjectCallback(this.LoadSeparatorCallback), null, false);
    }

    private void LoadSeparatorCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("HistoryCard.LoadSeparatorCallback() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("HistoryCard.LoadSeparatorCallback() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                this.seperator = component;
                MeshRenderer renderer = this.seperator.GetRootObject().transform.FindChild("Blue").gameObject.GetComponent<MeshRenderer>();
                MeshRenderer renderer2 = this.seperator.GetRootObject().transform.FindChild("Red").gameObject.GetComponent<MeshRenderer>();
                if (base.isFatigue)
                {
                    renderer2.enabled = true;
                    renderer.enabled = false;
                }
                else
                {
                    bool flag = base.m_entity.IsControlledByFriendlySidePlayer();
                    renderer.enabled = flag;
                    renderer2.enabled = !flag;
                }
                this.seperator.transform.parent = base.transform;
                TransformUtil.Identity(this.seperator.transform);
                if (this.seperator.GetRootObject() != null)
                {
                    TransformUtil.Identity(this.seperator.GetRootObject().transform);
                }
                this.seperator.Hide();
            }
        }
    }

    public void LoadTileActor()
    {
        this.halfSize = false;
        AssetLoader.Get().LoadActor("HistoryTile_Card", new AssetLoader.GameObjectCallback(this.LoadTileActorCallback), null, false);
        if (this.historyChildren.Count > 0)
        {
            this.LoadArrow();
        }
    }

    private void LoadTileActorCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("HistoryCard.LoadTileActorCallback() - FAILED to load actor \"{0}\"", actorName));
            HistoryManager.Get().OnHistoryTileComplete();
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("HistoryCard.LoadTileActorCallback() - ERROR actor \"{0}\" has no Actor component", actorName));
                HistoryManager.Get().OnHistoryTileComplete();
            }
            else
            {
                this.m_tileActor = component;
                this.m_tileActor.transform.parent = base.transform;
                TransformUtil.Identity(this.m_tileActor.transform);
                this.m_tileActor.transform.localScale = this.GetHistoryTokenScale();
                Material[] materialArray = new Material[2];
                materialArray[0] = this.m_tileActor.GetMeshRenderer().materials[0];
                if (this.halfSize)
                {
                    if (this.m_halfHistory != null)
                    {
                        materialArray[1] = this.m_halfHistory;
                        this.m_tileActor.GetMeshRenderer().materials = materialArray;
                    }
                    else
                    {
                        this.m_tileActor.GetMeshRenderer().materials[1].mainTexture = base.m_bigCardPortraitTexture;
                    }
                }
                else if (this.m_fullHistory != null)
                {
                    materialArray[1] = this.m_fullHistory;
                    this.m_tileActor.GetMeshRenderer().materials = materialArray;
                }
                else
                {
                    this.m_tileActor.GetMeshRenderer().materials[1].mainTexture = base.m_bigCardPortraitTexture;
                }
                Color white = Color.white;
                if (Board.Get() != null)
                {
                    white = Board.Get().m_HistoryTileColor;
                }
                if (!base.isFatigue)
                {
                    if (base.m_entity.IsControlledByFriendlySidePlayer())
                    {
                        white *= this.FRIENDLY_COLOR;
                    }
                    else
                    {
                        white *= this.OPPONENT_COLOR;
                    }
                }
                foreach (Renderer renderer in this.m_tileActor.GetMeshRenderer().GetComponentsInChildren<Renderer>())
                {
                    if (renderer.tag != "FakeShadow")
                    {
                        renderer.material.color = Board.Get().m_HistoryTileColor;
                    }
                }
                this.m_tileActor.GetMeshRenderer().materials[0].color = white;
                this.m_tileActor.GetMeshRenderer().materials[1].color = Board.Get().m_HistoryTileColor;
                if ((this.GetTileCollider() != null) && !this.GetHalfSize())
                {
                    HistoryManager.Get().SetBigTileSize(this.GetTileCollider().bounds.size.z);
                }
                HistoryManager.Get().SetAsideTileAndTryToUpdate(this);
                HistoryManager.Get().OnHistoryTileComplete();
            }
        }
    }

    private void LoadTriggerTileActor()
    {
        this.halfSize = true;
        AssetLoader.Get().LoadActor("HistoryTile_Trigger", new AssetLoader.GameObjectCallback(this.LoadTileActorCallback), null, false);
        if (this.historyChildren.Count > 0)
        {
            this.LoadArrow();
        }
    }

    public void LoadWeaponBreakActor()
    {
        this.halfSize = true;
        AssetLoader.Get().LoadActor("HistoryTile_Attack", new AssetLoader.GameObjectCallback(this.LoadTileActorCallback), null, false);
    }

    public void MarkAsShown()
    {
        if (!this.hasBeenShown)
        {
            this.hasBeenShown = true;
        }
    }

    public void NotifyMousedOut()
    {
        if (this.mousedOver)
        {
            this.mousedOver = false;
            if (this.m_gameEntityMousedOver)
            {
                GameState.Get().GetGameEntity().NotifyOfHistoryTokenMousedOut();
                this.m_gameEntityMousedOver = false;
            }
            KeywordHelpPanelManager.Get().HideKeywordHelp();
            if (base.m_mainCardActor != null)
            {
                base.m_mainCardActor.DeactivateAllSpells();
                base.m_mainCardActor.Hide();
            }
            for (int i = 0; i < this.historyChildren.Count; i++)
            {
                if (this.historyChildren[i].m_mainCardActor != null)
                {
                    this.historyChildren[i].m_mainCardActor.DeactivateAllSpells();
                    this.historyChildren[i].m_mainCardActor.Hide();
                }
            }
            if (this.seperator != null)
            {
                this.seperator.Hide();
            }
            HistoryManager.Get().UpdateLayout();
        }
    }

    public void NotifyMousedOver()
    {
        if (!this.mousedOver && (this != HistoryManager.Get().GetCurrentBigCard()))
        {
            if (this.m_loadChildrenInfo != null)
            {
                HistoryManager.Get().LoadChildren(this.m_loadChildrenInfo);
                this.m_numChildrenToLoad = this.m_loadChildrenInfo.Count;
                this.m_loadChildrenInfo = null;
            }
            this.mousedOver = true;
            SoundManager.Get().LoadAndPlay("history_event_mouseover", this.m_tileActor.gameObject);
            if (base.m_mainCardActor != null)
            {
                base.StartCoroutine(this.PositionWhenChildrenReady());
            }
            else
            {
                this.LoadBigCardActor(false);
            }
        }
    }

    public void NotifyOfSecretFinished()
    {
        this.secretFinished = true;
    }

    private void OnPathComplete()
    {
        this.ShowDisplayedCreator();
    }

    private void PositionMouseOverCard()
    {
        if (!this.mousedOver)
        {
            base.m_mainCardActor.Hide();
        }
        else
        {
            base.m_mainCardActor.Show();
            this.ShowDisplayedCreator();
            if (!base.IsInitialized())
            {
                base.InitializeActor();
            }
            base.DisplaySpells();
            if (UniversalInputManager.UsePhoneUI != null)
            {
                base.m_mainCardActor.transform.position = new Vector3(base.transform.position.x + this.MOUSE_OVER_X_OFFSET, base.transform.position.y + 7.524521f, this.GetZOffsetForThisTilesMouseOverCard());
            }
            else
            {
                base.m_mainCardActor.transform.position = new Vector3(base.transform.position.x + this.MOUSE_OVER_X_OFFSET, base.transform.position.y + 7.524521f, base.transform.position.z + this.GetZOffsetForThisTilesMouseOverCard());
            }
            base.m_mainCardActor.transform.localScale = new Vector3((float) this.MOUSE_OVER_SCALE, 1f, (float) this.MOUSE_OVER_SCALE);
            if (!this.m_gameEntityMousedOver)
            {
                this.m_gameEntityMousedOver = true;
                GameState.Get().GetGameEntity().NotifyOfHistoryTokenMousedOver(base.gameObject);
            }
            if (!base.isFatigue)
            {
                KeywordHelpPanelManager.Get().UpdateKeywordHelpForHistoryCard(base.m_entity, base.m_mainCardActor, this.m_createdByText);
            }
            if (this.historyChildren.Count > 0)
            {
                float max = 1f;
                float num2 = 1f;
                if ((this.historyChildren.Count > 4) && (this.historyChildren.Count < 9))
                {
                    num2 = 2f;
                    max = 0.5f;
                }
                else if (this.historyChildren.Count >= 9)
                {
                    num2 = 3f;
                    max = 0.3f;
                }
                int num3 = Mathf.CeilToInt(((float) this.historyChildren.Count) / num2);
                float num4 = num3 * this.X_SIZE_OF_MOUSE_OVER_CHILD;
                float num5 = 5f / num4;
                num5 = Mathf.Clamp(num5, 0.1f, max);
                int num6 = 0;
                int num7 = 1;
                for (int i = 0; i < this.historyChildren.Count; i++)
                {
                    float num10;
                    this.historyChildren[i].m_mainCardActor.Show();
                    if (!this.historyChildren[i].IsInitialized())
                    {
                        this.historyChildren[i].InitializeActor();
                    }
                    this.historyChildren[i].DisplaySpells();
                    float z = base.m_mainCardActor.transform.position.z;
                    switch (num2)
                    {
                        case 2f:
                            if (num7 == 1)
                            {
                                z += 0.78f;
                            }
                            else
                            {
                                z -= 0.78f;
                            }
                            break;

                        case 3f:
                            switch (num7)
                            {
                                case 1:
                                    z += 0.98f;
                                    goto Label_031B;

                                case 3:
                                    z -= 0.93f;
                                    goto Label_031B;
                            }
                            break;
                    }
                Label_031B:
                    num10 = base.m_mainCardActor.transform.position.x + ((this.X_SIZE_OF_MOUSE_OVER_CHILD * (1f + num5)) / 2f);
                    this.historyChildren[i].m_mainCardActor.transform.position = new Vector3(num10 + ((this.X_SIZE_OF_MOUSE_OVER_CHILD * num6) * num5), base.m_mainCardActor.transform.position.y, z);
                    this.historyChildren[i].m_mainCardActor.transform.localScale = new Vector3(num5, num5, num5);
                    num6++;
                    if (num6 >= num3)
                    {
                        num6 = 0;
                        num7++;
                    }
                }
                if (this.seperator != null)
                {
                    float num11 = 0.4f;
                    float num12 = this.X_SIZE_OF_MOUSE_OVER_CHILD / 2f;
                    this.seperator.Show();
                    this.seperator.transform.position = new Vector3(base.m_mainCardActor.transform.position.x + num12, base.m_mainCardActor.transform.position.y + num11, base.m_mainCardActor.transform.position.z);
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator PositionWhenChildrenReady()
    {
        return new <PositionWhenChildrenReady>c__Iterator94 { <>f__this = this };
    }

    public void RunCallbackNow()
    {
        if (!this.finishedCallbackHasRun)
        {
            this.finishedCallbackHasRun = true;
            this.finishedLoadingCallback();
        }
    }

    public void SetCardInfo(Entity entity, Texture bigTexture, int splatAmount, bool isDead, HistoryManager.FinishedCallback callbackFunc, bool counterspelled, bool waitForSecret, Material goldenMat)
    {
        base.m_entity = entity;
        base.m_bigCardPortraitTexture = bigTexture;
        base.m_bigCardPotraitGoldenMaterial = goldenMat;
        base.m_splatAmount = splatAmount;
        base.dead = isDead;
        this.finishedLoadingCallback = callbackFunc;
        this.wasCountered = counterspelled;
        this.waitingForSecretToFinish = waitForSecret;
    }

    public void SetFatigue(Texture bigTexture)
    {
        base.m_bigCardPortraitTexture = bigTexture;
        base.isFatigue = true;
    }

    public void SetHeroPowerFrontTexture(Texture frontTex)
    {
        this.m_HeroPowerFrontTex = frontTex;
    }

    public void SetLoadChildrenInfo(List<HistoryManager.HistoryCallbackData> callbacks)
    {
        this.m_loadChildrenInfo = callbacks;
    }

    private void ShowDisplayedCreator()
    {
        this.m_createdByText.gameObject.SetActive(this.m_haveDisplayedCreator);
    }

    [DebuggerHidden]
    private IEnumerator WaitAndThenContinueWithGameEvents()
    {
        return new <WaitAndThenContinueWithGameEvents>c__Iterator96 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForSecretIfNecessary()
    {
        return new <WaitForSecretIfNecessary>c__Iterator95 { <>f__this = this };
    }

    public bool WasCountered()
    {
        return this.wasCountered;
    }

    [CompilerGenerated]
    private sealed class <PositionWhenChildrenReady>c__Iterator94 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HistoryCard <>f__this;

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
                    if (!this.<>f__this.ChildrenLoaded())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.m_numChildrenToLoad = 0;
                    this.<>f__this.PositionMouseOverCard();
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
    private sealed class <WaitAndThenContinueWithGameEvents>c__Iterator96 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HistoryCard <>f__this;

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
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.RunCallbackNow();
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
    private sealed class <WaitForSecretIfNecessary>c__Iterator95 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HistoryCard <>f__this;
        internal Hashtable <moveArgs>__1;
        internal Vector3[] <pathToFollow>__0;

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
                    if (!this.<>f__this.waitingForSecretToFinish)
                    {
                        goto Label_008C;
                    }
                    this.<>f__this.gameObject.transform.localScale = new Vector3(1E-05f, 1E-05f, 1E-05f);
                    break;

                case 1:
                    break;

                default:
                    goto Label_0206;
            }
            if (!this.<>f__this.secretFinished)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            HistoryManager.Get().StartBigCardTimer();
        Label_008C:
            this.<>f__this.m_mainCardActor.transform.localScale = new Vector3(1.03f, 1.03f, 1.03f);
            if (this.<>f__this.m_entity != null)
            {
                if ((this.<>f__this.m_entity.GetCardType() == TAG_CARDTYPE.SPELL) || (this.<>f__this.m_entity.GetCardType() == TAG_CARDTYPE.HERO_POWER))
                {
                    this.<pathToFollow>__0 = HistoryManager.Get().GetBigCardPath();
                    this.<pathToFollow>__0[0] = this.<>f__this.m_mainCardActor.transform.position;
                    object[] args = new object[] { "path", this.<pathToFollow>__0, "time", 1f, "oncomplete", "OnPathComplete", "oncompletetarget", this.<>f__this.gameObject };
                    this.<moveArgs>__1 = iTween.Hash(args);
                    iTween.MoveTo(this.<>f__this.m_mainCardActor.gameObject, this.<moveArgs>__1);
                    iTween.ScaleTo(this.<>f__this.gameObject, new Vector3(1f, 1f, 1f), 1f);
                    SoundManager.Get().LoadAndPlay("play_card_from_hand_1");
                }
                else
                {
                    this.<>f__this.ShowDisplayedCreator();
                }
            }
            this.<>f__this.StartCoroutine(this.<>f__this.WaitAndThenContinueWithGameEvents());
            this.$PC = -1;
        Label_0206:
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

