using System;
using UnityEngine;

public class Flipbook : MonoBehaviour
{
    public bool m_animate = true;
    private float m_flipbookFrame;
    private int m_flipbookLastOffset;
    public Vector2[] m_flipbookOffsets = new Vector2[] { new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(0.5f, 0f) };
    public bool m_flipbookRandom;
    public float m_flipbookRate = 15f;
    private bool m_flipbookReverse;
    public float m_RandomRateMax;
    public float m_RandomRateMin;
    public bool m_RandomRateRange;
    public bool m_reverse = true;

    public void SetIndex(int i)
    {
        if ((i < 0) || (i >= this.m_flipbookOffsets.Length))
        {
            if (i < 0)
            {
                this.m_flipbookLastOffset = 0;
            }
            else
            {
                this.m_flipbookLastOffset = this.m_flipbookOffsets.Length;
            }
            object[] args = new object[] { i };
            Log.Kyle.PrintError("m_flipbookOffsets index out of range: {0}", args);
        }
        else
        {
            base.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", this.m_flipbookOffsets[i]);
        }
    }

    private void Start()
    {
        if (this.m_RandomRateRange)
        {
            this.m_flipbookRate = UnityEngine.Random.Range(this.m_RandomRateMin, this.m_RandomRateMax);
        }
    }

    private void Update()
    {
        float flipbookRate = this.m_flipbookRate;
        if (flipbookRate != 0f)
        {
            bool flag = false;
            if (flipbookRate < 0f)
            {
                flipbookRate *= -1f;
                flag = true;
            }
            if (this.m_animate)
            {
                if (this.m_flipbookFrame > flipbookRate)
                {
                    int i = 0;
                    if (this.m_flipbookRandom)
                    {
                        int num3 = 0;
                        do
                        {
                            i = UnityEngine.Random.Range(0, this.m_flipbookOffsets.Length);
                            num3++;
                        }
                        while ((i == this.m_flipbookLastOffset) && (num3 < 100));
                        this.m_flipbookLastOffset = i;
                    }
                    else
                    {
                        if (flag)
                        {
                            this.m_flipbookLastOffset -= Mathf.FloorToInt(this.m_flipbookFrame / flipbookRate);
                            if (this.m_flipbookLastOffset < 0)
                            {
                                this.m_flipbookLastOffset = Mathf.FloorToInt((float) (this.m_flipbookOffsets.Length - Mathf.Abs(this.m_flipbookLastOffset)));
                                if (this.m_flipbookLastOffset < 0)
                                {
                                    this.m_flipbookLastOffset = this.m_flipbookOffsets.Length - 1;
                                }
                            }
                        }
                        else if (!this.m_flipbookReverse)
                        {
                            if (this.m_reverse)
                            {
                                if (this.m_flipbookLastOffset >= (this.m_flipbookOffsets.Length - 1))
                                {
                                    this.m_flipbookLastOffset = this.m_flipbookOffsets.Length - 1;
                                    this.m_flipbookReverse = true;
                                }
                                else
                                {
                                    this.m_flipbookLastOffset++;
                                }
                            }
                            else
                            {
                                this.m_flipbookLastOffset += Mathf.FloorToInt(this.m_flipbookFrame / flipbookRate);
                                if (this.m_flipbookLastOffset >= this.m_flipbookOffsets.Length)
                                {
                                    this.m_flipbookLastOffset = Mathf.FloorToInt((float) (this.m_flipbookLastOffset - this.m_flipbookOffsets.Length));
                                    if (this.m_flipbookLastOffset >= this.m_flipbookOffsets.Length)
                                    {
                                        this.m_flipbookLastOffset = 0;
                                    }
                                }
                            }
                        }
                        else if (this.m_flipbookLastOffset <= 0)
                        {
                            this.m_flipbookLastOffset = 1;
                            this.m_flipbookReverse = false;
                        }
                        else
                        {
                            this.m_flipbookLastOffset -= Mathf.FloorToInt(this.m_flipbookFrame / flipbookRate);
                            if (this.m_flipbookLastOffset < 0)
                            {
                                this.m_flipbookLastOffset = Mathf.FloorToInt((float) (this.m_flipbookOffsets.Length - Mathf.Abs(this.m_flipbookLastOffset)));
                            }
                            if (this.m_flipbookLastOffset < 0)
                            {
                                this.m_flipbookLastOffset = this.m_flipbookOffsets.Length - 1;
                            }
                        }
                        i = this.m_flipbookLastOffset;
                    }
                    this.m_flipbookFrame = 0f;
                    this.SetIndex(i);
                }
                this.m_flipbookFrame += UnityEngine.Time.deltaTime * 60f;
            }
        }
    }
}

