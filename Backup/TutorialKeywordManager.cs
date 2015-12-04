using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TutorialKeywordManager : MonoBehaviour
{
    private Actor m_actor;
    private Card m_card;
    public TutorialKeywordTooltip m_keywordPanelPrefab;
    private List<TutorialKeywordTooltip> m_keywordPanels;
    private static TutorialKeywordManager s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public static TutorialKeywordManager Get()
    {
        return s_instance;
    }

    public Card GetCard()
    {
        return this.m_card;
    }

    public Vector3 GetPositionOfTopPanel()
    {
        if ((this.m_keywordPanels != null) && (this.m_keywordPanels.Count != 0))
        {
            return this.m_keywordPanels[0].transform.position;
        }
        return new Vector3(0f, 0f, 0f);
    }

    public void HideKeywordHelp()
    {
        if (this.m_keywordPanels != null)
        {
            foreach (TutorialKeywordTooltip tooltip in this.m_keywordPanels)
            {
                if (tooltip != null)
                {
                    UnityEngine.Object.Destroy(tooltip.gameObject);
                }
            }
        }
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void PrepareToUpdateKeywordHelp(Actor actor)
    {
        this.HideKeywordHelp();
        this.m_actor = actor;
        this.m_keywordPanels = new List<TutorialKeywordTooltip>();
    }

    public void SetupKeywordPanel(GAME_TAG tag)
    {
        string keywordName = GameStrings.GetKeywordName(tag);
        string keywordText = GameStrings.GetKeywordText(tag);
        this.SetupKeywordPanel(keywordName, keywordText);
    }

    public void SetupKeywordPanel(string headline, string description)
    {
        TutorialKeywordTooltip component = UnityEngine.Object.Instantiate<GameObject>(this.m_keywordPanelPrefab.gameObject).GetComponent<TutorialKeywordTooltip>();
        if (component != null)
        {
            component.Initialize(headline, description);
            this.m_keywordPanels.Add(component);
        }
    }

    private bool SetupKeywordPanelIfNecessary(EntityBase entityInfo, GAME_TAG tag)
    {
        if (entityInfo.HasTag(tag))
        {
            this.SetupKeywordPanel(tag);
            return true;
        }
        if (entityInfo.HasReferencedTag(tag))
        {
            this.SetupKeywordRefPanel(tag);
            return true;
        }
        return false;
    }

    public void SetupKeywordRefPanel(GAME_TAG tag)
    {
        string keywordName = GameStrings.GetKeywordName(tag);
        string refKeywordText = GameStrings.GetRefKeywordText(tag);
        this.SetupKeywordPanel(keywordName, refKeywordText);
    }

    private void SetUpPanels(EntityBase entityInfo)
    {
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.TAUNT);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.STEALTH);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.DIVINE_SHIELD);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.SPELLPOWER);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.ENRAGED);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.CHARGE);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.BATTLECRY);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.FROZEN);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.FREEZE);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.WINDFURY);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.SECRET);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.DEATHRATTLE);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.OVERLOAD);
        this.SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.COMBO);
    }

    public void UpdateKeywordHelp(Card c, Actor a)
    {
        this.UpdateKeywordHelp(c, a, true, null);
    }

    public void UpdateKeywordHelp(Card card, Actor actor, bool showOnRight, float? overrideScale = new float?())
    {
        this.m_card = card;
        this.UpdateKeywordHelp(card.GetEntity(), actor, showOnRight, overrideScale);
    }

    public void UpdateKeywordHelp(Entity entity, Actor actor, bool showOnRight, float? overrideScale = new float?())
    {
        float x = 1f;
        if (overrideScale.HasValue)
        {
            x = overrideScale.Value;
        }
        this.PrepareToUpdateKeywordHelp(actor);
        string[] strArray = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
        if (strArray != null)
        {
            this.SetupKeywordPanel(strArray[0], strArray[1]);
        }
        this.SetUpPanels(entity);
        TutorialKeywordTooltip tooltip = null;
        float num2 = 0f;
        float num3 = 0f;
        for (int i = 0; i < this.m_keywordPanels.Count; i++)
        {
            TutorialKeywordTooltip tooltip2 = this.m_keywordPanels[i];
            num2 = 1.05f;
            if (entity.IsHero())
            {
                num2 = 1.2f;
            }
            else if (entity.GetZone() == TAG_ZONE.PLAY)
            {
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    x = 1.7f;
                }
                num2 = 1.45f * x;
            }
            tooltip2.transform.localScale = new Vector3(x, x, x);
            num3 = -0.2f * this.m_actor.GetMeshRenderer().bounds.size.z;
            if ((UniversalInputManager.UsePhoneUI != null) && (entity.GetZone() == TAG_ZONE.PLAY))
            {
                num3 += 1.5f;
            }
            if (i == 0)
            {
                if (showOnRight)
                {
                    tooltip2.transform.position = this.m_actor.transform.position + new Vector3(this.m_actor.GetMeshRenderer().bounds.size.x * num2, 0f, this.m_actor.GetMeshRenderer().bounds.extents.z + num3);
                }
                else
                {
                    tooltip2.transform.position = this.m_actor.transform.position + new Vector3(-this.m_actor.GetMeshRenderer().bounds.size.x * num2, 0f, this.m_actor.GetMeshRenderer().bounds.extents.z + num3);
                }
            }
            else
            {
                tooltip2.transform.position = tooltip.transform.position - new Vector3(0f, 0f, (tooltip.GetHeight() * 0.35f) + (tooltip2.GetHeight() * 0.35f));
            }
            tooltip = tooltip2;
        }
        GameState.Get().GetGameEntity().NotifyOfHelpPanelDisplay(this.m_keywordPanels.Count);
    }
}

