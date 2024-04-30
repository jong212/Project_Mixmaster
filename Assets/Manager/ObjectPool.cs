using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    private void Awake()
    {
        //List<GameObject> : 몬스터 미리 생성해 저장할 리스트 자료형
        list = new Dictionary<string, List<GameObject>>();
        //inActiveList : 비활성화 리스트
        inActiveList = new Dictionary<string, List<GameObject>>();
    }
    //오브젝트풀에 생성할 최대 개수
    public int maxCount = 300;
    public Dictionary<string, GameObject> dicPrefabs;
    public Dictionary<string, List<GameObject>> list;
    // 안보이는 목록을 만들어서 관리하고싶다.
    Dictionary<string, List<GameObject>> inActiveList;

    /// <summary>
    /// ObjectPool에 객체를 미리 생성하고싶다.
    /// </summary>
    /// <param name="prefabName"></param>
    /// <param name="parent"></param>
    /// <param name="amount"></param>
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
    public List<GameObject> GetAllInactiveObjects(string key)
    {
        List<GameObject> activatedObjects = new List<GameObject>();

        if (!inActiveList.ContainsKey(key))
        {
            return activatedObjects;
        }

        // 비활성 상태인 모든 객체를 활성화하기
        foreach (GameObject obj in inActiveList[key])
        {
            obj.SetActive(true);
            activatedObjects.Add(obj);
        }

        // 모든 객체를 활성화한 후에는 비활성화 리스트를 비워줍니다.
        inActiveList[key].Clear();

        return activatedObjects;
    }

    /// <summary>
    /// 해당 key의 비활성 객체를 하나 얻어내고싶다.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public GameObject GetInactiveObject(string key)
    {
        if (false == inActiveList.ContainsKey(key))
        {
            return null;
        }
        // 만약 비활성목록이 0개 보다 크다면
        if (inActiveList[key].Count > 0)
        {
            
            inActiveList[key][0].SetActive(true);
            GameObject temp = inActiveList[key][0];
            //  비활성목록에서 제거하고
            inActiveList[key].RemoveAt(0);
            //  반환하고싶다.
            return temp;
        }
        // 그렇지않다면(만약 비활성목록이 0개라면)        //  null을 반환하고싶다.

        GameObject prefab = dicPrefabs[key];
        GameObject obj = Instantiate(prefab);
        obj.transform.parent = list[key][0].transform.parent;
        obj.name = key;
        list[key].Add(obj);
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 다 사용한 객체를 ObjectPool로 반환하고싶다.
    /// </summary>
    /// <param name="obj"></param>
    public void AddInactiveObject(GameObject obj)
    {
        obj.SetActive(false);
        string key = obj.name;
        if (inActiveList.ContainsKey(key))
        {
            if (false == inActiveList[key].Contains(obj))
            {
                inActiveList[key].Add(obj);
            }
        }
    }

    /// <summary>
    /// 해당 객체가(obj) ObjectPool에서 관리되는 녀석인지 알고싶다.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsObjectPoolObject(GameObject obj)
    {
        string key = obj.name;
        if (Instance.list.ContainsKey(key))
        {
            return Instance.list[key].Contains(obj);
        }

        return false;
    }


}