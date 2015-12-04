using PegasusShared;
using System;
using UnityEngine;

public class DebugMissionButton : PegUIElement
{
    public string m_characterPrefabName;
    public GameObject m_heroImage;
    private Actor m_heroPowerActor;
    private FullDef m_heroPowerDef;
    private GameObject m_heroPowerObject;
    public string m_introline;
    public int m_missionId;
    private bool m_mousedOver;
    public UberText m_name;

    private void OnCardDefLoaded(string cardID, CardDef cardDef, object userData)
    {
        this.m_heroImage.GetComponent<Renderer>().material.mainTexture = cardDef.GetPortraitTexture();
    }

    private void OnHeroPowerActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (!this.m_mousedOver)
        {
            UnityEngine.Object.Destroy(actorObject);
        }
        if (this.m_heroPowerActor != null)
        {
            UnityEngine.Object.Destroy(this.m_heroPowerActor.gameObject);
        }
        if ((this == null) || (base.gameObject == null))
        {
            UnityEngine.Object.Destroy(actorObject);
        }
        if (actorObject != null)
        {
            this.m_heroPowerActor = actorObject.GetComponent<Actor>();
            actorObject.transform.parent = base.gameObject.transform;
            this.m_heroPowerActor.SetCardDef(this.m_heroPowerDef.GetCardDef());
            this.m_heroPowerActor.SetEntityDef(this.m_heroPowerDef.GetEntityDef());
            this.m_heroPowerActor.UpdateAllComponents();
            actorObject.transform.position = base.transform.position + new Vector3(15f, 0f, 0f);
            actorObject.transform.localScale = Vector3.one;
            iTween.ScaleTo(actorObject, new Vector3(7f, 7f, 7f), 0.5f);
            SceneUtils.SetLayer(actorObject, GameLayer.Tooltip);
        }
    }

    private void OnHeroPowerDefLoaded(string cardID, FullDef def, object userData)
    {
        this.m_heroPowerDef = def;
        if (this.m_mousedOver)
        {
            AssetLoader.Get().LoadActor("History_HeroPower_Opponent", new AssetLoader.GameObjectCallback(this.OnHeroPowerActorLoaded), null, false);
        }
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_mousedOver = false;
        base.OnOut(oldState);
        if (this.m_heroPowerActor != null)
        {
            UnityEngine.Object.Destroy(this.m_heroPowerActor.gameObject);
        }
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        this.m_mousedOver = true;
        base.OnOver(oldState);
        if (!string.IsNullOrEmpty(GameUtils.GetMissionHeroPowerCardId(this.m_missionId)))
        {
            DefLoader.Get().LoadFullDef(GameUtils.GetMissionHeroPowerCardId(this.m_missionId), new DefLoader.LoadDefCallback<FullDef>(this.OnHeroPowerDefLoaded));
        }
    }

    protected override void OnRelease()
    {
        if (!string.IsNullOrEmpty(this.m_introline))
        {
            if (string.IsNullOrEmpty(this.m_characterPrefabName))
            {
                NotificationManager.Get().CreateKTQuote(this.m_introline, this.m_introline, true);
            }
            else
            {
                NotificationManager.Get().CreateCharacterQuote(this.m_characterPrefabName, GameStrings.Get(this.m_introline), this.m_introline, true, 0f, CanvasAnchor.BOTTOM_LEFT);
            }
        }
        base.OnRelease();
        long selectedDeckID = DeckPickerTrayDisplay.Get().GetSelectedDeckID();
        GameMgr.Get().FindGame(GameType.GT_VS_AI, this.m_missionId, selectedDeckID, 0L);
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void Start()
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(this.m_missionId);
        if (record == null)
        {
            object[] messageArgs = new object[] { this.m_missionId };
            Error.AddDevWarning("Error", "scenario {0} does not exist in the DBF", messageArgs);
        }
        else
        {
            if (this.m_name != null)
            {
                this.m_name.Text = record.GetLocString("SHORT_NAME");
            }
            int @int = record.GetInt("CLIENT_PLAYER2_HERO_CARD_ID");
            if (@int == 0)
            {
                @int = record.GetInt("PLAYER2_HERO_CARD_ID");
            }
            string cardId = GameUtils.TranslateDbIdToCardId(@int);
            if (cardId != null)
            {
                DefLoader.Get().LoadCardDef(cardId, new DefLoader.LoadDefCallback<CardDef>(this.OnCardDefLoaded), null, null);
            }
        }
    }
}

