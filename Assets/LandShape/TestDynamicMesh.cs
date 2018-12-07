using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

// 必須のコンポーネント
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class TestDynamicMesh : MonoBehaviour {

    [SerializeField]
    float wide = 5;

    [SerializeField]
    bool skipScene = false;

    public static GameObject self = null;

    // Use this for initialization
    void Awake() {
        if (self != null)
        {
            return;
        }

        self = gameObject;

        var filter = GetComponent<MeshFilter>();

        var mesh = filter.sharedMesh;
        mesh.name = "Dynamic mesh";

        // meshデータ代入用
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;
        var uvList = mesh.uv;

        mesh.vertices.CopyTo(vertices, 0);
        mesh.triangles = triangles;
        mesh.uv = uvList;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i] * wide;
        }
    
        //triangles = new int[]
        //{
        //    0,1,2,
        //    2,1,3
        //};

        //uvList = new Vector2[]
        //{
        //    new Vector2(0, 0),
        //    new Vector2(1,0),
        //    new Vector2(0,1),
        //    new Vector2(1,1)
        //};

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvList;

        // メッシュフィルターにメッシュ情報を渡させる
        filter.sharedMesh = mesh;

    }

    private void Start()
    {
        if (skipScene)
        {
            SceneManager.LoadScene("New Scene 1");
            skipScene = false;
        }
    }

    // Update is called once per frame
    void Update () {

    }
}
