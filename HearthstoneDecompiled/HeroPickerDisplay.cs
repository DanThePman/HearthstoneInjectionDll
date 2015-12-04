using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class HeroPickerDisplay : MonoBehaviour
{
    private static readonly Vector3 HERO_PICKER_END_POSITION;
    private static readonly PlatformDependentValue<Vector3> HERO_PICKER_START_POSITION;
    public GameObject m_deckPickerBone;
    private DeckPickerTrayDisplay m_deckPickerTray;
    private static HeroPickerDisplay s_instance;

    static HeroPickerDisplay()
    {
        PlatformDependentValue<Vector3> value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(-57.36467f, 2.4869f, -28.6f),
            Phone = new Vector3(-66.4f, 2.4869f, -28.6f)
        };
        HERO_PICKER_START_POSITION = value2;
        HERO_PICKER_END_POSITION = new Vector3(40.6f, 2.4869f, -28.6f);
    }

    private void Awake()
    {
        base.transform.localPosition = (Vector3) HERO_PICKER_START_POSITION;
        AssetLoader.Get().LoadActor((UniversalInputManager.UsePhoneUI == null) ? "DeckPickerTray" : "DeckPickerTray_phone", new AssetLoader.GameObjectCallback(this.DeckPickerTrayLoaded), null, false);
        if (s_instance != null)
        {
            Debug.LogWarning("HeroPickerDisplay is supposed to be a singleton, but a second instance of it is being created!");
        }
        s_instance = this;
    }

    private void DeckPickerTrayLoaded(string name, GameObject go, object callbackData)
    {
        this.m_deckPickerTray = go.GetComponent<DeckPickerTrayDisplay>();
        this.m_deckPickerTray.SetHeaderText(GameStrings.Get((SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL) ? "GLUE_CREATE_DECK" : "GLOBAL_TAVERN_BRAWL"));
        this.m_deckPickerTray.transform.parent = base.transform;
        this.m_deckPickerTray.transform.localScale = this.m_deckPickerBone.transform.localScale;
        this.m_deckPickerTray.transform.localPosition = this.m_deckPickerBone.transform.localPosition;
        this.m_deckPickerTray.Init();
        this.ShowTray();
    }

    public static HeroPickerDisplay Get()
    {
        return s_instance;
    }

    public void HideTray(float delay = 0f)
    {
        object[] args = new object[] { "position", (Vector3) HERO_PICKER_START_POSITION, "time", 0.5f, "isLocal", true, "oncomplete", "KillHeroPicker", "oncompletetarget", base.gameObject, "easeType", iTween.EaseType.easeInCubic, "delay", delay };
        iTween.MoveTo(base.gameObject, iTween.Hash(args));
    }

    private void KillHeroPicker()
    {
        this.m_deckPickerTray.Unload();
        UnityEngine.Object.DestroyImmediate(base.gameObject);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void ShowTray()
    {
        object[] args = new object[] { "position", HERO_PICKER_END_POSITION, "time", 0.5f, "isLocal", true, "easeType", iTween.EaseType.easeOutBounce };
        iTween.MoveTo(base.gameObject, iTween.Hash(args));
    }
}

