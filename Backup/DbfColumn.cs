using System;

public class DbfColumn
{
    private bool? m_client;
    private string m_comment;
    private DbfDataType m_dataType;
    private string m_name;

    public string GetComment()
    {
        return this.m_comment;
    }

    public DbfDataType GetDataType()
    {
        return this.m_dataType;
    }

    public string GetName()
    {
        return this.m_name;
    }

    public bool HasClientProperty()
    {
        return this.m_client.HasValue;
    }

    public bool IsClient()
    {
        bool? client = this.m_client;
        return (!client.HasValue ? false : client.Value);
    }

    public void SetClient(bool client)
    {
        this.m_client = new bool?(client);
    }

    public void SetComment(string comment)
    {
        this.m_comment = comment;
    }

    public void SetDataType(DbfDataType dataType)
    {
        this.m_dataType = dataType;
    }

    public void SetName(string name)
    {
        this.m_name = name;
    }

    public override string ToString()
    {
        return string.Format("name={0} dataType={1} comment={2}", this.m_name, this.m_dataType, this.m_comment);
    }
}

