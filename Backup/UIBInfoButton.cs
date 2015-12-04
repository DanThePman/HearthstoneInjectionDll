using System;
using System.Collections;
using UnityEngine;

[CustomEditClass]
public class UIBInfoButton : PegUIElement
{
    private const float DEPRESS_TIME = 0.1f;
    [SerializeField, CustomEditField(Sections="Button Objects")]
    public Transform m_DownBone;
    [CustomEditField(Sections="Highlight"), SerializeField]
    public GameObject m_Highlight;
    [CustomEditField(Sections="Button Objects"), SerializeField]
    public GameObject m_RootObject;
    private UIBHighlight m_UIBHighlight;
    [CustomEditField(Sections="Button Objects"), SerializeField]
    public Transform m_UpBone;
    private const float RAISE_TIME = 0.1f;

    protected override void Awake()
    {
        base.Awake();
        UIBHighlight component = base.GetComponent<UIBHighlight>();
        if (component == null)
        {
            component = base.gameObject.AddComponent<UIBHighlight>();
        }
        this.m_UIBHighlight = component;
        if (this.m_UIBHighlight != null)
        {
            this.m_UIBHighlight.m_MouseOverHighlight = this.m_Highlight;
            this.m_UIBHighlight.m_HideMouseOverOnPress = false;
        }
    }

    private void Depress()
    {
        object[] args = new object[] { "position", this.m_DownBone.localPosition, "time", 0.1f, "easeType", iTween.EaseType.linear, "isLocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_RootObject, hashtable);
    }

    public void Deselect()
    {
        this.Raise();
    }

    private void Raise()
    {
        object[] args = new object[] { "position", this.m_UpBone.localPosition, "time", 0.1f, "easeType", iTween.EaseType.linear, "isLocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_RootObject, hashtable);
    }

    public void Select()
    {
        this.Depress();
    }
}

