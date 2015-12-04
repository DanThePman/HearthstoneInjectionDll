using System;
using UnityEngine;

public class CollectionCardLock : MonoBehaviour
{
    public GameObject m_allyBg;
    public GameObject m_lockPlate;
    public GameObject m_lockPlateBone;
    public UberText m_lockText;
    public GameObject m_spellBg;
    public GameObject m_weaponBg;
    public GameObject m_weaponLockPlateBone;

    public void Hide()
    {
        base.gameObject.SetActive(false);
    }

    public void SetLockText(string text)
    {
        this.m_lockText.Text = text;
    }

    private void Start()
    {
    }

    public void UpdateLockVisual(EntityDef entityDef, CollectionCardVisual.LockType lockType)
    {
        if ((entityDef == null) || (lockType == CollectionCardVisual.LockType.NONE))
        {
            base.gameObject.SetActive(false);
        }
        else
        {
            GameObject allyBg;
            base.gameObject.SetActive(true);
            TAG_CARDTYPE cardType = entityDef.GetCardType();
            this.m_allyBg.SetActive(false);
            this.m_spellBg.SetActive(false);
            this.m_weaponBg.SetActive(false);
            switch (cardType)
            {
                case TAG_CARDTYPE.MINION:
                    allyBg = this.m_allyBg;
                    this.m_lockPlate.transform.localPosition = this.m_lockPlateBone.transform.localPosition;
                    break;

                case TAG_CARDTYPE.SPELL:
                    allyBg = this.m_spellBg;
                    this.m_lockPlate.transform.localPosition = this.m_lockPlateBone.transform.localPosition;
                    break;

                case TAG_CARDTYPE.WEAPON:
                    allyBg = this.m_weaponBg;
                    this.m_lockPlate.transform.localPosition = this.m_weaponLockPlateBone.transform.localPosition;
                    break;

                default:
                    allyBg = this.m_spellBg;
                    break;
            }
            float num = 0f;
            switch (lockType)
            {
                case CollectionCardVisual.LockType.MAX_COPIES_IN_DECK:
                {
                    num = 0f;
                    int num2 = !entityDef.IsElite() ? 2 : 1;
                    object[] args = new object[] { num2 };
                    this.SetLockText(GameStrings.Format("GLUE_COLLECTION_LOCK_MAX_DECK_COPIES", args));
                    break;
                }
                case CollectionCardVisual.LockType.NO_MORE_INSTANCES:
                    num = 1f;
                    this.SetLockText(GameStrings.Get("GLUE_COLLECTION_LOCK_NO_MORE_INSTANCES"));
                    break;
            }
            this.m_lockPlate.GetComponent<Renderer>().material.SetFloat("_Desaturate", num);
            allyBg.GetComponent<Renderer>().material.SetFloat("_Desaturate", num);
            allyBg.SetActive(true);
        }
    }
}

