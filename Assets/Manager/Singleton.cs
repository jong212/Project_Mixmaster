using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]


public class Singleton<T> : MonoBehaviour where T : MonoBehaviour //모노만 상속받은 클래스만 사용할 수 잇도록 한정
{
    private static T m_instance; // 매니저 객체를 저장하는데 사용
    private static bool m_isApplicationQuit= false; //유니티 에디터 상에서 유니티 종료할 때 싱글턴 객체 생성되지 않도록 막기 위해 
    

    public static T Instance
    {
        get
        {
            if (true == m_isApplicationQuit)
                return null;

            if(null == m_instance)
            {
                T[] _finds = FindObjectsOfType<T>();
                if( _finds.Length > 0)
                {
                    m_instance = _finds[0];
                    DontDestroyOnLoad(m_instance.gameObject);
                }
                if(_finds.Length > 1)
                {
                    for(int i =1; i< _finds.Length; i++)
                    {
                        Destroy(_finds[i].gameObject );
                    }
                    Debug.LogError("There is more than one" + typeof(T).Name + "in the scene");
                }
                if( null == m_instance)
                {
                    GameObject _createGameObject = new GameObject(typeof(T).Name);
                    DontDestroyOnLoad(_createGameObject);
                    m_instance = _createGameObject.AddComponent<T>();
                }
            }
           

            return m_instance;

        }
    }

    private void OnApplicationQuit()
    {
        m_isApplicationQuit = true;
    }
}
