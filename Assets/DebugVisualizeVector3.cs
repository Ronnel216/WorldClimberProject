using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugVisualizeVector3 : MonoBehaviour {

    static public Vector3 visualize;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.FromToRotation(Vector3.up, visualize);
	}
}
