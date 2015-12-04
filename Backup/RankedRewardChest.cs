using System;
using System.Collections.Generic;
using UnityEngine;

public class RankedRewardChest : MonoBehaviour
{
    public MeshFilter m_baseMeshFilter;
    public MeshRenderer m_baseMeshRenderer;
    public List<ChestVisual> m_chests;
    public MeshRenderer m_glowMeshRenderer;
    public GameObject m_legendaryGem;
    public UberText m_rankBanner;
    public UberText m_rankNumber;
    public GameObject m_starDestinationBone;
    public static int NUM_REWARD_TIERS = 4;
    private static string s_rewardChestEarnedText = "GLOBAL_REWARD_CHEST_TIER{0}_EARNED";
    private static string s_rewardChestNameText = "GLOBAL_REWARD_CHEST_TIER{0}";

    public bool DoesChestVisualChange(int rank1, int rank2)
    {
        return (this.GetChestVisualFromRank(rank1) != this.GetChestVisualFromRank(rank2));
    }

    public static string GetChestEarnedFromRank(int rank)
    {
        int chestIndexFromRank = GetChestIndexFromRank(rank);
        return GameStrings.Format(string.Format(s_rewardChestEarnedText, NUM_REWARD_TIERS - chestIndexFromRank), new object[0]);
    }

    public static int GetChestIndexFromRank(int rank)
    {
        if (rank <= 5)
        {
            return 3;
        }
        if (rank <= 10)
        {
            return 2;
        }
        if (rank <= 15)
        {
            return 1;
        }
        if (rank <= 20)
        {
            return 0;
        }
        return -1;
    }

    public static string GetChestNameFromRank(int rank)
    {
        int chestIndexFromRank = GetChestIndexFromRank(rank);
        return GameStrings.Format(string.Format(s_rewardChestNameText, NUM_REWARD_TIERS - chestIndexFromRank), new object[0]);
    }

    public ChestVisual GetChestVisualFromRank(int rank)
    {
        int chestIndexFromRank = GetChestIndexFromRank(rank);
        if (chestIndexFromRank >= 0)
        {
            return this.m_chests[chestIndexFromRank];
        }
        return null;
    }

    public void SetRank(int rank)
    {
        Log.EndOfGame.Print("setting chest to rank " + rank, new object[0]);
        ChestVisual chestVisualFromRank = this.GetChestVisualFromRank(rank);
        this.m_baseMeshFilter.mesh = chestVisualFromRank.m_chestMesh;
        this.m_baseMeshRenderer.material = chestVisualFromRank.m_chestMaterial;
        if (this.m_glowMeshRenderer != null)
        {
            this.m_glowMeshRenderer.material = chestVisualFromRank.m_glowMaterial;
        }
        if (rank == 0)
        {
            this.m_legendaryGem.SetActive(true);
            this.m_rankNumber.gameObject.SetActive(false);
        }
        else
        {
            this.m_legendaryGem.SetActive(false);
            this.m_rankNumber.gameObject.SetActive(true);
        }
        this.m_rankNumber.gameObject.SetActive(true);
        this.m_rankNumber.Text = rank.ToString();
        this.m_rankBanner.Text = GameStrings.Get(chestVisualFromRank.chestName);
    }
}

