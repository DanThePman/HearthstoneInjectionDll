using System;
using UnityEngine;

public class ZoneSecret : Zone
{
    private const float LAYOUT_ANIM_SEC = 1f;
    private const float MAX_LAYOUT_PYRAMID_LEVEL = 2f;

    private void Awake()
    {
        if (GameState.Get() != null)
        {
            GameState.Get().RegisterGameOverListener(new GameState.GameOverCallback(this.OnGameOver), null);
        }
    }

    private bool CanAnimateCard(Card card)
    {
        if (card.IsDoNotSort())
        {
            return false;
        }
        return true;
    }

    private void OnGameOver(object userData)
    {
        if (((TAG_PLAYSTATE) base.GetController().GetTag<TAG_PLAYSTATE>(GAME_TAG.PLAYSTATE)) != TAG_PLAYSTATE.WON)
        {
            for (int i = 0; i < base.m_cards.Count; i++)
            {
                Card card = base.m_cards[i];
                if (this.CanAnimateCard(card))
                {
                    card.HideCard();
                }
            }
        }
    }

    public override void UpdateLayout()
    {
        base.m_updatingLayout = true;
        if (base.IsBlockingLayout())
        {
            base.UpdateLayoutFinished();
        }
        else if (UniversalInputManager.UsePhoneUI != null)
        {
            this.UpdateLayout_Phone();
        }
        else
        {
            this.UpdateLayout_Default();
        }
    }

    private void UpdateLayout_Default()
    {
        Vector2 vector = new Vector2(1f, 2f);
        if (base.m_controller != null)
        {
            Card heroCard = base.m_controller.GetHeroCard();
            if (heroCard != null)
            {
                Bounds bounds = heroCard.GetActor().GetMeshRenderer().bounds;
                vector.x = bounds.extents.x;
                vector.y = bounds.extents.z * 0.9f;
            }
        }
        float num = 0.6f * vector.y;
        int num2 = 0;
        for (int i = 0; i < base.m_cards.Count; i++)
        {
            Card card = base.m_cards[i];
            if (this.CanAnimateCard(card))
            {
                float num6;
                card.ShowCard();
                Vector3 position = base.transform.position;
                float objA = (i + 1) >> 1;
                int num5 = i & 1;
                if (objA > 2f)
                {
                    num6 = 1f;
                }
                else if (object.Equals(objA, 1f))
                {
                    num6 = 0.6f;
                }
                else
                {
                    num6 = objA / 2f;
                }
                if (num5 == 0)
                {
                    position.x += vector.x * num6;
                }
                else
                {
                    position.x -= vector.x * num6;
                }
                position.z -= vector.y * (num6 * num6);
                if (objA > 2f)
                {
                    position.z -= num * (objA - 2f);
                }
                iTween.Stop(card.gameObject);
                ZoneTransitionStyle transitionStyle = card.GetTransitionStyle();
                card.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
                if (transitionStyle == ZoneTransitionStyle.INSTANT)
                {
                    card.EnableTransitioningZones(false);
                    card.transform.position = position;
                    card.transform.rotation = base.transform.rotation;
                    card.transform.localScale = base.transform.localScale;
                }
                else
                {
                    card.EnableTransitioningZones(true);
                    num2++;
                    iTween.MoveTo(card.gameObject, position, 1f);
                    iTween.RotateTo(card.gameObject, base.transform.localEulerAngles, 1f);
                    iTween.ScaleTo(card.gameObject, base.transform.localScale, 1f);
                }
            }
        }
        if (num2 > 0)
        {
            base.StartFinishLayoutTimer(1f);
        }
        else
        {
            base.UpdateLayoutFinished();
        }
    }

    private void UpdateLayout_Phone()
    {
        for (int i = 0; i < base.m_cards.Count; i++)
        {
            Card card = base.m_cards[i];
            if (this.CanAnimateCard(card))
            {
                card.EnableTransitioningZones(false);
                iTween.Stop(card.gameObject);
                if (i == 0)
                {
                    if (!card.IsShown())
                    {
                        card.ShowExhaustedChange(card.GetEntity().IsExhausted());
                        card.ShowCard();
                    }
                    card.GetActor().UpdateAllComponents();
                }
                card.transform.position = base.transform.position;
                card.transform.rotation = base.transform.rotation;
                card.transform.localScale = base.transform.localScale;
            }
        }
        base.UpdateLayoutFinished();
    }
}

