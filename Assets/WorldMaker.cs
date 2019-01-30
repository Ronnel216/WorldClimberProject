using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WorldMaker : MonoBehaviour
{
    [SerializeField]
    string wallCreaterTag = "Wall";


    void Awake()
    {

    }
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var wallCreaterObjs = GameObject.FindGameObjectsWithTag(wallCreaterTag);

            foreach (var obj in wallCreaterObjs)
            {
                SaveMesh(obj.GetComponent<MeshFilter>().sharedMesh);
            }

            foreach (var obj in wallCreaterObjs)
            {
                SaveObject(obj);
            }
        }
    }

    void SaveMesh(Mesh mesh)
    {
        //// 面数に必要な分だけ確保
        //vertNum = new Vector2Int(polyNum.x + 1, polyNum.y + 1);

        //var filter = GetComponent<MeshFilter>();
        //filter.sharedMesh = CreateMesh();             // CreateMeshはWorldCreaterの関数 コピペ
        AssetDatabase.CreateAsset(mesh, "Assets/Temp/" + mesh.name + ".asset");

    }

    void SaveObject(GameObject obj)
    {
        PrefabUtility.CreatePrefab("Assets/Temp/Walls/" + obj.name + ".prefab", obj, ReplacePrefabOptions.Default); //? option合ってるかわからん
        //AssetDatabase.CreateAsset(obj, "Assets/Temp/" + obj.name + ".prefab");
    }

}