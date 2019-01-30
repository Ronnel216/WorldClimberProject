﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WorldMaker : MonoBehaviour
{
    [SerializeField]
    string wallCreaterTag = "Wall";
    WallCreater[] wallCreaters = null;

    void Awake()
    {

    }
    void Start()
    {
        var wallCreaterObjs = GameObject.FindGameObjectsWithTag(wallCreaterTag);
        wallCreaters = new WallCreater[wallCreaterObjs.Length];

        int i = 0;
        foreach (var obj in wallCreaterObjs)
            wallCreaters[i++] = obj.GetComponent<WallCreater>();

        foreach (var creater in wallCreaters)
        {
            Debug.Assert(creater != null);
        }

        //foreach (var creater in wallCreaters)
        //{
        //    for (i = 0; i < 4; i++)
        //    {
        //        creater.Execute(i);
        //    }
        //}

    }
    int step = 0;
    int createrId = 0;

    void Update()
    {
        if (createrId < wallCreaters.Length)
        {
            Debug.Log("Execute" + step + "Start");
            var isExecuted = wallCreaters[createrId].Execute(step);
            Debug.Log("Execute" + step + "Finished");
            step++;

            if (isExecuted == false)
            {
                step = 0;
                createrId++;
            }
        }

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
        AssetDatabase.CreateAsset(mesh, "Assets/Temp/Walls/" + mesh.name + ".asset");

    }

    void SaveObject(GameObject obj)
    {
        PrefabUtility.CreatePrefab("Assets/Temp/Walls/" + obj.name + ".prefab", obj, ReplacePrefabOptions.Default); //? option合ってるかわからん
    }

}