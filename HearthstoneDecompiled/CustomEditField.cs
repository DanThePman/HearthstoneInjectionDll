using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class CustomEditField : Attribute
{
    public bool AllowSceneObject = true;
    public bool Hide;
    public string Label;
    public bool ListSortable;
    public int ListSortPriority = -1;
    public bool ListTable;
    public string Parent;
    public string Range;
    public string Sections;
    public EditType T;

    public override string ToString()
    {
        if (this.Sections == null)
        {
            return this.T.ToString();
        }
        return string.Format("Sections={0} T={1}", this.Sections, this.T);
    }
}

