using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sun : MonoBehaviour {


    Color temp;
	// Use this for initialization
	void Start () {
        temp = GetComponent<Light>().color;

    }
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.Rotate(new Vector3((Mathf.Sin(Time.time / 100f)) / 1000f, 0, 0));

    }
}
