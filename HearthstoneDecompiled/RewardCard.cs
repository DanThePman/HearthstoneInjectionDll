using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class RewardCard : MonoBehaviour
{
    private Actor m_actor;
    private CardDef m_cardDef;
    private CardFlair m_cardFlair;
    public string m_CardID = string.Empty;
    private EntityDef m_entityDef;
    private GameLayer m_layer = GameLayer.IgnoreFullScreenEffects;
    private bool m_Ready;

    public void Death()
    {
        this.m_actor.ActivateSpell(SpellType.DEATH);
    }

    public bool IsReady()
    {
        return this.m_Ready;
    }

    public void LoadCard(CardRewardData cardData, GameLayer layer = 0x13)
    {
        this.m_layer = layer;
        this.m_CardID = cardData.CardID;
        this.m_cardFlair = new CardFlair(cardData.Premium);
        DefLoader.Get().LoadFullDef(this.m_CardID, new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
    }

    private void OnActorLoaded(string name, GameObject actorObject, object userData)
    {
        if (actorObject == null)
        {
            Debug.LogWarning(string.Format("RewardCard.OnActorLoaded() - FAILED to load actor \"{0}\"", name));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                Debug.LogWarning(string.Format("RewardCard.OnActorLoaded() - ERROR actor \"{0}\" has no Actor component", name));
            }
            else
            {
                this.m_actor = component;
                this.m_actor.TurnOffCollider();
                SceneUtils.SetLayer(component.gameObject, this.m_layer);
                this.m_actor.SetEntityDef(this.m_entityDef);
                this.m_actor.SetCardDef(this.m_cardDef);
                this.m_actor.SetCardFlair(this.m_cardFlair);
                this.m_actor.UpdateAllComponents();
                this.m_actor.transform.parent = base.transform;
                this.m_actor.transform.localPosition = Vector3.zero;
                this.m_actor.transform.localEulerAngles = new Vector3(270f, 0f, 0f);
                this.m_actor.transform.localScale = Vector3.one;
                this.m_actor.Show();
                this.m_Ready = true;
            }
        }
    }

    private void OnDestroy()
    {
        this.m_Ready = false;
    }

    private void OnFullDefLoaded(string cardId, FullDef fullDef, object userData)
    {
        if (fullDef == null)
        {
            Debug.LogWarning(string.Format("RewardCard.OnFullDefLoaded() - FAILED to load \"{0}\"", cardId));
        }
        else
        {
            this.m_entityDef = fullDef.GetEntityDef();
            this.m_cardDef = fullDef.GetCardDef();
            string handActor = ActorNames.GetHandActor(this.m_entityDef, this.m_cardFlair.Premium);
            AssetLoader.Get().LoadActor(handActor, new AssetLoader.GameObjectCallback(this.OnActorLoaded), null, false);
        }
    }
}

