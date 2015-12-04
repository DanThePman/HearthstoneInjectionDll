using System;

public class BnetErrorInfo
{
    private int m_clientContext;
    private BattleNetErrors m_error;
    private BnetFeature m_feature;
    private BnetFeatureEvent m_featureEvent;

    private BnetErrorInfo()
    {
    }

    public BnetErrorInfo(BnetFeature feature, BnetFeatureEvent featureEvent, BattleNetErrors error)
    {
        this.m_feature = feature;
        this.m_featureEvent = featureEvent;
        this.m_error = error;
        this.m_clientContext = 0;
    }

    public BnetErrorInfo(BnetFeature feature, BnetFeatureEvent featureEvent, BattleNetErrors error, int context)
    {
        this.m_feature = feature;
        this.m_featureEvent = featureEvent;
        this.m_error = error;
        this.m_clientContext = context;
    }

    public void CopyFrom(BattleNet.DllErrorInfo src)
    {
        this.m_feature = (BnetFeature) src.feature;
        this.m_featureEvent = (BnetFeatureEvent) src.featureEvent;
        this.m_error = (BattleNetErrors) src.code;
        this.m_clientContext = src.context;
    }

    public static BnetErrorInfo CreateFromDll(BattleNet.DllErrorInfo src)
    {
        BnetErrorInfo info = new BnetErrorInfo();
        info.CopyFrom(src);
        return info;
    }

    public int GetContext()
    {
        return this.m_clientContext;
    }

    public BattleNetErrors GetError()
    {
        return this.m_error;
    }

    public BnetFeature GetFeature()
    {
        return this.m_feature;
    }

    public BnetFeatureEvent GetFeatureEvent()
    {
        return this.m_featureEvent;
    }

    public string GetName()
    {
        return this.m_error.ToString();
    }

    public void SetError(BattleNetErrors error)
    {
        this.m_error = error;
    }

    public void SetFeature(BnetFeature feature)
    {
        this.m_feature = feature;
    }

    public void SetFeatureEvent(BnetFeatureEvent featureEvent)
    {
        this.m_featureEvent = featureEvent;
    }

    public override string ToString()
    {
        if (Enum.IsDefined(typeof(BattleNetErrors), this.m_error))
        {
            return string.Format("[event={0} error={1} {2}]", this.m_featureEvent, (int) this.m_error, this.m_error.ToString());
        }
        return string.Format("[event={0} code={1} name={2}]", this.m_featureEvent, (int) this.m_error, this.m_error.ToString());
    }
}

