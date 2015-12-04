using System;
using UnityEngine;

public class CraftCardCountTab : MonoBehaviour
{
    public UberText m_count;
    public UberText m_plus;
    public GameObject m_shadow;
    private Vector3 origPos = new Vector3(0f, 0f, 0f);

    private void Awake()
    {
        this.origPos = this.m_count.transform.localPosition;
    }

    public void UpdateText(int numCopies)
    {
        if (numCopies > 9)
        {
            this.m_count.Text = "9";
            this.m_plus.gameObject.SetActive(true);
            this.m_count.transform.localPosition = new Vector3(0.08628464f, this.origPos.y, this.origPos.z);
        }
        else
        {
            if (numCopies >= 2)
            {
                this.m_shadow.SetActive(true);
                this.m_shadow.GetComponent<Animation>().Play("Crafting2ndCardShadow");
            }
            else
            {
                this.m_shadow.SetActive(false);
            }
            this.m_count.Text = numCopies.ToString();
            this.m_plus.gameObject.SetActive(false);
            this.m_count.transform.localPosition = this.origPos;
        }
    }
}

