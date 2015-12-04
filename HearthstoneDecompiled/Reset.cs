using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class Reset : Scene
{
    private void Start()
    {
        SceneMgr.Get().NotifySceneLoaded();
        base.StartCoroutine("WaitThenReset");
    }

    [DebuggerHidden]
    private IEnumerator WaitThenReset()
    {
        return new <WaitThenReset>c__Iterator1B6();
    }

    [CompilerGenerated]
    private sealed class <WaitThenReset>c__Iterator1B6 : IDisposable, IEnumerator, IEnumerator<object>
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
                case 1:
                    if (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    ApplicationMgr.Get().Reset();
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

