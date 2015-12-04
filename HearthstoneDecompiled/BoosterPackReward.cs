using System;
using System.Collections;
using UnityEngine;

[CustomEditClass]
public class BoosterPackReward : Reward
{
    public GameObject m_BoosterPackBone;
    public GameLayer m_Layer = GameLayer.IgnoreFullScreenEffects;
    private UnopenedPack m_unopenedPack;

    protected override void HideReward()
    {
        base.HideReward();
        base.m_root.SetActive(false);
    }

    protected override void InitData()
    {
        base.SetData(new BoosterPackRewardData(), false);
    }

    protected override void OnDataSet(bool updateVisuals)
    {
        if (updateVisuals)
        {
            this.m_BoosterPackBone.gameObject.SetActive(false);
            BoosterPackRewardData data = base.Data as BoosterPackRewardData;
            string headline = string.Empty;
            string details = string.Empty;
            string source = string.Empty;
            if (base.Data.Origin == NetCache.ProfileNotice.NoticeOrigin.OUT_OF_BAND_LICENSE)
            {
                headline = GameStrings.Get("GLOBAL_REWARD_BOOSTER_HEADLINE_OUT_OF_BAND");
                object[] args = new object[] { data.Count };
                source = GameStrings.Format("GLOBAL_REWARD_BOOSTER_DETAILS_OUT_OF_BAND", args);
            }
            else if (data.Count <= 1)
            {
                string key = "GLOBAL_REWARD_BOOSTER_HEADLINE_GENERIC";
                headline = GameStrings.Get(key);
            }
            else
            {
                object[] objArray2 = new object[] { data.Count };
                headline = GameStrings.Format("GLOBAL_REWARD_BOOSTER_HEADLINE_MULTIPLE", objArray2);
            }
            base.SetRewardText(headline, details, source);
            DbfRecord record = GameDbf.Booster.GetRecord(data.Id);
            if (record != null)
            {
                base.SetReady(false);
                string assetName = record.GetAssetName("PACK_OPENING_PREFAB");
                AssetLoader.Get().LoadActor(assetName, new AssetLoader.GameObjectCallback(this.OnUnopenedPackPrefabLoaded), null, false);
            }
        }
    }

    private void OnUnopenedPackPrefabLoaded(string name, GameObject go, object callbackData)
    {
        go.transform.parent = this.m_BoosterPackBone.transform.parent;
        go.transform.position = this.m_BoosterPackBone.transform.position;
        go.transform.rotation = this.m_BoosterPackBone.transform.rotation;
        go.transform.localScale = this.m_BoosterPackBone.transform.localScale;
        this.m_unopenedPack = go.GetComponent<UnopenedPack>();
        this.UpdatePackStacks();
        base.SetReady(true);
    }

    protected override void ShowReward(bool updateCacheValues)
    {
        base.m_root.SetActive(true);
        SceneUtils.SetLayer(base.m_root, this.m_Layer);
        Vector3 localScale = this.m_unopenedPack.transform.localScale;
        this.m_unopenedPack.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        object[] args = new object[] { "scale", localScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic };
        iTween.ScaleTo(this.m_unopenedPack.gameObject, iTween.Hash(args));
        this.m_unopenedPack.transform.localEulerAngles = new Vector3(this.m_unopenedPack.transform.localEulerAngles.x, this.m_unopenedPack.transform.localEulerAngles.y, 180f);
        object[] objArray2 = new object[] { "amount", new Vector3(0f, 0f, 540f), "time", 1.5f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self };
        Hashtable hashtable = iTween.Hash(objArray2);
        iTween.RotateAdd(this.m_unopenedPack.gameObject, hashtable);
    }

    private void UpdatePackStacks()
    {
        BoosterPackRewardData data = base.Data as BoosterPackRewardData;
        if (data == null)
        {
            Debug.LogWarning(string.Format("BoosterPackReward.UpdatePackStacks() - Data {0} is not CardRewardData", base.Data));
        }
        else
        {
            bool flag = data.Count > 1;
            this.m_unopenedPack.m_SingleStack.m_RootObject.SetActive(!flag);
            this.m_unopenedPack.m_MultipleStack.m_RootObject.SetActive(flag);
            this.m_unopenedPack.m_MultipleStack.m_AmountText.enabled = flag;
            if (flag)
            {
                this.m_unopenedPack.m_MultipleStack.m_AmountText.Text = data.Count.ToString();
            }
        }
    }
}

