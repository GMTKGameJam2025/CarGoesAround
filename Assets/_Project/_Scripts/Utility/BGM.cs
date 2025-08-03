using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class BGM : MonoBehaviour
{
    public static BGM instance;
 
    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}