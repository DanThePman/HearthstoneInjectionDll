using System;
using System.Collections.Generic;
using UnityEngine;

public class GamesWonIndicator : MonoBehaviour
{
    private const float FUDGE_FACTOR = 0.01f;
    public GamesWonIndicatorSegment m_gamesWonSegmentPrefab;
    private InnKeeperTrigger m_innkeeperTrigger;
    private int m_numActiveSegments;
    public GameObject m_root;
    public GameObject m_segmentContainer;
    private List<GamesWonIndicatorSegment> m_segments = new List<GamesWonIndicatorSegment>();
    public UberText m_winCountText;

    public void Hide()
    {
        this.m_root.SetActive(false);
    }

    public void Init(Reward.Type rewardType, int rewardAmount, int numSegments, int numActiveSegments, InnKeeperTrigger trigger)
    {
        this.m_innkeeperTrigger = trigger;
        this.m_numActiveSegments = numActiveSegments;
        Vector3 position = this.m_segmentContainer.transform.position;
        float num = 0f;
        float num2 = 0f;
        for (int i = 0; i < numSegments; i++)
        {
            GamesWonIndicatorSegment.Type lEFT;
            if (i == 0)
            {
                lEFT = GamesWonIndicatorSegment.Type.LEFT;
            }
            else if (i == (numSegments - 1))
            {
                lEFT = GamesWonIndicatorSegment.Type.RIGHT;
            }
            else
            {
                lEFT = GamesWonIndicatorSegment.Type.MIDDLE;
            }
            bool hideCrown = i >= (numActiveSegments - 1);
            GamesWonIndicatorSegment item = UnityEngine.Object.Instantiate<GamesWonIndicatorSegment>(this.m_gamesWonSegmentPrefab);
            item.Init(lEFT, rewardType, rewardAmount, hideCrown);
            item.transform.parent = this.m_segmentContainer.transform;
            item.transform.localScale = Vector3.one;
            float num4 = item.GetWidth() - 0.01f;
            if (lEFT != GamesWonIndicatorSegment.Type.RIGHT)
            {
                position.x += num4;
            }
            else
            {
                position.x -= 0.01f;
            }
            item.transform.position = position;
            item.transform.rotation = Quaternion.identity;
            num = num4;
            num2 += num4;
            this.m_segments.Add(item);
        }
        Vector3 vector2 = this.m_segmentContainer.transform.position;
        vector2.x -= num2 / 2f;
        vector2.x += num / 5f;
        this.m_segmentContainer.transform.position = vector2;
        object[] args = new object[] { this.m_numActiveSegments, numSegments };
        this.m_winCountText.Text = GameStrings.Format("GAMEPLAY_WIN_REWARD_PROGRESS", args);
    }

    public void Show()
    {
        this.m_root.SetActive(true);
        if (this.m_numActiveSegments <= 0)
        {
            Debug.LogError(string.Format("GamesWonIndicator.Show(): cannot do animation; numActiveSegments={0} but should be greater than zero", this.m_numActiveSegments));
        }
        else if (this.m_numActiveSegments > this.m_segments.Count)
        {
            Debug.LogError(string.Format("GamesWonIndicator.Show(): cannot do animation; numActiveSegments = {0} but m_segments.Count = {1}", this.m_numActiveSegments, this.m_segments.Count));
        }
        else
        {
            this.m_segments[this.m_numActiveSegments - 1].AnimateReward();
            if (this.m_innkeeperTrigger == InnKeeperTrigger.NONE)
            {
            }
        }
    }

    public enum InnKeeperTrigger
    {
        NONE
    }
}

