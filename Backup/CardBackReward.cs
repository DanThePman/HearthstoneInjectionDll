using System;
using System.Collections;
using UnityEngine;

public class CardBackReward : Reward
{
    public GameObject m_cardbackBone;
    private int m_numCardBacksLoaded;

    protected override void HideReward()
    {
        base.HideReward();
        base.m_root.SetActive(false);
    }

    protected override void InitData()
    {
        base.SetData(new CardBackRewardData(), false);
    }

    private void OnBackCardBackLoaded(CardBackManager.LoadCardBackData cardbackData)
    {
        GameObject gameObject = cardbackData.m_GameObject;
        gameObject.transform.parent = this.m_cardbackBone.transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
        gameObject.transform.localScale = Vector3.one;
        SceneUtils.SetLayer(gameObject, base.gameObject.layer);
        this.m_numCardBacksLoaded++;
        if (this.m_numCardBacksLoaded == 2)
        {
            base.SetReady(true);
        }
    }

    protected override void OnDataSet(bool updateVisuals)
    {
        if (updateVisuals)
        {
            string headline = GameStrings.Get("GLOBAL_REWARD_CARD_BACK_HEADLINE");
            base.SetRewardText(headline, string.Empty, string.Empty);
            CardBackRewardData data = base.Data as CardBackRewardData;
            if (data == null)
            {
                Debug.LogWarning(string.Format("CardBackReward.OnDataSet() - Data {0} is not CardBackRewardData", base.Data));
            }
            else
            {
                base.SetReady(false);
                CardBackManager.Get().LoadCardBackByIndex(data.CardBackID, new CardBackManager.LoadCardBackData.LoadCardBackCallback(this.OnFrontCardBackLoaded), true, "Card_Hidden");
                CardBackManager.Get().LoadCardBackByIndex(data.CardBackID, new CardBackManager.LoadCardBackData.LoadCardBackCallback(this.OnBackCardBackLoaded), true, "Card_Hidden");
            }
        }
    }

    private void OnFrontCardBackLoaded(CardBackManager.LoadCardBackData cardbackData)
    {
        GameObject gameObject = cardbackData.m_GameObject;
        gameObject.transform.parent = this.m_cardbackBone.transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        gameObject.transform.localScale = Vector3.one;
        SceneUtils.SetLayer(gameObject, base.gameObject.layer);
        this.m_numCardBacksLoaded++;
        if (this.m_numCardBacksLoaded == 2)
        {
            base.SetReady(true);
        }
    }

    protected override void ShowReward(bool updateCacheValues)
    {
        CardBackRewardData data = base.Data as CardBackRewardData;
        if (data == null)
        {
            Debug.LogWarning(string.Format("CardBackReward.ShowReward() - Data {0} is not CardBackRewardData", base.Data));
        }
        else
        {
            if (!data.IsDummyReward && updateCacheValues)
            {
                CardBackManager.Get().AddNewCardBack(data.CardBackID);
            }
            base.m_root.SetActive(true);
            this.m_cardbackBone.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            object[] args = new object[] { "amount", new Vector3(0f, 0f, 540f), "time", 1.5f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self };
            Hashtable hashtable = iTween.Hash(args);
            iTween.RotateAdd(this.m_cardbackBone.gameObject, hashtable);
        }
    }
}

