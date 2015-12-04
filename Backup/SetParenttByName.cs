using System;
using UnityEngine;

[CustomEditClass]
public class SetParenttByName : MonoBehaviour
{
    [CustomEditField(T=EditType.SCENE_OBJECT)]
    public string m_ParentName;

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
        if (!string.IsNullOrEmpty(this.m_ParentName))
        {
            GameObject obj2 = this.FindGameObject(this.m_ParentName);
            if (obj2 == null)
            {
                object[] args = new object[] { this.m_ParentName };
                Log.Kyle.Print("SetParenttByName failed to locate parent object: {0}", args);
            }
            else
            {
                base.transform.parent = obj2.transform;
            }
        }
    }
}

