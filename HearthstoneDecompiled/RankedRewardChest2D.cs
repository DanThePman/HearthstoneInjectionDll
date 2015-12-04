using System;
using System.Collections.Generic;
using UnityEngine;

public class RankedRewardChest2D : PegUIElement
{
    public GameObject m_emptyRewardChest;
    public GameObject m_legendaryGem;
    private int m_rank;
    public UberText m_rewardChestDescriptionText;
    public UberText m_rewardChestRankText;
    public List<GameObject> m_rewardChests;

    protected override void Awake()
    {
        base.Awake();
        foreach (GameObject obj2 in this.m_rewardChests)
        {
            obj2.SetActive(false);
        }
        this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.ChestOver));
        this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.ChestOut));
    }

    private void ChestOut(UIEvent e)
    {
        base.gameObject.GetComponent<TooltipZone>().HideTooltip();
    }

    public void ChestOver(UIEvent e)
    {
        string chestNameFromRank;
        string str2;
        if (RankedRewardChest.GetChestIndexFromRank(this.m_rank) >= 0)
        {
            chestNameFromRank = RankedRewardChest.GetChestNameFromRank(this.m_rank);
            str2 = GameStrings.Format("GLUE_QUEST_LOG_CHEST_TOOLTIP_BODY", new object[0]);
        }
        else
        {
            chestNameFromRank = GameStrings.Format("GLUE_QUEST_LOG_NO_CHEST", new object[0]);
            str2 = GameStrings.Format("GLUE_QUEST_LOG_CHEST_TOOLTIP_BODY_NO_CHEST", new object[0]);
        }
        base.gameObject.GetComponent<TooltipZone>().ShowLayerTooltip(chestNameFromRank, str2);
    }

    public void SetRank(int rank)
    {
        this.m_rank = rank;
        int chestIndexFromRank = RankedRewardChest.GetChestIndexFromRank(rank);
        if (chestIndexFromRank >= 0)
        {
            this.m_rewardChests[chestIndexFromRank].SetActive(true);
            this.m_emptyRewardChest.SetActive(false);
            this.m_rewardChestRankText.Text = rank.ToString();
        }
        else
        {
            this.m_emptyRewardChest.SetActive(true);
            this.m_rewardChestRankText.TextAlpha = 0.2f;
            this.m_rewardChestRankText.Text = 20.ToString();
        }
        bool flag = this.m_rank == 0;
        this.m_legendaryGem.SetActive(flag);
        this.m_rewardChestRankText.gameObject.SetActive(!flag);
    }
}

