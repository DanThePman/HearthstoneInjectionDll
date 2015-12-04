using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TextureOffsetStates : MonoBehaviour
{
    private string m_currentState;
    private Material m_originalMaterial;
    public TextureOffsetState[] m_states;

    private void Awake()
    {
        this.m_originalMaterial = base.GetComponent<Renderer>().sharedMaterial;
    }

    public string CurrentState
    {
        get
        {
            return this.m_currentState;
        }
        set
        {
            <>c__AnonStorey390 storey = new <>c__AnonStorey390 {
                value = value
            };
            TextureOffsetState state = Enumerable.FirstOrDefault<TextureOffsetState>(this.m_states, new Func<TextureOffsetState, bool>(storey.<>m__291));
            if (state != null)
            {
                this.m_currentState = storey.value;
                if (state.Material == null)
                {
                    base.GetComponent<Renderer>().material = this.m_originalMaterial;
                }
                else
                {
                    base.GetComponent<Renderer>().material = state.Material;
                }
                base.GetComponent<Renderer>().material.mainTextureOffset = state.Offset;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <>c__AnonStorey390
    {
        internal string value;

        internal bool <>m__291(TextureOffsetState s)
        {
            return s.Name.Equals(this.value, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

