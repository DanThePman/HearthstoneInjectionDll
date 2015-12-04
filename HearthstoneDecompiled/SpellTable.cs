using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class SpellTable : MonoBehaviour
{
    public List<SpellTableEntry> m_Table = new List<SpellTableEntry>();

    public SpellTableEntry FindEntry(SpellType type)
    {
        foreach (SpellTableEntry entry in this.m_Table)
        {
            if (entry.m_Type == type)
            {
                return entry;
            }
        }
        return null;
    }

    public bool FindSpell(SpellType spellType, out Spell spell)
    {
        foreach (SpellTableEntry entry in this.m_Table)
        {
            if (entry.m_Type == spellType)
            {
                spell = entry.m_Spell;
                return true;
            }
        }
        spell = null;
        return false;
    }

    public Spell GetSpell(SpellType spellType)
    {
        foreach (SpellTableEntry entry in this.m_Table)
        {
            if (entry.m_Type == spellType)
            {
                if ((entry.m_Spell == null) && (entry.m_SpellPrefabName != null))
                {
                    string name = FileUtils.GameAssetPathToName(entry.m_SpellPrefabName);
                    GameObject obj2 = AssetLoader.Get().LoadActor(name, true, true);
                    Spell spell = obj2.GetComponent<Spell>();
                    if (spell != null)
                    {
                        entry.m_Spell = spell;
                        TransformUtil.AttachAndPreserveLocalTransform(obj2.transform, base.gameObject.transform);
                    }
                }
                if (entry.m_Spell == null)
                {
                    Debug.LogError(string.Concat(new object[] { "Unable to load spell ", spellType, " from spell table ", base.gameObject.name }));
                    return null;
                }
                Spell component = UnityEngine.Object.Instantiate<GameObject>(entry.m_Spell.gameObject).GetComponent<Spell>();
                component.SetSpellType(spellType);
                return component;
            }
        }
        return null;
    }

    public void Hide()
    {
        foreach (SpellTableEntry entry in this.m_Table)
        {
            if (entry.m_Spell != null)
            {
                entry.m_Spell.Hide();
            }
        }
    }

    public bool IsLoaded(SpellType spellType)
    {
        Spell spell;
        this.FindSpell(spellType, out spell);
        return (spell != null);
    }

    public void ReleaseAllSpells()
    {
        foreach (SpellTableEntry entry in this.m_Table)
        {
            if (entry.m_Spell != null)
            {
                UnityEngine.Object.DestroyImmediate(entry.m_Spell.gameObject);
                UnityEngine.Object.DestroyImmediate(entry.m_Spell);
                entry.m_Spell = null;
            }
        }
    }

    public void ReleaseSpell(GameObject spellObject)
    {
        UnityEngine.Object.Destroy(spellObject);
    }

    public void SetSpell(SpellType type, Spell spell)
    {
        foreach (SpellTableEntry entry in this.m_Table)
        {
            if (entry.m_Type == type)
            {
                if (entry.m_Spell == null)
                {
                    entry.m_Spell = spell;
                    TransformUtil.AttachAndPreserveLocalTransform(spell.gameObject.transform, base.gameObject.transform);
                }
                return;
            }
        }
        Debug.LogError(string.Concat(new object[] { "Set invalid spell type ", type, " in spell table ", base.gameObject.name }));
    }

    public void Show()
    {
        foreach (SpellTableEntry entry in this.m_Table)
        {
            if ((entry.m_Spell != null) && (entry.m_Type != SpellType.NONE))
            {
                entry.m_Spell.Show();
            }
        }
    }
}

