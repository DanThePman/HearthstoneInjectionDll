using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode, CustomEditClass]
public class NestedPrefab : MonoBehaviour
{
    private List<EditorMesh> m_EditorMeshes = new List<EditorMesh>();
    private string m_lastPrefab;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_Prefab;
    private GameObject m_PrefabGameObject;
    private readonly Color SELECTED_WIRE_COLOR = new Color(0.3f, 0.3f, 0.5f, 0.5f);

    private void LoadPrefab()
    {
        string name = FileUtils.GameAssetPathToName(this.m_Prefab);
        GameObject obj2 = AssetLoader.Get().LoadGameObject(name, true, false);
        Quaternion localRotation = obj2.transform.localRotation;
        Vector3 localScale = obj2.transform.localScale;
        obj2.transform.parent = base.transform;
        obj2.transform.localPosition = Vector3.zero;
        obj2.transform.localRotation = localRotation;
        obj2.transform.localScale = localScale;
    }

    private void OnEnable()
    {
        this.UpdateMesh();
    }

    private void SetupEditorMesh(GameObject go, Matrix4x4 goMtx)
    {
        if (go != null)
        {
            Vector3 pos = (Vector3) (go.transform.position * -1f);
            Matrix4x4 matrixx = goMtx * Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
            foreach (Renderer renderer in go.GetComponentsInChildren(typeof(Renderer), true))
            {
                MeshFilter component = renderer.GetComponent<MeshFilter>();
                if (((component != null) && (renderer.sharedMaterials != null)) && (renderer.sharedMaterials.Length != 0))
                {
                    EditorMesh item = new EditorMesh {
                        mesh = component.sharedMesh,
                        matrix = matrixx * renderer.transform.localToWorldMatrix,
                        materials = new List<Material>(renderer.sharedMaterials)
                    };
                    this.m_EditorMeshes.Add(item);
                }
            }
            foreach (NestedPrefab prefab in go.GetComponentsInChildren(typeof(NestedPrefab), true))
            {
                if (prefab.enabled && prefab.gameObject.activeSelf)
                {
                    this.SetupEditorMesh(prefab.m_PrefabGameObject, matrixx * prefab.transform.localToWorldMatrix);
                }
            }
        }
    }

    private void UpdateMesh()
    {
        this.LoadPrefab();
        this.m_EditorMeshes.Clear();
        if (base.enabled && (this.m_PrefabGameObject != null))
        {
            this.SetupEditorMesh(this.m_PrefabGameObject, Matrix4x4.identity);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct EditorMesh
    {
        public Mesh mesh;
        public Matrix4x4 matrix;
        public List<Material> materials;
    }
}

