using System;
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

    enum WallType
    {
        Undefind = -1,
        Default = 0,
        Dekoboko,
        PushBottomToForward
    }

    [SerializeField, Range(0, 2 << 10)]
    int seed = 0;

    [SerializeField]
    int baseNumVertex = 50;

    [SerializeField]
    float baseBumpy = 1.0f;

    [SerializeField]
    Vector2 noiseCycle = new Vector2(18f, 18f);

    // 0 ～ 1　の範囲
    [SerializeField]
    Vector2 noiseClamp = new Vector2(0, 1);

    [SerializeField]
    Vector2 cellSizeFactor = new Vector2(1, 1);

    // 左上右下     左上:targetsA 右下:targetsB
    [SerializeField]
    WallConnecter[] connecters = new WallConnecter[4];

    // 0:出っ張り具合 
    // 1:出っ張り位置の下のへこみ具合
    // 2:出っ張り位置の上のへこみ具合
    [SerializeField]
    float[] baseGrippableBumpySize = new float[3];

    // 湾曲性質
    // x軸, y軸　それぞれで湾曲させる
    [SerializeField]
    Vector2 curveDegressToAllOver = new Vector2();

    // x,yで軸の表現 zで角度指定
    // x,yは0～1の正規表現
    [SerializeField]
    Vector3[] curveDegressToAxis = new Vector3[0];

    Vector3 wallSize = Vector3.zero;

    public Mesh GetMesh()
    {
        return GetComponent<MeshFilter>().sharedMesh;
    }

    public void ApplyLineConnector()
    {
        var lineConnectors = GetComponentsInChildren<LineRenderByConnector>();
        foreach (var c in lineConnectors)
        {
            c.Apply();
        }
    }

    public bool Execute(int step)
    {

        /*
         * 0:InitSize
         * 1:InitRandState
         * 2:CalcVertexNum
         * 3:
         * 
         */

        switch (step)
        {
            case 0:
                {
                    GetComponent<MeshRenderer>().enabled = true;
                    wallSize = transform.lossyScale;
                    transform.localScale = Vector3.one;

                    InitRandState();
                }

                break;
            case 1:
                {
                    var numVertex = CalcVertexNum();

                    List<List<int>> grippableList = null;
                    Mesh mesh = CreateMesh(numVertex, out grippableList);

                    // メッシュの設定　再計算　リネーム
                    SetMeshAndRecalcRename(mesh);

                    // それぞれ地形の接続
                    Connect(mesh, numVertex);

                    // 掴み位置の設定
                    CreateGripPoint(mesh.vertices, grippableList);
                }
                break;
            case 2:
                {
                    // コライダーの設定
                    var collider = GetComponent<MeshCollider>();
                    collider.sharedMesh = GetMesh();
                }

                break;
            case 3:
                {
                    // 子をオブジェクト削除
                    foreach (Transform n in gameObject.transform)
                    {
                        if (n.tag == "GrippingPoint") continue;
                        GameObject.Destroy(n.gameObject);
                    }
                }

                break;
            case 4:
                {
                    Destroy(this);                   
                }
                break;
            default:
                return false;
                
        }

        return true;
    }

    private void Connect(Mesh mesh, Vector2Int numVertex)
    {
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
                        indexes[i] = CalcIndex(0, i, numVertex.x, numVertex.y);
                    break;

                case 1:
                    indexes = new int[numVertex.x];
                    for (int i = 0; i < numVertex.x; i++)
                        indexes[i] = CalcIndex(i, 0, numVertex.x, numVertex.y);
                    break;
                case 2:
                    indexes = new int[numVertex.y];
                    for (int i = 0; i < numVertex.y; i++)
                        indexes[i] = CalcIndex(numVertex.x - 1, i, numVertex.x, numVertex.y);

                    break;
                case 3:
                    indexes = new int[numVertex.x];
                    for (int i = 0; i < numVertex.x; i++)
                        indexes[i] = CalcIndex(i, numVertex.y - 1, numVertex.x, numVertex.y);
                    break;

                default:
                    break;

            }

            connecters[dir].targetsIndex[dir == 0 ? 1 : 0] = indexes;
            connecters[dir].targets[dir == 0 ? 1 : 0] = mesh;
            connecters[dir].transforms[dir == 0 ? 1 : 0] = transform;
            connecters[dir].Connect();
        }
    }

    public void InitRandState()
    {
        // 乱数のシード値表現について

        // 乱数初期化
        var postion = gameObject.transform.position;
        postion /= 120f;    // 120m周期
        seed = ((int)(Mathf.PerlinNoise(postion.x, postion.z) * (2 << 18))) + (seed << 18);

        UnityEngine.Random.InitState(seed);

    }

    public Vector2Int CalcVertexNum()
    {
        // 配置頂点
        // 全体頂点数
        int allNum = (int)Mathf.Sqrt(baseNumVertex);
        // スケールによって頂点の分配比率を設定
        Vector2Int numVertex = new Vector2Int(
            (int)(allNum * (wallSize.x / wallSize.y)),
            (int)(allNum * (wallSize.y / wallSize.x)));

        Debug.Log("頂点分配率 : " + numVertex);

        return numVertex;
    }

    public Mesh CreateMesh(Vector2Int numVertex, out List<List<int>> grippableList)
    {
        // ベースのメッシュを生成
        Mesh mesh = CreateBaseMesh(numVertex);

        // 壁のベースとして調整 ---------------
        var vertices = mesh.vertices;

        // 配置に偏りを作る
        for (int x = 1; x < numVertex.x - 1; x++)
        {
            for (int y = 1; y < numVertex.y - 1; y++)
            {
                int i = CalcIndex(x, y, numVertex.x, numVertex.y);
                float xRange = (wallSize.x / numVertex.x) * cellSizeFactor.x;
                float yRange = (wallSize.y / numVertex.y) * cellSizeFactor.y;
                xRange /= 2;
                yRange /= 2;

                vertices[i] += new Vector3(UnityEngine.Random.Range(-xRange, xRange), UnityEngine.Random.Range(-yRange, yRange), 0f);
            }
        }


        // メッシュ全体を立てる
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = Quaternion.AngleAxis(90f, Vector3.left) * vertices[i];



        System.Func<int, int, int, Vector3[], Vector3> putElevation = (int x, int y, int i, Vector3[] verticesConst) =>
        {
            float noise = Mathf.PerlinNoise(verticesConst[i].x / noiseCycle.x, verticesConst[i].y / noiseCycle.y);
            return Vector3.forward * baseBumpy * noise;
        };

        // 起伏を付加
        for (int x = 0; x < numVertex.x; x++)
        {
            for (int y = 0; y < numVertex.y; y++)
            {
                int i = CalcIndex(x, y, numVertex.x, numVertex.y);
                //vertices[i] = vertices[i] + putElevation(x, y, i, vertices);
                float noise = Mathf.PerlinNoise(vertices[i].x / noiseCycle.x, vertices[i].y / noiseCycle.y);
                noise = Mathf.Clamp(noise, noiseClamp.x, noiseClamp.y);
                vertices[i] = vertices[i] + Vector3.forward * baseBumpy * noise;

            }
        }

        // 特性の付加 --------------------------

        // 地形生成用のマップ i:y j:x
        var map = new char[numVertex.x * numVertex.y];
        for (int i = 0; i < map.Length; i++)
            map[i] = (char)0;

        // 掴み位置特性の付加
        var lineList = gameObject.GetComponentsInChildren<LineRenderer>();

        //var parameterList = gameObject.GetComponentInChildren<GrippableParameter>();

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
                int lerpDivision = (int)(posList[j] - posList[j + 1]).sqrMagnitude;
                for (int lerpStep = 0; lerpStep < lerpDivision; lerpStep++)
                {
                    var cellPos = calcCellPos(Vector2.Lerp(posList[j], posList[j + 1], lerpStep / (float)lerpDivision), gameObject.transform.position, wallSize, numVertex);
                    var tempIndex0 = CalcIndex((int)cellPos.x, (int)cellPos.y, numVertex.x, numVertex.y);
                    Debug.Assert(tempIndex0 != -1);

                    //if (map[tempIndex0] == (char)WallChipID.Grippable) continue;

                    var tempIndexTop = CalcIndex((int)cellPos.x, (int)cellPos.y + 1, numVertex.x, numVertex.y);
                    if (tempIndexTop == -1) continue;
                    var tempIndexBottom = CalcIndex((int)cellPos.x, (int)cellPos.y - 1, numVertex.x, numVertex.y);
                    if (tempIndexBottom == -1) continue;

                    map[tempIndex0] = (char)WallChipID.Grippable;

                    tempIndex0 = CalcIndex((int)cellPos.x + 1, (int)cellPos.y, numVertex.x, numVertex.y);
                    if (tempIndex0 != -1) map[tempIndex0] = (char)WallChipID.Grippable;

                    tempIndex0 = CalcIndex((int)cellPos.x - 1, (int)cellPos.y, numVertex.x, numVertex.y);
                    if (tempIndex0 != -1) map[tempIndex0] = (char)WallChipID.Grippable;

                    map[tempIndexBottom] = (char)WallChipID.WallReverse;


                    map[tempIndexTop] = true ? (char)WallChipID.Wall : (char) WallChipID.Ground;


                    int wallIndex = 0;
                    wallIndex = CalcIndex((int)cellPos.x - 1, (int)cellPos.y + 1, numVertex.x, numVertex.y);
                    if (wallIndex != -1) map[wallIndex] = (char)WallChipID.Wall;
                    wallIndex = CalcIndex((int)cellPos.x + 1, (int)cellPos.y + 1, numVertex.x, numVertex.y);
                    if (wallIndex != -1) map[wallIndex] = (char)WallChipID.Wall;
                    wallIndex = CalcIndex((int)cellPos.x, (int)cellPos.y + 2, numVertex.x, numVertex.y);
                    if (wallIndex != -1) map[wallIndex] = (char)WallChipID.Wall;

                    //vertices[tempIndex0] += Vector3.back * grippableBumpySize[0];    // verticesの配置はx, yが逆
                }
            }
        }


        // 掴み位置特性の付加
        grippableList = new List<List<int>>();
        grippableList.Add(new List<int>());
        bool isConnected = false;
        for (int j = 0; j < numVertex.y; j++)
        {
            grippableList.Add(new List<int>());
            for (int i = 0; i < numVertex.x; i++)
            {
                int tempIndex0 = CalcIndex(i, j, numVertex.x, numVertex.y); // 頂点インデックスに変換する
                Debug.Assert(tempIndex0 != -1);

                if (map[tempIndex0] != (char)WallChipID.Grippable)
                {
                    if (isConnected)
                    {
                        isConnected = false;
                        grippableList.Add(new List<int>());
                    }
                    continue;
                };

                isConnected = true;
                vertices[tempIndex0] += Vector3.back * baseGrippableBumpySize[0];    // verticesの配置はx, yが逆
                grippableList[grippableList.Count - 1].Add(tempIndex0);
            }
        }

        // 逆壁特性の付加
        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                int tempIndex1 = CalcIndex(i, j, numVertex.x, numVertex.y);
                if (tempIndex1 == -1) continue;
                if (map[tempIndex1] != (char)WallChipID.WallReverse) continue;

                int tempIndex0 = CalcIndex(i, j + 1, numVertex.x, numVertex.y); // 上のセルを調査
                if (tempIndex0 == -1) continue;

                vertices[tempIndex1] = new Vector3(vertices[tempIndex1].x, vertices[tempIndex1].y, vertices[tempIndex0].z + baseGrippableBumpySize[1]);
            }
        }

        // 地面特性の付加
        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                int tempIndex0 = CalcIndex(i, j, numVertex.x, numVertex.y);
                if (tempIndex0 == -1) continue;
                if (map[tempIndex0] != (char)WallChipID.Ground) continue;

                int tempIndex1 = CalcIndex(i, j - 1, numVertex.x, numVertex.y);
                if (tempIndex1 != -1) vertices[tempIndex0] = vertices[tempIndex1] + Vector3.back * vertices[tempIndex0].z;

            }
        }

        // 壁特性の付加
        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                int tempIndex0 = CalcIndex(i, j, numVertex.x, numVertex.y);
                if (tempIndex0 == -1) continue;
                if (map[tempIndex0] != (char)WallChipID.Wall) continue;

                int tempIndex1 = CalcIndex(i, j - 1, numVertex.x, numVertex.y);
                if (tempIndex1 != -1) vertices[tempIndex0] = new Vector3(vertices[tempIndex0].x, vertices[tempIndex0].y, vertices[tempIndex1].z + baseGrippableBumpySize[2]);
            }
        }

        // 湾曲特性の付加        
        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                int tempIndex = CalcIndex(i, j, numVertex.x, numVertex.y);
                if (tempIndex == -1) continue;
                float degressPerVert = curveDegressToAllOver.y / numVertex.x;
                vertices[tempIndex] = Quaternion.AngleAxis(degressPerVert * (i - (numVertex.x / 2)), Vector3.down) * vertices[tempIndex];
            }
        }

        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                int tempIndex = CalcIndex(i, j, numVertex.x, numVertex.y);
                if (tempIndex == -1) continue;
                float degressPerVert = curveDegressToAllOver.x / numVertex.y;
                vertices[tempIndex] = Quaternion.AngleAxis(degressPerVert * (j - (numVertex.y / 2)), Vector3.right) * vertices[tempIndex];
            }
        }

        // 軸指定で曲げる
        //? ?これちゃんと動いてる？
        for (int i = 0; i < numVertex.x; i++)
        {
            for (int j = 0; j < numVertex.y; j++)
            {
                int tempIndex = CalcIndex(i, j, numVertex.x, numVertex.y);
                if (tempIndex == -1) continue;

                foreach (var curvedToAxis in curveDegressToAxis)
                {
                    float curveDegress = curvedToAxis.z / 2;
                    if ((float)i / j < curvedToAxis.x / curvedToAxis.y)
                        curveDegress = -curveDegress;
                    
                    vertices[tempIndex] = Quaternion.AngleAxis(curveDegress, new Vector3(curvedToAxis.x, curvedToAxis.y)) * vertices[tempIndex];
                }
            }
        }

        // メッシュへの反映
        mesh.vertices = vertices;

        return mesh;
    }

    public void CreateGripPoint(Vector3[] vertices, List<List<int>> grippableList)
    {
        for (int i = 0; i < grippableList.Count; i++) // 次の頂点も同時に参照するため -1
        {
            for (int j = 0; j < grippableList[i].Count - 1; j++)
            {
                Vector3[] worldVert = new Vector3[2] { vertices[grippableList[i][j]], vertices[grippableList[i][j + 1]] };
                for (int worldVertI = 0; worldVertI < worldVert.Length; worldVertI++)
                    worldVert[worldVertI] = transform.TransformPoint(worldVert[worldVertI]);
                GrippablePoint2.CreateEdges(worldVert[0], worldVert[1], gameObject.transform);
            }
        }
    }

    public void SetMeshAndRecalcRename(Mesh mesh)
    {
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.MarkDynamic();
        mesh.name = "WallMesh_" + gameObject.name;

        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;
    }

    int CalcIndex(int x, int y, int xSize, int ySize)
    {
        // indexを求める 不正値の場合は -1
        if (y < 0 && ySize <= y && x < 0 && xSize <= x) return -1;
        var result = y + x * ySize;
        return result;

    }

    Mesh CreateBaseMesh(Vector2Int numVertex)
    {
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

                // 添字進行
                index++;
            }
        }

        // メッシュの生成
        var triangulator = new DelaunyTriangulation.Triangulator();
        Mesh mesh = triangulator.CreateInfluencePolygon(ver);
        return mesh;
    }

}
