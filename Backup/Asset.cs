using System;
using System.Runtime.InteropServices;

public class Asset
{
    private AssetFamily m_family;
    private Locale m_locale;
    private string m_name;
    private string m_path;
    private bool m_persistent;

    public Asset(string name, AssetFamily family, bool persistent = false)
    {
        this.m_name = name;
        this.m_family = family;
        this.m_persistent = persistent;
        this.m_path = string.Format(AssetPathInfo.FamilyInfo[family].format, name);
    }

    public string GetDirectory()
    {
        return AssetPathInfo.FamilyInfo[this.m_family].sourceDir;
    }

    public string[] GetExtensions()
    {
        return AssetPathInfo.FamilyInfo[this.m_family].exts;
    }

    public AssetFamily GetFamily()
    {
        return this.m_family;
    }

    public Locale GetLocale()
    {
        return this.m_locale;
    }

    public string GetName()
    {
        return this.m_name;
    }

    public string GetPath()
    {
        return this.m_path;
    }

    public string GetPath(Locale locale)
    {
        return FileUtils.MakeLocalizedPathFromSourcePath(locale, this.m_path);
    }

    public bool IsPersistent()
    {
        return this.m_persistent;
    }

    public void SetLocale(Locale locale)
    {
        this.m_locale = locale;
    }
}

