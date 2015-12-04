using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class BRMAnvilWeapons : MonoBehaviour
{
    private int m_LastWeaponIndex;
    public List<AnvilWeapon> m_Weapons;

    public int RandomSubWeapon(AnvilWeapon weapon)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < weapon.m_Events.Count; i++)
        {
            if (i != weapon.m_CurrentWeaponIndex)
            {
                list.Add(i);
            }
        }
        int num2 = UnityEngine.Random.Range(0, list.Count);
        weapon.m_CurrentWeaponIndex = list[num2];
        return list[num2];
    }

    public void RandomWeaponEvent()
    {
        List<int> list = new List<int>();
        for (int i = 0; i < this.m_Weapons.Count; i++)
        {
            if (i != this.m_LastWeaponIndex)
            {
                list.Add(i);
            }
        }
        if ((this.m_Weapons.Count > 0) && (list.Count > 0))
        {
            int num2 = UnityEngine.Random.Range(0, list.Count);
            AnvilWeapon weapon = this.m_Weapons[list[num2]];
            this.m_LastWeaponIndex = list[num2];
            weapon.m_FSM.SendEvent(weapon.m_Events[this.RandomSubWeapon(weapon)]);
        }
    }

    [Serializable]
    public class AnvilWeapon
    {
        [HideInInspector]
        public int m_CurrentWeaponIndex;
        public List<string> m_Events;
        public PlayMakerFSM m_FSM;
    }
}

