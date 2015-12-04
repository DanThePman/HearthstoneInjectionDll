using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class RewardCardBack : MonoBehaviour
{
    private Actor m_actor;
    public GameObject m_cardbackBone;
    public int m_CardBackID = -1;
    public UberText m_cardbackName;
    public UberText m_cardbackTitle;
    private GameLayer m_layer = GameLayer.IgnoreFullScreenEffects;
    private bool m_Ready;

    private void Awake()
    {
    }

    public void Death()
    {
        this.m_actor.ActivateSpell(SpellType.DEATH);
    }

    public bool IsReady()
    {
        return this.m_Ready;
    }

    public void LoadCardBack(CardBackRewardData cardbackData, GameLayer layer = 0x13)
    {
        this.m_layer = layer;
        this.m_CardBackID = cardbackData.CardBackID;
        CardBackManager.Get().LoadCardBackByIndex(this.m_CardBackID, new CardBackManager.LoadCardBackData.LoadCardBackCallback(this.OnCardBackLoaded), "Card_Hidden");
    }

    private void OnCardBackLoaded(CardBackManager.LoadCardBackData cardbackData)
    {
        GameObject gameObject = cardbackData.m_GameObject;
        gameObject.transform.parent = this.m_cardbackBone.transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        gameObject.transform.localScale = Vector3.one;
        SceneUtils.SetLayer(gameObject, this.m_layer);
        this.m_actor = gameObject.GetComponent<Actor>();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_cardbackTitle.Text = "GLOBAL_SEASON_END_NEW_CARDBACK_TITLE_PHONE";
        }
        this.m_cardbackName.Text = cardbackData.m_Name;
        this.m_Ready = true;
    }

    private void OnDestroy()
    {
        this.m_Ready = false;
    }
}

