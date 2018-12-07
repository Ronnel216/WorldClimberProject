using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 必須のコンポーネント
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class DynamicMeth : MonoBehaviour {

    // 設定するMaterial
    [SerializeField]
    Material mat = null;

    // 軸となる頂点参照用
    [SerializeField]
    List<GameObject> nodeList = new List<GameObject>();

    // 捻じれのレベル (仮)
    [SerializeField] // (0f　～ 1f)
    float twistLevel = 0f;

    // 幅
    [SerializeField]
    float widthScale = 1;

    // 表面の面数
    [SerializeField]
    int faceNum = 3;

    // 木の節ごとの情報
    class TreeNode
    {
        public TreeNode()
        {
            axisVertex = new Vector3();
            width = 1.0f;            
        }
        public Vector3 axisVertex;
        public float width;
        public TreeNode[] nextNodeList;

        public static int num = 0;
    }

    // Use this for initialization
    void Start () {
        // 設定の有効性の確認
        CheckSetting();

        // ノードの生成
        TreeNode rootNodeList;
        CreateTreeNode(out rootNodeList);

        //TreeNode[] testNode = new TreeNode[2];
        //testNode[0] = new TreeNode();
        //testNode[1] = new TreeNode();
        //testNode[1].axisVertex = Vector3.left;
        //treeNodeList[2].nextNodeList = testNode;

        // メッシュ情報の生成
        var mesh = new Mesh();
        mesh.name = "Dyanmic Tree Mesh";

        // meshデータ代入用
        Vector3[] vertices;
        int[] triangles;
        Vector2[] uvList;
               
        // メッシュデータの生成
        CreateTreeMesh(out vertices, out triangles, out uvList, rootNodeList);

        // データの設定
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvList;

        // 法線計算
        mesh.RecalculateNormals();

        // メッシュフィルターにメッシュ情報を渡させる
        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        // Materialの設定
        var renderer = GetComponent<MeshRenderer>();
        renderer.material = mat;
	}

    /// <summary>
    /// 設定の有効性の確認
    /// </summary>
    private void CheckSetting()
    {
        string errMsg = "";
        //// 軸となる頂点は1つ以上必要
        //if (nodeList.Count < 1) errMsg =
        //        "Need Node : Num > 0\n";

        //// 面数は2つ以上必要
        //if (faceNum <= 1) errMsg =
        //        "Need FaceNum : Num > 2\n";

        //// 設定の有効性の確認
        //if (errMsg.Length > 0)
        //{
        //    Debug.Log("Setting Err\n" + errMsg);
        //    Debug.Break();
        //}
    }

    /// <summary>
    /// 木構造のノード生成
    /// </summary>
    private void CreateTreeNode(out TreeNode rootNodeList)
    {
        rootNodeList = new TreeNode();
        TreeNode.num++;

        int i = 0;

        // 仮
        TreeNode currentNode = rootNodeList;
        for (i = 0; i < nodeList.Count; i++)
        {
            currentNode.nextNodeList = new TreeNode[1];
            currentNode.nextNodeList[0] = new TreeNode();
            currentNode = currentNode.nextNodeList[0];
            TreeNode.num++;
        }

        // ノード + 基点 分のノードを作成
        //rootNodeList = new TreeNode[nodeList.Count + 1];
        //for (int i = 0; i < rootNodeList.Length; i++)
        //    rootNodeList[i] = new TreeNode();

        // 軸となる頂点部分の設定
        rootNodeList.axisVertex = gameObject.transform.position;
        currentNode = rootNodeList.nextNodeList[0];
        i = 1;
        while (currentNode != null)
        {
            currentNode.axisVertex = nodeList[i - 1].transform.position;
            currentNode = currentNode.nextNodeList[0];
        }

        //for (int i = 1; i < rootNodeList.Length; i++)
        //    rootNodeList[i].axisVertex = nodeList[i - 1].transform.position;

        // 幅の設定
        currentNode = rootNodeList;
        i = 0;
        while (currentNode != null)
        {
            currentNode.width = TreeNode.num - (i + 1);
            currentNode = currentNode.nextNodeList[0];
        }

        //for (int i = 0; i < rootNodeList.Length; i++)
        //    rootNodeList[i].width = rootNodeList.Length - (i + 1);
    }

    /// <summary>
    /// メッシュデータを生成  (再帰呼び出し)
    /// </summary>
    void CreateTreeMesh(out Vector3[] vertices, out int[] triangles, out Vector2[] uvList, TreeNode rootNodeList)
    {
        var vertRe = new List<Vector3>();
        var triRe = new List<int>();
        var uvRe = new List<Vector2>();

        CreateTreeMesh(ref vertRe, ref triRe, ref uvRe, rootNodeList);

        vertices = vertRe.ToArray();
        triangles = triRe.ToArray();
        uvList = uvRe.ToArray();

    }

    /// <summary>
    /// メッシュデータを生成
    /// </summary>
    void CreateTreeMesh(ref List<Vector3> vertices, ref List<int> triangles, ref  List<Vector2> uvList, TreeNode rootNodeList)
    {
        //// 子を優先 (メモリ節約)
        //foreach (var treeNode in rootNodeList.nextNodeList)
        //{
        //    CreateTreeMesh(ref vertices, ref triangles, ref uvList, treeNode);
        //}

        // 仮の入れ物
        Vector3[] vert;
        int[] tri;
        Vector2[] uv;

        // メッシュ頂点の算出
        int[] indexOrder;  // 一面分の頂点を結ぶ順番
        CalcMeshVerticesBaseNode(out vert, out indexOrder, rootNodeList);

        // 頂点を結ぶ順番の設定
        int meshCount = faceNum * nodeList.Count;       // 面数
        SetTrianglesIndex(out tri, meshCount, TreeNode.num, indexOrder);

        // uv座標の設定
        SetUVList(out uv, vert.Length, faceNum);

        // データの追加
        vertices.AddRange(vert);
        triangles.AddRange(tri);
        uvList.AddRange(uv);

    }


    /// <summary>
    /// UVの設定
    /// </summary>
    void SetUVList(out Vector2[] uvList, int vertices, int faceNum)
    {
        /* ループで設定
        (0f, 0f), (.5f, 0f), (1f, 0.f)         
        (0f, 1f), (.5f, 1f), (1f, 1.f) 
        (0f, 0f), (.5f, 0f), (1f, 0.f) 
         */

        uvList = new Vector2[vertices];
        for (int i = 0; i < nodeList.Count; i++)
        {
            int offset = i * faceNum;
            for (int j = 0; j < faceNum; j++)
            {
                uvList[offset + j] = new Vector2((float)j / (faceNum - 1), 0f);
                uvList[offset + j + faceNum] = new Vector2((float)j / (faceNum - 1), 1f);
            }

        }

    }

    /// <summary>
    /// ノードを元に頂点を生成する
    /// </summary>
    void CalcMeshVerticesBaseNode(out Vector3[] vertices, out int[] indexOrder, TreeNode rootNode)
    {
        /*　一つの軸となる頂点に対して必要な頂点数
         面数　　1, 2, 3, 4, 5 ...
         頂点数  2, 2, 3, 4, 5 ...
         */

        /*　生成するメッシュ 一つの軸に対する面数 : n  軸の数 : m     頂点配置 : 軸を基準に筒状に配置
        ...
        m(n + 1), m(n + 2), m(n + 3) ...
        n + 1, n + 2, n + 3 ...
        0, 1, 2 ...
         */

        // 軸の向きを算出
        var directionList = new Vector3[TreeNode.num];
        TreeNode backNode = null;
        var currentNode = rootNode;
        var nextNode = rootNode.nextNodeList[0];
        for (int i = 0; i < TreeNode.num; i++)
        {            
            // 始点            
            if (i == 0)
                directionList[i] = nextNode.axisVertex - currentNode.axisVertex;
            // 終点
            else if (i == TreeNode.num - 1)
                directionList[i] = currentNode.axisVertex - backNode.axisVertex;
            // 中点
            else
                directionList[i] = backNode.axisVertex - nextNode.axisVertex;           

            directionList[i].Normalize();

            // 対象のノードを進める
            backNode = currentNode;
            currentNode = nextNode;
            nextNode = nextNode.nextNodeList[0];
        }

        // 軸の表面となる頂点の算出 ------------------
        vertices = new Vector3[TreeNode.num * faceNum];
        float baseRadius = 0.0f;              // 配置の際に基準となる軸を元にした角度
        float nextRadius = 360.0f / faceNum;  // 均等に分割するため (変えても面白いかも)
        
        // 面の方向
        Vector3 faceDirection = gameObject.transform.rotation * -Vector3.forward;       
        
        // 表面頂点の算出用
        System.Func<int, int, float, Vector3> calcVertexMove = (int rotIndex, int directionIndex, float width) => {
            Vector3 faceAxisVertexBase = faceDirection * (width / 2f);   // 幅
            faceAxisVertexBase *= widthScale;                            // 幅スケールの反映

            float distortion = UnityEngine.Random.Range(0f, nextRadius * twistLevel); // 歪み (歪みすぎ防止のため * twistLevel)
            return Quaternion.AngleAxis(baseRadius - (nextRadius * rotIndex) + distortion, directionList[directionIndex]) * faceAxisVertexBase;
        };

        // 表面頂点の算出
        currentNode = rootNode;
        int n = 0;  // 順番に
        while (currentNode != null)
        {
            int offset = n * faceNum;
            Vector3 offsetPos = currentNode.axisVertex;
            for (int j = 0; j < faceNum; j++)
                vertices[offset + j] = offsetPos + calcVertexMove(j, n, currentNode.width);
            n++;
        }

        /*
         面数         　1, 2, 3, 4, 5 ...      faceNum
         上の始まり　　 2, 3, 4, 5, 6 ...      faceNum + 1
         順番のやつ     6, 12, 18, 24, 30 ...  faceNum * 6
         半分　　　　　 3, 6,   9, 12, 15 ...  faceNum * 6 / 2
         */

        // 処理する面の左下のインデックス ( 順に面に対してナンバリング )
        // n とする

        // { 1, 0, faceNum + 1, faceNum + 1, faceNum + 1 + 1, 1 }

        // { 1, 0, faceNum + 1, faceNum + 1, faceNum + 1 + 1, 1,
        //   1 + 1, 0 + 1, faceNum + 1 + 1, faceNum + 1 + 1, faceNum + 1 + 1 + 1, 1 + 1}

        // 各面の頂点順は
        // {1 + n, 0 + n, faceNum + 1 + n, faceNum + 1 + n, faceNum + 1 + 1 + n, 1 + n}

        // 各面の頂点を結ぶ順番
        int index = 0;
        indexOrder = new int[faceNum * 6];
        int topLeftIndex = faceNum + 1 - 1;
        for (int i = 0; i < faceNum; i++)
        {         
            if (i + 1 < faceNum)
            {
                indexOrder[index++] = 1 + i;                // ┘
                indexOrder[index++] = 0 + i;                // └
                indexOrder[index++] = topLeftIndex + i;     // ┌

                indexOrder[index++] = topLeftIndex + i;     // ┌
                indexOrder[index++] = topLeftIndex + 1 + i; // ┐
                indexOrder[index++] = 1 + i;                // ┘
            }
            // 最後は繋げる (一部の値を左端で指定)
            else
            {
                indexOrder[index++] = 0;                    // ┘ (左端)
                indexOrder[index++] = 0 + i;                // └
                indexOrder[index++] = topLeftIndex + i;     // ┌

                indexOrder[index++] = topLeftIndex + i;     // ┌
                indexOrder[index++] = topLeftIndex;         // ┐ (左端)
                indexOrder[index++] = 0;                    // ┘ (左端)
            }
        }

    }
    /// <summary>
    /// ノードを元に頂点を生成する
    /// </summary>
    void CalcMeshVerticesBaseNode(out Vector3[] vertices, out int[] indexOrder, TreeNode[] treeNodeList)
    {
        /*　一つの軸となる頂点に対して必要な頂点数
         面数　　1, 2, 3, 4, 5 ...
         頂点数  2, 2, 3, 4, 5 ...
         */

        /*　生成するメッシュ 一つの軸に対する面数 : n  軸の数 : m     頂点配置 : 軸を基準に筒状に配置
        ...
        m(n + 1), m(n + 2), m(n + 3) ...
        n + 1, n + 2, n + 3 ...
        0, 1, 2 ...
         */

        // 軸の向きを算出
        var directionList = new Vector3[treeNodeList.Length];
        for (int i = 0; i < treeNodeList.Length; i++)
        {
            // 始点            
            if (i == 0)
                directionList[i] = treeNodeList[i + 1].axisVertex - treeNodeList[i].axisVertex;
            // 終点
            else if (i == treeNodeList.Length - 1)
                directionList[i] = treeNodeList[i].axisVertex - treeNodeList[i - 1].axisVertex;
            // 中点
            else
                directionList[i] = treeNodeList[i + 1].axisVertex - treeNodeList[i - 1].axisVertex;

            directionList[i].Normalize();
        }

        // 軸の表面となる頂点の算出 ------------------
        vertices = new Vector3[treeNodeList.Length * faceNum];
        float baseRadius = 0.0f;              // 配置の際に基準となる軸を元にした角度
        float nextRadius = 360.0f / faceNum;  // 均等に分割するため (変えても面白いかも)

        // 面の方向
        Vector3 faceDirection = gameObject.transform.rotation * -Vector3.forward;

        // 表面頂点の算出用
        System.Func<int, int, float, Vector3> calcVertexMove = (int rotIndex, int directionIndex, float width) => {
            Vector3 faceAxisVertexBase = faceDirection * (width / 2f);   // 幅
            faceAxisVertexBase *= widthScale;                            // 幅スケールの反映

            float distortion = UnityEngine.Random.Range(0f, nextRadius * twistLevel); // 歪み (歪みすぎ防止のため * distortionLevel)
            return Quaternion.AngleAxis(baseRadius - (nextRadius * rotIndex) + distortion, directionList[directionIndex]) * faceAxisVertexBase;
        };

        // 表面頂点の算出
        for (int i = 0; i < treeNodeList.Length; i++)
        {
            int offset = i * faceNum;
            Vector3 offsetPos = treeNodeList[i].axisVertex;
            for (int j = 0; j < faceNum; j++)
                vertices[offset + j] = offsetPos + calcVertexMove(j, i, treeNodeList[i].width);
        }

        /*
         面数         　1, 2, 3, 4, 5 ...      faceNum
         上の始まり　　 2, 3, 4, 5, 6 ...      faceNum + 1
         順番のやつ     6, 12, 18, 24, 30 ...  faceNum * 6
         半分　　　　　 3, 6,   9, 12, 15 ...  faceNum * 6 / 2
         */

        // 処理する面の左下のインデックス ( 順に面に対してナンバリング )
        // n とする

        // { 1, 0, faceNum + 1, faceNum + 1, faceNum + 1 + 1, 1 }

        // { 1, 0, faceNum + 1, faceNum + 1, faceNum + 1 + 1, 1,
        //   1 + 1, 0 + 1, faceNum + 1 + 1, faceNum + 1 + 1, faceNum + 1 + 1 + 1, 1 + 1}

        // 各面の頂点順は
        // {1 + n, 0 + n, faceNum + 1 + n, faceNum + 1 + n, faceNum + 1 + 1 + n, 1 + n}

        // 各面の頂点を結ぶ順番
        int index = 0;
        indexOrder = new int[faceNum * 6];
        int topLeftIndex = faceNum + 1 - 1;
        for (int i = 0; i < faceNum; i++)
        {
            if (i + 1 < faceNum)
            {
                indexOrder[index++] = 1 + i;                // ┘
                indexOrder[index++] = 0 + i;                // └
                indexOrder[index++] = topLeftIndex + i;     // ┌

                indexOrder[index++] = topLeftIndex + i;     // ┌
                indexOrder[index++] = topLeftIndex + 1 + i; // ┐
                indexOrder[index++] = 1 + i;                // ┘
            }
            // 最後は繋げる (一部の値を左端で指定)
            else
            {
                indexOrder[index++] = 0;                    // ┘ (左端)
                indexOrder[index++] = 0 + i;                // └
                indexOrder[index++] = topLeftIndex + i;     // ┌

                indexOrder[index++] = topLeftIndex + i;     // ┌
                indexOrder[index++] = topLeftIndex;         // ┐ (左端)
                indexOrder[index++] = 0;                    // ┘ (左端)
            }
        }

    }

    /// <summary>
    /// 頂点の順番の設定
    /// </summary>
    void SetTrianglesIndex(out int[] triangles, int meshCount, int axisCount, int[] indexOrder)
    {
        // 三角形ポリゴン2枚で四角形ポリゴンを表現
        // 一面に対して6個の頂点が必要
        triangles = new int[(meshCount * 6)];

        /*
         9, 10, 11
         6, 7, 8
         3, 4, 5
         0, 1, 2
         */

        // 頂点を結ぶ設定 (右回り)    Loop回数は 軸の数を示す
        int positionIndex = 0;
        for (int i = 0; i < axisCount; i++)
        {
            int offset = i * (faceNum + 1 - 1);
            foreach (var index in indexOrder)
                triangles[positionIndex++] = offset + index;

        }


    }

    //void CalcMeshVerticesBaseNodeOneFace(out Vector3[] vertices, out int[] indexOrder, List<GameObject> axisNodeList)
    //{
    //    // 面の方向
    //    Vector3 faceDirection = -Vector3.forward;

    //    // 軸となる頂点部分の取得(node頂点)
    //    axisVertexList.Add(gameObject.transform.position);  // 自身(root)
    //    foreach (var node in axisNodeList)
    //        axisVertexList.Add(node.transform.position);    // 節目(node)

    //    // 一つの軸となる頂点に対して必要な頂点数
    //    /*
    //     面数　　1, 2, 3, 4, 5 ...
    //     頂点数  2, 2, 3, 4, 5 ...
    //     */
    //    const int vertexNumForAxis = 2;


    //    // 軸の向きを算出
    //    var directionList = new Vector3[axisVertexList.Count];
    //    for (int i = 0; i < axisVertexList.Count; i++)
    //    {
    //        // 始点            
    //        if (i == 0)
    //            directionList[i] = axisVertexList[i + 1] - axisVertexList[i];
    //        // 終点
    //        else if (i == axisVertexList.Count - 1)
    //            directionList[i] = axisVertexList[i] - axisVertexList[i - 1];
    //        // 中点
    //        else
    //            directionList[i] = axisVertexList[i + 1] - axisVertexList[i - 1];

    //        directionList[i].Normalize();
    //    }

    //    // 頂点生成
    //    vertices = new Vector3[axisVertexList.Count * vertexNumForAxis];
    //    for (int i = 0; i < axisVertexList.Count; i++)
    //    {
    //        // 軸となる頂点から右方向へのベクトル (軸のサイドに頂点を配置するために)
    //        Vector3 side = Quaternion.AngleAxis(90.0f, faceDirection) * directionList[i];
    //        side.Normalize();

    //        // 軸となる頂点の両端の頂点の設定
    //        int offset = i * vertexNumForAxis;
    //        vertices[offset] = axisVertexList[i] + (-side * width / 2.0f);
    //        vertices[offset + 1] = axisVertexList[i] + (side * width / 2.0f);

    //    }

    //    // 一面分の頂点を結ぶ順番
    //    indexOrder = new int[] { 1, 0, 2, 2, 3, 1 };

    //}
}
