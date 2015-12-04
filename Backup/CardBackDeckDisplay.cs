using System;
using UnityEngine;

public class CardBackDeckDisplay : MonoBehaviour
{
    private CardBackManager m_CardBackManager;
    public bool m_FriendlyDeck = true;

    private void Start()
    {
        this.m_CardBackManager = CardBackManager.Get();
        if (this.m_CardBackManager == null)
        {
            Debug.LogError("Failed to get CardBackManager!");
            base.enabled = false;
        }
        this.UpdateDeckCardBacks();
    }

    public void UpdateDeckCardBacks()
    {
        if (this.m_CardBackManager != null)
        {
            this.m_CardBackManager.UpdateDeck(base.gameObject, this.m_FriendlyDeck);
        }
    }
}

