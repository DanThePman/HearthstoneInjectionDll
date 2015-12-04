using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class RewardBoxesDisplay : MonoBehaviour
{
    public const string DEFAULT_PREFAB = "RewardBoxes";
    private bool m_addRewardsToCacheValues = true;
    public GameObject m_ClickCatcher;
    private List<Achievement> m_completeAchievesToDisplay = new List<Achievement>();
    private bool m_destroyed;
    [CustomEditField(Sections="Reward Panel")]
    public NormalButton m_DoneButton;
    private List<System.Action> m_doneCallbacks;
    private List<GameObject> m_InstancedObjects;
    private GameLayer m_layer = GameLayer.IgnoreFullScreenEffects;
    public bool m_playBoxFlyoutSound = true;
    private GameObject[] m_RewardObjects;
    private List<RewardPackageData> m_RewardPackages;
    private List<RewardData> m_Rewards;
    public RewardSet m_RewardSets;
    public GameObject m_Root;
    private bool m_useDarkeningClickCatcher;
    private static RewardBoxesDisplay s_Instance;

    private void AddRewardsToCacheValues()
    {
        bool flag = false;
        foreach (RewardData data in this.m_Rewards)
        {
            switch (data.RewardType)
            {
                case Reward.Type.ARCANE_DUST:
                {
                    ArcaneDustRewardData data2 = (ArcaneDustRewardData) data;
                    NetCache.Get().OnArcaneDustBalanceChanged((long) data2.Amount);
                    break;
                }
                case Reward.Type.CARD:
                {
                    CardRewardData data3 = (CardRewardData) data;
                    CollectionManager.Get().OnCardRewardOpened(data3.CardID, data3.Premium, data3.Count);
                    break;
                }
                case Reward.Type.GOLD:
                    flag = true;
                    break;
            }
        }
        if (flag)
        {
            NetCache.Get().RefreshNetObject<NetCache.NetCacheGoldBalance>();
        }
    }

    private void AllDone()
    {
        Vector3 zero = Vector3.zero;
        for (int i = 0; i < this.m_RewardPackages.Count; i++)
        {
            RewardPackageData data = this.m_RewardPackages[i];
            zero += data.m_TargetBone.position;
        }
        this.m_DoneButton.transform.position = (Vector3) (zero / ((float) this.m_RewardPackages.Count));
        this.m_DoneButton.gameObject.SetActive(true);
        this.m_DoneButton.SetText(GameStrings.Get("GLOBAL_DONE"));
        Spell component = this.m_DoneButton.m_button.GetComponent<Spell>();
        component.AddFinishedCallback(new Spell.FinishedCallback(this.OnDoneButtonShown));
        component.ActivateState(SpellStateType.BIRTH);
    }

    public void AnimateRewards()
    {
        int count = this.m_Rewards.Count;
        this.m_RewardPackages = this.GetPackageData(count);
        this.m_RewardObjects = new GameObject[count];
        for (int i = 0; i < this.m_RewardPackages.Count; i++)
        {
            RewardPackageData data = this.m_RewardPackages[i];
            if (data.m_TargetBone == null)
            {
                UnityEngine.Debug.LogWarning("RewardBoxesDisplay: AnimateRewards package target bone is null!");
                return;
            }
            if (i >= this.m_RewardObjects.Length)
            {
                UnityEngine.Debug.LogWarning("RewardBoxesDisplay: AnimateRewards reward index exceeded!");
                return;
            }
            this.m_RewardObjects[i] = this.CreateRewardInstance(i, data.m_TargetBone.position, false);
        }
        this.RewardPackageAnimation();
    }

    private void Awake()
    {
        s_Instance = this;
        this.m_addRewardsToCacheValues = !Login.IsLoginSceneActive();
        this.m_InstancedObjects = new List<GameObject>();
        this.m_doneCallbacks = new List<System.Action>();
        CollectionManager.Get().RegisterAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
        RenderUtils.SetAlpha(this.m_ClickCatcher, 0f);
    }

    private bool CheckAllRewardsActive()
    {
        foreach (GameObject obj2 in this.m_RewardObjects)
        {
            if (!obj2.activeSelf)
            {
                return false;
            }
        }
        return true;
    }

    private void CleanUp()
    {
        foreach (GameObject obj2 in this.m_InstancedObjects)
        {
            if (obj2 != null)
            {
                UnityEngine.Object.Destroy(obj2);
            }
        }
        this.FadeFullscreenEffectsOut();
        s_Instance = null;
    }

    private GameObject CreateRewardInstance(int rewardIndex, Vector3 rewardPos, bool activeOnStart)
    {
        RewardData data = this.m_Rewards[rewardIndex];
        GameObject go = null;
        switch (data.RewardType)
        {
            case Reward.Type.ARCANE_DUST:
            {
                go = UnityEngine.Object.Instantiate<GameObject>(this.m_RewardSets.m_RewardDust);
                TransformUtil.AttachAndPreserveLocalTransform(go.transform, this.m_Root.transform);
                go.transform.position = rewardPos;
                go.SetActive(true);
                UberText componentInChildren = go.GetComponentInChildren<UberText>();
                ArcaneDustRewardData data2 = (ArcaneDustRewardData) data;
                componentInChildren.Text = data2.Amount.ToString();
                go.SetActive(activeOnStart);
                break;
            }
            case Reward.Type.BOOSTER_PACK:
            {
                BoosterPackRewardData data3 = data as BoosterPackRewardData;
                int id = data3.Id;
                if (id == 0)
                {
                    id = 1;
                    UnityEngine.Debug.LogWarning("RewardBoxesDisplay - booster reward is not valid. ID = 0");
                }
                Log.Kyle.Print(string.Format("Booster DB ID: {0}", id), new object[0]);
                string assetName = GameDbf.Booster.GetRecord(id).GetAssetName("ARENA_PREFAB");
                if (string.IsNullOrEmpty(assetName))
                {
                    UnityEngine.Debug.LogError(string.Format("RewardBoxesDisplay - no prefab found for booster {0}!", data3.Id));
                }
                else
                {
                    go = AssetLoader.Get().LoadGameObject(assetName, true, false);
                    TransformUtil.AttachAndPreserveLocalTransform(go.transform, this.m_Root.transform);
                    go.transform.position = rewardPos;
                    go.SetActive(activeOnStart);
                }
                break;
            }
            case Reward.Type.CARD:
            {
                go = UnityEngine.Object.Instantiate<GameObject>(this.m_RewardSets.m_RewardCard);
                TransformUtil.AttachAndPreserveLocalTransform(go.transform, this.m_Root.transform);
                go.transform.position = rewardPos;
                go.SetActive(true);
                CardRewardData cardData = (CardRewardData) data;
                go.GetComponentInChildren<RewardCard>().LoadCard(cardData, this.m_layer);
                go.SetActive(activeOnStart);
                break;
            }
            case Reward.Type.CARD_BACK:
            {
                go = UnityEngine.Object.Instantiate<GameObject>(this.m_RewardSets.m_RewardCardBack);
                TransformUtil.AttachAndPreserveLocalTransform(go.transform, this.m_Root.transform);
                go.transform.position = rewardPos;
                go.SetActive(true);
                CardBackRewardData cardbackData = (CardBackRewardData) data;
                go.GetComponentInChildren<RewardCardBack>().LoadCardBack(cardbackData, this.m_layer);
                go.SetActive(activeOnStart);
                break;
            }
            case Reward.Type.GOLD:
            {
                go = UnityEngine.Object.Instantiate<GameObject>(this.m_RewardSets.m_RewardGold);
                TransformUtil.AttachAndPreserveLocalTransform(go.transform, this.m_Root.transform);
                go.transform.position = rewardPos;
                go.SetActive(true);
                UberText text2 = go.GetComponentInChildren<UberText>();
                GoldRewardData data4 = (GoldRewardData) data;
                text2.Text = data4.Amount.ToString();
                go.SetActive(activeOnStart);
                break;
            }
        }
        if (go == null)
        {
            UnityEngine.Debug.LogWarning("RewardBoxesDisplay: Unable to create reward, object null!");
            return null;
        }
        if (rewardIndex >= this.m_RewardObjects.Length)
        {
            UnityEngine.Debug.LogWarning("RewardBoxesDisplay: CreateRewardInstance reward index exceeded!");
            return null;
        }
        SceneUtils.SetLayer(go, this.m_layer);
        this.m_RewardObjects[rewardIndex] = go;
        this.m_InstancedObjects.Add(go);
        return go;
    }

    public void DebugLogRewards()
    {
        UnityEngine.Debug.Log("BOX REWARDS:");
        for (int i = 0; i < this.m_Rewards.Count; i++)
        {
            RewardData data = this.m_Rewards[i];
            UnityEngine.Debug.Log(string.Format("  reward {0}={1}", i, data));
        }
    }

    private void FadeFullscreenEffectsIn()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr == null)
        {
            UnityEngine.Debug.LogWarning("RewardBoxesDisplay: FullScreenFXMgr.Get() returned null!");
        }
        else
        {
            mgr.SetBlurBrightness(0.85f);
            mgr.SetBlurDesaturation(0f);
            mgr.Vignette(0.4f, 0.5f, iTween.EaseType.easeOutCirc, null);
            mgr.Blur(1f, 0.5f, iTween.EaseType.easeOutCirc, null);
            if (this.m_useDarkeningClickCatcher)
            {
                iTween.FadeTo(this.m_ClickCatcher, 0.75f, 0.5f);
            }
        }
    }

    private void FadeFullscreenEffectsOut()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr == null)
        {
            UnityEngine.Debug.LogWarning("RewardBoxesDisplay: FullScreenFXMgr.Get() returned null!");
        }
        else
        {
            mgr.StopVignette(2f, iTween.EaseType.easeOutCirc, new FullScreenFXMgr.EffectListener(this.FadeFullscreenEffectsOutFinished));
            mgr.StopBlur(2f, iTween.EaseType.easeOutCirc, null);
            if (this.m_useDarkeningClickCatcher)
            {
                iTween.FadeTo(this.m_ClickCatcher, 0f, 0.5f);
            }
        }
    }

    private void FadeFullscreenEffectsOutFinished()
    {
        foreach (System.Action action in this.m_doneCallbacks)
        {
            action();
        }
        this.m_doneCallbacks.Clear();
        if (!this.m_destroyed)
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }

    private void FadeVignetteIn()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr == null)
        {
            UnityEngine.Debug.LogWarning("RewardBoxesDisplay: FullScreenFXMgr.Get() returned null!");
        }
        else
        {
            mgr.DisableBlur();
            mgr.Vignette(1.4f, 1.5f, iTween.EaseType.easeOutCirc, null);
        }
    }

    public static RewardBoxesDisplay Get()
    {
        return s_Instance;
    }

    public List<RewardPackageData> GetPackageData(int rewardCount)
    {
        for (int i = 0; i < this.m_RewardSets.m_RewardData.Count; i++)
        {
            if (this.m_RewardSets.m_RewardData[i].m_PackageData.Count == rewardCount)
            {
                return this.m_RewardSets.m_RewardData[i].m_PackageData;
            }
        }
        UnityEngine.Debug.LogError("RewardBoxesDisplay: GetPackageData - no package data found with a reward count of " + rewardCount);
        return null;
    }

    private void OnCollectionAchievesCompleted(List<Achievement> achievements)
    {
        this.m_completeAchievesToDisplay.AddRange(achievements);
        this.ShowCompleteAchieve(null);
    }

    private void OnDestroy()
    {
        this.CleanUp();
        this.m_destroyed = true;
    }

    private void OnDisable()
    {
    }

    private void OnDoneButtonHidden(Spell spell, object userData)
    {
        this.FadeFullscreenEffectsOut();
    }

    private void OnDoneButtonPressed(UIEvent e)
    {
        Navigation.GoBack();
    }

    private void OnDoneButtonShown(Spell spell, object userData)
    {
        this.m_DoneButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDoneButtonPressed));
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    private void OnEnable()
    {
    }

    private bool OnNavigateBack()
    {
        UnityEngine.Debug.Log("navigating back!");
        if (!this.m_DoneButton.m_button.activeSelf)
        {
            return false;
        }
        foreach (GameObject obj2 in this.m_RewardObjects)
        {
            if (obj2 != null)
            {
                PlayMakerFSM rfsm = obj2.GetComponent<PlayMakerFSM>();
                if (rfsm != null)
                {
                    rfsm.SendEvent("Death");
                }
                foreach (UberText text in obj2.GetComponentsInChildren<UberText>())
                {
                    object[] args = new object[] { "alpha", 0f, "time", 0.8f, "includechildren", true, "easetype", iTween.EaseType.easeInOutCubic };
                    iTween.FadeTo(text.gameObject, iTween.Hash(args));
                }
                RewardCard componentInChildren = obj2.GetComponentInChildren<RewardCard>();
                if (componentInChildren != null)
                {
                    componentInChildren.Death();
                }
            }
        }
        SceneUtils.EnableColliders(this.m_DoneButton.gameObject, false);
        this.m_DoneButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDoneButtonPressed));
        Spell component = this.m_DoneButton.m_button.GetComponent<Spell>();
        component.AddFinishedCallback(new Spell.FinishedCallback(this.OnDoneButtonHidden));
        component.ActivateState(SpellStateType.DEATH);
        CollectionManager.Get().RemoveAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
        if (this.m_addRewardsToCacheValues)
        {
            this.AddRewardsToCacheValues();
        }
        return true;
    }

    public void OpenReward(int rewardIndex, Vector3 rewardPos)
    {
        if (rewardIndex >= this.m_RewardObjects.Length)
        {
            UnityEngine.Debug.LogWarning("RewardBoxesDisplay: OpenReward reward index exceeded!");
        }
        else
        {
            GameObject obj2 = this.m_RewardObjects[rewardIndex];
            if (obj2 == null)
            {
                UnityEngine.Debug.LogWarning("RewardBoxesDisplay: OpenReward object is null!");
            }
            else
            {
                if (!obj2.activeSelf)
                {
                    obj2.SetActive(true);
                }
                if (this.CheckAllRewardsActive())
                {
                    this.AllDone();
                }
            }
        }
    }

    public void RegisterDoneCallback(System.Action action)
    {
        this.m_doneCallbacks.Add(action);
    }

    [DebuggerHidden]
    private IEnumerator RewardPackageActivate(RewardBoxData boxData)
    {
        return new <RewardPackageActivate>c__Iterator6D { boxData = boxData, <$>boxData = boxData, <>f__this = this };
    }

    private void RewardPackageAnimation()
    {
        if (this.m_RewardSets.m_RewardPackage == null)
        {
            UnityEngine.Debug.LogWarning("RewardBoxesDisplay: missing Reward Package!");
        }
        else
        {
            this.FadeFullscreenEffectsIn();
            for (int i = 0; i < this.m_RewardPackages.Count; i++)
            {
                RewardPackageData data = this.m_RewardPackages[i];
                if ((data.m_TargetBone == null) || (data.m_StartBone == null))
                {
                    UnityEngine.Debug.LogWarning("RewardBoxesDisplay: missing reward target bone!");
                }
                else
                {
                    GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.m_RewardSets.m_RewardPackage);
                    TransformUtil.AttachAndPreserveLocalTransform(item.transform, this.m_Root.transform);
                    item.transform.position = data.m_StartBone.position;
                    item.SetActive(true);
                    this.m_InstancedObjects.Add(item);
                    Vector3 localScale = item.transform.localScale;
                    item.transform.localScale = Vector3.zero;
                    SceneUtils.EnableColliders(item, false);
                    object[] args = new object[] { "scale", localScale, "time", this.m_RewardSets.m_AnimationTime, "delay", data.m_StartDelay, "easetype", iTween.EaseType.linear };
                    iTween.ScaleTo(item, iTween.Hash(args));
                    PlayMakerFSM component = item.GetComponent<PlayMakerFSM>();
                    if (component == null)
                    {
                        UnityEngine.Debug.LogWarning("RewardBoxesDisplay: missing reward Playmaker FSM!");
                    }
                    else
                    {
                        if (!this.m_playBoxFlyoutSound)
                        {
                            component.FsmVariables.FindFsmBool("PlayFlyoutSound").Value = false;
                        }
                        RewardPackage package = item.GetComponent<RewardPackage>();
                        package.m_RewardIndex = i;
                        RewardBoxData data2 = new RewardBoxData {
                            m_GameObject = item,
                            m_RewardPackage = package,
                            m_FSM = component,
                            m_Index = i
                        };
                        object[] objArray2 = new object[] { 
                            "position", data.m_TargetBone.transform.position, "time", this.m_RewardSets.m_AnimationTime, "delay", data.m_StartDelay, "easetype", iTween.EaseType.linear, "onstarttarget", base.gameObject, "onstart", "RewardPackageOnStart", "onstartparams", data2, "oncompletetarget", base.gameObject, 
                            "oncomplete", "RewardPackageOnComplete", "oncompleteparams", data2
                         };
                        iTween.MoveTo(item, iTween.Hash(objArray2));
                    }
                }
            }
        }
    }

    private void RewardPackageOnComplete(RewardBoxData boxData)
    {
        base.StartCoroutine(this.RewardPackageActivate(boxData));
    }

    private void RewardPackageOnStart(RewardBoxData boxData)
    {
        boxData.m_FSM.SendEvent("Birth");
    }

    private void RewardPackagePressed(UIEvent e)
    {
        Log.Kyle.Print("box clicked!", new object[0]);
    }

    public void SetLayer(GameLayer layer)
    {
        this.m_layer = layer;
        SceneUtils.SetLayer(base.gameObject, this.m_layer);
    }

    public void SetRewards(List<RewardData> rewards)
    {
        this.m_Rewards = rewards;
    }

    public void ShowAlreadyOpenedRewards()
    {
        this.m_RewardPackages = this.GetPackageData(this.m_Rewards.Count);
        this.m_RewardObjects = new GameObject[this.m_Rewards.Count];
        this.FadeFullscreenEffectsIn();
        this.ShowOpenedRewards();
        this.AllDone();
    }

    private void ShowCompleteAchieve(object userData)
    {
        if (this.m_completeAchievesToDisplay.Count != 0)
        {
            Achievement quest = this.m_completeAchievesToDisplay[0];
            this.m_completeAchievesToDisplay.RemoveAt(0);
            QuestToast.ShowQuestToast(new QuestToast.DelOnCloseQuestToast(this.ShowCompleteAchieve), true, quest, false);
        }
    }

    public void ShowOpenedRewards()
    {
        for (int i = 0; i < this.m_RewardPackages.Count; i++)
        {
            RewardPackageData data = this.m_RewardPackages[i];
            if (data.m_TargetBone == null)
            {
                UnityEngine.Debug.LogWarning("RewardBoxesDisplay: AnimateRewards package target bone is null!");
                return;
            }
            if (i >= this.m_RewardObjects.Length)
            {
                UnityEngine.Debug.LogWarning("RewardBoxesDisplay: AnimateRewards reward index exceeded!");
                return;
            }
            this.m_RewardObjects[i] = this.CreateRewardInstance(i, data.m_TargetBone.position, true);
        }
    }

    private void Start()
    {
        if (this.m_RewardSets.m_RewardPackage != null)
        {
            this.m_RewardSets.m_RewardPackage.SetActive(false);
        }
    }

    public void UseDarkeningClickCatcher(bool value)
    {
        this.m_useDarkeningClickCatcher = value;
    }

    [CompilerGenerated]
    private sealed class <RewardPackageActivate>c__Iterator6D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal RewardBoxesDisplay.RewardBoxData <$>boxData;
        internal RewardBoxesDisplay <>f__this;
        internal RewardBoxesDisplay.RewardBoxData boxData;

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
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    SceneUtils.EnableColliders(this.boxData.m_GameObject, true);
                    this.boxData.m_RewardPackage.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.<>f__this.RewardPackagePressed));
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

    [Serializable]
    public class BoxRewardData
    {
        public List<RewardBoxesDisplay.RewardPackageData> m_PackageData;
    }

    private enum Events
    {
        GVG_PROMOTION
    }

    public class RewardBoxData
    {
        public PlayMakerFSM m_FSM;
        public GameObject m_GameObject;
        public int m_Index;
        public RewardPackage m_RewardPackage;
    }

    public class RewardCardLoadData
    {
        public CardRewardData m_CardRewardData;
        public EntityDef m_EntityDef;
        public Transform m_ParentTransform;
    }

    [Serializable]
    public class RewardPackageData
    {
        public Transform m_StartBone;
        public float m_StartDelay;
        public Transform m_TargetBone;
    }

    [Serializable]
    public class RewardSet
    {
        public float m_AnimationTime = 1f;
        public GameObject m_RewardCard;
        public GameObject m_RewardCardBack;
        public List<RewardBoxesDisplay.BoxRewardData> m_RewardData;
        public GameObject m_RewardDust;
        public GameObject m_RewardGold;
        public GameObject m_RewardPackage;
    }
}

