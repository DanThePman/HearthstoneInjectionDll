using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellCache : MonoBehaviour
{
    private Map<string, SpellTable> m_spellTableCache = new Map<string, SpellTable>();
    private static SpellCache s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public void Clear()
    {
        foreach (KeyValuePair<string, SpellTable> pair in this.m_spellTableCache)
        {
            pair.Value.ReleaseAllSpells();
        }
    }

    public static SpellCache Get()
    {
        if ((s_instance == null) && !Application.isEditor)
        {
            Debug.LogError("Attempting to access null SpellCache");
            return null;
        }
        return s_instance;
    }

    public SpellTable GetSpellTable(string tablePath)
    {
        SpellTable table;
        string key = FileUtils.GameAssetPathToName(tablePath);
        if (!this.m_spellTableCache.TryGetValue(key, out table))
        {
            table = this.LoadSpellTable(key);
        }
        return table;
    }

    private SpellTable LoadSpellTable(string tableName)
    {
        GameObject obj2 = AssetLoader.Get().LoadActor(tableName, false, false);
        if (obj2 == null)
        {
            object[] messageArgs = new object[] { base.name };
            Error.AddDevFatal("SpellCache.LoadSpellTable() - {0} failed to load", messageArgs);
            return null;
        }
        SpellTable component = obj2.GetComponent<SpellTable>();
        if (component == null)
        {
            object[] objArray2 = new object[] { base.name };
            Error.AddDevFatal("SpellCache.LoadSpellTable() - {0} has no SpellTable component", objArray2);
            return null;
        }
        component.transform.parent = base.transform;
        this.m_spellTableCache.Add(tableName, component);
        return component;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnScenePreLoad(SceneMgr.Mode prevMode, SceneMgr.Mode mode, object userData)
    {
        switch (mode)
        {
            case SceneMgr.Mode.GAMEPLAY:
                this.PreloadSpell("Card_Hand_Ability_SpellTable", SpellType.SPELL_POWER_HINT_IDLE);
                this.PreloadSpell("Card_Hand_Ability_SpellTable", SpellType.SPELL_POWER_HINT_BURST);
                this.PreloadSpell("Card_Hand_Ability_SpellTable", SpellType.POWER_UP);
                this.PreloadSpell("Card_Hand_Ally_SpellTable", SpellType.SUMMON_OUT_MEDIUM);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.OPPONENT_ATTACK);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.STEALTH);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.DAMAGE);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.DEATH);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.SUMMON_OUT);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.FROZEN);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.FRIENDLY_ATTACK);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.SUMMON_IN_MEDIUM);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.SUMMON_IN);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.SUMMON_IN_OPPONENT);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.BATTLECRY);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.ENCHANT_POSITIVE);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.ENCHANT_NEGATIVE);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.ENCHANT_NEUTRAL);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.TAUNT_STEALTH);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.TRIGGER);
                this.PreloadSpell("Card_Play_Ally_SpellTable", SpellType.Zzz);
                this.PreloadSpell("Card_Hidden_SpellTable", SpellType.SUMMON_OUT);
                this.PreloadSpell("Card_Hidden_SpellTable", SpellType.SUMMON_IN);
                this.PreloadSpell("Card_Hidden_SpellTable", SpellType.SUMMON_OUT_WEAPON);
                this.PreloadSpell("Card_Play_Hero_SpellTable", SpellType.ENDGAME_WIN);
                this.PreloadSpell("Card_Play_Hero_SpellTable", SpellType.OPPONENT_ATTACK);
                this.PreloadSpell("Card_Play_Hero_SpellTable", SpellType.FRIENDLY_ATTACK);
                this.PreloadSpell("Card_Play_Hero_SpellTable", SpellType.FROZEN);
                this.PreloadSpell("Card_Play_Hero_SpellTable", SpellType.DAMAGE);
                this.PreloadSpell("Card_Play_Weapon_SpellTable", SpellType.ENCHANT_POSITIVE);
                this.PreloadSpell("Card_Play_Weapon_SpellTable", SpellType.ENCHANT_NEUTRAL);
                this.PreloadSpell("Card_Play_Weapon_SpellTable", SpellType.ENCHANT_NEGATIVE);
                this.PreloadSpell("Card_Play_Weapon_SpellTable", SpellType.DAMAGE);
                this.PreloadSpell("Card_Play_Weapon_SpellTable", SpellType.DEATH);
                this.PreloadSpell("Card_Play_Weapon_SpellTable", SpellType.SHEATHE);
                this.PreloadSpell("Card_Play_Weapon_SpellTable", SpellType.UNSHEATHE);
                this.PreloadSpell("Card_Play_Weapon_SpellTable", SpellType.SUMMON_IN_OPPONENT);
                this.PreloadSpell("Card_Play_Weapon_SpellTable", SpellType.SUMMON_IN_FRIENDLY);
                this.PreloadSpell("Card_Play_Hero_SpellTable", SpellType.FRIENDLY_ATTACK);
                break;

            case SceneMgr.Mode.COLLECTIONMANAGER:
            case SceneMgr.Mode.TAVERN_BRAWL:
                this.PreloadSpell("Card_Hand_Ally_SpellTable", SpellType.DEATHREVERSE);
                this.PreloadSpell("Card_Hand_Ability_SpellTable", SpellType.DEATHREVERSE);
                this.PreloadSpell("Card_Hand_Weapon_SpellTable", SpellType.DEATHREVERSE);
                this.PreloadSpell("Card_Hand_Ally_SpellTable", SpellType.GHOSTCARD);
                this.PreloadSpell("Card_Hand_Ability_SpellTable", SpellType.GHOSTCARD);
                this.PreloadSpell("Card_Hand_Weapon_SpellTable", SpellType.GHOSTCARD);
                break;
        }
    }

    private void PreloadSpell(string tableName, SpellType type)
    {
        SpellTable spellTable = this.GetSpellTable(tableName);
        if (spellTable == null)
        {
            object[] messageArgs = new object[] { tableName };
            Error.AddDevFatal("SpellCache.PreloadSpell() - Preloaded nonexistent SpellTable {0}", messageArgs);
        }
        else
        {
            SpellTableEntry entry = spellTable.FindEntry(type);
            if (entry == null)
            {
                object[] objArray2 = new object[] { tableName, type };
                Error.AddDevFatal("SpellCache.PreloadSpell() - SpellTable {0} has no spell of type {1}", objArray2);
            }
            else if (entry.m_Spell == null)
            {
                string name = FileUtils.GameAssetPathToName(entry.m_SpellPrefabName);
                GameObject obj2 = AssetLoader.Get().LoadActor(name, true, true);
                if (obj2 == null)
                {
                    object[] objArray3 = new object[] { name };
                    Error.AddDevFatal("SpellCache.PreloadSpell() - {0} does not contain a spell component: ", objArray3);
                }
                else
                {
                    Spell component = obj2.GetComponent<Spell>();
                    if (component == null)
                    {
                        object[] objArray4 = new object[] { name };
                        Error.AddDevFatal("SpellCache.PreloadSpell() - {0} does not contain a spell component: ", objArray4);
                    }
                    else
                    {
                        spellTable.SetSpell(type, component);
                    }
                }
            }
        }
    }

    private void Start()
    {
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().RegisterScenePreLoadEvent(new SceneMgr.ScenePreLoadCallback(this.OnScenePreLoad));
        }
    }
}

