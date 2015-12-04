using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UberTextMgr : MonoBehaviour
{
    private bool m_Active;
    private string m_AtlasCharacters;
    private string m_AtlasNumbers = "0123456789.";
    public Font m_BelweFont;
    public Font m_BelweOutlineFont;
    public Font m_BlizzardGlobal;
    private List<Font> m_Fonts;
    public Font m_FranklinGothicFont;
    private Locale m_Locale;
    private static UberTextMgr s_Instance;

    private void Awake()
    {
        s_Instance = this;
    }

    private string BuildCharacterSet()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0x21; i < 0x7f; i++)
        {
            builder.Append((char) i);
        }
        for (int j = 0xc0; j < 0x100; j++)
        {
            builder.Append((char) j);
        }
        return builder.ToString();
    }

    private void ForceEnglishCharactersInAtlas()
    {
        if (this.m_FranklinGothicFont == null)
        {
            Debug.LogError("UberTextMgr: m_FranklinGothicFont == null");
        }
        else
        {
            this.m_FranklinGothicFont.RequestCharactersInTexture(this.m_AtlasCharacters, 40, FontStyle.Normal);
            this.m_FranklinGothicFont.RequestCharactersInTexture(this.m_AtlasCharacters, 40, FontStyle.Italic);
            if (this.m_BelweOutlineFont == null)
            {
                Debug.LogError("UberTextMgr: m_BelweOutlineFont == null");
            }
            else
            {
                this.m_BelweOutlineFont.RequestCharactersInTexture(this.m_AtlasCharacters, 40, FontStyle.Normal);
                this.m_BelweOutlineFont.RequestCharactersInTexture(this.m_AtlasNumbers, 0x41, FontStyle.Normal);
            }
        }
    }

    public static UberTextMgr Get()
    {
        return s_Instance;
    }

    private Font GetLocalizedFont(Font font)
    {
        FontTable table = FontTable.Get();
        if (table == null)
        {
            Debug.LogError("UberTextMgr: Error loading FontTable");
            return null;
        }
        FontDef fontDef = table.GetFontDef(font);
        if (fontDef == null)
        {
            Debug.LogError("UberTextMgr: Error loading fontDef for: " + font.name);
            return null;
        }
        return fontDef.m_Font;
    }

    private void LogBelweAtlasUpdate()
    {
        int width = this.m_BelweFont.material.mainTexture.width;
        int height = this.m_BelweFont.material.mainTexture.height;
        object[] args = new object[] { width, height };
        Log.Kyle.Print("Belwe Atlas Updated: {0}, {1}", args);
    }

    private void LogBelweOutlineAtlasUpdate()
    {
        int width = this.m_BelweOutlineFont.material.mainTexture.width;
        int height = this.m_BelweOutlineFont.material.mainTexture.height;
        object[] args = new object[] { width, height };
        Log.Kyle.Print("BelweOutline Atlas Updated: {0}, {1}", args);
    }

    private void LogBlizzardGlobalAtlasUpdate()
    {
        int width = this.m_BlizzardGlobal.material.mainTexture.width;
        int height = this.m_BlizzardGlobal.material.mainTexture.height;
        object[] args = new object[] { width, height };
        Log.Kyle.Print("Blizzard Global Atlas Updated: {0}, {1}", args);
    }

    private void LogFontAtlasUpdate(Font font)
    {
        if (font == this.m_BelweFont)
        {
            this.LogBelweAtlasUpdate();
        }
        else if (font == this.m_BelweOutlineFont)
        {
            this.LogBelweOutlineAtlasUpdate();
        }
        else if (font == this.m_FranklinGothicFont)
        {
            this.LogFranklinGothicAtlasUpdate();
        }
        else if (font == this.m_BlizzardGlobal)
        {
            this.LogBlizzardGlobalAtlasUpdate();
        }
    }

    private void LogFranklinGothicAtlasUpdate()
    {
        int width = this.m_FranklinGothicFont.material.mainTexture.width;
        int height = this.m_FranklinGothicFont.material.mainTexture.height;
        object[] args = new object[] { width, height };
        Log.Kyle.Print("Franklin Gothic Atlas Updated: {0}, {1}", args);
    }

    private void Start()
    {
        this.m_AtlasCharacters = this.BuildCharacterSet();
        object[] args = new object[] { this.m_AtlasCharacters };
        Log.Kyle.Print("Updating Atlas to include: {0}", args);
    }

    public void StartAtlasUpdate()
    {
        Log.Kyle.Print("UberTextMgr.StartAtlasUpdate()", new object[0]);
        this.m_BelweFont = this.GetLocalizedFont(this.m_BelweFont);
        this.m_BelweOutlineFont = this.GetLocalizedFont(this.m_BelweOutlineFont);
        this.m_FranklinGothicFont = this.GetLocalizedFont(this.m_FranklinGothicFont);
        this.m_Active = true;
        Font.textureRebuilt += new Action<Font>(this.LogFontAtlasUpdate);
    }

    private void Update()
    {
        if (this.m_Active)
        {
            this.ForceEnglishCharactersInAtlas();
        }
    }
}

