using UnityEngine;

[CustomEditClass]
public class ButtonListMenuDef : MonoBehaviour
{
    [CustomEditField(Sections="Layout")]
    public NineSliceElement m_background;
    [CustomEditField(Sections="Layout")]
    public NineSliceElement m_border;
    [CustomEditField(Sections="Layout")]
    public MultiSliceElement m_buttonContainer;
    [CustomEditField(Sections="Header")]
    public MultiSliceElement m_header;
    [CustomEditField(Sections="Header")]
    public GameObject m_headerMiddle;
    [CustomEditField(Sections="Header")]
    public UberText m_headerText;
    [CustomEditField(Sections="Template Items")]
    public Vector3 m_horizontalDividerMinPadding = new Vector3(0f, 0f, 0.18f);
    [CustomEditField(Sections="Template Items")]
    public UIBButton m_templateButton;
    [CustomEditField(Sections="Template Items")]
    public GameObject m_templateHorizontalDivider;
}

