using System;
using UnityEngine;

public class QuestTile : MonoBehaviour
{
    public PlayMakerFSM m_BurnPlaymaker;
    public NormalButton m_cancelButton;
    public GameObject m_cancelButtonRoot;
    private bool m_canShowCancelButton;
    public GameObject m_defaultBone;
    public UberText m_goldAmount;
    public GameObject m_nameLine;
    public GameObject m_progress;
    public UberText m_progressText;
    private Achievement m_quest;
    public UberText m_questName;
    public UberText m_requirement;
    public GameObject m_rewardIcon;

    private void Awake()
    {
        this.SetCanShowCancelButton(false);
        this.m_cancelButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCancelButtonReleased));
    }

    public int GetQuestID()
    {
        if (this.m_quest == null)
        {
            return 0;
        }
        return this.m_quest.ID;
    }

    private void LoadCenterImage()
    {
        if (((this.m_quest.Rewards == null) || (this.m_quest.Rewards.Count == 0)) || (this.m_quest.Rewards[0] == null))
        {
            Debug.LogError("QuestTile.LoadCenterImage() - This quest doesn't grant a reward!!!");
        }
        else
        {
            RewardData data = this.m_quest.Rewards[0];
            Vector2 zero = Vector2.zero;
            Vector3 vector2 = Vector3.zero;
            this.m_rewardIcon.transform.localPosition = this.m_defaultBone.transform.localPosition;
            switch (data.RewardType)
            {
                case Reward.Type.BOOSTER_PACK:
                    zero = new Vector2(0f, 0.75f);
                    break;

                case Reward.Type.CARD:
                    zero = new Vector2(0.5f, 0f);
                    break;

                case Reward.Type.FORGE_TICKET:
                    zero = new Vector2(0.75f, 0.75f);
                    vector2 = new Vector3(0.9192683f, 0.9192683f, 0.9192683f);
                    break;

                case Reward.Type.GOLD:
                {
                    zero = new Vector2(0.25f, 0.75f);
                    GoldRewardData data2 = (GoldRewardData) data;
                    this.m_goldAmount.Text = data2.Amount.ToString();
                    this.m_goldAmount.gameObject.SetActive(true);
                    break;
                }
                case Reward.Type.MOUNT:
                {
                    long amount = 0L;
                    if ((this.m_quest.Rewards.Count > 1) && (this.m_quest.Rewards[1].RewardType == Reward.Type.GOLD))
                    {
                        amount = (this.m_quest.Rewards[1] as GoldRewardData).Amount;
                    }
                    zero = new Vector2(0.25f, 0.75f);
                    this.m_goldAmount.Text = amount.ToString();
                    this.m_goldAmount.gameObject.SetActive(true);
                    break;
                }
            }
            if (vector2 != Vector3.zero)
            {
                this.m_rewardIcon.transform.localScale = vector2;
            }
            this.m_rewardIcon.GetComponent<Renderer>().material.mainTextureOffset = zero;
        }
    }

    private void OnCancelButtonReleased(UIEvent e)
    {
        if (this.m_quest != null)
        {
            AchieveManager.Get().CancelQuest(this.m_quest.ID);
            this.m_BurnPlaymaker.SendEvent("Death");
        }
    }

    public void PlayBirth()
    {
        this.m_BurnPlaymaker.SendEvent("Birth");
    }

    public void SetCanShowCancelButton(bool canShowCancel)
    {
        this.m_canShowCancelButton = canShowCancel;
        this.UpdateCancelButtonVisibility();
    }

    public void SetupTile(Achievement quest)
    {
        quest.AckCurrentProgressAndRewardNotices();
        this.m_goldAmount.gameObject.SetActive(false);
        this.m_quest = quest;
        if (this.m_quest.MaxProgress > 1)
        {
            this.m_progressText.Text = this.m_quest.Progress + "/" + this.m_quest.MaxProgress;
            this.m_progress.SetActive(true);
        }
        else
        {
            this.m_progressText.Text = string.Empty;
            this.m_progress.SetActive(false);
        }
        bool flag = this.m_questName.isHidden();
        if (flag)
        {
            this.m_questName.Show();
        }
        this.m_questName.Text = quest.Name;
        TransformUtil.SetPoint(this.m_nameLine, Anchor.TOP, this.m_questName, Anchor.BOTTOM);
        this.m_nameLine.transform.localPosition = new Vector3(this.m_nameLine.transform.localPosition.x, this.m_nameLine.transform.localPosition.y, this.m_nameLine.transform.localPosition.z - this.m_questName.GetLocalizationPositionOffset().y);
        if (flag)
        {
            this.m_questName.Hide();
        }
        this.m_requirement.Text = quest.Description;
        this.LoadCenterImage();
    }

    public void UpdateCancelButtonVisibility()
    {
        bool flag = false;
        if (this.m_canShowCancelButton && (this.m_quest != null))
        {
            flag = AchieveManager.Get().CanCancelQuest(this.m_quest.ID);
        }
        this.m_cancelButtonRoot.gameObject.SetActive(flag);
    }
}

