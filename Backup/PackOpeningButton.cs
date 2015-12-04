using System;
using UnityEngine;

public class PackOpeningButton : BoxMenuButton
{
    public UberText m_count;
    public GameObject m_countFrame;

    public string GetGetPackCount()
    {
        return this.m_count.Text;
    }

    public void SetPackCount(int packs)
    {
        if (packs < 0)
        {
            this.m_count.Text = string.Empty;
        }
        else
        {
            object[] args = new object[] { packs };
            this.m_count.Text = GameStrings.Format("GLUE_PACK_OPENING_BOOSTER_COUNT", args);
        }
    }
}

