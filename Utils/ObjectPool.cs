using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T : Component
{
    public T prefab { get; private set; }
    public Transform parent { get; private set; }
    List<T> _objects = new List<T>();
    public int count { get; private set; }
    public int totalCount { get { return _objects.Count; } }
    public T this[int i] { get { return _objects[i]; } }

    public ObjectPool(T prefab, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
    }

    public T Instantiate()
    {
        T obj;
        if (_objects.Count > count)
            obj = _objects[count];
        else
        {
            if (prefab == null)
            {
                obj = new GameObject(typeof(T).Name).AddComponent<T>();
                Transform t = obj.transform;
                t.SetParent(parent);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
            }
            else
                obj = Object.Instantiate(prefab, parent);
            _objects.Add(obj);
        }

        obj.gameObject.SetActive(true);
        ++count;
        return obj;
    }

    public void Destroy(T obj)
    {
        Destroy(_objects.IndexOf(obj));
    }

    public void Destroy(int index)
    {
        if (index < 0 || index >= count)
        {
            Debug.LogError("Invalid index: " + index.ToString());
            return;
        }

        // swap positions
        T obj = _objects[index];
        if (count > 1)
        {
            _objects[index] = _objects[count - 1];
            _objects[count - 1] = obj;
        }

        obj.gameObject.SetActive(false);
        --count;
    }

    public void Clear(bool destroy = false)
    {
        if (destroy)
        {
            foreach (T obj in _objects)
                Object.Destroy(obj.gameObject);
            _objects.Clear();
        }
        if (destroy)
        {
            for (int i = 0; i < count; ++i)
                _objects[i].gameObject.SetActive(false);
            _objects.Clear();
        }
        count = 0;
    }

    public void Resize(int size)
    {
        if (size < 0)
        {
            Debug.LogError("Invalid object pool size: " + size.ToString());
            return;
        }

        while (count > size)
            Destroy(count - 1);
        while (count < size)
            Instantiate();
    }

    public int IndexOf(T obj)
    {
        return _objects.IndexOf(obj, 0, count);
    }

    public T Find(System.Func<T, bool> func)
    {
        for (int i = 0; i < count; ++i)
        {
            if (func(_objects[i]))
                return _objects[i];
        }
        return null;
    }

    public int FindIndex(System.Func<T, bool> func)
    {
        for (int i = 0; i < count; ++i)
        {
            if (func(_objects[i]))
                return i;
        }
        return -1;
    }

    public bool Contains(T obj)
    {
        return IndexOf(obj) != -1;
    }

    public bool Exists(System.Func<T, bool> func)
    {
        return Find(func) != null;
    }

    public void Swap(int a, int b)
    {
        T aobj = _objects[a];
        _objects[a] = _objects[b];
        _objects[b] = aobj;
    }
}
