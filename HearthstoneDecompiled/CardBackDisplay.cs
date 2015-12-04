using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CardBackDisplay : MonoBehaviour
{
    public Actor m_Actor;
    private CardBackManager m_CardBackManager;
    private bool m_FriendlySide = true;

    public void SetCardBack(bool friendlySide)
    {
        if (this.m_CardBackManager == null)
        {
            this.m_CardBackManager = CardBackManager.Get();
        }
        this.m_CardBackManager.UpdateCardBack(base.gameObject, friendlySide);
    }

    [DebuggerHidden]
    private IEnumerator SetCardBackDisplay()
    {
        return new <SetCardBackDisplay>c__Iterator1D { <>f__this = this };
    }

    private void Start()
    {
        this.m_CardBackManager = CardBackManager.Get();
        if (this.m_CardBackManager == null)
        {
            UnityEngine.Debug.LogError("Failed to get CardBackManager!");
            base.enabled = false;
        }
        this.UpdateCardBack();
    }

    public void UpdateCardBack()
    {
        if (this.m_CardBackManager != null)
        {
            base.StartCoroutine(this.SetCardBackDisplay());
        }
    }

    [CompilerGenerated]
    private sealed class <SetCardBackDisplay>c__Iterator1D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardBackDisplay <>f__this;
        internal Player <controller>__1;
        internal Entity <entity>__0;

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
                    if (this.<>f__this.m_Actor != null)
                    {
                        if (this.<>f__this.m_Actor.GetCardbackUpdateIgnore())
                        {
                            goto Label_0157;
                        }
                        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
                        {
                            break;
                        }
                        goto Label_00AF;
                    }
                    this.<>f__this.m_CardBackManager.UpdateCardBack(this.<>f__this.gameObject, true);
                    goto Label_0157;

                case 1:
                    break;

                default:
                    goto Label_0157;
            }
            while (this.<>f__this.m_Actor.GetEntity() == null)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
        Label_00AF:
            this.<>f__this.m_FriendlySide = true;
            this.<entity>__0 = this.<>f__this.m_Actor.GetEntity();
            if (this.<entity>__0 != null)
            {
                this.<controller>__1 = this.<entity>__0.GetController();
                if ((this.<controller>__1 != null) && (this.<controller>__1.GetSide() == Player.Side.OPPOSING))
                {
                    this.<>f__this.m_FriendlySide = false;
                }
            }
            this.<>f__this.m_CardBackManager.UpdateCardBack(this.<>f__this.gameObject, this.<>f__this.m_FriendlySide);
            this.<>f__this.m_Actor.SeedMaterialEffects();
            goto Label_0157;
            this.$PC = -1;
        Label_0157:
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

