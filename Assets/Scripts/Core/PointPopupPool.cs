using System.Collections.Generic;
using UnityEngine;

public class PointPopupPool : SingletonBase<PointPopupPool>
{
    public GameObject prefab;
    public int initialSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    protected override void Awake()
    {
        base.Awake(); // Gọi SingletonBase để gán Instance

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);

        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        pool.Enqueue(obj);
    }
}
