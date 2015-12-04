using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass, ExecuteInEditMode]
public class ReplaceMaterials : MonoBehaviour
{
    public List<MaterialData> m_Materials;

    private GameObject FindGameObject(string gameObjName)
    {
        if (gameObjName[0] != '/')
        {
            return GameObject.Find(gameObjName);
        }
        char[] separator = new char[] { '/' };
        string[] strArray = gameObjName.Split(separator);
        return GameObject.Find(strArray[strArray.Length - 1]);
    }

    private void Start()
    {
        foreach (MaterialData data in this.m_Materials)
        {
            GameObject obj2 = this.FindGameObject(data.GameObjectName);
            if ((obj2 == null) && !data.ReplaceChildMaterials)
            {
                object[] args = new object[] { data.GameObjectName };
                Log.Kyle.Print("ReplaceMaterials failed to locate object: {0}", args);
            }
            else if (data.ReplaceChildMaterials)
            {
                foreach (Renderer renderer in obj2.GetComponentsInChildren<Renderer>())
                {
                    if (renderer != null)
                    {
                        RenderUtils.SetMaterial(renderer, data.MaterialIndex, data.NewMaterial);
                    }
                }
            }
            else
            {
                Renderer component = obj2.GetComponent<Renderer>();
                if (obj2 == null)
                {
                    object[] objArray2 = new object[] { data.GameObjectName };
                    Log.Kyle.Print("ReplaceMaterials failed to get Renderer: {0}", objArray2);
                }
                else
                {
                    RenderUtils.SetMaterial(component, data.MaterialIndex, data.NewMaterial);
                }
            }
        }
    }

    [Serializable]
    public class MaterialData
    {
        public GameObject DisplayGameObject;
        [CustomEditField(T=EditType.SCENE_OBJECT)]
        public string GameObjectName;
        public int MaterialIndex;
        public Material NewMaterial;
        public bool ReplaceChildMaterials;
    }
}

