using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 未初期化　警告用
/// </summary>
public class NotInitializeMessage : MonoBehaviour {

    public static GameObject Create(string message)
    {
        var obj = new GameObject();
        obj.AddComponent<NotInitializeMessage>();
        obj.name = message;
        return obj;
    }

    void Awake()
    {
        Debug.Log("NotInitialize : " + gameObject.name);
        Debug.Break();
    }
}
