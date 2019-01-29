using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallConnecter : MonoBehaviour {

    // 左上右下
    public Transform[] transforms = new Transform[2];
    public Mesh[] targets = new Mesh[2] { null, null };
    public int[][] targetsIndex = new int[2][];



    public void Connect()
    {
        if (targets[0] == null || targets[1] == null) return;
        Debug.Log("Connect");

        Vector3[][] temp = new Vector3[2][] { targets[0].vertices, targets[1].vertices };

        // とりあえず　接続頂点数が同じとする
        for (int i = 0; i < targetsIndex[0].Length; i++)
        {
            Vector3[] worldPos = new Vector3[2] { targets[1].vertices[targetsIndex[1][i]], targets[0].vertices[targetsIndex[0][i]] };
            for (int two = 0; i < 2; i++)
                worldPos[two] = transforms[two].TransformPoint(worldPos[two]);

            Vector3 center = (worldPos[0] + worldPos[1]) / 2f;
            Debug.Log(targetsIndex[0][i]);
            Debug.Log(targetsIndex[1][i]);

            temp[0][targetsIndex[0][i]] = transforms[0].InverseTransformPoint(center);
            temp[1][targetsIndex[1][i]] = transforms[1].InverseTransformPoint(center);
        }

        // 仮で 0 を引っ張る
        targets[0].vertices = temp[0];
        targets[1].vertices = temp[1];

    }

    //// Use this for initialization
    //void Start () {

    //    var temp = targets[0].vertices;

    //    // とりあえず　接続頂点数が同じとする
    //    for (int i = 0; i < targetsIndex[0].Length; i++)
    //    {
    //        temp[targetsIndex[0][i]] = Vector3.forward * 100;
    //    }

    //    // 仮で 0 を引っ張る
    //    temp.CopyTo(targets[0].vertices, 0);

    //}

    // Update is called once per frame
    void Update () {
		
	}
}
