using System;
using UnityEngine;

[CustomEditClass]
public class UIBScrollableTrack : MonoBehaviour
{
    public Vector3 m_hideRotation = new Vector3(0f, 0f, 180f);
    private bool m_lastEnabled;
    public UIBScrollable m_parentScrollbar;
    public float m_rotateAnimationTime = 0.5f;
    public GameObject m_scrollTrack;
    public Vector3 m_showRotation = Vector3.zero;

    private void Awake()
    {
        if (this.m_parentScrollbar == null)
        {
            Debug.LogError("Parent scroll bar not set!");
        }
        else
        {
            this.m_parentScrollbar.AddEnableScrollListener(new UIBScrollable.EnableScroll(this.OnScrollEnabled));
        }
    }

    private void OnEnable()
    {
        if (this.m_scrollTrack != null)
        {
            this.m_lastEnabled = this.m_parentScrollbar.IsEnabled();
            this.m_scrollTrack.transform.localEulerAngles = !this.m_lastEnabled ? this.m_hideRotation : this.m_showRotation;
        }
    }

    private void OnScrollEnabled(bool enabled)
    {
        if (((this.m_scrollTrack != null) && this.m_scrollTrack.activeInHierarchy) && (this.m_lastEnabled != enabled))
        {
            Vector3 hideRotation;
            Vector3 showRotation;
            this.m_lastEnabled = enabled;
            if (enabled)
            {
                hideRotation = this.m_hideRotation;
                showRotation = this.m_showRotation;
            }
            else
            {
                hideRotation = this.m_showRotation;
                showRotation = this.m_hideRotation;
            }
            this.m_scrollTrack.transform.localEulerAngles = hideRotation;
            iTween.StopByName(this.m_scrollTrack, "rotate");
            object[] args = new object[] { "rotation", showRotation, "islocal", true, "time", this.m_rotateAnimationTime, "name", "rotate" };
            iTween.RotateTo(this.m_scrollTrack, iTween.Hash(args));
        }
    }
}

