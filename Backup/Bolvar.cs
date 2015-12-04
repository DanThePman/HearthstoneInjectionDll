using System;

public class Bolvar : SuperSpell
{
    public AttackRangePrefabs[] m_atkPrefabs;

    private Spell DetermineRangePrefab(int atk)
    {
        if (this.m_atkPrefabs.Length != 0)
        {
            if (atk > this.GetRangePrefabMax())
            {
                return this.m_atkPrefabs[this.m_atkPrefabs.Length - 1].m_Prefab;
            }
            if (atk < this.GetRangePrefabMin())
            {
                return this.m_atkPrefabs[0].m_Prefab;
            }
            for (int i = 0; i < this.m_atkPrefabs.Length; i++)
            {
                if ((atk >= this.m_atkPrefabs[i].m_MinAtk) && (atk <= this.m_atkPrefabs[i].m_MaxAtk))
                {
                    return this.m_atkPrefabs[i].m_Prefab;
                }
            }
        }
        return null;
    }

    private int GetRangePrefabMax()
    {
        if (this.m_atkPrefabs.Length == 0)
        {
            return 0;
        }
        int maxAtk = this.m_atkPrefabs[0].m_MaxAtk;
        for (int i = 1; i < this.m_atkPrefabs.Length; i++)
        {
            if (this.m_atkPrefabs[i].m_MaxAtk > maxAtk)
            {
                maxAtk = this.m_atkPrefabs[i].m_MaxAtk;
            }
        }
        return maxAtk;
    }

    private int GetRangePrefabMin()
    {
        if (this.m_atkPrefabs.Length == 0)
        {
            return 0;
        }
        int minAtk = this.m_atkPrefabs[0].m_MinAtk;
        for (int i = 1; i < this.m_atkPrefabs.Length; i++)
        {
            if (this.m_atkPrefabs[i].m_MinAtk < minAtk)
            {
                minAtk = this.m_atkPrefabs[i].m_MinAtk;
            }
        }
        return minAtk;
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        Card sourceCard = base.GetSourceCard();
        Spell prefab = this.DetermineRangePrefab(sourceCard.GetEntity().GetATK());
        Spell spell2 = base.CloneSpell(prefab);
        spell2.SetSource(sourceCard.gameObject);
        spell2.Activate();
        base.m_effectsPendingFinish--;
        base.FinishIfPossible();
    }
}

