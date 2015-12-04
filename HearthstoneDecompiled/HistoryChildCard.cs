using System;
using UnityEngine;

public class HistoryChildCard : HistoryItem
{
    public void LoadActor()
    {
        string historyActor = ActorNames.GetHistoryActor(base.m_entity);
        AssetLoader.Get().LoadActor(historyActor, new AssetLoader.GameObjectCallback(this.LoadActorCallback), null, false);
    }

    private void LoadActorCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            Debug.LogWarning(string.Format("HistoryChildCard.LoadActorCallback() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                Debug.LogWarning(string.Format("HistoryChildCard.LoadActorCallback() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                base.m_mainCardActor = component;
                base.m_mainCardActor.SetCardFlair(base.m_entity.GetCardFlair());
                base.m_mainCardActor.SetHistoryChildCard(this);
                base.m_mainCardActor.UpdateAllComponents();
                SceneUtils.SetLayer(base.m_mainCardActor.gameObject, GameLayer.Tooltip);
                base.m_mainCardActor.Hide();
            }
        }
    }

    public void SetCardInfo(Entity entity, Texture bigTexture, int splatAmount, bool isDead, Material goldenMaterial)
    {
        base.m_entity = entity;
        base.m_bigCardPortraitTexture = bigTexture;
        base.m_splatAmount = splatAmount;
        base.dead = isDead;
        base.m_bigCardPotraitGoldenMaterial = goldenMaterial;
    }
}

