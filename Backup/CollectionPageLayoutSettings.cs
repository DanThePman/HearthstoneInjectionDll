using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class CollectionPageLayoutSettings
{
    [CustomEditField(ListTable=true)]
    public List<Variables> m_layoutVariables = new List<Variables>();

    public Variables GetVariables(CollectionManagerDisplay.ViewMode mode)
    {
        <GetVariables>c__AnonStorey2D3 storeyd = new <GetVariables>c__AnonStorey2D3 {
            mode = mode
        };
        Variables variables = this.m_layoutVariables.Find(new Predicate<Variables>(storeyd.<>m__A7));
        if (variables == null)
        {
            return new Variables();
        }
        return variables;
    }

    [CompilerGenerated]
    private sealed class <GetVariables>c__AnonStorey2D3
    {
        internal CollectionManagerDisplay.ViewMode mode;

        internal bool <>m__A7(CollectionPageLayoutSettings.Variables v)
        {
            return (this.mode == v.m_ViewMode);
        }
    }

    [Serializable]
    public class Variables
    {
        public int m_ColumnCount = 4;
        public float m_ColumnSpacing;
        public Vector3 m_Offset;
        public int m_RowCount = 2;
        public float m_RowSpacing;
        public float m_Scale;
        public CollectionManagerDisplay.ViewMode m_ViewMode;
    }
}

