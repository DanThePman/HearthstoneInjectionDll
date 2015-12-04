using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class KelThuzad_StealTurn : Spell
{
    public GameObject m_Lightning;

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.StartCoroutine(this.SpellEffect(prevStateType));
        base.OnAction(prevStateType);
    }

    [DebuggerHidden]
    private IEnumerator SpellEffect(SpellStateType prevStateType)
    {
        return new <SpellEffect>c__Iterator215();
    }

    [CompilerGenerated]
    private sealed class <SpellEffect>c__Iterator215 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
                    this.$current = new WaitForSeconds(0.25f);
                    this.$PC = 1;
                    goto Label_00D6;

                case 1:
                    if (TurnTimer.Get() != null)
                    {
                        TurnTimer.Get().OnEndTurnRequested();
                    }
                    EndTurnButton.Get().m_EndTurnButtonMesh.GetComponent<Animation>()["ENDTURN_YOUR_TIMER_DONE"].speed = 0.7f;
                    EndTurnButton.Get().OnTurnTimerEnded(true);
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 2;
                    goto Label_00D6;

                case 2:
                    EndTurnButton.Get().m_EndTurnButtonMesh.GetComponent<Animation>()["ENDTURN_YOUR_TIMER_DONE"].speed = 1f;
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00D6:
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
}

