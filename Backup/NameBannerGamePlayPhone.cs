using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NameBannerGamePlayPhone : MonoBehaviour
{
    public float FUDGE_FACTOR = 0.1915f;
    public GameObject m_alphaBanner;
    public GameObject m_alphaBannerMiddle;
    public Transform m_nameBone;
    private Transform m_nameBoneToUse;
    public UberText m_playerName;
    private Player.Side m_playerSide;
    private const float UNKNOWN_NAME_WAIT = 5f;

    private void Awake()
    {
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        this.m_playerName.Text = string.Empty;
        this.m_nameBoneToUse = this.m_nameBone;
    }

    public void FadeIn()
    {
        iTween.FadeTo(this.m_alphaBanner.gameObject, 1f, 1f);
        iTween.FadeTo(this.m_playerName.gameObject, 1f, 1f);
    }

    public Player.Side GetPlayerSide()
    {
        return this.m_playerSide;
    }

    private bool IsReady(Player p)
    {
        if (p == null)
        {
            return false;
        }
        if (p.GetName() == null)
        {
            return false;
        }
        return true;
    }

    public void SetName(string name)
    {
        this.m_playerName.Text = name;
        Vector3 vector = TransformUtil.ComputeWorldScale(this.m_playerName.gameObject);
        float num = (this.FUDGE_FACTOR * vector.x) * this.m_playerName.GetTextWorldSpaceBounds().size.x;
        float num2 = (this.m_playerName.GetTextWorldSpaceBounds().size.x * vector.x) + num;
        float x = this.m_alphaBannerMiddle.GetComponent<Renderer>().bounds.size.x;
        if (num2 > x)
        {
            TransformUtil.SetLocalScaleX(this.m_alphaBanner, num2 / x);
        }
    }

    public void SetPlayerSide(Player.Side side)
    {
        this.m_playerSide = side;
        this.UpdateAnchor();
        base.StartCoroutine(this.UpdateName());
        base.StartCoroutine(this.UpdateUnknownName());
    }

    public void Unload()
    {
        UnityEngine.Object.DestroyImmediate(base.gameObject);
    }

    private void Update()
    {
        this.UpdateAnchor();
    }

    private void UpdateAnchor()
    {
        if (this.m_playerSide == Player.Side.OPPOSING)
        {
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateName()
    {
        return new <UpdateName>c__IteratorC2 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator UpdateUnknownName()
    {
        return new <UpdateUnknownName>c__IteratorC3 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <UpdateName>c__IteratorC2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Map<int, Player>.ValueCollection.Enumerator <$s_601>__2;
        internal NameBannerGamePlayPhone <>f__this;
        internal Player <p>__0;
        internal Player <player>__3;
        internal Map<int, Player> <playerMap>__1;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if (GameState.Get().GetPlayerMap().Count == 0)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_01E5;
                    }
                    this.<p>__0 = null;
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_0129;

                case 4:
                    goto Label_01C7;

                default:
                    goto Label_01E3;
            }
            while (this.<p>__0 == null)
            {
                this.<playerMap>__1 = GameState.Get().GetPlayerMap();
                this.<$s_601>__2 = this.<playerMap>__1.Values.GetEnumerator();
                try
                {
                    while (this.<$s_601>__2.MoveNext())
                    {
                        this.<player>__3 = this.<$s_601>__2.Current;
                        if (this.<player>__3.GetSide() == this.<>f__this.m_playerSide)
                        {
                            this.<p>__0 = this.<player>__3;
                            goto Label_00F3;
                        }
                    }
                }
                finally
                {
                    this.<$s_601>__2.Dispose();
                }
            Label_00F3:
                this.$current = null;
                this.$PC = 2;
                goto Label_01E5;
            }
        Label_0129:
            while (this.<p>__0.GetName() == null)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_01E5;
            }
            this.<>f__this.SetName(this.<p>__0.GetName());
            this.<>f__this.m_nameBoneToUse = this.<>f__this.m_nameBone;
            this.<>f__this.m_alphaBanner.SetActive(true);
            this.<>f__this.m_playerName.transform.position = this.<>f__this.m_nameBoneToUse.position;
            if (GameMgr.Get().IsTutorial())
            {
                goto Label_01E3;
            }
        Label_01C7:
            while (this.<p>__0.GetHero().GetClass() == TAG_CLASS.INVALID)
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_01E5;
            }
            this.$PC = -1;
        Label_01E3:
            return false;
        Label_01E5:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateUnknownName>c__IteratorC3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal NameBannerGamePlayPhone <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = new WaitForSeconds(5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (!(this.<>f__this.m_playerName.Text != string.Empty))
                    {
                        this.<>f__this.SetName(GameStrings.Get("GAMEPLAY_UNKNOWN_OPPONENT_NAME"));
                        this.$PC = -1;
                        break;
                    }
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

