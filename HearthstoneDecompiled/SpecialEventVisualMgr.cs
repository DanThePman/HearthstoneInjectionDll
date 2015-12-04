using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class SpecialEventVisualMgr : MonoBehaviour
{
    public List<SpecialEventVisualDef> m_EventDefs = new List<SpecialEventVisualDef>();
    private static SpecialEventVisualMgr s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public static SpecialEventVisualMgr Get()
    {
        return s_instance;
    }

    public static SpecialEventType GetActiveEventType()
    {
        if (SpecialEventManager.Get().IsEventActive(SpecialEventType.GVG_PROMOTION, false))
        {
            return SpecialEventType.GVG_PROMOTION;
        }
        if (SpecialEventManager.Get().IsEventActive(SpecialEventType.SPECIAL_EVENT_PRE_TAVERN_BRAWL, false))
        {
            return SpecialEventType.SPECIAL_EVENT_PRE_TAVERN_BRAWL;
        }
        return SpecialEventType.IGNORE;
    }

    public bool LoadEvent(SpecialEventType eventType)
    {
        for (int i = 0; i < this.m_EventDefs.Count; i++)
        {
            SpecialEventVisualDef def = this.m_EventDefs[i];
            if (def.m_EventType == eventType)
            {
                string name = FileUtils.GameAssetPathToName(def.m_Prefab);
                AssetLoader.Get().LoadGameObject(name, null, null, false);
                return true;
            }
        }
        return false;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnEventFinished(Spell spell, object userData)
    {
        if (spell.GetActiveState() == SpellStateType.NONE)
        {
            UnityEngine.Object.Destroy(spell.gameObject);
        }
    }

    public bool UnloadEvent(SpecialEventType eventType)
    {
        for (int i = 0; i < this.m_EventDefs.Count; i++)
        {
            SpecialEventVisualDef def = this.m_EventDefs[i];
            if (def.m_EventType == eventType)
            {
                GameObject obj2 = GameObject.Find(FileUtils.GameAssetPathToName(def.m_Prefab + "(Clone)"));
                if (obj2 != null)
                {
                    UnityEngine.Object.Destroy(obj2);
                }
            }
        }
        return false;
    }
}

