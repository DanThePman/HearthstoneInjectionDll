using System;
using UnityEngine;

public class FatalErrorScene : Scene
{
    protected override void Awake()
    {
        AssetLoader.Get().LoadGameObject("FatalErrorScreen", new AssetLoader.GameObjectCallback(this.OnFatalErrorScreenLoaded), null, false);
        base.Awake();
        Navigation.Clear();
        Network.AppAbort();
        if (DialogManager.Get() != null)
        {
            Log.Mike.Print("FatalErrorScene.Awake() - calling DialogManager.Get().Suppress()", new object[0]);
            DialogManager.Get().Suppress(true);
        }
        foreach (Camera camera in Camera.allCameras)
        {
            FullScreenEffects component = camera.GetComponent<FullScreenEffects>();
            if (component != null)
            {
                component.Disable();
            }
        }
    }

    private void OnFatalErrorScreenLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            base.gameObject.AddComponent<FatalErrorDialog>();
        }
    }

    private void Start()
    {
        SceneMgr.Get().NotifySceneLoaded();
    }

    public override void Unload()
    {
        DialogManager.Get().Suppress(false);
    }
}

