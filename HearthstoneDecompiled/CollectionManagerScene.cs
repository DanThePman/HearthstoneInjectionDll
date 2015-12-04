using System;
using UnityEngine;

[CustomEditClass]
public class CollectionManagerScene : Scene
{
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public String_MobileOverride m_CollectionManagerPrefab;
    private bool m_unloading;

    protected override void Awake()
    {
        base.Awake();
        AssetLoader.Get().LoadUIScreen(FileUtils.GameAssetPathToName((string) this.m_CollectionManagerPrefab), new AssetLoader.GameObjectCallback(this.OnUIScreenLoaded), null, false);
    }

    public override bool IsUnloading()
    {
        return this.m_unloading;
    }

    private void OnUIScreenLoaded(string name, GameObject screen, object callbackData)
    {
        if (screen == null)
        {
            Debug.LogError(string.Format("CollectionManagerScene.OnUIScreenLoaded() - failed to load screen {0}", name));
        }
    }

    public override void Unload()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            BnetBar.Get().ToggleActive(true);
        }
        this.m_unloading = true;
        CollectionManagerDisplay.Get().Unload();
        Network.SendAckCardsSeen();
        this.m_unloading = false;
    }

    private void Update()
    {
        Network.Get().ProcessNetwork();
    }
}

