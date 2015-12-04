using System;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool destroyOnSpawn = true;
    public GameObject prefab;
    public bool spawnOnAwake;

    protected virtual void Awake()
    {
        if (this.spawnOnAwake)
        {
            this.Spawn();
        }
    }

    public GameObject Spawn()
    {
        GameObject destination = UnityEngine.Object.Instantiate<GameObject>(this.prefab);
        destination.transform.parent = base.transform.parent;
        TransformUtil.CopyLocal(destination, base.transform);
        SceneUtils.SetLayer(destination, base.gameObject.layer);
        if (this.destroyOnSpawn)
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
        return destination;
    }

    public T Spawn<T>() where T: MonoBehaviour
    {
        if (this.prefab.GetComponent<T>() != null)
        {
            return this.Spawn().GetComponent<T>();
        }
        Debug.Log(string.Format("The prefab for spawner {0} does not have component {1}", base.gameObject.name, typeof(T).Name));
        return null;
    }
}

