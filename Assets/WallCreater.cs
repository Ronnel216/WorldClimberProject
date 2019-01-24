﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCreater : MonoBehaviour {

    enum WallChipID //! Chipの種類によっては周囲のChipのIDを変えず直接影響を反映する
    {
        Undefind = -1,
        None = 0,
        Ground,
        Grippable,
        WallReverse
    }

    [SerializeField, Range(0, 2 << 10)]
    int seed = 0;

    [SerializeField]
    int baseNumVertex = 50;

    [SerializeField]
    float baseBumpy = 1.0f;

    [SerializeField]
    float noisecCycle = 5f;

    [SerializeField]
    Vector2 cellSizeFactor = new Vector2(1, 1);

    // 0:出っ張り具合 
    // 1:出っ張り位置の下のへっこみ具合
    [SerializeField]
    float[] grippableBumpySize = new float[2];

    Vector3 wallSize = Vector3.zero;

    // Use this for initialization
    void Awake() {
        wallSize = transform.lossyScale;
        transform.localScale = Vector3.one;

        CreateMesh();
    }

    public void CreateMesh()
    {
        //// 面数に必要な分だけ確保
        //vertNum = new Vector2Int(polyNum.x + 1, polyNum.y + 1);

        //var filter = GetComponent<MeshFilter>();
        //filter.sharedMesh = CreateMesh();             // CreateMeshはWorldCreaterの関数 コピペ
        //AssetDatabase.CreateAsset(filter.sharedMesh, "Assets/Temp/"+ filter.sharedMesh.name +".asset");

        // 乱数のシード値表現について

        // 乱数初期化
        var postion = gameObject.transform.position;
        postion /= 120f;    // 120m周期
        seed = ((int)(Mathf.PerlinNoise(postion.x, postion.z) * (2 << 18))) + (seed << 18);

        UnityEngine.Random.InitState(seed);

        // 配置頂点
        // 全体頂点数
        int allNum = (int)Mathf.Sqrt(baseNumVertex);
        // スケールによって頂点の分配比率を設定
        Vector2Int numVertex = new Vector2Int(
            (int)(allNum * (wallSize.x / wallSize.y)),
            (int)(allNum * (wallSize.y / wallSize.x)));

        Debug.Log("頂点分配率 : " + numVertex);
        var ver = new Vector2[numVertex.x * numVertex.y];
        Debug.Log("頂点数 : " + ver.Length);

        // 頂点の配置
        int index = 0;
        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                Vector2 offset = new Vector2((float)i / numVertex.x, (float)j / numVertex.y);
                Vector2 addjust = new Vector2(
                    wallSize.x / (numVertex.x - 1),
                    wallSize.y / (numVertex.y - 1));  // 範囲調整用 i, j は(ループ回数-1)までの値しかとらないため
                offset.x *= wallSize.x + addjust.x;
                offset.y *= wallSize.y + addjust.y;
                offset += new Vector2(-wallSize.x / 2, -wallSize.y / 2);    // 中心に移動
                ver[index] = offset;

                // 配置に偏りを作る
                float xRange = (wallSize.x / numVertex.x / 2) * cellSizeFactor.x;
                float yRange = (wallSize.y / numVertex.y / 2) * cellSizeFactor.y;
                ver[index] += new Vector2(UnityEngine.Random.Range(-xRange, xRange), UnityEngine.Random.Range(-yRange, yRange));

                // 添字進行
                index++;
            }
        }

        // メッシュの生成
        var triangulator = new DelaunyTriangulation.Triangulator();
        Mesh mesh = triangulator.CreateInfluencePolygon(ver);

        // 壁のベースとして調整
        var vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            float noise = Mathf.PerlinNoise(vertices[i].x / noisecCycle, vertices[i].z / noisecCycle);
            vertices[i] += Vector3.up * baseBumpy * noise;
            vertices[i] = Quaternion.AngleAxis(90f, Vector3.left) * vertices[i];
        }

        // indexを求める 不正値の場合は -1
        System.Func<int, int, int, int, int> calcIndex = (int i, int j, int xSize, int ySize) => {
            if (j < 0 && ySize <= j && i < 0 && xSize <= i) return -1;
            var result = j + i * ySize;
            return result;
        };

        // 地形生成用のマップ i:y j:x
        var map = new char[numVertex.x, numVertex.y];
        for (int i = 0; i < numVertex.x; i++)
            for (int j = 0; j < numVertex.y; j++)
                map[i, j] = (char)0;

        //for (int i = 0; i < numVertex.x; i++)
        //    for (int j = 0; j < numVertex.y; j++)
        //        if (j == numVertex.y / 2) map[i, j] = (char)WallChipID.Grippable;

        //! 掴み位置を上下で隣接して配置するのは禁止
        //! 掴み位置が一か所に対して2つ隣接するのは禁止
        for (int i = 0; i < numVertex.x; i++)
            for (int j = 0; j < numVertex.y; j++)
                if (i == numVertex.y / 2) map[i, j] = (char)WallChipID.Grippable;

        //for (int i = 0; i < numVertex.y; i++)
        //    for (int j = 0; j < numVertex.x; j++)
        //        if (i == numVertex.y / 2 - 1) map[j, i] = (char)WallChipID.WallReverse;

        //for (int i = 0; i < numVertex.y; i++)
        //    for (int j = 0; j < numVertex.x; j++)
        //        if (i == numVertex.y / 2 + 1) map[j, i] = (char)WallChipID.Ground;

        // 掴み位置特性の付加
        //var a = new List<int>();
        //for (int i = 0; i < numVertex.x; i++)
        //{ 
        //    for (int j = 0; j < numVertex.y; j++)
        //    {
        //        int tempIndex0 = calcIndex(i, j, numVertex.x, numVertex.y); // 頂点インデックスに変換する
        //        Debug.Assert(tempIndex0 != -1);

        //        if (map[i, j] != (char)WallChipID.Grippable) continue;
        //        vertices[tempIndex0] += Vector3.back * grippableBumpySize[0];    // verticesの配置はx, yが逆
        //        a.Add(tempIndex0);

        //    }
        //}



        // 掴み位置特性の付加
        var lineList = gameObject.GetComponentsInChildren<LineRenderer>();

        System.Func<Vector3, Vector3, Vector3, Vector2Int, int> calcCellIndex =
            (Vector3 linePos, Vector3 offset, Vector3 size, Vector2Int num) =>
        { 
            Vector2 cellPos = linePos - offset;
            cellPos.x += size.x / 2;
            cellPos.x /= size.x / num.x;
            cellPos.y += size.y / 2;
            cellPos.y /= size.y / num.y;

            return calcIndex((int)cellPos.x, (int)cellPos.y, num.x, num.y); // 頂点インデックスに変換する
        };

        for (int i = 0; i < lineList.Length; i++) // 次の頂点も同時に参照するため -1
        {
            Vector3[] posList = new Vector3[lineList[i].positionCount];
            lineList[i].GetPositions(posList);
            for (int j = 0; j < posList.Length; j++)
            {
                int tempIndex0 = calcCellIndex(posList[j], gameObject.transform.position, wallSize, numVertex);
                Debug.Assert(tempIndex0 != -1);
                vertices[tempIndex0] += Vector3.back * grippableBumpySize[0] * 100;    // verticesの配置はx, yが逆

                //int loop = 100;
                //for (int hoge = 0; hoge < loop; hoge++)
                //{
                //    var tempPos = Vector2.Lerp(cellPos, cellPos1, hoge / (float)loop);
                //    var indexxx = calcIndex((int)tempPos.x, (int)tempPos.y, numVertex.x, numVertex.y); // 頂点インデックスに変換する
                //    Debug.Assert(indexxx != -1);
                //    vertices[indexxx] += Vector3.back * grippableBumpySize[0];
                //}

            }
        }


        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                
            }
        }

        // 逆壁特性の付加
        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                int tempIndex1 = calcIndex(i, j, numVertex.x, numVertex.y);
                if (tempIndex1 == -1) continue;
                if (map[i, j] != (char)WallChipID.WallReverse) continue;

                int tempIndex0 = calcIndex(i, j + 1, numVertex.x, numVertex.y); // 上のセルを調査
                if (tempIndex0 == -1) continue;

                vertices[tempIndex1] = new Vector3(vertices[tempIndex1].x, vertices[tempIndex1].y, vertices[tempIndex0].z + grippableBumpySize[1]);
            }
        }

        // 地面特性の付加
        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                int tempIndex0 = calcIndex(i, j, numVertex.x, numVertex.y);
                if (tempIndex0 == -1) continue;
                if (map[i, j] != (char)WallChipID.Ground) continue;

                int tempIndex1 = calcIndex(i, j - 1, numVertex.x, numVertex.y);
                if (tempIndex1 != -1) vertices[tempIndex0] = vertices[tempIndex1] + Vector3.back * vertices[tempIndex0].z;

            }
        }

        //// 湾曲特性の付加
        //float curveRadian = Mathf.PI / (numVertex.x - 1);
        //for (int i = 0; i < numVertex.x; i++)
        //{
        //    for (int j = 0; j < numVertex.y; j++)
        //    {
        //        int tempIndex = calcIndex(i, j, numVertex.x, numVertex.y);
        //        if (tempIndex == -1) continue;
        //        vertices[tempIndex] = Quaternion.AngleAxis((Mathf.Rad2Deg * curveRadian) * i, Vector3.down) * vertices[tempIndex];
        //    }
        //}
       
        for (int i = 0; i < lineList.Length; i++) // 次の頂点も同時に参照するため -1
        {
            Vector3[] posList = new Vector3[lineList[i].positionCount];
            lineList[i].GetPositions(posList);
            for (int j = 0; j < posList.Length - 1; j++)
                GrippablePoint2.CreateEdges(posList[j], posList[j + 1]);
        }

        // メッシュへの反映
        mesh.vertices = vertices;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.MarkDynamic();
        mesh.name = "OriginalWallMesh";

        Debug.Log("頂点数 : " + mesh.vertices.Length);

        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

}
