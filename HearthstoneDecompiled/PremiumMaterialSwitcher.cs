using System;
using UnityEngine;

public class PremiumMaterialSwitcher : MonoBehaviour
{
    public Material[] m_PremiumMaterials;
    private Material[] OrgMaterials;

    public void SetToPremium(int premium)
    {
        if (premium < 1)
        {
            if ((base.GetComponent<Renderer>().materials != null) && (this.OrgMaterials != null))
            {
                Material[] materials = base.GetComponent<Renderer>().materials;
                for (int i = 0; (i < this.m_PremiumMaterials.Length) && (i < materials.Length); i++)
                {
                    if (this.m_PremiumMaterials[i] != null)
                    {
                        materials[i] = this.OrgMaterials[i];
                    }
                }
                base.GetComponent<Renderer>().materials = materials;
                this.OrgMaterials = null;
            }
        }
        else if (this.m_PremiumMaterials.Length >= 1)
        {
            if (this.OrgMaterials == null)
            {
                this.OrgMaterials = base.GetComponent<Renderer>().materials;
            }
            Material[] materialArray2 = base.GetComponent<Renderer>().materials;
            for (int j = 0; (j < this.m_PremiumMaterials.Length) && (j < materialArray2.Length); j++)
            {
                if (this.m_PremiumMaterials[j] != null)
                {
                    materialArray2[j] = this.m_PremiumMaterials[j];
                }
            }
            base.GetComponent<Renderer>().materials = materialArray2;
        }
    }

    private void Start()
    {
    }
}

