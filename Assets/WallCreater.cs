using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCreater : MonoBehaviour {

    [SerializeField, Range(0, 2 << 10)]
    int seed = 0;

    [SerializeField]
    int baseNumVertex = 50;

    [SerializeField, Range(1f, float.MaxValue)]
    float cellSize = 1f;

    [SerializeField]
    float baseBumpy = 1.0f;

    Vector3 wallSize = Vector3.zero;

    // Use this for initialization
    void Awake () {
        wallSize = transform.lossyScale;
        wallSize /= cellSize;
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
                    wallSize.x * cellSize / (numVertex.x - 1),
                    wallSize.y * cellSize / (numVertex.y - 1));  // 範囲調整用 i, j は(ループ回数-1)までの値しかとらないため
                offset.x *= wallSize.x * cellSize + addjust.x;
                offset.y *= wallSize.y * cellSize + addjust.y;
                offset += new Vector2(-wallSize.x / 2, -wallSize.y / 2) * cellSize;    // 中心に移動
                ver[index] = offset;                

                // 配置に偏りを作る
                ver[index] += new Vector2(UnityEngine.Random.Range(-cellSize / 2, cellSize / 2), UnityEngine.Random.Range(-cellSize / 2, cellSize / 2));

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
            vertices[i] += Vector3.up * baseBumpy *  Mathf.PerlinNoise(vertices[i].x, vertices[i].y);
            vertices[i] = Quaternion.AngleAxis(90f, Vector3.left) * vertices[i];
        }

        // メッシュの反映
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
