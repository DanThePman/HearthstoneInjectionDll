using System;
using UnityEngine;

public class CardBackSummon : MonoBehaviour
{
    private Actor m_Actor;
    private CardBackManager m_CardBackManager;

    private void Start()
    {
        this.UpdateEffect();
    }

    private void UpdateEchoTexture()
    {
        if (this.m_CardBackManager == null)
        {
            this.m_CardBackManager = CardBackManager.Get();
            if (this.m_CardBackManager == null)
            {
                Debug.LogError("CardBackSummonIn failed to get CardBackManager!");
                base.enabled = false;
            }
        }
        if (this.m_Actor == null)
        {
            this.m_Actor = SceneUtils.FindComponentInParents<Actor>(base.gameObject);
            if (this.m_Actor == null)
            {
                Debug.LogError("CardBackSummonIn failed to get Actor!");
            }
        }
        Texture mainTexture = base.GetComponent<Renderer>().material.mainTexture;
        if (this.m_CardBackManager.IsActorFriendly(this.m_Actor))
        {
            CardBack friendlyCardBack = this.m_CardBackManager.GetFriendlyCardBack();
            if (friendlyCardBack != null)
            {
                mainTexture = friendlyCardBack.m_HiddenCardEchoTexture;
            }
        }
        else
        {
            CardBack opponentCardBack = this.m_CardBackManager.GetOpponentCardBack();
            if (opponentCardBack != null)
            {
                mainTexture = opponentCardBack.m_HiddenCardEchoTexture;
            }
        }
        if (mainTexture != null)
        {
            base.GetComponent<Renderer>().material.mainTexture = mainTexture;
        }
    }

    public void UpdateEffect()
    {
        this.UpdateEchoTexture();
    }
}

