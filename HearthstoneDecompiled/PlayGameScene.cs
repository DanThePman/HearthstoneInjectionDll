using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class PlayGameScene : Scene
{
    private bool m_deckPickerIsLoaded;

    protected PlayGameScene()
    {
    }

    public abstract string GetScreenName();
    protected virtual bool IsLoaded()
    {
        return this.m_deckPickerIsLoaded;
    }

    public void OnDeckPickerLoaded()
    {
        this.m_deckPickerIsLoaded = true;
    }

    private void OnUIScreenLoaded(string name, GameObject screen, object callbackData)
    {
        if (screen == null)
        {
            UnityEngine.Debug.LogError(string.Format("PlayGameScene.OnUIScreenLoaded() - failed to load screen {0}", name));
        }
        else
        {
            base.StartCoroutine(this.WaitForAllToBeLoaded());
        }
    }

    public override void PreUnload()
    {
        if (DeckPickerTrayDisplay.Get() != null)
        {
            DeckPickerTrayDisplay.Get().PreUnload();
        }
    }

    protected void Start()
    {
        AssetLoader.Get().LoadUIScreen(this.GetScreenName(), new AssetLoader.GameObjectCallback(this.OnUIScreenLoaded), null, false);
    }

    public override void Unload()
    {
        DeckPickerTray.Get().Unload();
        this.m_deckPickerIsLoaded = false;
    }

    protected void Update()
    {
        Network.Get().ProcessNetwork();
    }

    [DebuggerHidden]
    private IEnumerator WaitForAllToBeLoaded()
    {
        return new <WaitForAllToBeLoaded>c__Iterator84 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitForAllToBeLoaded>c__Iterator84 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PlayGameScene <>f__this;

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
                    if (!this.<>f__this.IsLoaded())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    SceneMgr.Get().NotifySceneLoaded();
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

