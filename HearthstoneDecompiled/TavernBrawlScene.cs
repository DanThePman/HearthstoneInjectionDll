using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class TavernBrawlScene : Scene
{
    private bool m_collectionManagerNeeded;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public String_MobileOverride m_CollectionManagerPrefab;
    private bool m_collectionManagerPrefabLoaded;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public String_MobileOverride m_TavernBrawlNoDeckPrefab;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public String_MobileOverride m_TavernBrawlPrefab;
    private bool m_tavernBrawlPrefabLoaded;
    private bool m_unloading;

    protected override void Awake()
    {
        base.Awake();
    }

    public override bool IsUnloading()
    {
        return this.m_unloading;
    }

    [DebuggerHidden]
    private IEnumerator NotifySceneLoadedWhenReady()
    {
        return new <NotifySceneLoadedWhenReady>c__Iterator1E8 { <>f__this = this };
    }

    private void OnCollectionManagerLoaded(string name, GameObject screen, object callbackData)
    {
        this.m_collectionManagerPrefabLoaded = true;
        if (screen == null)
        {
            UnityEngine.Debug.LogError(string.Format("TavernBrawlScene.OnCollectionManagerLoaded() - failed to load screen {0}", name));
        }
    }

    private void OnServerDataReady()
    {
        this.m_collectionManagerNeeded = (TavernBrawlManager.Get().CurrentMission() != null) && TavernBrawlManager.Get().CurrentMission().canEditDeck;
        if (this.m_collectionManagerNeeded)
        {
            AssetLoader.Get().LoadUIScreen(FileUtils.GameAssetPathToName((string) this.m_TavernBrawlPrefab), new AssetLoader.GameObjectCallback(this.OnTavernBrawlLoaded), null, false);
            AssetLoader.Get().LoadUIScreen(FileUtils.GameAssetPathToName((string) this.m_CollectionManagerPrefab), new AssetLoader.GameObjectCallback(this.OnCollectionManagerLoaded), null, false);
        }
        else
        {
            AssetLoader.Get().LoadUIScreen(FileUtils.GameAssetPathToName((string) this.m_TavernBrawlNoDeckPrefab), new AssetLoader.GameObjectCallback(this.OnTavernBrawlLoaded), null, false);
        }
        base.StartCoroutine(this.NotifySceneLoadedWhenReady());
    }

    private void OnTavernBrawlLoaded(string name, GameObject screen, object callbackData)
    {
        this.m_tavernBrawlPrefabLoaded = true;
        if (screen == null)
        {
            UnityEngine.Debug.LogError(string.Format("TavernBrawlScene.OnTavernBrawlLoaded() - failed to load screen {0}", name));
        }
    }

    private void Start()
    {
        TavernBrawlManager.Get().EnsureScenarioDataReady(new TavernBrawlManager.CallbackEnsureServerDataReady(this.OnServerDataReady));
    }

    public override void Unload()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            BnetBar.Get().ToggleActive(true);
        }
        this.m_unloading = true;
        if (CollectionManagerDisplay.Get() != null)
        {
            CollectionManagerDisplay.Get().Unload();
        }
        if (TavernBrawlDisplay.Get() != null)
        {
            TavernBrawlDisplay.Get().Unload();
        }
        Network.SendAckCardsSeen();
        this.m_unloading = false;
    }

    private void Update()
    {
        Network.Get().ProcessNetwork();
    }

    [CompilerGenerated]
    private sealed class <NotifySceneLoadedWhenReady>c__Iterator1E8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TavernBrawlScene <>f__this;
        internal CollectionDeck <currentDeck>__1;
        internal TavernBrawlMission <currentMission>__0;

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
                    if (!this.<>f__this.m_tavernBrawlPrefabLoaded)
                    {
                        this.$current = 0;
                        this.$PC = 1;
                        goto Label_00FD;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_00FB;
            }
            while (this.<>f__this.m_collectionManagerNeeded && !this.<>f__this.m_collectionManagerPrefabLoaded)
            {
                this.$current = 0;
                this.$PC = 2;
                goto Label_00FD;
            }
            this.<currentMission>__0 = TavernBrawlManager.Get().CurrentMission();
            this.<currentDeck>__1 = TavernBrawlManager.Get().CurrentDeck();
            if (((this.<currentMission>__0 != null) && this.<currentMission>__0.canCreateDeck) && (this.<currentDeck>__1 != null))
            {
                CollectionManagerDisplay.Get().ShowTavernBrawlDeck(this.<currentDeck>__1.ID);
            }
            SceneMgr.Get().NotifySceneLoaded();
            this.$PC = -1;
        Label_00FB:
            return false;
        Label_00FD:
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

