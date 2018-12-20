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

        transform.rotation = Quaternion.FromToRotation(Vector3.right, edge[0] - edge[1]);  // 仮
                 
    }

    public Vector3 CalcMovement(Vector3 movement)
    {
        Vector3 grippingVec = Vector3.zero;

        grippingVec = edge[0] - gameObject.transform.position;
        if (Vector3.Dot(movement, grippingVec) > 0)
            return grippingVec.normalized;

        grippingVec = edge[1] - gameObject.transform.position;
        if (Vector3.Dot(movement, grippingVec) > 0)
            return grippingVec.normalized;

        return Vector3.zero;
    }

    public float SetHandsPosition(GameObject forward, GameObject back, Collider selfColi, float lerp = 1.0f)
    {
        GameObject[] hands = new GameObject[2] { forward, back};
        var pos = ClimberMethod.GetHandsPosition(hands[0], hands[1]);
        Vector3[] target = new Vector3[2];
        target[0] = selfColi.ClosestPoint(pos[0]);
        target[1] = selfColi.ClosestPoint(pos[1]);
        Vector3[] result = new Vector3[2];

        for (var i = 0; i < 2; i++)
        {
            result[i] = Vector3.Lerp(hands[i].transform.position, target[i], lerp);
        }

        hands[0].transform.position = result[0];
        hands[1].transform.position = result[1];

        float totalSqr = 0.0f;
        for (var i = 0; i < 2; i++)
            totalSqr += (hands[i].transform.position - target[i]).sqrMagnitude;

        return totalSqr;
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
