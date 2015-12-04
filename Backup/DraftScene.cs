using System;
using UnityEngine;

public class DraftScene : Scene
{
    private bool m_unloading;

    protected override void Awake()
    {
        base.Awake();
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_BUTTON_DRAFT);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            AssetLoader.Get().LoadUIScreen("Draft_phone", new AssetLoader.GameObjectCallback(this.OnPhoneUIScreenLoaded), null, false);
        }
        else
        {
            AssetLoader.Get().LoadUIScreen("Draft", new AssetLoader.GameObjectCallback(this.OnUIScreenLoaded), null, false);
        }
    }

    public override bool IsUnloading()
    {
        return this.m_unloading;
    }

    private void OnPhoneUIScreenLoaded(string name, GameObject screen, object callbackData)
    {
        if (screen == null)
        {
            Debug.LogError(string.Format("DraftScene.OnUIScreenLoaded() - failed to load screen {0}", name));
        }
        else
        {
            screen.transform.position = new Vector3(26.1f, 0f, -9.88f);
            screen.transform.localScale = new Vector3(1.38f, 1.38f, 1.38f);
        }
    }

    private void OnUIScreenLoaded(string name, GameObject screen, object callbackData)
    {
        if (screen == null)
        {
            Debug.LogError(string.Format("DraftScene.OnUIScreenLoaded() - failed to load screen {0}", name));
        }
        else
        {
            screen.transform.position = new Vector3(-0.5f, 1.27f, 0f);
        }
    }

    public override void Unload()
    {
        this.m_unloading = true;
        DraftDisplay.Get().Unload();
        this.m_unloading = false;
    }
}

