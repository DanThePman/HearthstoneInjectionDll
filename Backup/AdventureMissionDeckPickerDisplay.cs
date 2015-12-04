using System;
using UnityEngine;

[CustomEditClass]
public class AdventureMissionDeckPickerDisplay : MonoBehaviour
{
    private DeckPickerTrayDisplay m_deckPickerTray;
    public GameObject m_deckPickerTrayContainer;

    private void Awake()
    {
        GameObject obj2 = AssetLoader.Get().LoadActor((UniversalInputManager.UsePhoneUI == null) ? "DeckPickerTray" : "DeckPickerTray_phone", false, false);
        if (obj2 == null)
        {
            Debug.LogError("Unable to load DeckPickerTray.");
        }
        else
        {
            this.m_deckPickerTray = obj2.GetComponent<DeckPickerTrayDisplay>();
            if (this.m_deckPickerTray == null)
            {
                Debug.LogError("DeckPickerTrayDisplay component not found in DeckPickerTray object.");
            }
            else
            {
                if (this.m_deckPickerTrayContainer != null)
                {
                    GameUtils.SetParent(this.m_deckPickerTray, this.m_deckPickerTrayContainer, false);
                }
                this.m_deckPickerTray.AddDeckTrayLoadedListener(new DeckPickerTrayDisplay.DeckTrayLoaded(this.OnTrayLoaded));
                this.m_deckPickerTray.Init();
                this.m_deckPickerTray.SetPlayButtonText(GameStrings.Get("GLOBAL_PLAY"));
                AdventureConfig config = AdventureConfig.Get();
                AdventureDbId selectedAdventure = config.GetSelectedAdventure();
                AdventureModeDbId selectedMode = config.GetSelectedMode();
                string locString = GameUtils.GetAdventureDataRecord((int) selectedAdventure, (int) selectedMode).GetLocString("NAME");
                this.m_deckPickerTray.SetHeaderText(locString);
            }
        }
    }

    private void OnTrayLoaded()
    {
        AdventureSubScene component = base.GetComponent<AdventureSubScene>();
        if (component != null)
        {
            component.SetIsLoaded(true);
        }
    }
}

