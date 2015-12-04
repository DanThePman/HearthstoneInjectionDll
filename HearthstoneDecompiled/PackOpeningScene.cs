using System;
using UnityEngine;

public class PackOpeningScene : Scene
{
    private PackOpening m_packOpening;

    protected override void Awake()
    {
        base.Awake();
        string screenName = (UniversalInputManager.UsePhoneUI == null) ? "PackOpening" : "PackOpening_phone";
        AssetLoader.Get().LoadUIScreen(screenName, new AssetLoader.GameObjectCallback(this.OnUIScreenLoaded), null, false);
    }

    private void OnUIScreenLoaded(string name, GameObject screen, object callbackData)
    {
        if (screen == null)
        {
            Debug.LogError(string.Format("PackOpeningScene.OnPackOpeningLoaded() - failed to load {0}", name));
        }
        else
        {
            this.m_packOpening = screen.GetComponent<PackOpening>();
            if (this.m_packOpening == null)
            {
                Debug.LogError(string.Format("PackOpeningScene.OnPackOpeningLoaded() - {0} did not have a {1} component", name, typeof(PackOpening)));
            }
        }
    }

    public override void Unload()
    {
        base.Unload();
        DefLoader.Get().ClearCardDefs();
    }

    private void Update()
    {
        Network.Get().ProcessNetwork();
    }
}

