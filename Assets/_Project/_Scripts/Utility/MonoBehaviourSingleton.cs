using UnityEngine;
public class MonoBehaviourSingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindAnyObjectByType<T>();
                if (!_instance)
                {
                    GameObject newObj = new GameObject("Auto-Generated " + typeof(T));
                    newObj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }
}