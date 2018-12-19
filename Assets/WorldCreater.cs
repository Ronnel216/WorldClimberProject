using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

// 必須のコンポーネント
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class WorldCreater : MonoBehaviour {

    // 一面あたりに必要な頂点指定数
    /* 例
     * 0,1,
     * 2,3
     * triangles = {0, 1, 2, 1, 3, 2} 
     */
    const int TrianglesNumPerFace = 6;

    // 地形のポリゴン数　面数
    [SerializeField]
    Vector2Int polyNum = new Vector2Int(10, 5);
    // 頂点数
    Vector2Int vertNum;

    // Use this for initialization
    void Start () {
        // 面数に必要な分だけ確保
        vertNum = new Vector2Int(polyNum.x + 1, polyNum.y + 1);

        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = CreateMesh();
        AssetDatabase.CreateAsset(filter.sharedMesh, "Assets/Temp/"+ filter.sharedMesh.name +".asset");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    

    /// <summary>
    /// メッシュの生成
    /// </summary>
    /// <returns></returns>
    Mesh CreateMesh()
    {
        // メッシュ設定データ
        Vector3[] vertices = new Vector3[vertNum.x * vertNum.y];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[(polyNum.x * polyNum.y) * TrianglesNumPerFace];

        // メッシュデータの生成 -----

        // 頂点の配置 地形の形作り
        ShapeLandShape(ref vertices, ref uv);

        // 頂点インデックスの割り当て
        for (int i = 0; i < polyNum.x * polyNum.y; i++)
        {
            // ポリゴン 面の番号
            int polyNo = (i % polyNum.x) * TrianglesNumPerFace + (i / polyNum.x) * (polyNum.x * TrianglesNumPerFace);

            // 頂点インデックス計算用
            /*
             * x.x + y.x, x.y + y.x,
             * x.x + y.y, x.y + y.y
             */
            Vector2Int x = new Vector2Int((i % polyNum.x), (i % polyNum.x) + 1);
            Vector2Int y = new Vector2Int((i / polyNum.x) * (polyNum.x + 1), (i / polyNum.x) * vertNum.x + vertNum.x);

            triangles[polyNo]     = x.x + y.x;
            triangles[polyNo + 1] = x.y + y.x;
            triangles[polyNo + 2] = x.x + y.y;

            triangles[polyNo + 3] = x.y + y.x;
            triangles[polyNo + 4] = x.y + y.y;
            triangles[polyNo + 5] = x.x + y.y;

        }

        // 生成したデータの設定 ---

        Mesh mesh = new Mesh();
        mesh.name = "TestMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        // 再計算
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void ShapeLandShape(ref Vector3[] vertices, ref Vector2[] uv)
    {
        // 頂点の位置の倍率係数
        // 1mとの比較で表記
        // uvに使用する
        Vector2[] vertFactors = new Vector2[vertices.Length];

        // 頂点の配置
        for (int i = 0; i < vertices.Length; i++)
        {
            int x = i % vertNum.x, y = i / vertNum.x;

            // 頂点の変位
            // 仮　頂点の位置は角度で決めるので　変位での計算は多分しない
            Vector3 translationVec = new Vector3(1, -1);
            
            vertices[i] = new Vector3(x * translationVec.x, y * translationVec.y);
            if (y == 2) // 仮でゆがませている
            {
                vertices[i] = new Vector3(x * translationVec.x, y * translationVec.y, -1);
            }

            if (y == 3) // 仮でゆがませている
            {
                vertices[i] = new Vector3(x * translationVec.x, y * translationVec.y, -1);
            }

            vertices[i] -= new Vector3(vertNum.x / 2.0f, -vertNum.y, 0);     // 仮のメッシュのOffset
        }

        for (int i = 0; i < vertFactors.Length; i++) // 係数の調整
        {
            Debug.Log("vertFactor" + i.ToString() + vertFactors[i]);

        }

        // uvの設定
        // ! 面の数は　x > 1 && y > 1とする
        for (int i = 0; i < vertices.Length; i++)
        {
            int x = i % vertNum.x, y = i / vertNum.x;
            Vector3 uvWide = new Vector3(1.0f/vertNum.x, 1.0f/ vertNum.y, 0);  // 一面に付きテクスチャ一枚

            //Vector2 point = new Vector2(uvWide.x * vertices[i].x, uvWide.y * vertices[i].y);　// xy平面のみ対応
            Vector2 point = new Vector2(vertices[i].x, vertices[i].y);　// xy平面のみ対応

            if (i == 2)
            {
            }

            uv[i] = point;
        }       
        
    }
}
