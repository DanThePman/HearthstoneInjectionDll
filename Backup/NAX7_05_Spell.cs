using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NAX7_05_Spell : Spell
{
    protected override void OnBirth(SpellStateType prevStateType)
    {
        base.StartCoroutine(this.SpellEffect(prevStateType));
    }

    [DebuggerHidden]
    private IEnumerator SpellEffect(SpellStateType prevStateType)
    {
        return new <SpellEffect>c__Iterator29 { prevStateType = prevStateType, <$>prevStateType = prevStateType, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <SpellEffect>c__Iterator29 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SpellStateType <$>prevStateType;
        internal NAX7_05_Spell <>f__this;
        internal Board <board>__0;
        internal Transform <crystal>__1;
        internal PlayMakerFSM <fsm>__2;
        internal SpellStateType prevStateType;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            this.$PC = -1;
            if (this.$PC == 0)
            {
                this.<board>__0 = Board.Get();
                this.<crystal>__1 = this.<board>__0.transform.FindChild("Board_NAX").FindChild("NAX_Crystal_Skinned");
                this.<fsm>__2 = this.<crystal>__1.GetComponent<PlayMakerFSM>();
                if (this.<fsm>__2 == null)
                {
                    UnityEngine.Debug.LogWarning("NAX7_05_Spell unable to get playmaker fsm");
                }
                else
                {
                    this.<fsm>__2.SendEvent("ClickTop");
                    this.<>f__this.OnBirth(this.prevStateType);
                    this.<>f__this.OnSpellFinished();
                }
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

