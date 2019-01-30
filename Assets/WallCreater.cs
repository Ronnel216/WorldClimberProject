using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCreater : MonoBehaviour {

    enum WallChipID //! Chipの種類によっては周囲のChipのIDを変えず直接影響を反映する
    {
        Undefind = -1,
        None = 0,
        Ground,
        Grippable,
        WallReverse,
        Wall
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

    // 左上右下     左上:targetsA 右下:targetsB
    [SerializeField]
    WallConnecter[] connecters = new WallConnecter[4];

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
                //if (i == 0 || j == 0 || i == numVertex.x - 1 || j == numVertex.y - 1) continue;

                Vector2 offset = new Vector2((float)i / numVertex.x, (float)j / numVertex.y);
                Vector2 addjust = new Vector2(
                    wallSize.x / (numVertex.x - 1),
                    wallSize.y / (numVertex.y - 1));  // 範囲調整用 i, j は(ループ回数-1)までの値しかとらないため
                offset.x *= wallSize.x + addjust.x;
                offset.y *= wallSize.y + addjust.y;
                offset += new Vector2(-wallSize.x / 2, -wallSize.y / 2);    // 中心に移動
                ver[index] = offset;

                //// 配置に偏りを作る
                //float xRange = (wallSize.x / numVertex.x / 2) * cellSizeFactor.x;
                //float yRange = (wallSize.y / numVertex.y / 2) * cellSizeFactor.y;
                //ver[index] += new Vector2(UnityEngine.Random.Range(-xRange, xRange), UnityEngine.Random.Range(-yRange, yRange));

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
        var map = new char[numVertex.x * numVertex.y];
        for (int i = 0; i < map.Length; i++)
            map[i] = (char)0;

        //for (int i = 0; i < numVertex.x; i++)
        //    for (int j = 0; j < numVertex.y; j++)
        //        if (j == numVertex.y / 2) map[i, j] = (char)WallChipID.Grippable;

        //! 掴み位置を上下で隣接して配置するのは禁止
        //! 掴み位置が一か所に対して2つ隣接するのは禁止
        //for (int i = 0; i < numVertex.x; i++)
        //    for (int j = 0; j < numVertex.y; j++)
        //        if (i == numVertex.y / 2) map[i, j] = (char)WallChipID.Grippable;

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

        System.Func<Vector3, Vector3, Vector3, Vector2Int, Vector2> calcCellPos =
            (Vector3 linePos, Vector3 offset, Vector3 size, Vector2Int num) =>
        { 
            Vector2 cellPos = linePos - offset;
            cellPos.x += size.x / 2;
            cellPos.x /= size.x / num.x;
            cellPos.y += size.y / 2;
            cellPos.y /= size.y / num.y;

            return cellPos; // 頂点インデックスに変換する
        };

        var grippableIndexes = new List<int>();
        for (int i = 0; i < lineList.Length; i++) 
        {
            Vector3[] posList = new Vector3[lineList[i].positionCount];
            lineList[i].GetPositions(posList);
            for (int j = 0; j < posList.Length - 1; j++) // 次の頂点も同時に参照するため -1
            {
                for (int hoge = 0; hoge < 10000; hoge++)
                {
                    var cellPos = calcCellPos(Vector2.Lerp(posList[j], posList[j + 1], hoge / 10000f), gameObject.transform.position, wallSize, numVertex);
                    var tempIndex0 = calcIndex((int)cellPos.x, (int)cellPos.y, numVertex.x, numVertex.y);
                    Debug.Assert(tempIndex0 != -1);

                    if (map[tempIndex0] == (char)WallChipID.Grippable) continue;

                    var tempIndexTop = calcIndex((int)cellPos.x, (int)cellPos.y + 1, numVertex.x, numVertex.y);
                    if (tempIndexTop == -1) continue;
                    var tempIndexBottom = calcIndex((int)cellPos.x, (int)cellPos.y - 1, numVertex.x, numVertex.y);
                    if (tempIndexBottom == -1) continue;

                    map[tempIndex0] = (char)WallChipID.Grippable;
                    map[tempIndexBottom] = (char)WallChipID.WallReverse;


                    map[tempIndexTop] = true ? (char)WallChipID.Wall : (char) WallChipID.Ground;


                    int wallIndex = 0;
                    wallIndex = calcIndex((int)cellPos.x - 1, (int)cellPos.y + 1, numVertex.x, numVertex.y);
                    if (wallIndex != -1) map[wallIndex] = (char)WallChipID.Wall;
                    wallIndex = calcIndex((int)cellPos.x + 1, (int)cellPos.y + 1, numVertex.x, numVertex.y);
                    if (wallIndex != -1) map[wallIndex] = (char)WallChipID.Wall;
                    wallIndex = calcIndex((int)cellPos.x, (int)cellPos.y + 2, numVertex.x, numVertex.y);
                    if (wallIndex != -1) map[wallIndex] = (char)WallChipID.Wall;

                    //vertices[tempIndex0] += Vector3.back * grippableBumpySize[0];    // verticesの配置はx, yが逆
                }
            }
        }

        // 掴み位置特性の付加
        var grippableList = new List<List<int>>();
        grippableList.Add(new List<int>());
        for (int j = 0; j < numVertex.y; j++)
        {
            grippableList.Add(new List<int>());
            for (int i = 0; i < numVertex.x; i++)
            {
                int tempIndex0 = calcIndex(i, j, numVertex.x, numVertex.y); // 頂点インデックスに変換する
                Debug.Assert(tempIndex0 != -1);

                if (map[tempIndex0] != (char)WallChipID.Grippable) continue;
                vertices[tempIndex0] += Vector3.back * grippableBumpySize[0];    // verticesの配置はx, yが逆
                grippableList[grippableList.Count - 1].Add(tempIndex0);
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
                if (map[tempIndex1] != (char)WallChipID.WallReverse) continue;

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
                if (map[tempIndex0] != (char)WallChipID.Ground) continue;

                int tempIndex1 = calcIndex(i, j - 1, numVertex.x, numVertex.y);
                if (tempIndex1 != -1) vertices[tempIndex0] = vertices[tempIndex1] + Vector3.back * vertices[tempIndex0].z;

            }
        }

        // 壁特性の付加
        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                int tempIndex0 = calcIndex(i, j, numVertex.x, numVertex.y);
                if (tempIndex0 == -1) continue;
                if (map[tempIndex0] != (char)WallChipID.Wall) continue;

                int tempIndex1 = calcIndex(i, j - 1, numVertex.x, numVertex.y);
                if (tempIndex1 != -1) vertices[tempIndex0] = new Vector3(vertices[tempIndex0].x, vertices[tempIndex0].y, vertices[tempIndex1].z + grippableBumpySize[2]);
            }
        }


        // 湾曲特性の付加
        //float curveRadian = -Mathf.PI / (numVertex.x - 1) / 4;
        //for (int i = 0; i < numVertex.x; i++)
        //{
        //    for (int j = 0; j < numVertex.y; j++)
        //    {
        //        int tempIndex = calcIndex(i, j, numVertex.x, numVertex.y);
        //        if (tempIndex == -1) continue;
        //        vertices[tempIndex] = Quaternion.AngleAxis((Mathf.Rad2Deg * curveRadian) * i, Vector3.down) * vertices[tempIndex];
        //    }
        //}        

        for (int i = 0; i < grippableList.Count; i++) // 次の頂点も同時に参照するため -1
        {
            for (int j = 0; j < grippableList[i].Count - 1; j++)
                GrippablePoint2.CreateEdges(vertices[grippableList[i][j]], vertices[grippableList[i][j+1]]);
        }

        // メッシュへの反映
        mesh.vertices = vertices;

        for (int dir = 0; dir < connecters.Length; dir++)
        {
            if (connecters[dir] == null) continue;

            int[] indexes = null;
            // 端の配列を作成　外に出してもいいかも
            switch (dir)
            {
                case 0:
                    indexes = new int[numVertex.y];
                    for (int i = 0; i < numVertex.y; i++)
                        indexes[i] = calcIndex(0, i, numVertex.x, numVertex.y);
                    break;

                case 1:
                    indexes = new int[numVertex.x];
                    for (int i = 0; i < numVertex.x; i++)
                        indexes[i] = calcIndex(i, 0, numVertex.x, numVertex.y);
                    break;
                case 2:
                    indexes = new int[numVertex.y];
                    for (int i = 0; i < numVertex.y; i++)
                        indexes[i] = calcIndex(numVertex.x - 1, i, numVertex.x, numVertex.y);
                     
                    break;
                case 3:
                    indexes = new int[numVertex.x];
                    for (int i = 0; i < numVertex.x; i++)
                        indexes[i] = calcIndex(i, numVertex.y - 1, numVertex.x, numVertex.y);
                    break;

                default:
                    break;

            }

            connecters[dir].targetsIndex[dir == 0 ? 1 : 0] = indexes;
            connecters[dir].targets[dir == 0 ? 1 : 0] = mesh;
            connecters[dir].transforms[dir == 0 ? 1 : 0] = transform;
            connecters[dir].Connect();
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.MarkDynamic();
        mesh.name = "OriginalWallMesh";

        Debug.Log("頂点数 : " + mesh.vertices.Length);

        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        // 仮
        var collider = GetComponent<MeshCollider>();
        collider.sharedMesh = mesh;

    }

    // Update is called once per frame
    void Update () {
		
	}

}
