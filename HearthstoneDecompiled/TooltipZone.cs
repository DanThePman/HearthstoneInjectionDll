using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TooltipZone : MonoBehaviour
{
    private GameObject m_tooltip;
    public GameObject targetObject;
    public Transform tooltipDisplayLocation;
    public GameObject tooltipPrefab;
    public Transform touchTooltipLocation;

    public void AnchorTooltipTo(GameObject target, Anchor targetAnchorPoint, Anchor tooltipAnchorPoint)
    {
        if (this.m_tooltip != null)
        {
            TransformUtil.SetPoint(this.m_tooltip, tooltipAnchorPoint, target, targetAnchorPoint);
        }
    }

    public GameObject GetTooltipObject()
    {
        return this.m_tooltip;
    }

    public void HideTooltip()
    {
        if (this.m_tooltip != null)
        {
            UnityEngine.Object.Destroy(this.m_tooltip);
        }
    }

    public bool IsShowingTooltip()
    {
        return (this.m_tooltip != null);
    }

    public void ShowBoxTooltip(string headline, string bodytext)
    {
        this.ShowTooltip(headline, bodytext, (float) KeywordHelpPanel.BOX_SCALE, true);
    }

    public void ShowGameplayTooltip(string headline, string bodytext)
    {
        this.ShowTooltip(headline, bodytext, (float) KeywordHelpPanel.GAMEPLAY_SCALE, true);
    }

    public void ShowGameplayTooltipLarge(string headline, string bodytext)
    {
        this.ShowTooltip(headline, bodytext, (float) KeywordHelpPanel.GAMEPLAY_SCALE_LARGE, false);
    }

    public KeywordHelpPanel ShowLayerTooltip(string headline, string bodytext)
    {
        KeywordHelpPanel panel = this.ShowTooltip(headline, bodytext, 1f, true);
        if (this.tooltipDisplayLocation != null)
        {
            panel.transform.parent = this.tooltipDisplayLocation.transform;
            panel.transform.localScale = Vector3.one;
            SceneUtils.SetLayer(this.m_tooltip, this.tooltipDisplayLocation.gameObject.layer);
        }
        return panel;
    }

    public void ShowSocialTooltip(Component target, string headline, string bodytext, float scale, GameLayer layer)
    {
        this.ShowSocialTooltip(target.gameObject, headline, bodytext, scale, layer);
    }

    public void ShowSocialTooltip(GameObject targetObject, string headline, string bodytext, float scale, GameLayer layer)
    {
        this.ShowTooltip(headline, bodytext, scale, true);
        SceneUtils.SetLayer(this.m_tooltip, layer);
        Camera camera = CameraUtils.FindFirstByLayer(targetObject.layer);
        Camera camera2 = CameraUtils.FindFirstByLayer(this.m_tooltip.layer);
        if (camera != camera2)
        {
            Vector3 position = camera.WorldToScreenPoint(this.m_tooltip.transform.position);
            Vector3 vector2 = camera2.ScreenToWorldPoint(position);
            this.m_tooltip.transform.position = vector2;
        }
    }

    public KeywordHelpPanel ShowTooltip(string headline, string bodytext)
    {
        float scale = 1f;
        if (SceneMgr.Get().IsInGame())
        {
            scale = (float) KeywordHelpPanel.GAMEPLAY_SCALE;
        }
        else
        {
            scale = (float) KeywordHelpPanel.COLLECTION_MANAGER_SCALE;
        }
        return this.ShowTooltip(headline, bodytext, scale, true);
    }

    public KeywordHelpPanel ShowTooltip(string headline, string bodytext, float scale, bool enablePhoneScale = true)
    {
        if (this.m_tooltip != null)
        {
            return this.m_tooltip.GetComponent<KeywordHelpPanel>();
        }
        if ((UniversalInputManager.UsePhoneUI != null) && enablePhoneScale)
        {
            scale *= 2f;
        }
        this.m_tooltip = UnityEngine.Object.Instantiate<GameObject>(this.tooltipPrefab);
        KeywordHelpPanel component = this.m_tooltip.GetComponent<KeywordHelpPanel>();
        component.Reset();
        component.Initialize(headline, bodytext);
        component.SetScale(scale);
        if (UniversalInputManager.Get().IsTouchMode() && (this.touchTooltipLocation != null))
        {
            component.transform.position = this.touchTooltipLocation.position;
            component.transform.rotation = this.touchTooltipLocation.rotation;
        }
        else if (this.tooltipDisplayLocation != null)
        {
            component.transform.position = this.tooltipDisplayLocation.position;
            component.transform.rotation = this.tooltipDisplayLocation.rotation;
        }
        component.transform.parent = base.transform;
        return component;
    }
}

