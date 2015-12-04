using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class EchoOfMedivh : SpawnToHandSpell
{
    private Map<int, Card> m_originCards = new Map<int, Card>();

    private string GetCardIdForTarget(int targetIndex)
    {
        GameObject obj2 = base.m_targets[targetIndex];
        return obj2.GetComponent<Card>().GetEntity().GetCardId();
    }

    protected override Vector3 GetOriginForTarget(int targetIndex = 0)
    {
        Card card = this.m_originCards[targetIndex];
        return card.transform.position;
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        this.m_originCards.Clear();
        Player controller = base.GetSourceCard().GetEntity().GetController();
        ZonePlay battlefieldZone = controller.GetBattlefieldZone();
        if (controller.IsRevealed())
        {
            for (int i = 0; i < base.m_targets.Count; i++)
            {
                string cardIdForTarget = this.GetCardIdForTarget(i);
                for (int j = 0; j < battlefieldZone.GetCardCount(); j++)
                {
                    Card cardAtIndex = battlefieldZone.GetCardAtIndex(j);
                    if (cardAtIndex.GetPredictedZonePosition() == 0)
                    {
                        string cardId = cardAtIndex.GetEntity().GetCardId();
                        if ((cardIdForTarget == cardId) && !this.m_originCards.ContainsValue(cardAtIndex))
                        {
                            this.m_originCards.Add(i, cardAtIndex);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            int index = 0;
            for (int k = 0; k < base.m_targets.Count; k++)
            {
                Card card3 = battlefieldZone.GetCardAtIndex(index);
                this.m_originCards.Add(k, card3);
                index++;
            }
        }
        base.OnAction(prevStateType);
    }
}

