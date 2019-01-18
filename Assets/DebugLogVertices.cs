using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogVertices : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log(GetComponent<MeshFilter>().mesh.vertices.Length);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
