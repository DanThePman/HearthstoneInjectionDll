using System;
using UnityEngine;

public class CreditsScene : Scene
{
    private bool m_unloading;

    protected override void Awake()
    {
        base.Awake();
        AssetLoader.Get().LoadUIScreen("Credits", new AssetLoader.GameObjectCallback(this.OnUIScreenLoaded), null, false);
        if (InactivePlayerKicker.Get() != null)
        {
            InactivePlayerKicker.Get().SetShouldCheckForInactivity(false);
        }
    }

    public override bool IsUnloading()
    {
        return this.m_unloading;
    }

    private void OnUIScreenLoaded(string name, GameObject screen, object callbackData)
    {
        if (screen == null)
        {
            Debug.LogError(string.Format("CreditsScene.OnUIScreenLoaded() - failed to load screen {0}", name));
        }
    }

    public override void Unload()
    {
        this.m_unloading = true;
        CreditsDisplay.Get().Unload();
        if (InactivePlayerKicker.Get() != null)
        {
            InactivePlayerKicker.Get().SetShouldCheckForInactivity(true);
        }
        this.m_unloading = false;
    }
}

