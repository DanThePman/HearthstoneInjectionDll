using System;

public class DbfLocValue
{
    private Locale m_locale;
    private object m_value;

    public Locale GetLocale()
    {
        return this.m_locale;
    }

    public object GetValue()
    {
        return this.m_value;
    }

    public void SetLocale(Locale locale)
    {
        this.m_locale = locale;
    }

    public void SetValue(object val)
    {
        this.m_value = val;
    }

    public override string ToString()
    {
        return string.Format("locale={0} value={1}", this.m_locale, this.m_value);
    }
}

