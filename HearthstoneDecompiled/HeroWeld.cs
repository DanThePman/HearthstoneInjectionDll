using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HeroWeld : MonoBehaviour
{
    private Light[] m_lights;

    [DebuggerHidden]
    private IEnumerator DestroyWhenFinished()
    {
        return new <DestroyWhenFinished>c__Iterator282 { <>f__this = this };
    }

    public void DoAnim()
    {
        base.gameObject.SetActive(true);
        this.m_lights = base.gameObject.GetComponentsInChildren<Light>();
        foreach (Light light in this.m_lights)
        {
            light.enabled = true;
        }
        string name = "HeroWeldIn";
        base.gameObject.GetComponent<Animation>().Stop(name);
        base.gameObject.GetComponent<Animation>().Play(name);
        base.StartCoroutine(this.DestroyWhenFinished());
    }

    [CompilerGenerated]
    private sealed class <DestroyWhenFinished>c__Iterator282 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Light[] <$s_1801>__0;
        internal int <$s_1802>__1;
        internal HeroWeld <>f__this;
        internal Light <light>__2;

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
                    this.<$s_1801>__0 = this.<>f__this.m_lights;
                    this.<$s_1802>__1 = 0;
                    while (this.<$s_1802>__1 < this.<$s_1801>__0.Length)
                    {
                        this.<light>__2 = this.<$s_1801>__0[this.<$s_1802>__1];
                        this.<light>__2.enabled = false;
                        this.<$s_1802>__1++;
                    }
                    UnityEngine.Object.Destroy(this.<>f__this.gameObject);
                    this.$PC = -1;
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

