using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripPoint : MonoBehaviour {

    [SerializeField]
    Vector3[] edge = new Vector3[2];   
    
    void Awake()
    {
        float radius = 0.1f;
        transform.localScale = new Vector3(radius, radius, radius);

        transform.position = (edge[0] + edge[1]) / 2;
        transform.localScale = new Vector3((edge[0] - edge[1]).magnitude, radius, radius); // 仮
                 
    }

    public Vector3 CalcMovement(Vector3 movement)
    {
        return movement.normalized;    // 仮　実際はedgeを元に計算を行う
    }

    public void SetHandsPosition(GameObject forward, GameObject back)
    {
        GameObject[] hands = new GameObject[2] { forward, back};
        var pos = ClimberMethod.GetHandsPosition(hands[0], hands[1]);
        hands[0].transform.position = new Vector3(pos[0].x, edge[0].y, pos[0].z);   // 仮    実際は傾きや位置などで変わる
        hands[1].transform.position = new Vector3(pos[1].x, edge[0].y, pos[1].z);   // 仮

    }

    /// <summary>
    /// 移動方向に近い端を取得する
    /// </summary>
    /// <param name="movement"></param>
    /// <returns></returns>
    public Vector3 GetEdge(Vector3 movement)
    {
        float a0 = ((edge[0] - edge[1]) - movement).sqrMagnitude;
        float a1 = ((edge[1] - edge[0]) - movement).sqrMagnitude;

        if (a0 < a1) return edge[0];
        return edge[1];
    }

}
