using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class UIBObjectSpacing : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<SpacedObject> <>f__am$cache5;
    [CompilerGenerated]
    private static Predicate<SpacedObject> <>f__am$cache6;
    [SerializeField]
    private Vector3 m_Alignment = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField]
    private Vector3 m_LocalOffset;
    [SerializeField]
    private Vector3 m_LocalSpacing;
    public List<SpacedObject> m_Objects = new List<SpacedObject>();
    public bool m_reverse;

    public void AddObject(Component comp, bool countIfNull = true)
    {
        this.AddObject(comp, Vector3.zero, countIfNull);
    }

    public void AddObject(GameObject obj, bool countIfNull = true)
    {
        this.AddObject(obj, Vector3.zero, countIfNull);
    }

    public void AddObject(Component comp, Vector3 offset, bool countIfNull = true)
    {
        this.AddObject(comp.gameObject, offset, countIfNull);
    }

    public void AddObject(GameObject obj, Vector3 offset, bool countIfNull = true)
    {
        SpacedObject item = new SpacedObject {
            m_Object = obj,
            m_Offset = offset,
            m_CountIfNull = countIfNull
        };
        this.m_Objects.Add(item);
    }

    public void AddSpace(int index)
    {
        SpacedObject item = new SpacedObject {
            m_CountIfNull = true
        };
        this.m_Objects.Insert(index, item);
    }

    public void AnimateUpdatePositions(float animTime, iTween.EaseType tweenType = 2)
    {
        List<AnimationPosition> list = new List<AnimationPosition>();
        if (<>f__am$cache5 == null)
        {
            <>f__am$cache5 = o => o.m_CountIfNull || ((o.m_Object != null) && o.m_Object.activeInHierarchy);
        }
        List<SpacedObject> list2 = this.m_Objects.FindAll(<>f__am$cache5);
        if (this.m_reverse)
        {
            list2.Reverse();
        }
        Vector3 localOffset = this.m_LocalOffset;
        Vector3 localSpacing = this.m_LocalSpacing;
        Vector3 zero = Vector3.zero;
        for (int i = 0; i < list2.Count; i++)
        {
            SpacedObject obj2 = list2[i];
            GameObject obj3 = obj2.m_Object;
            if (obj3 != null)
            {
                AnimationPosition item = new AnimationPosition {
                    m_targetPos = localOffset + obj2.m_Offset,
                    m_object = obj3
                };
                list.Add(item);
            }
            Vector3 offset = obj2.m_Offset;
            if (i < (list2.Count - 1))
            {
                offset += localSpacing;
            }
            localOffset += offset;
            zero += offset;
        }
        zero.x *= this.m_Alignment.x;
        zero.y *= this.m_Alignment.y;
        zero.z *= this.m_Alignment.z;
        for (int j = 0; j < list.Count; j++)
        {
            AnimationPosition position = list[j];
            object[] args = new object[] { "position", position.m_targetPos - zero, "islocal", true, "easetype", tweenType, "time", animTime };
            iTween.MoveTo(position.m_object, iTween.Hash(args));
        }
    }

    public void ClearObjects()
    {
        this.m_Objects.Clear();
    }

    public void UpdatePositions()
    {
        if (<>f__am$cache6 == null)
        {
            <>f__am$cache6 = o => o.m_CountIfNull || ((o.m_Object != null) && o.m_Object.activeInHierarchy);
        }
        List<SpacedObject> list = this.m_Objects.FindAll(<>f__am$cache6);
        if (this.m_reverse)
        {
            list.Reverse();
        }
        Vector3 localOffset = this.m_LocalOffset;
        Vector3 localSpacing = this.m_LocalSpacing;
        Vector3 zero = Vector3.zero;
        for (int i = 0; i < list.Count; i++)
        {
            SpacedObject obj2 = list[i];
            GameObject obj3 = obj2.m_Object;
            if (obj3 != null)
            {
                obj3.transform.localPosition = localOffset + obj2.m_Offset;
            }
            Vector3 offset = obj2.m_Offset;
            if (i < (list.Count - 1))
            {
                offset += localSpacing;
            }
            localOffset += offset;
            zero += offset;
        }
        zero.x *= this.m_Alignment.x;
        zero.y *= this.m_Alignment.y;
        zero.z *= this.m_Alignment.z;
        for (int j = 0; j < list.Count; j++)
        {
            GameObject obj4 = list[j].m_Object;
            if (obj4 != null)
            {
                Transform transform = obj4.transform;
                transform.localPosition -= zero;
            }
        }
    }

    [CustomEditField(Range="0 - 1")]
    public Vector3 Alignment
    {
        get
        {
            return this.m_Alignment;
        }
        set
        {
            this.m_Alignment = value;
            this.m_Alignment.x = Mathf.Clamp01(this.m_Alignment.x);
            this.m_Alignment.y = Mathf.Clamp01(this.m_Alignment.y);
            this.m_Alignment.z = Mathf.Clamp01(this.m_Alignment.z);
            this.UpdatePositions();
        }
    }

    public Vector3 LocalOffset
    {
        get
        {
            return this.m_LocalOffset;
        }
        set
        {
            this.m_LocalOffset = value;
            this.UpdatePositions();
        }
    }

    public Vector3 LocalSpacing
    {
        get
        {
            return this.m_LocalSpacing;
        }
        set
        {
            this.m_LocalSpacing = value;
            this.UpdatePositions();
        }
    }

    private class AnimationPosition
    {
        public GameObject m_object;
        public Vector3 m_targetPos;
    }

    [Serializable]
    public class SpacedObject
    {
        public bool m_CountIfNull;
        public GameObject m_Object;
        public Vector3 m_Offset;
    }
}

