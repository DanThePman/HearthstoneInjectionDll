using System;
using System.Collections;
using UnityEngine;

public class CollectionClassTab : PegUIElement
{
    private static readonly string CLASS_ICONS_TEXTURE_NAME = "ClassIcons";
    private TAG_CLASS m_classTag;
    public Vector3 m_DeselectedLocalScale = new Vector3(0.44f, 0.44f, 0.44f);
    public float m_DeselectedLocalYPos;
    public GameObject m_glowMesh;
    private bool m_isVisible = true;
    public GameObject m_newItemCount;
    public UberText m_newItemCountText;
    private int m_numNewItems;
    private bool m_selected;
    public Vector3 m_SelectedLocalScale = new Vector3(0.66f, 0.66f, 0.66f);
    public float m_SelectedLocalYPos = 0.1259841f;
    private bool m_shouldBeVisible = true;
    private bool m_showLargeTab;
    public CollectionManagerDisplay.ViewMode m_tabViewMode;
    private Vector3 m_targetLocalPos;
    public static readonly float SELECT_TAB_ANIM_TIME = 0.2f;

    public void AnimateToTargetPosition(float animationTime, iTween.EaseType easeType)
    {
        object[] args = new object[] { "position", this.m_targetLocalPos, "isLocal", true, "time", animationTime, "easetype", easeType, "name", "position", "oncomplete", "OnMovedToTargetPos", "onompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.StopByName(base.gameObject, "position");
        iTween.MoveTo(base.gameObject, hashtable);
    }

    public TAG_CLASS GetClass()
    {
        return this.m_classTag;
    }

    private static Vector2 GetTextureOffset(TAG_CLASS classTag)
    {
        if (CollectionPageManager.s_classTextureOffsets.ContainsKey(classTag))
        {
            return CollectionPageManager.s_classTextureOffsets[classTag];
        }
        Debug.LogWarning(string.Format("CollectionClassTab.GetTextureOffset(): No class texture offsets exist for class {0}", classTag));
        if (CollectionPageManager.s_classTextureOffsets.ContainsKey(TAG_CLASS.INVALID))
        {
            return CollectionPageManager.s_classTextureOffsets[TAG_CLASS.INVALID];
        }
        return Vector2.zero;
    }

    public void Init(TAG_CLASS? classTag)
    {
        if (classTag.HasValue)
        {
            this.m_classTag = classTag.Value;
        }
        this.SetClassIconsTextureOffset(base.gameObject.GetComponent<Renderer>());
        if (this.m_glowMesh != null)
        {
            this.SetClassIconsTextureOffset(this.m_glowMesh.GetComponent<Renderer>());
        }
        this.SetGlowActive(false);
        this.UpdateNewItemCount(0);
    }

    public bool IsVisible()
    {
        return this.m_isVisible;
    }

    private void OnMovedToTargetPos()
    {
        if (!this.m_showLargeTab)
        {
            Vector3 localPosition = base.transform.localPosition;
            localPosition.y = this.m_DeselectedLocalYPos;
            base.transform.localPosition = localPosition;
        }
    }

    private void SetClassIconsTextureOffset(Renderer renderer)
    {
        if (renderer != null)
        {
            Vector2 textureOffset = GetTextureOffset(this.m_classTag);
            foreach (Material material in renderer.materials)
            {
                if ((material.mainTexture != null) && material.mainTexture.name.Contains(CLASS_ICONS_TEXTURE_NAME))
                {
                    material.mainTextureOffset = textureOffset;
                }
            }
        }
    }

    public void SetGlowActive(bool active)
    {
        if (this.m_selected)
        {
            active = true;
        }
        if (this.m_glowMesh != null)
        {
            this.m_glowMesh.SetActive(active);
        }
    }

    public void SetIsVisible(bool isVisible)
    {
        this.m_isVisible = isVisible;
        base.SetEnabled(this.m_isVisible);
    }

    public void SetLargeTab(bool large)
    {
        if (large != this.m_showLargeTab)
        {
            if (large)
            {
                Vector3 localPosition = base.transform.localPosition;
                localPosition.y = this.m_SelectedLocalYPos;
                base.transform.localPosition = localPosition;
                object[] args = new object[] { "scale", this.m_SelectedLocalScale, "time", SELECT_TAB_ANIM_TIME, "name", "scale" };
                Hashtable hashtable = iTween.Hash(args);
                iTween.ScaleTo(base.gameObject, hashtable);
                SoundManager.Get().LoadAndPlay("class_tab_click", base.gameObject);
            }
            else
            {
                Vector3 vector2 = base.transform.localPosition;
                vector2.y = this.m_DeselectedLocalYPos;
                base.transform.localPosition = vector2;
                iTween.StopByName(base.gameObject, "scale");
                base.transform.localScale = this.m_DeselectedLocalScale;
            }
            this.m_showLargeTab = large;
        }
    }

    public void SetSelected(bool selected)
    {
        if (this.m_selected != selected)
        {
            this.m_selected = selected;
            this.SetGlowActive(this.m_selected);
        }
    }

    public void SetTargetLocalPosition(Vector3 targetLocalPos)
    {
        this.m_targetLocalPos = targetLocalPos;
    }

    public void SetTargetVisibility(bool visible)
    {
        this.m_shouldBeVisible = visible;
    }

    public bool ShouldBeVisible()
    {
        return this.m_shouldBeVisible;
    }

    public void UpdateNewItemCount(int numNewItems)
    {
        this.m_numNewItems = numNewItems;
        this.UpdateNewItemCountVisuals();
    }

    private void UpdateNewItemCountVisuals()
    {
        if (this.m_newItemCountText != null)
        {
            object[] args = new object[] { this.m_numNewItems };
            this.m_newItemCountText.Text = GameStrings.Format("GLUE_COLLECTION_NEW_CARD_CALLOUT", args);
        }
        if (this.m_newItemCount != null)
        {
            this.m_newItemCount.SetActive(this.m_numNewItems > 0);
        }
    }

    public bool WillSlide()
    {
        return (Mathf.Abs((float) (this.m_targetLocalPos.x - base.transform.localPosition.x)) > 0.05f);
    }
}

