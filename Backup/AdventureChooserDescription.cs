using System;
using UnityEngine;

[CustomEditClass]
public class AdventureChooserDescription : MonoBehaviour
{
    [CustomEditField(Sections="Description")]
    public UberText m_DescriptionObject;
    private string m_DescText;
    private string m_RequiredText;
    [SerializeField]
    private Color32 m_WarningTextColor = new Color32(0xff, 210, 0x17, 0xff);

    public string GetText()
    {
        return this.m_DescriptionObject.Text;
    }

    private void RefreshText()
    {
        string descText = null;
        if (!string.IsNullOrEmpty(this.m_RequiredText))
        {
            string introduced2 = this.m_WarningTextColor.r.ToString("X2");
            string introduced3 = this.m_WarningTextColor.g.ToString("X2");
            string str2 = introduced2 + introduced3 + this.m_WarningTextColor.b.ToString("X2");
            string[] textArray1 = new string[] { "<color=#", str2, ">• ", this.m_RequiredText, " •</color>\n", this.m_DescText };
            descText = string.Concat(textArray1);
        }
        else
        {
            descText = this.m_DescText;
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_DescriptionObject.CharacterSize = 70f;
        }
        this.m_DescriptionObject.Text = descText;
    }

    public void SetText(string requiredText, string descText)
    {
        this.m_RequiredText = requiredText;
        this.m_DescText = descText;
        this.RefreshText();
    }

    [CustomEditField(Sections="Description")]
    public Color WarningTextColor
    {
        get
        {
            return (Color) this.m_WarningTextColor;
        }
        set
        {
            this.m_WarningTextColor = value;
            this.RefreshText();
        }
    }
}

