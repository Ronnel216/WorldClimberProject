using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCollider : MonoBehaviour {

    List<Collider> colliders;

    void Awake()
    {
         colliders = new List<Collider>();
    }

    public List<Collider> Colliders
    {
        get
        {
            return colliders;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        colliders.Add(collider);
    }

    void OnTriggerExit(Collider collider)
    {
        colliders.Remove(collider);
    }
}
