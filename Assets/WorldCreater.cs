using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 必須のコンポーネント
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class WorldCreater : MonoBehaviour {


    // 面のサイズ
    [SerializeField]
    Vector2Int size = new Vector2Int(10, 5);

    // Use this for initialization
    void Start () {
        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = CreateMesh();

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
        // 面数に必要な分だけ確保
        Vector3[] vertices = new Vector3[(size.x + 1) * (size.y + 1)];
        int[] triangles = new int[(size.x * size.y) * 6];
        Debug.Log("vert " + vertices.Length);
        Debug.Log("tri " + triangles.Length);
        // メッシュデータの生成
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(i % (size.x + 1), -i / (size.x + 1));
            Debug.Log("vert " + vertices[i]);

        }

        for (int i = 0; i < size.x * size.y; i++)
        {
            /*
             * x.x + y.x, x.y + y.x,
             * x.x + y.y, x.y + y.y
             */

            int polyNo = (i % size.x) * 6 + (i / size.x) * (size.x * 6);
            Vector2Int x = new Vector2Int((i % size.x), i % size.x + 1);
            Vector2Int y = new Vector2Int((i / size.x) * (size.x + 1), (i / size.x) * (size.x + 1) + (size.x + 1));
            triangles[polyNo]     = x.x + y.x;
            triangles[polyNo + 1] = x.y + y.x;
            triangles[polyNo + 2] = x.x + y.y;

            triangles[polyNo + 3] = x.y + y.x;
            triangles[polyNo + 4] = x.y + y.y;
            triangles[polyNo + 5] = x.x + y.y;


            Debug.Log("triNo" + (polyNo + 0).ToString() + " : "+ triangles[polyNo]     );
            Debug.Log("triNo" + (polyNo + 1).ToString() + " : "+ triangles[polyNo + 1] );
            Debug.Log("triNo" + (polyNo + 2).ToString() + " : "+ triangles[polyNo + 2] );

            Debug.Log("triNo" + (polyNo + 3).ToString() + " : "+ triangles[polyNo + 3] );
            Debug.Log("triNo" + (polyNo + 4).ToString() + " : "+ triangles[polyNo + 4] );
            Debug.Log("triNo" + (polyNo + 5).ToString() + " : "+ triangles[polyNo + 5]);

        }

        Mesh mesh = new Mesh();
        mesh.name = "TestMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }
}
