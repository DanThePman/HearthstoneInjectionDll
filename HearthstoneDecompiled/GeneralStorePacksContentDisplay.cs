using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStorePacksContentDisplay : MonoBehaviour
{
    public MeshRenderer m_background;
    public List<GameObject> m_packStacks = new List<GameObject>();
    private GeneralStorePacksContent m_parent;
    private List<AnimatedLowPolyPack> m_showingPacks = new List<AnimatedLowPolyPack>();
    private static readonly float PACK_FLY_OUT_X_DEG_VARIATION_MAG = 10f;
    private static readonly float PACK_FLY_OUT_Z_DEG_VARIATION_MAG = 10f;
    private static readonly Vector3 PACK_SCALE = new Vector3(0.06f, 0.06f, 0.06f);
    private static readonly int PACK_STACK_SEED = 2;
    private static readonly float PACK_X_VARIATION_MAG = 0.015f;
    private static readonly float PACK_Y_OFFSET = 0.02f;
    private static readonly float PACK_Z_VARIATION_MAG = 0.01f;
    private static Map<int, AnimatedLowPolyPack> s_packTemplates = new Map<int, AnimatedLowPolyPack>();

    public void ClearPacks()
    {
        foreach (AnimatedLowPolyPack pack in this.m_showingPacks)
        {
            UnityEngine.Object.Destroy(pack.gameObject);
        }
        this.m_showingPacks.Clear();
    }

    private int DeterminePackColumn(int packNumber)
    {
        double num = new System.Random(PACK_STACK_SEED + packNumber).NextDouble();
        double num3 = 0.0;
        float num4 = 1f / ((float) this.m_packStacks.Count);
        int num2 = 0;
        while (num2 < (this.m_packStacks.Count - 1))
        {
            num3 += num4;
            if (num <= num3)
            {
                return num2;
            }
            num2++;
        }
        return num2;
    }

    private Vector3 DeterminePackLocalPos(int column, List<AnimatedLowPolyPack> packs)
    {
        <DeterminePackLocalPos>c__AnonStorey345 storey = new <DeterminePackLocalPos>c__AnonStorey345 {
            column = column
        };
        List<AnimatedLowPolyPack> list = packs.FindAll(new Predicate<AnimatedLowPolyPack>(storey.<>m__196));
        Vector3 zero = Vector3.zero;
        zero.x = UnityEngine.Random.Range(-PACK_X_VARIATION_MAG, PACK_X_VARIATION_MAG);
        zero.y = PACK_Y_OFFSET * list.Count;
        zero.z = UnityEngine.Random.Range(-PACK_Z_VARIATION_MAG, PACK_Z_VARIATION_MAG);
        if ((storey.column % 2) == 0)
        {
            zero.y += 0.03f;
        }
        return zero;
    }

    private AnimatedLowPolyPack[] GetCurrentPacks(int id, int count)
    {
        if (count > this.m_showingPacks.Count)
        {
            AnimatedLowPolyPack component = null;
            if (!s_packTemplates.TryGetValue(id, out component))
            {
                StorePackDef storePackDef = this.m_parent.GetStorePackDef(id);
                component = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(storePackDef.m_lowPolyPrefab), true, false).GetComponent<AnimatedLowPolyPack>();
                s_packTemplates[id] = component;
                component.gameObject.SetActive(false);
            }
            for (int i = this.m_showingPacks.Count; i < count; i++)
            {
                AnimatedLowPolyPack child = UnityEngine.Object.Instantiate<AnimatedLowPolyPack>(component);
                child.gameObject.SetActive(true);
                int column = this.DeterminePackColumn(i);
                GameUtils.SetParent(child, this.m_packStacks[column], true);
                child.transform.localScale = PACK_SCALE;
                child.Init(column, this.DeterminePackLocalPos(column, this.m_showingPacks), new Vector3(0f, 3.5f, -0.1f));
                SceneUtils.SetLayer(child, this.m_packStacks[column].layer);
                float y = UnityEngine.Random.Range(-this.m_parent.m_PackYDegreeVariationMag, this.m_parent.m_PackYDegreeVariationMag);
                Vector3 flyInLocalAngles = new Vector3(0f, y, 0f);
                float x = UnityEngine.Random.Range(-PACK_FLY_OUT_X_DEG_VARIATION_MAG, PACK_FLY_OUT_X_DEG_VARIATION_MAG);
                float z = UnityEngine.Random.Range(-PACK_FLY_OUT_Z_DEG_VARIATION_MAG, PACK_FLY_OUT_Z_DEG_VARIATION_MAG);
                Vector3 flyOutLocalAngles = new Vector3(x, 0f, z);
                child.SetFlyingLocalRotations(flyInLocalAngles, flyOutLocalAngles);
                this.m_showingPacks.Add(child);
            }
        }
        return this.m_showingPacks.ToArray();
    }

    public void SetParent(GeneralStorePacksContent parent)
    {
        this.m_parent = parent;
    }

    public int ShowPacks(int numVisiblePacks, float flyInTime, float flyOutTime, float flyInDelay, float flyOutDelay, bool forceImmediate = false)
    {
        bool flag = this.m_parent.IsContentActive();
        AnimatedLowPolyPack[] currentPacks = this.GetCurrentPacks(this.m_parent.GetBoosterId(), numVisiblePacks);
        int num = 0;
        for (int i = currentPacks.Length - 1; i >= numVisiblePacks; i--)
        {
            AnimatedLowPolyPack pack = currentPacks[i];
            if (flag && !forceImmediate)
            {
                if (pack.FlyOut(flyOutTime, flyOutDelay * num))
                {
                    num++;
                }
            }
            else
            {
                pack.FlyOutImmediate();
            }
        }
        int num3 = 0;
        for (int j = 0; j < numVisiblePacks; j++)
        {
            AnimatedLowPolyPack pack2 = currentPacks[j];
            if (flag && !forceImmediate)
            {
                if (pack2.FlyIn(flyInTime, flyInDelay * num3))
                {
                    num3++;
                }
            }
            else
            {
                pack2.FlyInImmediate();
            }
        }
        if (num3 > num)
        {
            return num3;
        }
        return -num;
    }

    public void UpdatePackType(StorePackDef packDef)
    {
        this.ClearPacks();
        if ((this.m_background != null) && (packDef != null))
        {
            this.m_background.material = packDef.m_background;
        }
    }

    [CompilerGenerated]
    private sealed class <DeterminePackLocalPos>c__AnonStorey345
    {
        internal int column;

        internal bool <>m__196(AnimatedLowPolyPack obj)
        {
            return (obj.Column == this.column);
        }
    }
}

