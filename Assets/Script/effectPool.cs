using System.Collections.Generic;
using UnityEngine;

public class effectPool : Singleton<effectPool>
{
    public Dictionary<string, GameObject> dicPrefabs;
    public Dictionary<string, List<GameObject>> list;
    private Dictionary<string, List<GameObject>> inActiveList;
    public int maxCount = 300;

    private void Awake()
    {
        list = new Dictionary<string, List<GameObject>>();
        inActiveList = new Dictionary<string, List<GameObject>>();
    }

    public void CreateInstance(string prefabName, Transform parent, int amount)
    {
        string key = prefabName;
        maxCount = amount;
        if (dicPrefabs == null)
        {
            dicPrefabs = new Dictionary<string, GameObject>();
        }
        GameObject prefab;
        if (dicPrefabs.ContainsKey(key))
        {
            prefab = dicPrefabs[key];
        }
        else
        {
            prefab = (GameObject)Resources.Load("Prefabs/" + prefabName);
            dicPrefabs.Add(key, prefab);
        }

        // 미리 maxCount만큼 만들어놓고 비활성화 하고싶다.
        // 목록에 담아놓고싶다.
        for (int i = 0; i < maxCount; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.parent = parent;
            obj.name = key;
            obj.SetActive(false);
            // 만약 list에 key가 존재하지 않는다면 
            if (false == list.ContainsKey(key))
            {
                // key와 value를 추가하고싶다.
                list.Add(key, new List<GameObject>());
                inActiveList.Add(key, new List<GameObject>());
            }

            list[key].Add(obj);
            inActiveList[key].Add(obj);
        }
    }

    // Activate an inactive object from the pool
    // Activate an inactive object from the pool or create a new one if necessary
    public void ActivateObject(string prefabName)
    {
        string key = prefabName;
        if (inActiveList.ContainsKey(key) && inActiveList[key].Count > 0)
        {
            GameObject obj = inActiveList[key][0];
            obj.SetActive(true);
            inActiveList[key].Remove(obj);
            list[key].Add(obj);
        }
        else
        {
            if (list.ContainsKey(key))
            {
                // Create a new object since no inactive objects are available
                GameObject prefab = dicPrefabs[key];
                GameObject newObj = Instantiate(prefab);
                newObj.name = key;
                newObj.SetActive(true);
                list[key].Add(newObj);
                Debug.LogWarning("No inactive object available for prefab: " + prefabName + ". Created a new one.");
            }
            else
            {
                Debug.LogWarning("Prefab key not found in the object pool: " + prefabName);
            }
        }
    }

    // Deactivate an active object and return it to the pool
    public void DeactivateObject(string prefabName, GameObject obj)
    {
        string key = prefabName;
        if (list.ContainsKey(key) && list[key].Contains(obj))
        {
            obj.SetActive(false);
            list[key].Remove(obj);
            inActiveList[key].Add(obj);
        }
        else
        {
            Debug.LogWarning("The provided object is not managed by the object pool for prefab: " + prefabName);
        }
    }
}
