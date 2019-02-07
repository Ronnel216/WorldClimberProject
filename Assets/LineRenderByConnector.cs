using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderByConnector : MonoBehaviour {

    //[SerializeField]
    //bool isWriteLine = false;

    public void Apply()
    {
        //if (isWriteLine == false) return;
        var render = GetComponent<LineRenderer>();
        var connectors = GetConnector();
        render.positionCount = connectors.Length;
        render.SetPositions(connectors);
        
    }

    public Vector3[] GetConnector()
    {
        var list = gameObject.GetComponentsInChildren<LineConnector>();
        var result = new Vector3[list.Length];

        int i = 0;
        foreach (var l in list)
        {
            result[i++] = l.GetPos();
        }

        return result;
    }

}
