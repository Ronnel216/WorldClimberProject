using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMaker : MonoBehaviour
{
    [SerializeField]
    string wallCreaterTag = "Wall";



    void Start()
    {
        var wallCreaterObjs = GameObject.FindGameObjectsWithTag(wallCreaterTag);
        var wallCreaters = new WallCreater[wallCreaterObjs.Length];

        int i = 0;
        foreach (var obj in wallCreaterObjs)
            wallCreaters[i++] = obj.GetComponent<WallCreater>();

        foreach (var creater in wallCreaters)
        {
            Debug.Assert(creater != null);
        }

        foreach (var creater in wallCreaters)
        {
            creater.Execute(0);
        }
    }

    void SaveMesh()
    {
        //// 面数に必要な分だけ確保
        //vertNum = new Vector2Int(polyNum.x + 1, polyNum.y + 1);

        //var filter = GetComponent<MeshFilter>();
        //filter.sharedMesh = CreateMesh();             // CreateMeshはWorldCreaterの関数 コピペ
        //AssetDatabase.CreateAsset(filter.sharedMesh, "Assets/Temp/"+ filter.sharedMesh.name +".asset");

    }
}