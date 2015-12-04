using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ForgeMedal : PegUIElement
{
    private List<ForgeMedalInfo> m_forgeMedalInfos = new List<ForgeMedalInfo>();
    private Medal m_medal;
    private Medal m_nextMedal;

    protected override void Awake()
    {
        base.Awake();
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(Medal.Type.MEDAL_NOVICE), new Vector2(0f, 0f)));
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(Medal.Type.MEDAL_APPRENTICE), new Vector2(0.25f, 0f)));
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(Medal.Type.MEDAL_JOURNEYMAN), new Vector2(0.5f, 0f)));
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(Medal.Type.MEDAL_COPPER_A), new Vector2(0.75f, 0f)));
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(Medal.Type.MEDAL_SILVER_A), new Vector2(0f, -0.25f)));
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(Medal.Type.MEDAL_GOLD_A), new Vector2(0.25f, -0.25f)));
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(Medal.Type.MEDAL_PLATINUM_A), new Vector2(0.5f, -0.25f)));
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(Medal.Type.MEDAL_DIAMOND_A), new Vector2(0.75f, -0.25f)));
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(Medal.Type.MEDAL_MASTER_A), new Vector2(0f, -0.5f)));
        this.m_forgeMedalInfos.Add(new ForgeMedalInfo(new Medal(1), new Vector2(0.25f, -0.5f)));
        this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.MedalOver));
        this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.MedalOut));
    }

    private void MedalOut(UIEvent e)
    {
        base.gameObject.GetComponent<TooltipZone>().HideTooltip();
    }

    private void MedalOver(UIEvent e)
    {
        float num;
        object[] args = new object[] { this.m_medal.GetBaseMedalName() };
        string headline = GameStrings.Format("GLOBAL_MEDAL_TOOLTIP_HEADER", args);
        string bodytext = string.Empty;
        if (this.m_nextMedal == null)
        {
            bodytext = GameStrings.Get("GLOBAL_MEDAL_TOOLTIP_BODY_ARENA_GRAND_MASTER");
        }
        else
        {
            object[] objArray2 = new object[] { this.m_nextMedal.GetBaseMedalName() };
            bodytext = GameStrings.Format("GLOBAL_MEDAL_TOOLTIP_BODY", objArray2);
        }
        if (SceneMgr.Get().IsInGame())
        {
            num = 0.3f;
        }
        else if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
        {
            num = 6f;
        }
        else
        {
            num = 3f;
        }
        base.gameObject.GetComponent<TooltipZone>().ShowTooltip(headline, bodytext, num, true);
    }

    public void SetMedal(int wins)
    {
        ForgeMedalInfo info = this.m_forgeMedalInfos[wins];
        this.m_medal = info.ForgeMedal;
        int num = wins + 1;
        this.m_nextMedal = (this.m_forgeMedalInfos.Count <= num) ? null : this.m_forgeMedalInfos[num].ForgeMedal;
        base.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", info.TextureCoords);
    }

    private class ForgeMedalInfo
    {
        public ForgeMedalInfo(Medal forgeMedal, Vector2 textureCoords)
        {
            this.ForgeMedal = forgeMedal;
            this.TextureCoords = textureCoords;
        }

        public Medal ForgeMedal { get; private set; }

        public Vector2 TextureCoords { get; private set; }
    }
}

