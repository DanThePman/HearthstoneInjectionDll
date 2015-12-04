using System;
using UnityEngine;

public class SpriteSheet : MonoBehaviour
{
    public int _uvTieX = 1;
    public int _uvTieY = 1;
    public float m_fps = 30f;
    private int m_LastIdx = -1;
    private float m_NextFrame;
    public bool m_Old_Mode;
    private Vector2 m_Size = Vector2.one;
    private int m_X;
    private int m_Y;

    private void Start()
    {
        this.m_NextFrame = UnityEngine.Time.timeSinceLevelLoad + (1f / this.m_fps);
        if (base.GetComponent<Renderer>() == null)
        {
            Debug.LogError("SpriteSheet needs a Renderer on: " + base.gameObject.name);
            base.enabled = false;
        }
        this.m_Size = new Vector2(1f / ((float) this._uvTieX), 1f / ((float) this._uvTieY));
    }

    private void Update()
    {
        if (this.m_Old_Mode)
        {
            int num = (int) ((UnityEngine.Time.time * this.m_fps) % ((float) (this._uvTieX * this._uvTieY)));
            if (num != this.m_LastIdx)
            {
                base.GetComponent<Renderer>().material.mainTextureOffset = new Vector2((num % this._uvTieX) * this.m_Size.x, (1f - this.m_Size.y) - ((num / this._uvTieY) * this.m_Size.y));
                base.GetComponent<Renderer>().material.mainTextureScale = this.m_Size;
                this.m_LastIdx = num;
            }
        }
        else if (UnityEngine.Time.timeSinceLevelLoad >= this.m_NextFrame)
        {
            this.m_X++;
            if (this.m_X > (this._uvTieX - 1))
            {
                this.m_Y++;
                this.m_X = 0;
            }
            if (this.m_Y > (this._uvTieY - 1))
            {
                this.m_Y = 0;
            }
            base.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(this.m_X * this.m_Size.x, 1f - (this.m_Y * this.m_Size.y));
            base.GetComponent<Renderer>().material.mainTextureScale = this.m_Size;
            this.m_NextFrame = UnityEngine.Time.timeSinceLevelLoad + (1f / this.m_fps);
        }
    }
}

